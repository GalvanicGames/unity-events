using UnityEngine;

namespace UnityEvents.Example
{
	/// <summary>
	/// Example of using an event system that isn't controlled by any of the update ticks. Events will only be processed
	/// when told to be processed.
	/// </summary>
	public class ExampleControlledEventSystem
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
			// Event systems are associated with an EventEntity, this is how an event system can know who to send
			// and event to but keep all events together for performance.
			//
			// For example the global sim event system is its own EventEntity. As is the Global UI system and each
			// GameObject is converted to an EventEntity.
			//
			// It is better to use a single UnityEventSystem with multiple entities than an UnityEventSystem for each
			// entity. For example if there are multiple 'global' systems that all get processed at the same time then
			// it is more performant to have a since UnityEventSystem and an EventEntity for each 'global system' that 
			// talk to the same UnityEventSystem.
			//
			// GlobalEventSystem, GameObject systems, and TickEventSystem uses EventManager which uses one UnityEventSystem
			// for each update tick.
			EventEntity entity1 = EventEntity.CreateEntity();
			EventEntity entity2 = EventEntity.CreateEntity();
			
			// Can subscribe a listener to both entities
			_system.Subscribe<EvExampleEvent>(entity1, OnExampleEvent);
			_system.Subscribe<EvExampleEvent>(entity2, OnExampleEventDoublePrint);
			
			// We queue up events, avoid the send verb here since we have to manually process the events.
			_system.QueueEvent(entity1, new EvExampleEvent(1));
			
			// Now we process the queued events. We only sent an event to the entity1 system, the listener to entity2
			// will NOT invoke.
			_system.ProcessEvents();
			
			// Each listener needs to unsubscribe from the appropriate event entity
			_system.Unsubscribe<EvExampleEvent>(entity1, OnExampleEvent);
			_system.Unsubscribe<EvExampleEvent>(entity2, OnExampleEventDoublePrint);
			
			// To have a global system that you control just store an EventEntity and just use that.
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