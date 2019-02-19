using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEvents.Internal;

namespace UnityEvents
{
	/// <summary>
	/// Event system that processes regular events.
	/// </summary>
	/// <typeparam name="T_Event">The event the system is responsible for.</typeparam>
	public class EventHandlerStandard<T_Event> : IEventSystem, IDisposable where T_Event : struct
	{
		private NativeList<QueuedEvent<T_Event>> _queuedEvents;
		private NativeList<EventTarget> _subscribers;

		private List<Action<T_Event>> _subscriberCallbacks;
		private Dictionary<EntityCallbackId<T_Event>, int> _entityCallbackToIndex;

		private bool _disposed;
		
		private readonly int _batchCount;

		private const int DEFAULT_EVENTS_TO_PROCESS_CAPACITY = 10;
		private const int DEFAULT_SUBSCRIBER_CAPACITY = 100;
		private const int DEFAULT_PARALLEL_BATCH_COUNT = 32;

		public EventHandlerStandard() : this(
			DEFAULT_SUBSCRIBER_CAPACITY,
			DEFAULT_EVENTS_TO_PROCESS_CAPACITY,
			DEFAULT_PARALLEL_BATCH_COUNT)
		{
		}

		/// <summary>
		/// Create a system with some custom settings.
		/// </summary>
		/// <param name="subscriberStartingCapacity">The starting capacity of subscriber containers.</param>
		/// <param name="queuedEventsStartingCapacity">The starting capacity of queued events containers.</param>
		/// <param name="parallelBatchCount">The batch count allowed per thread for parallel processing.</param>
		public EventHandlerStandard(
			int subscriberStartingCapacity,
			int queuedEventsStartingCapacity,
			int parallelBatchCount)
		{
#if !DISABLE_EVENT_SAFETY_CHKS
			if (!UnsafeUtility.IsBlittable<T_Event>())
			{
				throw new EventTypeNotBlittableException(typeof(T_Event));
			}
#endif
			_batchCount = parallelBatchCount;

			_subscribers = new NativeList<EventTarget>(subscriberStartingCapacity, Allocator.Persistent);
			_subscriberCallbacks = new List<Action<T_Event>>(subscriberStartingCapacity);
			_entityCallbackToIndex = new Dictionary<EntityCallbackId<T_Event>, int>(subscriberStartingCapacity);

			_queuedEvents = new NativeList<QueuedEvent<T_Event>>(queuedEventsStartingCapacity, Allocator.Persistent);
		}

		/// <summary>
		/// Disposes of some internally held state.
		/// </summary>
		public void Dispose()
		{
#if !DISABLE_EVENT_SAFETY_CHKS
			if (_disposed)
			{
				_disposed = true;
				return;
			}
#endif
			_queuedEvents.Dispose();
			_subscribers.Dispose();
		}

		/// <summary>
		/// Subscribe a listener to the system.
		/// </summary>
		/// <param name="target">The target to subscribe to.</param>
		/// <param name="callback">The callback that is invoked when an event fires.</param>
		public void Subscribe(EventTarget target, Action<T_Event> callback)
		{
#if !DISABLE_EVENT_SAFETY_CHKS
			if (_entityCallbackToIndex.ContainsKey(new EntityCallbackId<T_Event>(target, callback)))
			{
				throw new MultipleSubscriptionsException<T_Event>(callback);
			}
#endif

			_entityCallbackToIndex.Add(new EntityCallbackId<T_Event>(target, callback), _subscribers.Length);
			_subscribers.Add(target);
			_subscriberCallbacks.Add(callback);
		}

		/// <summary>
		/// Unsubscribe a listener from the system.
		/// </summary>
		/// <param name="target">The target to unsubscribe from.</param>
		/// <param name="callback">The callback that was invoked during events.</param>
		public void Unsubscribe(EventTarget target, Action<T_Event> callback)
		{
			EntityCallbackId<T_Event> callbackId = new EntityCallbackId<T_Event>(target, callback);

			if (_entityCallbackToIndex.TryGetValue(callbackId, out int index))
			{
				_entityCallbackToIndex.Remove(callbackId);
				
				_subscribers.RemoveAtSwapBack(index);

				_subscriberCallbacks[index] = _subscriberCallbacks[_subscriberCallbacks.Count - 1];
				_subscriberCallbacks.RemoveAt(_subscriberCallbacks.Count - 1);

				if (index != _subscribers.Length)
				{
					EntityCallbackId<T_Event> otherCallbackId = new EntityCallbackId<T_Event>(
						_subscribers[index],
						_subscriberCallbacks[index]);

					_entityCallbackToIndex[otherCallbackId] = index;
				}
			}
		}

		/// <summary>
		/// Queue an event to be processed later.
		/// </summary>
		/// <param name="target">The target the event is for.</param>
		/// <param name="ev">The event to queue.</param>
		public void QueueEvent(EventTarget target, T_Event ev)
		{
			if (_subscribers.Length == 0)
			{
				return;
			}
			
			_queuedEvents.Add(new QueuedEvent<T_Event>(target, ev));
		}

		/// <summary>
		/// Process all queued events.
		/// </summary>
		public void ProcessEvents()
		{
			// Early bail to avoid setting up job stuff unnecessarily
			if (_queuedEvents.Length == 0)
			{
				return;
			}

			NativeQueue<UnityEvent<T_Event>> eventsToProcessQueue =
				new NativeQueue<UnityEvent<T_Event>>(Allocator.TempJob);

			BuildEventQueueJob job = new BuildEventQueueJob();
			job.queuedEvents = _queuedEvents;
			job.subscribers = _subscribers;
			job.eventsToProcess = eventsToProcessQueue.ToConcurrent();

			job.Schedule(_queuedEvents.Length, _batchCount).Complete();

			while (eventsToProcessQueue.TryDequeue(out UnityEvent<T_Event> ev))
			{
				try
				{
					_subscriberCallbacks[ev.subscriberIndex](ev.ev);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}

			eventsToProcessQueue.Dispose();
			_queuedEvents.Clear();
		}

		/// <summary>
		/// Reset the system. Removes all listeners and queued events.
		/// </summary>
		public void Reset()
		{
			_queuedEvents.Clear();
			_subscribers.Clear();
			_subscriberCallbacks.Clear();
			_entityCallbackToIndex.Clear();
		}

		/// <summary>
		/// Debug log an error for each subscriber that is still listening.
		/// </summary>
		public void VerifyNoSubscribers()
		{
			if (_subscribers.Length > 0)
			{
				// Just throw the first one, it'll get resolved
				throw new SubscriberStillListeningException<T_Event, T_Event>(_subscriberCallbacks);
			}
		}

		[BurstCompile]
		private struct BuildEventQueueJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeList<QueuedEvent<T_Event>> queuedEvents;

			[ReadOnly]
			public NativeList<EventTarget> subscribers;

			public NativeQueue<UnityEvent<T_Event>>.Concurrent eventsToProcess;

			public void Execute(int index)
			{
				int count = subscribers.Length;
				QueuedEvent<T_Event> ev = queuedEvents[index];

				for (int i = 0; i < count; i++)
				{
					if (subscribers[i].Equals(ev.target))
					{
						eventsToProcess.Enqueue(new UnityEvent<T_Event>(ev.ev, i));
					}
				}
			}
		}
	}
}