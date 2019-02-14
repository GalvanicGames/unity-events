using UnityEngine;

namespace UnityEvents.Example
{
	/// <summary>
	/// Example of using a custom event system that send events on an update tick. This could be used to create custom
	/// local event system and custom global event system. GlobalEventSystem is a static class that holds a tick
	/// based event system for simulation and ui and passes subscriptions/events through it.
	/// </summary>
	public class ExampleCustomTickEventSystem
	{
		// This system will process events in the Update tick. The example will only show regular events but this can
		// handle both. See ExampleSimpleJob.cs for a job example.
		private TickEventSystem _system = new TickEventSystem(EventUpdateTick.Update);

		// I have to be an unmanaged type! Need references? Use an id and have a lookup database system.
		private struct EvExampleEvent
		{
			public int exampleValue;

			public EvExampleEvent(int exampleValue)
			{
				this.exampleValue = exampleValue;
			}
		}
		
		public void UseCustomSystem()
		{
			// Subscribe to the custom event system
			_system.Subscribe<EvExampleEvent>(OnExampleEvent);
			
			// Can send an event to it. This will be processed in the Update loop.
			_system.SendEvent(new EvExampleEvent(10));
			
			// And unsubscribe from it
			_system.Unsubscribe<EvExampleEvent>(OnExampleEvent);
		}

		private void OnExampleEvent(EvExampleEvent ev)
		{
			Debug.Log("Event received! Value: " + ev.exampleValue);
		}
	}
}
