using System;
using Object = UnityEngine.Object;

namespace UnityEvents
{
	public struct EventEntity: IEquatable<EventEntity>
	{
		public readonly ulong id;

		public static readonly EventEntity NULL_ENTITY = new EventEntity(NULL_ID);
	
		// We reserve the first uint.MaxValue values for GameObjects
		private static ulong _ids = (ulong)uint.MaxValue + 1;
		private const ulong NULL_ID = ulong.MaxValue;

		public EventEntity(ulong id)
		{
			this.id = id;
		}

		public EventEntity(int id) : this(unchecked((uint)id))
		{
			
		}
	
		public static EventEntity CreateEntity()
		{
			return new EventEntity(_ids++);
		}

		public static EventEntity CreateEntity(Object obj)
		{
			if (obj == null)
			{
				return NULL_ENTITY;
			}
		
			return new EventEntity(obj.GetInstanceID());
		}

		public bool Equals(EventEntity other)
		{
			return id == other.id;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is EventEntity other && Equals(other);
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}
	}
	
}
