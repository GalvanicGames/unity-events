using System;
using UnityEvents.Internal;

namespace UnityEvents
{
	/// <summary>
	/// A system that handles global events for Fixed Update. Intended for gameplay events.
	/// </summary>
	public static class GlobalEventSystem
	{
		private static TickEventSystem _simSystem = new TickEventSystem(EventUpdateTick.FixedUpdate);
		private static TickEventSystem _uiSystem = new TickEventSystem(EventUpdateTick.LateUpdate);

		/// <summary>
		/// Subscribe a listener to the global event system.
		/// </summary>
		/// <param name="callback">The callback that's invoked when an event occurs.</param>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public static void Subscribe<T_Event>(Action<T_Event> callback) where T_Event : struct
		{
			_simSystem.Subscribe(callback);
		}
	
		/// <summary>
		/// Subscribe a job to the global event system.
		/// </summary>
		/// <param name="job">The job that is processed when an event occurs.</param>
		/// <param name="onComplete">The callback that's invoked when the job is done.</param>
		/// <typeparam name="T_Job">The job type.</typeparam>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public static void SubscribeWithJob<T_Job, T_Event>(T_Job job, Action<T_Job> onComplete) 
			where T_Job : struct, IJobForEvent<T_Event> 
			where T_Event : struct
		{
			_simSystem.SubscribeWithJob<T_Job, T_Event>(job, onComplete);
		}

		/// <summary>
		/// Unsubscribe a listener from the global event system.
		/// </summary>
		/// <param name="callback">The callback to unsubscribe.</param>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public static void Unsubscribe<T_Event>(Action<T_Event> callback) where T_Event : struct
		{
			_simSystem.Unsubscribe(callback);
		}
	
		/// <summary>
		/// Unsubscribe a job from the global event system.
		/// </summary>
		/// <param name="onComplete">The on complete callback to unsubscribe</param>
		/// <typeparam name="T_Job">The job type to unsubscribe.</typeparam>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public static void UnsubscribeWithJob<T_Job, T_Event>(Action<T_Job> onComplete) 
			where T_Job : struct, IJobForEvent<T_Event> 
			where T_Event : struct
		{
			_simSystem.UnsubscribeWithJob<T_Job, T_Event>(onComplete);
		}

		/// <summary>
		/// Send an event to the global event system.
		/// </summary>
		/// <param name="ev">The event to send.</param>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public static void SendEvent<T_Event>(T_Event ev) where T_Event : struct
		{
			_simSystem.SendEvent(ev);
		}
		
		/// <summary>
		/// Subscribe a listener to the global UI event system.
		/// </summary>
		/// <param name="callback">The callback that's invoked when an event occurs.</param>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public static void SubscribeUI<T_Event>(Action<T_Event> callback) where T_Event : struct
		{
			_uiSystem.Subscribe(callback);
		}
	
		/// <summary>
		/// Subscribe a job to the global UI event system.
		/// </summary>
		/// <param name="job">The job that is processed when an event occurs.</param>
		/// <param name="onComplete">The callback that's invoked when the job is done.</param>
		/// <typeparam name="T_Job">The job type.</typeparam>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public static void SubscribeUIWithJob<T_Job, T_Event>(T_Job job, Action<T_Job> onComplete) 
			where T_Job : struct, IJobForEvent<T_Event> 
			where T_Event : struct
		{
			_uiSystem.SubscribeWithJob<T_Job, T_Event>(job, onComplete);
		}

		/// <summary>
		/// Unsubscribe a listener from the global UI event system.
		/// </summary>
		/// <param name="callback">The callback to unsubscribe.</param>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public static void UnsubscribeUI<T_Event>(Action<T_Event> callback) where T_Event : struct
		{
			_uiSystem.Unsubscribe(callback);
		}
	
		/// <summary>
		/// Unsubscribe a job from the global UI event system.
		/// </summary>
		/// <param name="onComplete">The on complete callback to unsubscribe</param>
		/// <typeparam name="T_Job">The job type to unsubscribe.</typeparam>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public static void UnsubscribeUIWithJob<T_Job, T_Event>(Action<T_Job> onComplete) 
			where T_Job : struct, IJobForEvent<T_Event> 
			where T_Event : struct
		{
			_uiSystem.UnsubscribeWithJob<T_Job, T_Event>(onComplete);
		}

		/// <summary>
		/// Send an event to the global UI event system.
		/// </summary>
		/// <param name="ev">The event to send.</param>
		/// <typeparam name="T_Event">The event type.</typeparam>
		public static void SendEventUI<T_Event>(T_Event ev) where T_Event : struct
		{
			_uiSystem.SendEvent(ev);
		}
	}
}
