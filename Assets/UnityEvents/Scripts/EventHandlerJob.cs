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
	/// Event system that processes jobs for events.
	/// </summary>
	/// <typeparam name="T_Job">The job the system is responsible for.</typeparam>
	/// <typeparam name="T_Event">The event the system is responsible for.</typeparam>
	public class EventHandlerJob<T_Job, T_Event> :
		IJobEventSystem<T_Event>,
		IDisposable
		where T_Job : struct, IJobForEvent<T_Event>
		where T_Event : struct
	{
		private NativeList<QueuedEvent<T_Event>> _queuedEvents;
		private NativeList<Subscription> _subscribers;

		private List<Action<T_Job>> _subscriberCallbacks;
		private Dictionary<EntityCallbackId<T_Job>, int> _entityCallbackToIndex;

		private Dictionary<QueuedEvent<T_Event>, int> _cachedCurEvents;

		private bool _disposed;
		private readonly int _batchCount;

		private const int DEFAULT_EVENTS_TO_PROCESS_CAPACITY = 10;
		private const int DEFAULT_SUBSCRIBER_CAPACITY = 100;
		private const int DEFAULT_PARALLEL_BATCH_COUNT = 32;

		private struct UnityEventJob
		{
			public readonly T_Event ev;
			public readonly int subscriberIndex;

			public UnityEventJob(T_Event ev, int subscriberIndex)
			{
				this.ev = ev;
				this.subscriberIndex = subscriberIndex;
			}
		}

		private struct Subscription
		{
			public EventTarget target;
			public T_Job job;

			public Subscription(EventTarget target, T_Job job)
			{
				this.target = target;
				this.job = job;
			}
		}

		public EventHandlerJob() : this(
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
		public EventHandlerJob(
			int subscriberStartingCapacity,
			int queuedEventsStartingCapacity,
			int parallelBatchCount)
		{
#if !DISABLE_EVENT_SAFETY_CHKS
			if (!UnsafeUtility.IsBlittable<T_Job>())
			{
				throw new JobTypeNotBlittableException(typeof(T_Job));
			}

			if (!UnsafeUtility.IsBlittable<T_Event>())
			{
				throw new EventTypeNotBlittableException(typeof(T_Event));
			}
#endif
			_batchCount = parallelBatchCount;

			_subscribers = new NativeList<Subscription>(subscriberStartingCapacity, Allocator.Persistent);
			_subscriberCallbacks = new List<Action<T_Job>>(subscriberStartingCapacity);
			_entityCallbackToIndex = new Dictionary<EntityCallbackId<T_Job>, int>(subscriberStartingCapacity);

			_queuedEvents = new NativeList<QueuedEvent<T_Event>>(queuedEventsStartingCapacity, Allocator.Persistent);

#if !DISABLE_EVENT_SAFETY_CHKS
			_cachedCurEvents = new Dictionary<QueuedEvent<T_Event>, int>(subscriberStartingCapacity);
#endif
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
		/// Subscribe a job to the system.
		/// </summary>
		/// <param name="target">The entity to subscribe to.</param>
		/// <param name="job">The job, and initial job state, to process when an event fires.</param>
		/// <param name="onComplete">The callback to invoke when a job finishes.</param>
		public void Subscribe(EventTarget target, T_Job job, Action<T_Job> onComplete)
		{
#if !DISABLE_EVENT_SAFETY_CHKS
			if (_entityCallbackToIndex.ContainsKey(new EntityCallbackId<T_Job>(target, onComplete)))
			{
				throw new MultipleSubscriptionsException<T_Job>(onComplete);
			}
#endif

			_entityCallbackToIndex.Add(new EntityCallbackId<T_Job>(target, onComplete), _subscribers.Length);
			_subscribers.Add(new Subscription(target, job));
			_subscriberCallbacks.Add(onComplete);
		}

		/// <summary>
		/// Unsubscribe a job from the system.
		/// </summary>
		/// <param name="target">The entity to unsubscribe from.</param>
		/// <param name="onComplete">The callback that was invoked when a job finished.</param>
		public void Unsubscribe(EventTarget target, Action<T_Job> onComplete)
		{
			EntityCallbackId<T_Job> callbackId = new EntityCallbackId<T_Job>(target, onComplete);

			if (_entityCallbackToIndex.TryGetValue(callbackId, out int index))
			{
				_entityCallbackToIndex.Remove(callbackId);
				
				_subscribers.RemoveAtSwapBack(index);

				_subscriberCallbacks[index] = _subscriberCallbacks[_subscriberCallbacks.Count - 1];
				_subscriberCallbacks.RemoveAt(_subscriberCallbacks.Count - 1);

				if (index != _subscribers.Length)
				{
					EntityCallbackId<T_Job> otherCallbackId = new EntityCallbackId<T_Job>(
						_subscribers[index].target,
						_subscriberCallbacks[index]);

					_entityCallbackToIndex[otherCallbackId] = index;
				}
			}
		}

		/// <summary>
		/// Queue an event to be processed later.
		/// </summary>
		/// <param name="target">The entity the event is for.</param>
		/// <param name="ev">The event to queue.</param>
		public void QueueEvent(EventTarget target, T_Event ev)
		{
			QueuedEvent<T_Event> newEv = new QueuedEvent<T_Event>(target, ev);

#if !DISABLE_EVENT_SAFETY_CHKS
			if (_cachedCurEvents.TryGetValue(newEv, out int index))
			{
				// If avoiding this warning is too annoying then make this the default behaviour and have this be a message sent
				// when in verbose mode or something
				Debug.LogWarning(
					$"To prevent parallel corruption, event {ev.GetType().Name} cannot be sent to the same entity multiple times between processing. This event will replace the previous one!");

				_queuedEvents[index] = newEv;
			}
			else
			{
				_cachedCurEvents[newEv] = _queuedEvents.Length;
				_queuedEvents.Add(newEv);
			}
#else
			_queuedEvents.Add(newEv);
#endif
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

			NativeQueue<UnityEventJob> eventsToProcessQueue = new NativeQueue<UnityEventJob>(Allocator.TempJob);

			BuildEventQueueJob job = new BuildEventQueueJob();
			job.queuedEvents = _queuedEvents;
			job.subscribers = _subscribers;
			job.eventsToProcess = eventsToProcessQueue.ToConcurrent();

			job.Schedule(_queuedEvents.Length, 32).Complete();

			int eventCount = eventsToProcessQueue.Count;

			NativeArray<UnityEventJob> eventsToProcess = new NativeArray<UnityEventJob>(eventCount, Allocator.TempJob);

			EventQueueToEventArrayJob setJob = new EventQueueToEventArrayJob();
			setJob.eventsInQueue = eventsToProcessQueue;
			setJob.events = eventsToProcess;

			JobHandle setArrayHandle = setJob.Schedule();

			NativeArray<T_Job> jobsArray = new NativeArray<T_Job>(eventCount, Allocator.TempJob);

			CreateJobsArrayJob createJobArrayJob = new CreateJobsArrayJob();
			createJobArrayJob.events = eventsToProcess;
			createJobArrayJob.subscribers = _subscribers;
			createJobArrayJob.jobs = jobsArray;

			JobHandle createJobArrayHandle = createJobArrayJob.Schedule(
				eventCount,
				_batchCount,
				setArrayHandle);

			ExecuteEventJobsJob executeJob = new ExecuteEventJobsJob();
			executeJob.jobsResult = jobsArray;
			executeJob.evs = eventsToProcess;

			JobHandle executeHandle = executeJob.Schedule(
				eventCount,
				_batchCount,
				createJobArrayHandle);

			WriteBackToSubscribersJob writeBackJob = new WriteBackToSubscribersJob();
			writeBackJob.subscribers = _subscribers;
			writeBackJob.evs = eventsToProcess;
			writeBackJob.jobsResult = jobsArray;

			JobHandle writeBackHandle = writeBackJob.Schedule(
				eventCount,
				_batchCount,
				executeHandle);

			writeBackHandle.Complete();

			int count = eventsToProcess.Length;

			for (int i = 0; i < count; i++)
			{
				UnityEventJob ev = eventsToProcess[i];
				Subscription sub = _subscribers[ev.subscriberIndex];

				try
				{
					_subscriberCallbacks[ev.subscriberIndex](sub.job);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}

			eventsToProcessQueue.Dispose();
			eventsToProcess.Dispose();
			jobsArray.Dispose();

			_queuedEvents.Clear();

#if !DISABLE_EVENT_SAFETY_CHKS
			_cachedCurEvents.Clear();
#endif
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

#if !DISABLE_EVENT_SAFETY_CHKS
			_cachedCurEvents.Clear();
#endif
		}

		/// <summary>
		/// Debug log an error for each subscriber that is still listening.
		/// </summary>
		public void VerifyNoSubscribers()
		{
			if (_subscribers.Length > 0)
			{
				// Just throw the first one, it'll get resolved
				throw new SubscriberStillListeningException<T_Job, T_Event>(_subscriberCallbacks);
			}
		}

		[BurstCompile]
		private struct BuildEventQueueJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeList<QueuedEvent<T_Event>> queuedEvents;

			[ReadOnly]
			public NativeList<Subscription> subscribers;

			public NativeQueue<UnityEventJob>.Concurrent eventsToProcess;

			public void Execute(int index)
			{
				int count = subscribers.Length;
				QueuedEvent<T_Event> ev = queuedEvents[index];

				for (int i = 0; i < count; i++)
				{
					if (subscribers[i].target.Equals(ev.target))
					{
						eventsToProcess.Enqueue(new UnityEventJob(ev.ev, i));
					}
				}
			}
		}

		[BurstCompile]
		private struct EventQueueToEventArrayJob : IJob
		{
			public NativeQueue<UnityEventJob> eventsInQueue;

			[WriteOnly]
			public NativeArray<UnityEventJob> events;

			public void Execute()
			{
				int count = eventsInQueue.Count;

				for (int i = 0; i < count; i++)
				{
					events[i] = eventsInQueue.Dequeue();
				}
			}
		}

		[BurstCompile]
		private struct CreateJobsArrayJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<UnityEventJob> events;

			[ReadOnly]
			public NativeList<Subscription> subscribers;

			[WriteOnly]
			public NativeArray<T_Job> jobs;

			public void Execute(int index)
			{
				jobs[index] = subscribers[events[index].subscriberIndex].job;
			}
		}

		[BurstCompile]
		private struct ExecuteEventJobsJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<UnityEventJob> evs;

			public NativeArray<T_Job> jobsResult;

			public void Execute(int index)
			{
				T_Job job = jobsResult[index];
				job.ExecuteEvent(evs[index].ev);
				jobsResult[index] = job;
			}
		}

		[BurstCompile]
		private struct WriteBackToSubscribersJob : IJobParallelFor
		{
			[WriteOnly]
			[NativeDisableContainerSafetyRestriction]
			public NativeList<Subscription> subscribers;

			[ReadOnly]
			public NativeArray<UnityEventJob> evs;

			[ReadOnly]
			public NativeArray<T_Job> jobsResult;

			public void Execute(int index)
			{
				Subscription sub = subscribers[evs[index].subscriberIndex];
				sub.job = jobsResult[index];
				subscribers[evs[index].subscriberIndex] = sub;
			}
		}
	}
}