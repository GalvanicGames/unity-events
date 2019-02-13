using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class UnityEventSystemDOP<T_Event> : IEventSystem, IDisposable where T_Event : unmanaged
{
	private NativeList<QueuedEvent<T_Event>> _queuedEvents;
	private NativeList<EventEntity> _subscribers;

	private List<Action<T_Event>> _subscriberCallbacks;
	private Dictionary<EntityCallbackId<T_Event>, int> _entityCallbackToIndex;
	
	private readonly int _batchCount;
	
	private const int DEFAULT_EVENTS_TO_PROCESS_CAPACITY = 10;
	private const int DEFAULT_SUBSCRIBER_CAPACITY = 100;
	private const int DEFAULT_PARALLEL_BATCH_COUNT = 32;
	
	public UnityEventSystemDOP(): this(
		DEFAULT_SUBSCRIBER_CAPACITY, 
		DEFAULT_EVENTS_TO_PROCESS_CAPACITY,
		DEFAULT_PARALLEL_BATCH_COUNT)
	{
	}

	public UnityEventSystemDOP(
		int subscriberStartingCapacity, 
		int queuedEventsStartingCapacity,
		int parallelBatchCount)
	{
		_batchCount = parallelBatchCount;
		
		_subscribers = new NativeList<EventEntity>(subscriberStartingCapacity, Allocator.Persistent);
		_subscriberCallbacks = new List<Action<T_Event>>(subscriberStartingCapacity);
		_entityCallbackToIndex = new Dictionary<EntityCallbackId<T_Event>, int>(subscriberStartingCapacity);	
		
		_queuedEvents = new NativeList<QueuedEvent<T_Event>>(queuedEventsStartingCapacity, Allocator.Persistent);
	}
	
	public void Dispose()
	{
		_queuedEvents.Dispose();
		_subscribers.Dispose();
	}
	
	public void Subscribe(EventEntity entity, Action<T_Event> callback)
	{
#if !DISABLE_EVENT_SAFETY_CHKS
		if (_entityCallbackToIndex.ContainsKey(new EntityCallbackId<T_Event>(entity, callback)))
		{
			Debug.LogError("Not allowed to subscribe the same callback to the same entity! " + callback.Target.GetType().Name);
			return;
		}
#endif
		
		_entityCallbackToIndex.Add(new EntityCallbackId<T_Event>(entity, callback), _subscribers.Length);
		_subscribers.Add(entity);
		_subscriberCallbacks.Add(callback);
	}

	public void Unsubscribe(EventEntity entity, Action<T_Event> callback)
	{
		EntityCallbackId<T_Event> callbackId = new EntityCallbackId<T_Event>(entity, callback);
		
		if (_entityCallbackToIndex.TryGetValue(callbackId, out int index))
		{
			_subscribers.RemoveAtSwapBack(index);
			
			_subscriberCallbacks[index] = _subscriberCallbacks[_subscriberCallbacks.Count - 1];
			_subscriberCallbacks.RemoveAt(_subscriberCallbacks.Count - 1);

			_entityCallbackToIndex.Remove(callbackId);
		}
	}
	
	public void QueueEvent(EventEntity entity, T_Event ev)
	{
		_queuedEvents.Add(new QueuedEvent<T_Event>(entity, ev));
	}

	public void Reset()
	{
		_queuedEvents.Clear();
		_subscribers.Clear();
		_subscriberCallbacks.Clear();
		_entityCallbackToIndex.Clear();
	}

	public void ProcessEvents()
	{
		// Early bail to avoid setting up job stuff unnecessarily
		if (_queuedEvents.Length == 0)
		{
			return;
		}
		
		NativeQueue<UnityEvent<T_Event>> eventsToProcessQueue = new NativeQueue<UnityEvent<T_Event>>(Allocator.TempJob);

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
	
	public void VerifyNoSubscribers()
	{
		int count = _subscribers.Length;
		
		for (int i = 0; i < count; i++)
		{
			Action<T_Event> callback = _subscriberCallbacks[i];
			Debug.LogError($"Subscriber {callback.Target.GetType().Name} left listening to {typeof(T_Event).Name} event system!");
		}
	}

	[BurstCompile]
	private struct BuildEventQueueJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeList<QueuedEvent<T_Event>> queuedEvents;

		[ReadOnly]
		public NativeList<EventEntity> subscribers;

		public NativeQueue<UnityEvent<T_Event>>.Concurrent eventsToProcess;

		public void Execute(int index)
		{
			int count = subscribers.Length;
			QueuedEvent<T_Event> ev = queuedEvents[index];

			for (int i = 0; i < count; i++)
			{
				if (subscribers[i].Equals(ev.entity))
				{
					eventsToProcess.Enqueue(new UnityEvent<T_Event>(ev.ev, i));
				}
			}
		}
	}
}