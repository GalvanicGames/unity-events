using System.Collections.Generic;
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
		public LinkedListNode<System.Action<T>> callbackNode;

		// The following are only used by the local event systems.
		public MonoEventSystem monoEventSystem;
		public UnityEventSystem<T> eventSystem;
	}

	public enum EventSendMode
	{
		Default,
		Immediate,
		OnNextFixedUpdate
	}
}