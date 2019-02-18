using UnityEngine;
using UnityEvents.Internal;

namespace UnityEvents.Example
{
	/// <summary>
	/// Simple example of using jobs with the event system 
	/// </summary>
	public class ExampleSimpleJob : MonoBehaviour
	{
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

		private void OnEnable()
		{
			// Jobs work with the global simulation and global UI event systems as well as the GameObject system. This
			// will just show examples with the global simulation system.
			//
			// When an event is fired jobs will processed in parallel using the burst compiler. Can make otherwise
			// long tasks very short. Afterwards the callback functions are invoked so the listener can use the results
			// of the job.
			GlobalEventSystem.SubscribeWithJob<ExampleJob, EvExampleEvent>(new ExampleJob(), OnJobFinished);
		}

		private void OnDisable()
		{
			GlobalEventSystem.UnsubscribeWithJob<ExampleJob, EvExampleEvent>(OnJobFinished);
		}

		public void SendEvents()
		{
			// Job listeners trigger on events like anything else. You can have job listeners and regular listeners to
			// a single event.
			GlobalEventSystem.SendEvent(new EvExampleEvent(10));
		}

		private void OnJobFinished(ExampleJob ev)
		{
			Debug.Log("Job finished! Value: " + ev.result);
		}
	}
}
