using System;
using System.Collections.Generic;

namespace UnityEvents.Internal
{
	public class MultipleSubscriptionsException<T> : Exception
	{
		public MultipleSubscriptionsException(Action<T> callback)
			: base($"Not allowed to subscribe the same callback to the same entity! Target: {callback.Target.GetType().Name} Event: {typeof(T).Name}")
		{
			
		}
	}

	public class SubscriberStillListeningException<T_Callback, T_Event> : Exception
		where T_Event : struct
	{
		public SubscriberStillListeningException(List<Action<T_Callback>> listeners)
			: base(GenerateMessage(listeners))
		{
		}

		private static string GenerateMessage(List<Action<T_Callback>> listeners)
		{
			string msg = $"The following subscribers are still listening to the {typeof(T_Event).Name} system!";

			foreach (Action<T_Callback> listener in listeners)
			{
				if (listener == null)
				{
					msg += "\n<NULL>";
				}
				else
				{
					msg += $"\n{listener.Method.Name}";
				}
			}

			return msg;
		}
	}

	public class IndexOutOfReservedTargetsException : Exception
	{
		
	}

	public class EventTypeNotBlittableException : Exception
	{
		public EventTypeNotBlittableException(Type type) 
			: base($"Event type {type.Name} must be blittable!")
		{}
	}
	
	public class JobTypeNotBlittableException : Exception
	{
		public JobTypeNotBlittableException(Type type) 
			: base($"Job type {type.Name} must be blittable!")
		{}
	}
}