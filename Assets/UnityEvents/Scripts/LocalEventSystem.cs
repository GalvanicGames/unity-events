using UnityEngine;
using System.Collections.Generic;
using UnityEvents;

namespace UnityEventsInternal
{
	public class LocalEventSystem : MonoBehaviour
	{
		private Dictionary<System.Type, UnityEventSystemBase> _eventSystemsDict
			= new Dictionary<System.Type, UnityEventSystemBase>();

		private List<UnityEventSystemBase> _eventSystemsList = new List<UnityEventSystemBase>();
		private List<QueuedEventBase> _queuedFixedUpdateEvents = new List<QueuedEventBase>();
		private List<QueuedEventBase> _queuedLateUpdateEvents = new List<QueuedEventBase>(); 
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
				UnityEventSystem<T> system = (UnityEventSystem<T>)eventSystem;
				system.SendEvent(eventToSend);
			}

			public override void LockSystem()
			{
				UnityEventSystem<T> system = (UnityEventSystem<T>)eventSystem;
				system.lockSubscriptions = true;
			}

			public override void UnlockSystem()
			{
				UnityEventSystem<T> system = (UnityEventSystem<T>)eventSystem;
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
			_queuedFixedUpdateEvents.Clear();
		}

		public void Subscribe<T>(System.Action<T> callback) where T : struct
		{
			UnityEventSystem<T> system = GetSystem<T>();
			system.Subscribe(callback);
		}

		public void Subscribe<T>(System.Func<T, bool> terminableCallback) where T : struct
		{
			UnityEventSystem<T> system = GetSystem<T>();
			system.Subscribe(terminableCallback);
		}

		public void Unsubscribe<T>(System.Action<T> callback) where T : struct
		{
			UnityEventSystem<T> system = GetSystem<T>();
			system.Unsubscribe(callback);
		}

		public void Unsubscribe<T>(System.Func<T, bool> terminableCallback) where T : struct
		{
			UnityEventSystem<T> system = GetSystem<T>();
			system.Unsubscribe(terminableCallback);
		}

		public void TurnOnDebug<T>() where T : struct
		{
			GetSystem<T>().debug = true;
		}

		private AttributeSubscription RegisterCallback<T>(System.Action<T> callback) where T : struct
		{
			UnityEventSystem<T> system = GetSystem<T>();

			AttributeSubscription sub = new AttributeSubscription();
			sub.system = system;
			sub.node = system.RegisterCallback(callback);

			return sub;
		}

		private AttributeSubscription RegisterCallbackTerminable<T>(System.Func<T, bool> terminableCallback) where T : struct
		{
			UnityEventSystem<T> system = GetSystem<T>();

			AttributeSubscription sub = new AttributeSubscription();
			sub.system = system;
			sub.node = system.RegisterCallback(terminableCallback);

			return sub;
		}

		public void UpdateAttributeSubscription<T>(AttributeSubscription attSub) where T : struct
		{
			attSub.system = GetSystem<T>();
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

		public bool HasSubscribers<T>() where T : struct
		{
			return GetSystem<T>().HasSubscribers();
		}

		public bool SendEvent<T>(T ev, EventSendMode mode = EventSendMode.Default) where T : struct
		{
			UnityEventSystem<T> system = GetSystem<T>();
			return SendEventWithSystem(ev, system, mode);
		}

		public bool SendEventWithSystem<T>(T ev, UnityEventSystem<T> system, EventSendMode mode) where T : struct
		{
			if (mode == EventSendMode.Default)
			{
				mode = EventManager.defaultSendMode;
			}

			if (mode == EventSendMode.Immediate)
			{
				return system.SendEvent(ev);
			}

			QueuedEvent<T> newQueued = QueuedEvent<T>.Get();
			newQueued.eventToSend = ev;
			newQueued.eventSystem = system;

			if (_sendingQueuedEvents)
			{
				_secondaryQueuedEvents.Add(newQueued);
			}
			else
			{
				if (mode == EventSendMode.OnNextFixedUpdate)
				{
					_queuedFixedUpdateEvents.Add(newQueued);
				}
				else
				{
					_queuedLateUpdateEvents.Add(newQueued);
				}
			}

			// If sent on fixed or late update then the caller can't know if it was termianted, always return false.
			return false;
		}

		public EventHandle<T> GetEventHandle<T>() where T : struct
		{
			UnityEventSystem<T> system = GetSystem<T>();
			return new EventHandle<T>(system);
		}

		public SubscriptionHandle<T> GetSubscriptionHandle<T>(
			System.Action<T> callback) where T : struct
		{
			UnityEventSystem<T> system = GetSystem<T>();
			return system.GetSubscriptionHandle(callback);
		}

		public SubscriptionHandle<T> GetSubscriptionHandle<T>(
			System.Func<T, bool> terminableCallback) where T : struct
		{
			UnityEventSystem<T> system = GetSystem<T>();
			return system.GetSubscriptionHandle(terminableCallback);
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

				_queuedFixedUpdateEvents.Clear();
				_queuedLateUpdateEvents.Clear();
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
			if (_queuedFixedUpdateEvents.Count > 0)
			{
				SendQueuedEvents(_queuedFixedUpdateEvents);
			}
		}

		private void LateUpdate()
		{
			// Late Update keeps sending events until there aren't none... Obviously this can lead to an infinite loop
			while (_queuedLateUpdateEvents.Count > 0)
			{
				SendQueuedEvents(_queuedLateUpdateEvents);
			}
		}

		private void SendQueuedEvents(List<QueuedEventBase> queue)
		{
			_sendingQueuedEvents = true;

			// First we lock all of the systems
			for (int i = 0; i < queue.Count; i++)
			{
				queue[i].LockSystem();
			}

			// Send the events.
			for (int i = 0; i < queue.Count; i++)
			{
				queue[i].Send();
			}

			// Unlock the systems
			for (int i = 0; i < queue.Count; i++)
			{
				queue[i].UnlockSystem();
				queue[i].Release();
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
				queue.Clear();

				for (int i = 0; i < _secondaryQueuedEvents.Count; i++)
				{
					queue.Add(_secondaryQueuedEvents[i]);
				}

				_secondaryQueuedEvents.Clear();
			}
		}
	}
}
