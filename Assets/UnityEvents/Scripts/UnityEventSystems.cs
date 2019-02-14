using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEventsInternal;

namespace UnityEvents
{
	/// <summary>
	/// Holds a collection of event system that can handle all events.
	/// </summary>
	public class UnityEventSystems : IDisposable
	{
		private List<IEventSystem> _systems = new List<IEventSystem>();
		private Dictionary<Type, IEventSystem> _systemsCache = new Dictionary<Type, IEventSystem>();
		private Dictionary<Type, IEventSystem> _jobSystemsCache = new Dictionary<Type, IEventSystem>();

		private Dictionary<Type, List<IEventSystem>> _eventToJobSystems =
			new Dictionary<Type, List<IEventSystem>>();

		/// <summary>
		/// Subscribe a listener to an event.
		/// </summary>
		/// <param name="entity">The entity to subscribe to.</param>
		/// <param name="eventCallback">The event callback</param>
		/// <typeparam name="T_Event">The event</typeparam>
		public void Subscribe<T_Event>(EventEntity entity, Action<T_Event> eventCallback) where T_Event : unmanaged
		{
			UnityEventStandardSystem<T_Event> system = GetSystem<T_Event>();
			system.Subscribe(entity, eventCallback);
		}

		/// <summary>
		/// Subscribe a job that processes during an event.
		/// </summary>
		/// <param name="entity">The entity to subscribe to.</param>
		/// <param name="job">The job, and starting data, to run when the event fires.</param>
		/// <param name="onComplete">The callback that is invoked when the job has finished.</param>
		/// <typeparam name="T_Job">The event type.</typeparam>
		/// <typeparam name="T_Event">The job type.</typeparam>
		public void SubscribeWithJob<T_Job, T_Event>(EventEntity entity, T_Job job, Action<T_Job> onComplete)
			where T_Job : struct, IJobForEvent<T_Event>
			where T_Event : unmanaged
		{
			UnityEventJobSystem<T_Job, T_Event> system = GetJobSystem<T_Job, T_Event>();
			system.Subscribe(entity, job, onComplete);
		}

		/// <summary>
		/// Unsubscribe a listener from an event. 
		/// </summary>
		/// <param name="entity">The entity to unsubscribe from.</param>
		/// <param name="eventCallback">The event callback</param>
		/// <typeparam name="T_Event">The event</typeparam>
		public void Unsubscribe<T_Event>(EventEntity entity, Action<T_Event> eventCallback) where T_Event : unmanaged
		{
			UnityEventStandardSystem<T_Event> system = GetSystem<T_Event>();
			system.Unsubscribe(entity, eventCallback);
		}

		/// <summary>
		/// Unsubscribe a job that processed during from an event.
		/// </summary>
		/// <param name="entity">The entity to unsubscribe from.</param>
		/// <param name="onComplete">The callback that is invoked when the job has finished.</param>
		/// <typeparam name="T_Job">The job type.</typeparam>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public void UnsubscribeWithJob<T_Job, T_Event>(EventEntity entity, Action<T_Job> onComplete)
			where T_Job : struct, IJobForEvent<T_Event>
			where T_Event : unmanaged
		{
			UnityEventJobSystem<T_Job, T_Event> system = GetJobSystem<T_Job, T_Event>();
			system.Unsubscribe(entity, onComplete);
		}

		/// <summary>
		/// Queue an event.
		/// </summary>
		/// <param name="entity">The entity to queue an event with.</param>
		/// <param name="ev">The event to queue.</param>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public void QueueEvent<T_Event>(EventEntity entity, T_Event ev) where T_Event : unmanaged
		{
			UnityEventStandardSystem<T_Event> system = GetSystem<T_Event>();
			system.QueueEvent(entity, ev);

			List<IEventSystem> list = GetJobSystemsForEvent<T_Event>();

			int count = list.Count;

			for (int i = 0; i < count; i++)
			{
				IJobEventSystem<T_Event> typedSystem = (IJobEventSystem<T_Event>) list[i];
				typedSystem.QueueEvent(entity, ev);
			}
		}
		
		/// <summary>
		/// Process all queued events.
		/// </summary>
		public void ProcessEvents()
		{
			int count = _systems.Count;

			for (int i = 0; i < count; i++)
			{
				_systems[i].ProcessEvents();
			}
		}

		/// <summary>
		/// Reset all systems in the collection.
		/// </summary>
		public void Reset()
		{
			int count = _systems.Count;

			for (int i = 0; i < count; i++)
			{
				_systems[i].Reset();
			}
		}

		/// <summary>
		/// Verify there are no subscribers in the systems.
		/// </summary>
		public void VerifyNoSubscribers()
		{
			int count = _systems.Count;

			for (int i = 0; i < count; i++)
			{
				_systems[i].VerifyNoSubscribers();
			}
		}

		public void VerifyNoSubscribersLog()
		{
			int count = _systems.Count;

			for (int i = 0; i < count; i++)
			{
				try
				{
					_systems[i].VerifyNoSubscribers();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					throw;
				}
			}
		}

		/// <summary>
		/// Disposes any resources held by the systems.
		/// </summary>
		public void Dispose()
		{
			int count = _systems.Count;

			for (int i = 0; i < count; i++)
			{
				if (_systems[i] is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
		}

		private UnityEventStandardSystem<T_Event> GetSystem<T_Event>() where T_Event : unmanaged
		{
			IEventSystem system;

			if (!_systemsCache.TryGetValue(typeof(T_Event), out system))
			{
				system = new UnityEventStandardSystem<T_Event>();
				_systems.Add(system);
				_systemsCache[typeof(T_Event)] = system;
			}

			return (UnityEventStandardSystem<T_Event>) system;
		}

		private UnityEventJobSystem<T_Job, T_Event> GetJobSystem<T_Job, T_Event>()
			where T_Job : struct, IJobForEvent<T_Event>
			where T_Event : unmanaged
		{
			IEventSystem system;

			if (!_jobSystemsCache.TryGetValue(typeof(T_Job), out system))
			{
				system = new UnityEventJobSystem<T_Job, T_Event>();
				_systems.Add(system);
				_jobSystemsCache[typeof(T_Job)] = system;
			}

			return (UnityEventJobSystem<T_Job, T_Event>) system;
		}

		private List<IEventSystem> GetJobSystemsForEvent<T_Event>() where T_Event : unmanaged
		{
			List<IEventSystem> list;

			if (!_eventToJobSystems.TryGetValue(typeof(T_Event), out list))
			{
				list = new List<IEventSystem>();
				_eventToJobSystems[typeof(T_Event)] = list;
			}

			return list;
		}
	}
}