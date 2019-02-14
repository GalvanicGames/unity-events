using System;
using UnityEngine;
using UnityEventsInternal;

namespace UnityEvents
{
	/// <summary>
	/// EventManager manages systems with the various Unity lifetime ticks. Allows systems that run in FixedUpdate,
	/// Update, and LateUpdate.
	/// </summary>
	public class EventManager : MonoBehaviour
	{
		private static readonly UnityEventSystems _fixedUpdateSystems = new UnityEventSystems();
		private static readonly UnityEventSystems _updateSystems = new UnityEventSystems();
		private static readonly UnityEventSystems _lateUpdateSystems = new UnityEventSystems();

		/// <summary>
		/// Subscribe a listener to an event in the specific update tick.
		/// </summary>
		/// <param name="entity">The entity to subscribe to.</param>
		/// <param name="eventCallback">The event callback</param>
		/// <param name="type">The update type to subscribe to.</param>
		/// <typeparam name="T_Event">The event</typeparam>
		public static void Subscribe<T_Event>(EventEntity entity, Action<T_Event> eventCallback, EventUpdateType type)
			where T_Event : unmanaged
		{
			if (type == EventUpdateType.FixedUpdate)
			{
				_fixedUpdateSystems.Subscribe(entity, eventCallback);
			}
			else if (type == EventUpdateType.Update)
			{
				_updateSystems.Subscribe(entity, eventCallback);
			}
			else // Late Update
			{
				_lateUpdateSystems.Subscribe(entity, eventCallback);
			}
		}

		/// <summary>
		/// Subscribe a job that processes during an event in the specific update tick.
		/// </summary>
		/// <param name="entity">The entity to subscribe to.</param>
		/// <param name="job">The job, and starting data, to run when the event fires.</param>
		/// <param name="onComplete">The callback that is invoked when the job has finished.</param>
		/// <param name="type">The update type to subscribe to.</param>
		/// <typeparam name="T_Job">The event type.</typeparam>
		/// <typeparam name="T_Event">The job type.</typeparam>
		public static void SubscribeWithJob<T_Job, T_Event>(EventEntity entity, T_Job job, Action<T_Job> onComplete,
			EventUpdateType type)
			where T_Job : struct, IJobForEvent<T_Event>
			where T_Event : unmanaged
		{
			if (type == EventUpdateType.FixedUpdate)
			{
				_fixedUpdateSystems.SubscribeWithJob<T_Job, T_Event>(entity, job, onComplete);
			}
			else if (type == EventUpdateType.Update)
			{
				_updateSystems.SubscribeWithJob<T_Job, T_Event>(entity, job, onComplete);
			}
			else // Late Update
			{
				_lateUpdateSystems.SubscribeWithJob<T_Job, T_Event>(entity, job, onComplete);
			}
		}

		/// <summary>
		/// Unsubscribe a listener from an event in the specific update tick. 
		/// </summary>
		/// <param name="entity">The entity to unsubscribe from.</param>
		/// <param name="eventCallback">The event callback</param>
		/// <param name="type">The update type to unsubscribe to.</param>
		/// <typeparam name="T_Event">The event</typeparam>
		public static void Unsubscribe<T_Event>(EventEntity entity, Action<T_Event> eventCallback, EventUpdateType type)
			where T_Event : unmanaged
		{
			if (type == EventUpdateType.FixedUpdate)
			{
				_fixedUpdateSystems.Unsubscribe(entity, eventCallback);
			}
			else if (type == EventUpdateType.Update)
			{
				_updateSystems.Unsubscribe(entity, eventCallback);
			}
			else // Late Update
			{
				_lateUpdateSystems.Unsubscribe(entity, eventCallback);
			}
		}

		/// <summary>
		/// Unsubscribe a job that processed during from an event in the specific update tick.
		/// </summary>
		/// <param name="entity">The entity to unsubscribe from.</param>
		/// <param name="onComplete">The callback that is invoked when the job has finished.</param>
		/// <param name="type">The update type to unsubscribe to.</param>
		/// <typeparam name="T_Job">The job type.</typeparam>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public static void UnsubscribeWithJob<T_Job, T_Event>(EventEntity entity, Action<T_Job> onComplete,
			EventUpdateType type)
			where T_Job : struct, IJobForEvent<T_Event>
			where T_Event : unmanaged
		{
			if (type == EventUpdateType.FixedUpdate)
			{
				_fixedUpdateSystems.UnsubscribeWithJob<T_Job, T_Event>(entity, onComplete);
			}
			else if (type == EventUpdateType.Update)
			{
				_updateSystems.UnsubscribeWithJob<T_Job, T_Event>(entity, onComplete);
			}
			else // Late Update
			{
				_lateUpdateSystems.UnsubscribeWithJob<T_Job, T_Event>(entity, onComplete);
			}
		}

		/// <summary>
		/// Send an event to be processed in a specific update tick.
		/// </summary>
		/// <param name="entity">The entity to send the event to.</param>
		/// <param name="ev">The event to send</param>
		/// <param name="type">The update tick to send to.</param>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public static void SendEvent<T_Event>(EventEntity entity, T_Event ev, EventUpdateType type)
			where T_Event : unmanaged
		{
			if (type == EventUpdateType.FixedUpdate)
			{
				_fixedUpdateSystems.QueueEvent(entity, ev);
			}
			else if (type == EventUpdateType.Update)
			{
				_updateSystems.QueueEvent(entity, ev);
			}
			else // Late Update
			{
				_lateUpdateSystems.QueueEvent(entity, ev);
			}
		}

		/// <summary>
		/// Flushes all currently queued events NOW
		/// </summary>
		public static void FlushAll()
		{
			_fixedUpdateSystems.ProcessEvents();
			_updateSystems.ProcessEvents();
			_lateUpdateSystems.ProcessEvents();
		}

		/// <summary>
		/// Reset all the event systems with all update types.
		/// </summary>
		public static void ResetAll()
		{
			_fixedUpdateSystems.Reset();
			_updateSystems.Reset();
			_lateUpdateSystems.Reset();
		}

		/// <summary>
		/// Debug function to verify there are no lingering listeners. Throws an exception if there's a listener.
		/// </summary>
		public static void VerifyNoSubscribersAll()
		{
			_fixedUpdateSystems.VerifyNoSubscribers();
			_updateSystems.VerifyNoSubscribers();
			_lateUpdateSystems.VerifyNoSubscribers();
		}

		/// <summary>
		/// Debug function to verify there are no lingering listeners. Logs each offending system instead of throwing an
		/// exception.
		/// </summary>
		public static void VerifyNoSubscribersAllLog()
		{
			_fixedUpdateSystems.VerifyNoSubscribersLog();
			_updateSystems.VerifyNoSubscribersLog();
			_lateUpdateSystems.VerifyNoSubscribersLog();
		}

		private void FixedUpdate()
		{
			_fixedUpdateSystems.ProcessEvents();
		}

		private void Update()
		{
			_updateSystems.ProcessEvents();
		}

		private void LateUpdate()
		{
			_lateUpdateSystems.ProcessEvents();
		}

		private void OnDestroy()
		{
			ResetAll();

			_fixedUpdateSystems.Dispose();
			_updateSystems.Dispose();
			_lateUpdateSystems.Dispose();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			GameObject em = new GameObject("[EVENT MANAGER DO]", typeof(EventManager));
			DontDestroyOnLoad(em);
		}
	}
	
	public enum EventUpdateType
	{
		FixedUpdate,
		Update,
		LateUpdate
	}
}