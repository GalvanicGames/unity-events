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

		private static MonoEventSystem _helper
		{
			get
			{
				if (_mHelper == null)
				{
					GameObject obj = new GameObject(HELPER_NAME);
					_mHelper = obj.AddComponent<MonoEventSystem>();
				}

				return _mHelper;
			}
		}

		private static MonoEventSystem _mHelper;

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

		public static EventHandle<T> Subscribe<T>(System.Action<T> callback) where T : struct
		{
			return UnityEventSystem<T>.global.Subscribe(callback);
		}

		public static void Unsubscribe<T>(System.Action<T> callback) where T : struct
		{
			UnityEventSystem<T>.global.Unsubscribe(callback);
		}

		public static void Unsubscribe<T>(EventHandle<T> handle) where T : struct
		{
			UnityEventSystem<T>.global.Unsubscribe(handle);
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

		public static void SendEvent<T>(
			T ev,
			EventSendMode mode = EventSendMode.Default) where T : struct
		{
			if (mode == EventSendMode.Default)
			{
				mode = defaultSendMode;
			}

			if (mode == EventSendMode.Immediate)
			{
				UnityEventSystem<T>.global.SendEvent(ev);
			}
			else
			{
				// If we're sending on fixed update then we hook into
				// the global MonoEventSystem and have it handle the
				// system.
				_helper.SendEvent(ev, UnityEventSystem<T>.global, mode);
			}
		}

		// Extensions

		public static EventHandle<T> Subscribe<T>(this GameObject obj, System.Action<T> callback) where T : struct
		{
			return GetSystemComp(obj).Subscribe(callback);
		}

		public static EventHandle<T> Subscribe<T>(
			this GameObject obj,
			System.Action<T> callback,
			EventHandle<T> handle) where T : struct
		{
			return handle.monoEventSystem.Subscribe(callback, handle);
		}

		public static void Unsubscribe<T>(this GameObject obj, System.Action<T> callback) where T : struct
		{
			GetSystemComp(obj).Unsubscribe(callback);
		}

		public static void Unsubscribe<T>(
			this GameObject obj, 
			EventHandle<T> handle) where T : struct
		{
			handle.monoEventSystem.Unsubscribe(handle);
		}

		public static EventHandle<T> SendEvent<T>(
			this GameObject obj, 
			T ev,
			EventSendMode mode = EventSendMode.Default) where T : struct
		{
			return GetSystemComp(obj).SendEvent(ev, mode);
		}

		public static EventHandle<T> SendEvent<T>(
			this GameObject obj,
			T ev,
			EventHandle<T> handle,
			EventSendMode mode = EventSendMode.Default) where T : struct
		{
			return handle.monoEventSystem.SendEvent(ev, handle, mode);
		}

		public static void UnsubscribeTarget(this GameObject obj, object target)
		{
			GetSystemComp(obj).UnsubscribeTarget(target);
		}

		public static void ResetEventSystem(this GameObject obj)
		{
			GetSystemComp(obj).Reset();
		}

		public static void InitializeEventSystem(this GameObject obj)
		{
			GetSystemComp(obj);
		}

		public static EventHandle<T> GetHandle<T>(this GameObject obj) where T : struct
		{
			return GetSystemComp(obj).GetHandle<T>();
		}

		private static MonoEventSystem GetSystemComp(GameObject obj)
		{
			MonoEventSystem systemComp = obj.GetComponent<MonoEventSystem>();

			if (systemComp == null)
			{
				systemComp = obj.AddComponent<MonoEventSystem>();
			}

			return systemComp;
		}
	}

}