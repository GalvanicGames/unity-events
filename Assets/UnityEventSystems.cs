using System;
using System.Collections.Generic;

public class UnityEventSystems : IDisposable
{
	private static List<IEventSystem> _systems = new List<IEventSystem>();
	private static Dictionary<Type, IEventSystem> _systemsCache = new Dictionary<Type, IEventSystem>();
	private static Dictionary<Type, IEventSystem> _jobSystemsCache = new Dictionary<Type, IEventSystem>();
	private static Dictionary<Type, List<IEventSystem>> _eventToJobSystems = new Dictionary<Type, List<IEventSystem>>();

	public void Subscribe<T_Event>(EventEntity entity, Action<T_Event> eventCallback) where T_Event : unmanaged
	{
		UnityEventSystemDOP<T_Event> system = GetSystem<T_Event>();
		system.Subscribe(entity, eventCallback);
	}

	public void SubscribeWithJob<T_Job, T_Event>(EventEntity entity, T_Job job, Action<T_Job> onComplete)
		where T_Job : struct, IJobForEvent<T_Event>
		where T_Event : unmanaged
	{
		UnityEventJobSystem<T_Job, T_Event> system = GetJobSystem<T_Job, T_Event>();
		system.Subscribe(entity, job, onComplete);
	}

	public void Unsubscribe<T_Event>(EventEntity entity, Action<T_Event> eventCallback) where T_Event : unmanaged
	{
		UnityEventSystemDOP<T_Event> system = GetSystem<T_Event>();
		system.Unsubscribe(entity, eventCallback);
	}

	public void UnsubscribeWithJob<T_Job, T_Event>(EventEntity entity, Action<T_Job> onComplete)
		where T_Job : struct, IJobForEvent<T_Event>
		where T_Event : unmanaged
	{
		UnityEventJobSystem<T_Job, T_Event> system = GetJobSystem<T_Job, T_Event>();
		system.Unsubscribe(entity, onComplete);
	}

	public void QueueEvent<T_Event>(EventEntity entity, T_Event ev) where T_Event : unmanaged
	{
		UnityEventSystemDOP<T_Event> system = GetSystem<T_Event>();
		system.QueueEvent(entity, ev);

		List<IEventSystem> list = GetJobSystemsForEvent<T_Event>();
		
		int count = list.Count;

		for (int i = 0; i < count; i++)
		{
			IJobEventSystem<T_Event> typedSystem = (IJobEventSystem<T_Event>) list[i];
			typedSystem.QueueEvent(entity, ev);
		}
	}

	public UnityEventSystemDOP<T_Event> GetSystem<T_Event>() where T_Event : unmanaged
	{
		IEventSystem system;

		if (_systemsCache.TryGetValue(typeof(T_Event), out system))
		{
			system = new UnityEventSystemDOP<T_Event>();
			_systems.Add(system);
			_systemsCache[typeof(T_Event)] = system;
		}

		return (UnityEventSystemDOP<T_Event>) system;
	}

	public UnityEventJobSystem<T_Job, T_Event> GetJobSystem<T_Job, T_Event>()
		where T_Job : struct, IJobForEvent<T_Event>
		where T_Event : unmanaged
	{
		IEventSystem system;
		
		if (_jobSystemsCache.TryGetValue(typeof(T_Event), out system))
		{
			system = new UnityEventJobSystem<T_Job, T_Event>();
			_systems.Add(system);
			_jobSystemsCache[typeof(T_Event)] = system;
		}

		return (UnityEventJobSystem<T_Job, T_Event>) system;
	}

	public List<IEventSystem> GetJobSystemsForEvent<T_Event>() where T_Event : unmanaged
	{
		List<IEventSystem> list;

		if (!_eventToJobSystems.TryGetValue(typeof(T_Event), out list))
		{
			list = new List<IEventSystem>();
			_eventToJobSystems[typeof(T_Event)] = list;
		}

		return list;
	}

	public void ProcessEvents()
	{
		int count = _systems.Count;
		
		for (int i = 0; i < count; i++)
		{
			_systems[i].ProcessEvents();
		}
	}

	public void Reset()
	{
		int count = _systems.Count;
		
		for (int i = 0; i < count; i++)
		{
			_systems[i].Reset();
		}
	}

	public void VerifyNoSubscribers()
	{
		int count = _systems.Count;
		
		for (int i = 0; i < count; i++)
		{
			_systems[i].VerifyNoSubscribers();
		}
	}

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
}