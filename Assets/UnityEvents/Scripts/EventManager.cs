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
		/// <summary>
		/// The defalut send mode for all events.
		/// </summary>
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

		/// <summary>
		/// Subscribe to the global event.
		/// </summary>
		/// <typeparam name="T">The event that we are subscribing to.</typeparam>
		/// <param name="callback">The function that will be invoked when the event occurs.</param>
		public static void Subscribe<T>(System.Action<T> callback) where T : struct
		{
			UnityEventSystem<T>.global.Subscribe(callback);
		}

		/// <summary>
		/// Subscribe the terminable function to the global event.
		/// </summary>
		/// <typeparam name="T">The event we are subscribing to.</typeparam>
		/// <param name="terminableCallback">The terminable function that will be invoked when the event occurs.</param>
		public static void SubscribeTerminable<T>(System.Func<T, bool> terminableCallback) where T : struct
		{
			UnityEventSystem<T>.global.Subscribe(terminableCallback);
		}

		/// <summary>
		/// Unsubscribe the function from the global event.
		/// </summary>
		/// <typeparam name="T">The event we are unsubscribing from.</typeparam>
		/// <param name="callback">The function that we are unsubscribing.</param>
		public static void Unsubscribe<T>(System.Action<T> callback) where T : struct
		{
			UnityEventSystem<T>.global.Unsubscribe(callback);
		}

		/// <summary>
		/// Unsubscribe the terminable function from the global event.
		/// </summary>
		/// <typeparam name="T">The event we are unsubscribing from.</typeparam>
		/// <param name="terminableCallback">The terminable function we are unsubscribing.</param>
		public static void UnsubscribeTerminable<T>(System.Func<T, bool> terminableCallback) where T : struct
		{
			UnityEventSystem<T>.global.Unsubscribe(terminableCallback);
		}

		/// <summary>
		/// Unsubscribes all functions from every global event who belong to target.
		/// </summary>
		/// <param name="target">The target to check the functions against.</param>
		public static void UnsubscribeTarget(object target)
		{
			if (UnityEventSystemBase.onUnsubscribeTarget != null)
			{
				UnityEventSystemBase.onUnsubscribeTarget(target);
			}
		}

		/// <summary>
		/// Unsubscribes all the functions that are subscribed to the event.
		/// </summary>
		/// <typeparam name="T">The event to reset.</typeparam>
		public static void Reset<T>() where T : struct
		{
			UnityEventSystem<T>.global.Reset();
		}

		/// <summary>
		/// Unsubscribes all the functions to all global events.
		/// </summary>
		public static void ResetGlobal()
		{
			if (UnityEventSystemBase.onGlobalReset != null)
			{
				UnityEventSystemBase.onGlobalReset();
			}
		}

		/// <summary>
		/// Unsubscribes all functions from all event systems (global and local).
		/// </summary>
		public static void ResetAll()
		{
			if (UnityEventSystemBase.onResetAll != null)
			{
				UnityEventSystemBase.onResetAll();
			}
		}

		/// <summary>
		/// Sends an event that will invoked all subscribed functions.
		/// </summary>
		/// <typeparam name="T">Event type that is being sent.</typeparam>
		/// <param name="ev">The event that is sent.</param>
		/// <param name="mode">When should the functions be invoked?</param>
		/// <returns>Returns true if the event was terminated, false otherwise.</returns>
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

		/// <summary>
		/// Get a subscription handle for the function. This allows for more efficient unsubscription.
		/// </summary>
		/// <typeparam name="T">The event of the handle.</typeparam>
		/// <param name="callback">The function the handle will subscribe and unsubscribe with.</param>
		/// <returns>The handle that can be used to subscribe and unsubscribe.</returns>
		public static SubscriptionHandle<T> GetSubscriptionHandle<T>(System.Action<T> callback) where T : struct
		{
			return UnityEventSystem<T>.global.GetSubscriptionHandle(callback);
		}

		/// <summary>
		/// Get a subscription handle for the terminable function. This allows for more efficient unsubscription.
		/// </summary>
		/// <typeparam name="T">The event of the handle.</typeparam>
		/// <param name="terminableCallback">The terminable function the handle will subscribe and unsubscribe with.</param>
		/// <returns>The handle that can be used to subscribe and unsubscribe</returns>
		public static SubscriptionHandle<T> GetSubscriptionHandleTerminable<T>(System.Func<T, bool> terminableCallback) where T : struct
		{
			return UnityEventSystem<T>.global.GetSubscriptionHandle(terminableCallback);
		}

		/// <summary>
		/// Does the global event currently have any subscribers?
		/// </summary>
		/// <typeparam name="T">The event to check.</typeparam>
		/// <returns>True if there are subscribers, false others.</returns>
		public static bool HasSubscribers<T>() where T : struct
		{
			return UnityEventSystem<T>.global.HasSubscribers();
		}

		//
		// Local Event System extensions
		//

		/// <summary>
		/// Subscribe the function to the local event system of the GameObject.
		/// </summary>
		/// <typeparam name="T">The event we are subscribing to.</typeparam>
		/// <param name="obj">The GameObject who's local event system we are subscribing.</param>
		/// <param name="callback">The function we are subscribing with.</param>
		public static void Subscribe<T>(this GameObject obj, System.Action<T> callback) where T : struct
		{
			GetLocalSystem(obj).Subscribe(callback);
		}

		/// <summary>
		/// Subscribe the terminable function to the local event system of the GameObject.
		/// </summary>
		/// <typeparam name="T">The event we are subscribing to.</typeparam>
		/// <param name="obj">The GameObject who's local event system we are subscribing.</param>
		/// <param name="terminableCallback">The terminable function we are subscribing with.</param>
		public static void SubscribeTerminable<T>(this GameObject obj, System.Func<T, bool> terminableCallback) where T : struct
		{
			GetLocalSystem(obj).Subscribe(terminableCallback);
		}

		/// <summary>
		/// Unsubscribe the function from the local event system of the GameObject.
		/// </summary>
		/// <typeparam name="T">The event we are unsubscribing from.</typeparam>
		/// <param name="obj">The GameObject who's local event system we are unsubscribing from.</param>
		/// <param name="callback">The function we are unsubscribing.</param>
		public static void Unsubscribe<T>(this GameObject obj, System.Action<T> callback) where T : struct
		{
			GetLocalSystem(obj).Unsubscribe(callback);
		}

		/// <summary>
		/// Unsubscribe the terminable function from the local event system of the GameObject.
		/// </summary>
		/// <typeparam name="T">The event we are unsubscribing from.</typeparam>
		/// <param name="obj">The GameObject who's local event system we are unsubscribing from.</param>
		/// <param name="terminableCallback">The terminable function we are unsubscribing.</param>
		public static void UnsubscribeTerminable<T>(this GameObject obj, System.Func<T, bool> terminableCallback) where T : struct
		{
			GetLocalSystem(obj).Unsubscribe(terminableCallback);
		}

		/// <summary>
		/// Send the event to the GameObject's local event system.
		/// </summary>
		/// <typeparam name="T">The event type being sent.</typeparam>
		/// <param name="obj">The GameObject who's local event system is used.</param>
		/// <param name="ev">The event being sent.</param>
		/// <param name="mode">When should the functions be invoked?</param>
		/// <returns>Returns true if the event was terminated, false otherwise.</returns>
		public static bool SendEvent<T>(
			this GameObject obj,
			T ev,
			EventSendMode mode = EventSendMode.Default) where T : struct
		{
			return GetLocalSystem(obj).SendEvent(ev, mode);
		}

		/// <summary>
		/// Recurses down the GameObject's children sending the event.
		/// </summary>
		/// <typeparam name="T">The event type being sent.</typeparam>
		/// <param name="obj">The starting GameObject.</param>
		/// <param name="ev">The event being sent.</param>
		/// <param name="mode">When should the functions be invoked?</param>
		/// <returns>Returns true if the event was terminated, false otherwise.</returns>
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

		/// <summary>
		/// Unsubscribes all functions of the target on the GameObject's local event system.
		/// </summary>
		/// <param name="obj">The GameObject who's local event system is used.</param>
		/// <param name="target">The target which will have it's functions unsubscribed.</param>
		public static void UnsubscribeTarget(this GameObject obj, object target)
		{
			GetLocalSystem(obj).UnsubscribeTarget(target);
		}

		/// <summary>
		/// Unsubscribes all functions from the GameObject's local event system.
		/// </summary>
		/// <param name="obj">The GameObject who's local event system is used.</param>
		public static void ResetEventSystem(this GameObject obj)
		{
			GetLocalSystem(obj).Reset();
		}

		/// <summary>
		/// Initializes the GameObject with the LocalEventSystem component. This isn't necessarily and is lazily done but can be done ahead of time to avoid later garbage generation.
		/// </summary>
		/// <param name="obj">The GameObject who is being initialized.</param>
		public static void InitializeEventSystem(this GameObject obj)
		{
			GetLocalSystem(obj);
		}

		/// <summary>
		/// Get a handle that allows more efficient event sending. 
		/// </summary>
		/// <typeparam name="T">The event type.</typeparam>
		/// <param name="obj">The GameObject who's local system is used.</param>
		/// <returns>Returns the handle that can be used to send events.</returns>
		public static EventHandle<T> GetEventHandle<T>(this GameObject obj) where T : struct
		{
			return GetLocalSystem(obj).GetEventHandle<T>();
		}

		/// <summary>
		/// Get a handle that allows more efficient subscriptions and unsubscriptions.
		/// </summary>
		/// <typeparam name="T">The event type.</typeparam>
		/// <param name="obj">The GameObject who's local system is used.</param>
		/// <param name="callback">The function used for subscriptions and unsubscriptions.</param>
		/// <returns>Returns the handle that can be used for subscriptions and unsubscriptions.</returns>
		public static SubscriptionHandle<T> GetSubscriptionHandle<T>(this GameObject obj, System.Action<T> callback) where T : struct
		{
			return GetLocalSystem(obj).GetSubscriptionHandle(callback);
		}

		/// <summary>
		/// Get a handle that allows more efficient subscription and unsubscriptions of terminable functions.
		/// </summary>
		/// <typeparam name="T">The event type.</typeparam>
		/// <param name="obj">The GameObject who's local system is used.</param>
		/// <param name="terminableCallback">The terminable function used for subscriptions and unsubscriptions.</param>
		/// <returns>Returns the handle that can be used for subscriptions and unsubscriptions.</returns>
		public static SubscriptionHandle<T> GetSubscriptionHandleTerminable<T>(this GameObject obj, System.Func<T, bool> terminableCallback) where T : struct
		{
			return GetLocalSystem(obj).GetSubscriptionHandle(terminableCallback);
		}

		/// <summary>
		/// Does the GameObject's local event system have any subscribers for the event?
		/// </summary>
		/// <typeparam name="T">The event to check for.</typeparam>
		/// <param name="obj">The GameObject who's local event system is checked.</param>
		/// <returns>True if there are subscribers, false otherwise.</returns>
		public static bool HasSubscribers<T>(this GameObject obj) where T : struct
		{
			return GetLocalSystem(obj).HasSubscribers<T>();
		}

		#region private

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

		private static LocalEventSystem GetLocalSystem(GameObject obj)
		{
			LocalEventSystem system = obj.GetComponent<LocalEventSystem>();

			if (system == null)
			{
				system = obj.AddComponent<LocalEventSystem>();
			}

			return system;
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

		#endregion
	}
}