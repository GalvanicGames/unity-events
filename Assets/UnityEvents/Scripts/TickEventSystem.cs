using System;
using UnityEventsInternal;

namespace UnityEvents
{
	/// <summary>
	/// Event System that runs in a given update tick (FixedUpdate, Update, or LateUpdate). Can be used to create
	/// custom event systems.
	/// </summary>
	public class TickEventSystem
	{
		private EventUpdateTick _tick;
		private EventTarget _target;
		
		/// <summary>
		/// Create an tick based event system.
		/// </summary>
		/// <param name="updateTick">Which tick to run in.</param>
		public TickEventSystem(EventUpdateTick updateTick)
		{
			_tick = updateTick;
			_target = EventTarget.CreateTarget();
		}

		/// <summary>
		/// Subscribe a listener to the tick based event system.
		/// </summary>
		/// <param name="callback">The callback that's invoked when an event occurs.</param>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public void Subscribe<T_Event>(Action<T_Event> callback) where T_Event : struct
		{
			EventManager.Subscribe(_target, callback, _tick);
		}

		/// <summary>
		/// Subscribe a job to the tick based event system.
		/// </summary>
		/// <param name="job">The job that is processed when an event occurs.</param>
		/// <param name="onComplete">The callback that's invoked when the job is done.</param>
		/// <typeparam name="T_Job">The job type.</typeparam>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public void SubscribeWithJob<T_Job, T_Event>(T_Job job, Action<T_Job> onComplete)
			where T_Job : struct, IJobForEvent<T_Event>
			where T_Event : struct
		{
			EventManager.SubscribeWithJob<T_Job, T_Event>(_target, job, onComplete, _tick);
		}

		/// <summary>
		/// Unsubscribe a listener from the tick based event system.
		/// </summary>
		/// <param name="callback">The callback to unsubscribe.</param>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public void Unsubscribe<T_Event>(Action<T_Event> callback) where T_Event : struct
		{
			EventManager.Unsubscribe(_target, callback, _tick);
		}
		
		/// <summary>
		/// Unsubscribe a job from the tick based event system.
		/// </summary>
		/// <param name="onComplete">The on complete callback to unsubscribe</param>
		/// <typeparam name="T_Job">The job type to unsubscribe.</typeparam>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public void UnsubscribeWithJob<T_Job, T_Event>(Action<T_Job> onComplete)
			where T_Job : struct, IJobForEvent<T_Event>
			where T_Event : struct
		{
			EventManager.UnsubscribeWithJob<T_Job, T_Event>(_target, onComplete, _tick);
		}

		/// <summary>
		/// Send an event to the tick based event system.
		/// </summary>
		/// <param name="ev">The event to send.</param>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public void SendEvent<T_Event>(T_Event ev) where T_Event : struct
		{
			EventManager.SendEvent(_target, ev, _tick);
		}
	}
}
