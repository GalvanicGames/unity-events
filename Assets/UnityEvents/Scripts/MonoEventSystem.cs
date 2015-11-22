using UnityEngine;
using System.Collections.Generic;
using UnityEvents;

namespace UnityEventsInternal
{
	public class MonoEventSystem : MonoBehaviour
	{
		private Dictionary<System.Type, UnityEventSystemBase> _eventSystemsDict
			= new Dictionary<System.Type, UnityEventSystemBase>();

		private List<UnityEventSystemBase> _eventSystemsList = new List<UnityEventSystemBase>();
		private List<QueuedEventBase> _queuedEvents = new List<QueuedEventBase>();
		private List<QueuedEventBase> _secondaryQueuedEvents = new List<QueuedEventBase>(); 

		private bool _sendingQueuedEvents;
		private bool _reset;

		private abstract class QueuedEventBase
		{
			public UnityEventSystemBase eventSystem;

			public abstract void Send();
			public abstract void LockSystem();
			public abstract void UnlockSystem();
			public abstract void Release();
		}

		private class QueuedEvent<T> : QueuedEventBase where T : struct
		{
			public T eventToSend;

			private static Stack<QueuedEvent<T>> _pool;
			private const int INITIAL_STARTING_COUNT = 10;

			public override void Send()
			{
				UnityEventSystem<T> system = (UnityEventSystem<T>) eventSystem;
				system.SendEvent(eventToSend);
			}

			public override void LockSystem()
			{
				UnityEventSystem<T> system = (UnityEventSystem<T>) eventSystem;
				system.lockSubscriptions = true;
			}

			public override void UnlockSystem()
			{
				UnityEventSystem<T> system = (UnityEventSystem<T>) eventSystem;
				system.lockSubscriptions = false;
			}

			public static QueuedEvent<T> Get()
			{
				if (_pool == null)
				{
					_pool = new Stack<QueuedEvent<T>>();

					for (int i = 0; i < INITIAL_STARTING_COUNT; i++)
					{
						_pool.Push(new QueuedEvent<T>());
					}
				}

				if (_pool.Count > 0)
				{
					return _pool.Pop();
				}

				return new QueuedEvent<T>();
			}

			public override void Release()
			{
				_pool.Push(this);
			}
		}

		private void OnDestroy()
		{
			for (int i = 0; i < _eventSystemsList.Count; i++)
			{
				_eventSystemsList[i].CleanUp();
			}
		}

		private void OnDisable()
		{
			_queuedEvents.Clear();
		}

		public EventHandle<T> Subscribe<T>(System.Action<T> callback) where T : struct
		{
			return Subscribe(callback, GetSystem<T>());
		}

		public EventHandle<T> Subscribe<T>(System.Action<T> callback, EventHandle<T> handle) where T : struct
		{
			if (handle.monoEventSystem != this)
			{
				Debug.LogErrorFormat(
					"Wrong handle for local event system! Expecting handle for {0} (tid: {1}).",
					gameObject.name,
					transform.GetInstanceID());

				return handle;
			}

			return Subscribe(callback, handle.eventSystem);
		}

		private EventHandle<T> Subscribe<T>(
			System.Action<T> callback,
			UnityEventSystem<T> system) where T : struct
		{
			EventHandle<T> handle = system.Subscribe(callback);
			handle.eventSystem = system;
			handle.monoEventSystem = this;

			return handle;
		}

		public void Unsubscribe<T>(System.Action<T> callback) where T : struct
		{
			UnityEventSystem<T> system = GetSystem<T>();
			system.Unsubscribe(callback);
		}

		public void Unsubscribe<T>(EventHandle<T> handle) where T : struct
		{
			if (handle.monoEventSystem != this)
			{
				Debug.LogErrorFormat(
					"Wrong handle for local event system! Expecting handle for {0} (tid: {1}).",
					gameObject.name,
					transform.GetInstanceID());

				return;
			}

			handle.eventSystem.Unsubscribe(handle);
		}

		public void TurnOnDebug<T>() where T : struct
		{
			GetSystem<T>().debug = true;
		}

		private UnityEventSystem<T> GetSystem<T>() where T : struct
		{
			UnityEventSystemBase baseSystem;
			UnityEventSystem<T> system;

			if (_eventSystemsDict.TryGetValue(typeof(T), out baseSystem))
			{
				system = (UnityEventSystem<T>)baseSystem;
			}
			else
			{
				system = new UnityEventSystem<T>();
				_eventSystemsDict[typeof(T)] = system;
				_eventSystemsList.Add(system);
			}

			return system;
		}

		public EventHandle<T> SendEvent<T>(T ev, EventSendMode mode = EventSendMode.Default) where T : struct
		{
			return SendEvent(ev, GetSystem<T>(), mode);
		}

		public EventHandle<T> SendEvent<T>(
			T ev, 
			EventHandle<T> handle, 
			EventSendMode mode = EventSendMode.Default) where T : struct
		{
			if (handle.monoEventSystem != this)
			{
				Debug.LogErrorFormat(
					"Wrong handle for local event system! Expecting handle for {0} (tid: {1}).",
					gameObject.name,
					transform.GetInstanceID());

				return handle;
			}

			return SendEvent(ev, handle.eventSystem, mode);
		}

		public EventHandle<T> SendEvent<T>(
			T ev,
			UnityEventSystem<T> system,
			EventSendMode mode) where T : struct
		{
			if (mode == EventSendMode.Default)
			{
				mode = EventManager.defaultSendMode;
			}

			if (mode == EventSendMode.Immediate)
			{
				system.SendEvent(ev);
			}
			else
			{
				QueuedEvent<T> newQueued = QueuedEvent<T>.Get();
				newQueued.eventToSend = ev;
				newQueued.eventSystem = system;

				if (_sendingQueuedEvents)
				{
					_secondaryQueuedEvents.Add(newQueued);
				}
				else
				{
					_queuedEvents.Add(newQueued);
				}
			}

			return GetHandle(system);
		}

		public EventHandle<T> GetHandle<T>() where T : struct
		{
			return GetHandle(GetSystem<T>());
		}

		private EventHandle<T> GetHandle<T>(UnityEventSystem<T> system) where T : struct
		{
			EventHandle<T> handle = new EventHandle<T>();
			handle.eventSystem = system;
			handle.monoEventSystem = this;

			return handle;
		}

		public void Reset()
		{
			if (_sendingQueuedEvents)
			{
				_reset = true;
			}
			else
			{
				for (int i = 0; i < _eventSystemsList.Count; i++)
				{
					_eventSystemsList[i].Reset();
				}

				_queuedEvents.Clear();
				_secondaryQueuedEvents.Clear();
				_eventSystemsDict.Clear();
				_eventSystemsList.Clear();
			}
		}

		public void UnsubscribeTarget(object target)
		{
			for (int i = 0; i < _eventSystemsList.Count; i++)
			{
				_eventSystemsList[i].UnsubscribeTarget(target);
			}
		}

		private void FixedUpdate()
		{
			if (_queuedEvents.Count > 0)
			{
				_sendingQueuedEvents = true;

				// First we lock all of the systems
				for (int i = 0; i < _queuedEvents.Count; i++)
				{
					_queuedEvents[i].LockSystem();
				}

				// Send the events.
				for (int i = 0; i < _queuedEvents.Count; i++)
				{
					_queuedEvents[i].Send();
				}

				// Unlock the systems
				for (int i = 0; i < _queuedEvents.Count; i++)
				{
					_queuedEvents[i].UnlockSystem();
					_queuedEvents[i].Release();
				}

				_sendingQueuedEvents = false;

				if (_reset)
				{
					Reset();
					_reset = false;
				}
				else
				{
					// We might have received delayed event sends, those are
					// for the next fixed update.
					_queuedEvents.Clear();

					for (int i = 0; i < _secondaryQueuedEvents.Count; i++)
					{
						_queuedEvents.Add(_secondaryQueuedEvents[i]);
					}

					_secondaryQueuedEvents.Clear();
				}
			}
		}
	}
}
