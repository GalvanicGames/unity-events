using System;
using Object = UnityEngine.Object;

namespace UnityEvents
{
	public struct EventTarget: IEquatable<EventTarget>
	{
		public readonly ulong id;

		public static readonly EventTarget NULL_TARGET = new EventTarget(NULL_ID);
	
		// We reserve the first uint.MaxValue values for GameObjects
		private static ulong _ids = (ulong)uint.MaxValue + 1;
		private const ulong NULL_ID = ulong.MaxValue;

		public EventTarget(ulong id)
		{
			this.id = id;
		}

		public EventTarget(int id) : this(unchecked((uint)id))
		{
			
		}
	
		public static EventTarget CreateTarget()
		{
			return new EventTarget(_ids++);
		}

		public static EventTarget CreateTarget(Object obj)
		{
			if (obj == null)
			{
				return NULL_TARGET;
			}
		
			return new EventTarget(obj.GetInstanceID());
		}

		public bool Equals(EventTarget other)
		{
			return id == other.id;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is EventTarget other && Equals(other);
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}
	}
	
}
