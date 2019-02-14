using UnityEngine;

namespace UnityEvents.Example
{
	/// <summary>
	/// Example of using an event system that isn't controlled by any of the update ticks. Events will only be processed
	/// when told to be processed.
	///
	/// IF YOU ARE OK WITH EVENTS BEING PROCESSED IN FIXED UPDATE, UPDATE, OR LATE UPDATE then use TickEventSystem
	/// (example in ExampleCustomTickEventSystem.cs) as it will allow better subscription/event batching and will be more
	/// performant. ONLY IF you literally want to control when a system has it's events processed is when you would use
	/// the example in this file.
	/// </summary>
	public class ExampleCustomEventSystem
	{
		// This system will allow subscribers and events to be queued. Events have to be told when to process isntead
		// of happening automatically in a tick. The example will only show regular events but this can handle both.
		// See ExampleSimpleJob.cs for a job example. 
		private UnityEventSystem _system = new UnityEventSystem();

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
			// Event systems are associated with an EventTarget, this is how an event system can know who to send
			// an event to but keep all events together for performance.
			//
			// For example the global sim event system is its own EventTarget. As is the Global UI system and each
			// GameObject is converted to an EventTarget.
			//
			// It is better to use a single UnityEventSystem with multiple entities than an UnityEventSystem for each
			// target. For example if there are multiple 'global' systems that all get processed at the same time then
			// it is more performant to have a single UnityEventSystem and an EventTarget for each 'global system' that 
			// talk to the same UnityEventSystem.
			//
			// GlobalEventSystem, GameObject systems, and TickEventSystem all use EventManager which uses one UnityEventSystem
			// for each update tick.
			EventTarget target1 = EventTarget.CreateTarget();
			EventTarget target2 = EventTarget.CreateTarget();
			
			// Can subscribe a listener to both entities
			_system.Subscribe<EvExampleEvent>(target1, OnExampleEvent);
			_system.Subscribe<EvExampleEvent>(target2, OnExampleEventDoublePrint);
			
			// We queue up events, avoided the 'send' verb here since we have to manually process the events.
			_system.QueueEvent(target1, new EvExampleEvent(1));
			
			// Now we process the queued events. Since we only sent an event to the entity1 system, the listener to entity2
			// will NOT invoke.
			_system.ProcessEvents();
			
			// Each listener needs to unsubscribe from the appropriate event entity
			_system.Unsubscribe<EvExampleEvent>(target1, OnExampleEvent);
			_system.Unsubscribe<EvExampleEvent>(target2, OnExampleEventDoublePrint);
			
			// To have a global system that you control just store an EventEntity and use that.
		}

		private void OnExampleEvent(EvExampleEvent ev)
		{
			Debug.Log("Event received! Value: " + ev.exampleValue);
		}
		
		private void OnExampleEventDoublePrint(EvExampleEvent ev)
		{
			Debug.Log("Event received! Value: " + ev.exampleValue);
			Debug.Log("Event received! Value: " + ev.exampleValue);
		}
	}
}