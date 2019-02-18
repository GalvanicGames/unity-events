using UnityEngine;
using UnityEvents.Internal;

namespace UnityEvents.Example
{
	/// <summary>
	/// Example of using event handlers directly. Similar to UnityEventSystem but only works for a specific event.
	/// Can be used to only allow a single event.
	/// </summary>
	public class ExampleHandlers
	{
		private EventHandlerStandard<EvExampleEvent> _standardHandler = new EventHandlerStandard<EvExampleEvent>();
		private EventHandlerJob<ExampleJob, EvExampleEvent> _jobHandler = new EventHandlerJob<ExampleJob, EvExampleEvent>();
		
		// I have to be an unmanaged type! Need references? Use an id and have a lookup database system.
		private struct EvExampleEvent
		{
			public int exampleValue;

			public EvExampleEvent(int exampleValue)
			{
				this.exampleValue = exampleValue;
			}
		}
		
		private struct ExampleJob : IJobForEvent<EvExampleEvent>
		{
			// This result is stored across jobs, wipe it out at the beginning of each job if this isn't wanted!
			public int result;
			
			public void ExecuteEvent(EvExampleEvent ev)
			{
				result += ev.exampleValue;
			}
		}

		public void StandardHandlerExample()
		{
			// Handlers ultimately are what store the subscriptions and process events. Ideally they shouldn't need
			// to be used directly and TickEventSystem and UnityEventSystem would be sufficient in most cases.
			
			// See ExampleControlledEventSystem.cs for a description on EventEntities.
			EventTarget target = EventTarget.CreateTarget();
			
			// Subscribe, Unsubscribe, and Events will seem familiar but can ONLY use EvExampleEvent. Doesn't work for
			// all events.
			_standardHandler.Subscribe(target, OnExampleEvent);
			
			// Handlers have to be told to process events so we queue an event and process it later.
			_standardHandler.QueueEvent(target, new EvExampleEvent(7777));
			_standardHandler.ProcessEvents();
			
			_standardHandler.Unsubscribe(target, OnExampleEvent);
			
			// There's a job handler as well, they are separate and won't fire on the same events
			_jobHandler.Subscribe(target, new ExampleJob(), OnJobFinished);
			_jobHandler.QueueEvent(target, new EvExampleEvent(111));
			_jobHandler.ProcessEvents();
			_jobHandler.Unsubscribe(target, OnJobFinished);
			
		}
		
		private void OnExampleEvent(EvExampleEvent ev)
		{
			Debug.Log("Event received! Value: " + ev.exampleValue);
		}
		
		private void OnJobFinished(ExampleJob ev)
		{
			Debug.Log("Job finished! Value: " + ev.result);
		}
	}
}
