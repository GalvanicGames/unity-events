namespace UnityEvents.Internal
{
	public interface IEventSystem
	{
		void Reset();
		void ProcessEvents();
		void VerifyNoSubscribers();
	}

	public interface IJobEventSystem<T> : IEventSystem where T: struct
	{
		void QueueEvent(EventTarget target, T ev);
	}

	public interface IJobForEvent<T> where T : struct
	{
		void ExecuteEvent(T ev);
	}
	
}
