using System;
using UnityEngine;
using UnityEvents;

namespace UnityEventsInternal
{
	public static class GameObjectEventSystem
	{
		public static void Subscribe<T_Event>(this GameObject gObj, Action<T_Event> callback) where T_Event : unmanaged
		{
			EventManager.Subscribe(EventEntity.CreateEntity(gObj), callback, EventUpdateType.FixedUpdate);
		}

		public static void SubscribeWithJob<T_Job, T_Event>(this GameObject gObj, T_Job job, Action<T_Job> onComplete)
			where T_Job : struct, IJobForEvent<T_Event>
			where T_Event : unmanaged
		{
			EventManager.SubscribeWithJob<T_Job, T_Event>(
				EventEntity.CreateEntity(gObj),
				job,
				onComplete,
				EventUpdateType.FixedUpdate);
		}

		public static void Unsubscribe<T_Event>(this GameObject gObj, Action<T_Event> callback)
			where T_Event : unmanaged
		{
			EventManager.Unsubscribe(EventEntity.CreateEntity(gObj), callback, EventUpdateType.FixedUpdate);
		}

		public static void UnsubscribeWithJob<T_Job, T_Event>(this GameObject gObj, Action<T_Job> onComplete)
			where T_Job : struct, IJobForEvent<T_Event>
			where T_Event : unmanaged
		{
			EventManager.UnsubscribeWithJob<T_Job, T_Event>(
				EventEntity.CreateEntity(gObj),
				onComplete,
				EventUpdateType.FixedUpdate);
		}

		public static void SendEvent<T_Event>(this GameObject gObj, T_Event ev) where T_Event : unmanaged
		{
			EventManager.SendEvent(EventEntity.CreateEntity(gObj), ev, EventUpdateType.FixedUpdate);
		}

		public static void SubscribeUI<T_Event>(this GameObject gObj, Action<T_Event> callback)
			where T_Event : unmanaged
		{
			EventManager.Subscribe(EventEntity.CreateEntity(gObj), callback, EventUpdateType.LateUpdate);
		}

		public static void SubscribeUIWithJob<T_Job, T_Event>(this GameObject gObj, T_Job job, Action<T_Job> onComplete)
			where T_Job : struct, IJobForEvent<T_Event>
			where T_Event : unmanaged
		{
			EventManager.SubscribeWithJob<T_Job, T_Event>(
				EventEntity.CreateEntity(gObj),
				job,
				onComplete,
				EventUpdateType.LateUpdate);
		}

		public static void UnsubscribeUI<T_Event>(this GameObject gObj, Action<T_Event> callback)
			where T_Event : unmanaged
		{
			EventManager.Unsubscribe(
				EventEntity.CreateEntity(gObj),
				callback,
				EventUpdateType.LateUpdate);
		}

		public static void UnsubscribeUIWithJob<T_Job, T_Event>(this GameObject gObj, Action<T_Job> onComplete)
			where T_Job : struct, IJobForEvent<T_Event>
			where T_Event : unmanaged
		{
			EventManager.UnsubscribeWithJob<T_Job, T_Event>(
				EventEntity.CreateEntity(gObj),
				onComplete,
				EventUpdateType.LateUpdate);
		}

		public static void SendEventUI<T_Event>(this GameObject gObj, T_Event ev) where T_Event : unmanaged
		{
			EventManager.SendEvent(EventEntity.CreateEntity(gObj), ev, EventUpdateType.LateUpdate);
		}
	}
}