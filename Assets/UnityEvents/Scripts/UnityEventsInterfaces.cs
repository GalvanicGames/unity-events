using UnityEvents;

namespace UnityEventsInternal
{
	public interface IEventSystem
	{
		void Reset();
		void ProcessEvents();
		void VerifyNoSubscribers();
	}

	public interface IJobEventSystem<T> : IEventSystem where T: unmanaged
	{
		void QueueEvent(EventTarget target, T ev);
	}

	public interface IJobForEvent<T> where T : unmanaged
	{
		void ExecuteEvent(T ev);
	}
	
}
