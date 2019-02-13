using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManagerDO : MonoBehaviour
{
	private static readonly UnityEventSystems _fixedUpdateSystems = new UnityEventSystems();
	private static readonly UnityEventSystems _updateSystems = new UnityEventSystems();
	private static readonly UnityEventSystems _lateUpdateSystems = new UnityEventSystems();
	
	public static void Subscribe<T_Event>(EventEntity entity, Action<T_Event> eventCallback, EventUpdateType type) where T_Event : unmanaged
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

	public static void SubscribeWithJob<T_Job, T_Event>(EventEntity entity, T_Job job, Action<T_Job> onComplete, EventUpdateType type)
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

	public static void ResetAll()
	{
		_fixedUpdateSystems.Reset();
		_updateSystems.Reset();
		_lateUpdateSystems.Reset();
	}
	
	public static void Unsubscribe<T_Event>(EventEntity entity, Action<T_Event> eventCallback, EventUpdateType type) where T_Event : unmanaged
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

	public static void UnsubscribeWithJob<T_Job, T_Event>(EventEntity entity, Action<T_Job> onComplete, EventUpdateType type)
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

	public static void SendEvent<T_Event>(EventEntity entity, T_Event ev, EventUpdateType type) where T_Event : unmanaged
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

	public static void VerifyNoSubscribersAll()
	{
		_fixedUpdateSystems.VerifyNoSubscribers();
		_updateSystems.VerifyNoSubscribers();
		_lateUpdateSystems.VerifyNoSubscribers();
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
		GameObject em = new GameObject("[EVENT MANAGER DO]", typeof(EventManagerDO));
		DontDestroyOnLoad(em);
	}
}