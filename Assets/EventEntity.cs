using System;
using Object = UnityEngine.Object;

public struct EventEntity: IEquatable<EventEntity>
{
	private readonly ulong _id;

	public static readonly EventEntity NULL_ENTITY = new EventEntity(NULL_ID);
	
	// We reserve the first uint.MaxValue values for GameObjects
	private static ulong _ids = (ulong)uint.MaxValue + 1;
	private const ulong NULL_ID = ulong.MaxValue;

	public EventEntity(ulong id)
	{
		this._id = id;
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
		
		return new EventEntity(unchecked((uint)obj.GetInstanceID()));
	}

	public bool Equals(EventEntity other)
	{
		return _id == other._id;
	}

	public override bool Equals(object obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		return obj is EventEntity other && Equals(other);
	}

	public override int GetHashCode()
	{
		return _id.GetHashCode();
	}
}