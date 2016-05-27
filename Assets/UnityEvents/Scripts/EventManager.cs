using UnityEngine;
using UnityEventsInternal;

namespace UnityEvents
{
	/// <summary>
	/// EventManager owns the global event system. Subscribing/unsubscribing
	/// and sending events though EventManager could be used as the global.
	/// </summary>
	public static class EventManager
	{
		private const string HELPER_NAME = "[EventManager Helper]";

		private static LocalEventSystem _helper
		{
			get
			{
				if (_mHelper == null)
				{
					GameObject obj = new GameObject(HELPER_NAME);
					_mHelper = obj.AddComponent<LocalEventSystem>();
				}

				return _mHelper;
			}
		}

		private static LocalEventSystem _mHelper;

		public static EventSendMode defaultSendMode
		{
			get { return _defaultSendMode; }
			set
			{
				if (value == EventSendMode.Default)
				{
					Debug.LogWarning("Setting EventManager.defaultSendMode to Default has no meaning.");
					return;
				}

				_defaultSendMode = value;
			}
		}

		private static EventSendMode _defaultSendMode = EventSendMode.Immediate;

		public static void Subscribe<T>(System.Action<T> callback) where T : struct
		{
			UnityEventSystem<T>.global.Subscribe(callback);
		}

		public static void SubscribeTerminable<T>(System.Func<T, bool> terminableCallback) where T : struct
		{
			UnityEventSystem<T>.global.Subscribe(terminableCallback);
		}

		public static void Unsubscribe<T>(System.Action<T> callback) where T : struct
		{
			UnityEventSystem<T>.global.Unsubscribe(callback);
		}

		public static void UnsubscribeTerminable<T>(System.Func<T, bool> terminableCallback) where T : struct
		{
			UnityEventSystem<T>.global.Unsubscribe(terminableCallback);
		}

		public static void UnsubscribeTarget(object target)
		{
			if (UnityEventSystemBase.onUnsubscribeTarget != null)
			{
				UnityEventSystemBase.onUnsubscribeTarget(target);
			}
		}

		public static void Reset<T>() where T : struct
		{
			UnityEventSystem<T>.global.Reset();
		}

		public static void ResetGlobal()
		{
			if (UnityEventSystemBase.onGlobalReset != null)
			{
				UnityEventSystemBase.onGlobalReset();
			}
		}

		public static void ResetAll()
		{
			if (UnityEventSystemBase.onResetAll != null)
			{
				UnityEventSystemBase.onResetAll();
			}
		}

		public static bool SendEvent<T>(
			T ev,
			EventSendMode mode = EventSendMode.Default) where T : struct
		{
			if (mode == EventSendMode.Default)
			{
				mode = defaultSendMode;
			}

			if (mode == EventSendMode.Immediate)
			{
				return UnityEventSystem<T>.global.SendEvent(ev);
			}

			// If we're sending on fixed update then we hook into
			// the global LocalEventSystem and have it handle the
			// system.
			_helper.SendEventWithSystem(ev, UnityEventSystem<T>.global, mode);

			// Can't know if fixed update events terminated, always return false.
			return false;
		}

		public static SubscriptionHandle<T> GetSubscriptionHandle<T>(System.Action<T> callback) where T : struct
		{
			return UnityEventSystem<T>.global.GetSubscriptionHandle(callback);
		}

		public static SubscriptionHandle<T> GetSubscriptionHandleTerminable<T>(System.Func<T, bool> terminableCallback) where T : struct
		{
			return UnityEventSystem<T>.global.GetSubscriptionHandle(terminableCallback);
		}

		private static AttributeSubscription RegisterCallback<T>(System.Action<T> callback) where T : struct
		{
			AttributeSubscription sub = new AttributeSubscription();
			sub.system = UnityEventSystem<T>.global;
			sub.node = UnityEventSystem<T>.global.RegisterCallback(callback);

			return sub;
		}

		private static AttributeSubscription RegisterCallbackTerminable<T>(System.Func<T, bool> terminableCallback) where T : struct
		{
			AttributeSubscription sub = new AttributeSubscription();
			sub.system = UnityEventSystem<T>.global;
			sub.node = UnityEventSystem<T>.global.RegisterCallback(terminableCallback);

			return sub;
		}

		public static bool HasSubscribers<T>() where T : struct
		{
			return UnityEventSystem<T>.global.HasSubscribers();
		}

		// Extensions
		public static void Subscribe<T>(this GameObject obj, System.Action<T> callback) where T : struct
		{
			GetLocalSystem(obj).Subscribe(callback);
		}

		public static void SubscribeTerminable<T>(this GameObject obj, System.Func<T, bool> terminableCallback) where T : struct
		{
			GetLocalSystem(obj).Subscribe(terminableCallback);
		}

		public static void Unsubscribe<T>(this GameObject obj, System.Action<T> callback) where T : struct
		{
			GetLocalSystem(obj).Unsubscribe(callback);
		}

		public static void UnsubscribeTerminable<T>(this GameObject obj, System.Func<T, bool> terminableCallback) where T : struct
		{
			GetLocalSystem(obj).Unsubscribe(terminableCallback);
		}

		public static bool SendEvent<T>(
			this GameObject obj,
			T ev,
			EventSendMode mode = EventSendMode.Default) where T : struct
		{
			return GetLocalSystem(obj).SendEvent(ev, mode);
		}

		public static bool SendEventDeep<T>(
			this GameObject obj,
			T ev,
			EventSendMode mode = EventSendMode.Default) where T : struct
		{
			bool terminated = obj.SendEvent(ev, mode);

			if (terminated)
			{
				return true;
			}

			for (int i = 0; i < obj.transform.childCount; i++)
			{
				terminated = obj.transform.GetChild(i).gameObject.SendEventDeep(ev, mode);

				if (terminated)
				{
					return true;
				}
			}

			return false;
		}

		public static void UnsubscribeTarget(this GameObject obj, object target)
		{
			GetLocalSystem(obj).UnsubscribeTarget(target);
		}

		public static void ResetEventSystem(this GameObject obj)
		{
			GetLocalSystem(obj).Reset();
		}

		public static void InitializeEventSystem(this GameObject obj)
		{
			GetLocalSystem(obj);
		}

		public static EventHandle<T> GetEventHandle<T>(this GameObject obj) where T : struct
		{
			return GetLocalSystem(obj).GetEventHandle<T>();
		}

		public static SubscriptionHandle<T> GetSubscriptionHandle<T>(this GameObject obj, System.Action<T> callback) where T : struct
		{
			return GetLocalSystem(obj).GetSubscriptionHandle(callback);
		}

		public static SubscriptionHandle<T> GetSubscriptionHandleTerminable<T>(this GameObject obj, System.Func<T, bool> terminableCallback) where T : struct
		{
			return GetLocalSystem(obj).GetSubscriptionHandle(terminableCallback);
		}

		public static bool HasSubscribers<T>(this GameObject obj) where T : struct
		{
			return GetLocalSystem(obj).HasSubscribers<T>();
		}

		private static LocalEventSystem GetLocalSystem(GameObject obj)
		{
			LocalEventSystem system = obj.GetComponent<LocalEventSystem>();

			if (system == null)
			{
				system = obj.AddComponent<LocalEventSystem>();
			}

			return system;
		}
	}
}