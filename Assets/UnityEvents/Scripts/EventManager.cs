using System;
using UnityEngine;
using UnityEvents.Internal;

namespace UnityEvents
{
	/// <summary>
	/// EventManager manages systems with the various Unity lifetime ticks. Allows systems that run in FixedUpdate,
	/// Update, and LateUpdate.
	/// </summary>
	public class EventManager : MonoBehaviour
	{
		private static UnityEventSystem _fixedUpdateSystem = new UnityEventSystem();
		private static UnityEventSystem _updateSystem = new UnityEventSystem();
		private static UnityEventSystem _lateUpdateSystem = new UnityEventSystem();

		/// <summary>
		/// Subscribe a listener to an event in the specific update tick.
		/// </summary>
		/// <param name="target">The target to subscribe to.</param>
		/// <param name="eventCallback">The event callback</param>
		/// <param name="tick">The update type to subscribe to.</param>
		/// <typeparam name="T_Event">The event</typeparam>
		public static void Subscribe<T_Event>(EventTarget target, Action<T_Event> eventCallback, EventUpdateTick tick)
			where T_Event : struct
		{
			if (tick == EventUpdateTick.FixedUpdate)
			{
				_fixedUpdateSystem.Subscribe(target, eventCallback);
			}
			else if (tick == EventUpdateTick.Update)
			{
				_updateSystem.Subscribe(target, eventCallback);
			}
			else // Late Update
			{
				_lateUpdateSystem.Subscribe(target, eventCallback);
			}
		}

		/// <summary>
		/// Subscribe a job that processes during an event in the specific update tick.
		/// </summary>
		/// <param name="target">The target to subscribe to.</param>
		/// <param name="job">The job, and starting data, to run when the event fires.</param>
		/// <param name="onComplete">The callback that is invoked when the job has finished.</param>
		/// <param name="tick">The update type to subscribe to.</param>
		/// <typeparam name="T_Job">The event type.</typeparam>
		/// <typeparam name="T_Event">The job type.</typeparam>
		public static void SubscribeWithJob<T_Job, T_Event>(
			EventTarget target, 
			T_Job job, 
			Action<T_Job> onComplete,
			EventUpdateTick tick)
			where T_Job : struct, IJobForEvent<T_Event>
			where T_Event : struct
		{
			if (tick == EventUpdateTick.FixedUpdate)
			{
				_fixedUpdateSystem.SubscribeWithJob<T_Job, T_Event>(target, job, onComplete);
			}
			else if (tick == EventUpdateTick.Update)
			{
				_updateSystem.SubscribeWithJob<T_Job, T_Event>(target, job, onComplete);
			}
			else // Late Update
			{
				_lateUpdateSystem.SubscribeWithJob<T_Job, T_Event>(target, job, onComplete);
			}
		}

		/// <summary>
		/// Unsubscribe a listener from an event in the specific update tick. 
		/// </summary>
		/// <param name="target">The target to unsubscribe from.</param>
		/// <param name="eventCallback">The event callback</param>
		/// <param name="tick">The update type to unsubscribe to.</param>
		/// <typeparam name="T_Event">The event</typeparam>
		public static void Unsubscribe<T_Event>(EventTarget target, Action<T_Event> eventCallback, EventUpdateTick tick)
			where T_Event : struct
		{
			if (tick == EventUpdateTick.FixedUpdate)
			{
				_fixedUpdateSystem.Unsubscribe(target, eventCallback);
			}
			else if (tick == EventUpdateTick.Update)
			{
				_updateSystem.Unsubscribe(target, eventCallback);
			}
			else // Late Update
			{
				_lateUpdateSystem.Unsubscribe(target, eventCallback);
			}
		}

		/// <summary>
		/// Unsubscribe a job that processed during from an event in the specific update tick.
		/// </summary>
		/// <param name="target">The target to unsubscribe from.</param>
		/// <param name="onComplete">The callback that is invoked when the job has finished.</param>
		/// <param name="tick">The update type to unsubscribe to.</param>
		/// <typeparam name="T_Job">The job type.</typeparam>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public static void UnsubscribeWithJob<T_Job, T_Event>(EventTarget target, Action<T_Job> onComplete,
			EventUpdateTick tick)
			where T_Job : struct, IJobForEvent<T_Event>
			where T_Event : struct
		{
			if (tick == EventUpdateTick.FixedUpdate)
			{
				_fixedUpdateSystem.UnsubscribeWithJob<T_Job, T_Event>(target, onComplete);
			}
			else if (tick == EventUpdateTick.Update)
			{
				_updateSystem.UnsubscribeWithJob<T_Job, T_Event>(target, onComplete);
			}
			else // Late Update
			{
				_lateUpdateSystem.UnsubscribeWithJob<T_Job, T_Event>(target, onComplete);
			}
		}

		/// <summary>
		/// Send an event to be processed in a specific update tick.
		/// </summary>
		/// <param name="target">The target to send the event to.</param>
		/// <param name="ev">The event to send</param>
		/// <param name="tick">The update tick to send to.</param>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public static void SendEvent<T_Event>(EventTarget target, T_Event ev, EventUpdateTick tick)
			where T_Event : struct
		{
			if (tick == EventUpdateTick.FixedUpdate)
			{
				_fixedUpdateSystem.QueueEvent(target, ev);
			}
			else if (tick == EventUpdateTick.Update)
			{
				_updateSystem.QueueEvent(target, ev);
			}
			else // Late Update
			{
				_lateUpdateSystem.QueueEvent(target, ev);
			}
		}

		/// <summary>
		/// Flushes all currently queued events NOW
		/// </summary>
		public static void FlushAll()
		{
			_fixedUpdateSystem.ProcessEvents();
			_updateSystem.ProcessEvents();
			_lateUpdateSystem.ProcessEvents();
		}

		/// <summary>
		/// Reset all the event systems with all update types.
		/// </summary>
		public static void ResetAll()
		{
			_fixedUpdateSystem.Reset();
			_updateSystem.Reset();
			_lateUpdateSystem.Reset();
		}

		/// <summary>
		/// Debug function to verify there are no lingering listeners. Throws an exception if there's a listener.
		/// </summary>
		public static void VerifyNoSubscribersAll()
		{
			_fixedUpdateSystem.VerifyNoSubscribers();
			_updateSystem.VerifyNoSubscribers();
			_lateUpdateSystem.VerifyNoSubscribers();
		}

		/// <summary>
		/// Debug function to verify there are no lingering listeners. Logs each offending system instead of throwing an
		/// exception.
		/// </summary>
		public static void VerifyNoSubscribersAllLog()
		{
			_fixedUpdateSystem.VerifyNoSubscribersLog();
			_updateSystem.VerifyNoSubscribersLog();
			_lateUpdateSystem.VerifyNoSubscribersLog();
		}

		private void FixedUpdate()
		{
			_fixedUpdateSystem.ProcessEvents();
		}

		private void Update()
		{
			_updateSystem.ProcessEvents();
		}

		private void LateUpdate()
		{
			_lateUpdateSystem.ProcessEvents();
		}

		private void OnDestroy()
		{
			ResetAll();

			_fixedUpdateSystem.Dispose();
			_updateSystem.Dispose();
			_lateUpdateSystem.Dispose();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			GameObject em = new GameObject("[EVENT MANAGER DO]", typeof(EventManager));
			DontDestroyOnLoad(em);
		}
	}
	
	public enum EventUpdateTick
	{
		FixedUpdate,
		Update,
		LateUpdate
	}
}