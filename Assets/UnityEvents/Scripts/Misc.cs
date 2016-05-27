using System.Collections.Generic;
using UnityEngine;
using UnityEventsInternal;

namespace UnityEvents
{
	/// <summary>
	/// This is used for optimization, by passing this in the event system
	/// will be able to avoid dictionary look ups and list searching.
	/// 
	/// If optimization isn't a worry then these can be disregarded.
	/// </summary>
	public struct EventHandle<T> where T : struct
	{
		private UnityEventSystem<T> _eventSystem;

		public EventHandle(UnityEventSystem<T> system)
		{
			_eventSystem = system;
		}

		public override int GetHashCode()
		{
			return (_eventSystem != null ? _eventSystem.GetHashCode() : 0);
		}

		public bool Equals(EventHandle<T> other)
		{
			return Equals(_eventSystem, other._eventSystem);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			return obj is EventHandle<T> && Equals((EventHandle<T>) obj);
		}

		public static bool operator ==(EventHandle<T> a, EventHandle<T> b)
		{
			return a._eventSystem == b._eventSystem;
		}

		public static bool operator !=(EventHandle<T> a, EventHandle<T> b)
		{
			return !(a == b);
		}

		public void SendEvent(T ev)
		{
			_eventSystem.SendEvent(ev);
		}
	}

	public struct SubscriptionHandle<T> where T : struct
	{
		private UnityEventSystem<T> _system;
		private System.Action<T> _callback;
		private System.Func<T, bool> _terminableCallback; 
		private LinkedListNode<EventCallback<T>> _node;

		public SubscriptionHandle(
			UnityEventSystem<T> system,
			System.Action<T> callback)
		{
			_system = system;
			_callback = callback;
			_terminableCallback = null;
			_node = null;
		}

		public SubscriptionHandle(
			UnityEventSystem<T> system,
			System.Func<T, bool> terminableCallback)
		{
			_system = system;
			_callback = null;
			_terminableCallback = terminableCallback;
			_node = null;
		}

		public void Subscribe()
		{
			if (_callback != null)
			{
				_node = _system.SubscribeGetNode(_callback);
			}
			else
			{
				_node = _system.SubscribeGetNode(_terminableCallback);
			}
		}

		public void Unsubscribe()
		{			
			_system.UnsubscribeWithNode(_node);
		}
	}

	public enum EventSendMode
	{
		Default,
		Immediate,
		OnNextFixedUpdate,
		OnLateUpdate
	}

	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class GlobalEventListener : System.Attribute
	{
	
	}
	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class LocalEventListener : System.Attribute
	{
	}

	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class ParentCompEventListener : System.Attribute
	{
		public System.Type compToLookFor;
		public bool skipSelf;

		public ParentCompEventListener(System.Type compType, bool skipSelf = false)
		{
			compToLookFor = compType;
			this.skipSelf = skipSelf;
		}
	}
}

namespace UnityEventsInternal
{
	public class EventCallback<T>
	{
		public System.Action<T> callback;
		public System.Func<T, bool> terminableCallback; 
		public bool isActive;
	}


	public class AttributeSubscription
	{
		public UnityEventSystemBase system;
		public object node;
	}
}