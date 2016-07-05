using UnityEngine;
using UnityEvents;

public class Examples : MonoBehaviour
{
	public struct MyExampleEvent
	{
		// Can pass ANY information in these events!
		public int anIntProperty;
		public GameObject anObjectProperty;
		public bool wasGlobal;

		public MyExampleEvent(int anInt, GameObject anObj, bool wasGlobal)
		{
			anIntProperty = anInt;
			anObjectProperty = anObj;
			this.wasGlobal = wasGlobal;
		}
	}

	//
	// Standard Event Usage
	//

	public void Subscribe()
	{
		// Examples of how to subscribe to global and local events. We are
		// subscribing to the event MyExampleEvent which will invoke
		// the function OnStandardEvent when the global or local event
		// is sent. Note that nothing prevents a function from subscribing
		// multiple times.

		// Global subscription
		EventManager.Subscribe<MyExampleEvent>(OnStandardEvent);

		// Local subscription
		gameObject.Subscribe<MyExampleEvent>(OnStandardEvent);
	}

	public void Unsubscribe()
	{
		// Examples of how to unsubscribe to global and local events.
		// Specifically it will unsubscribe the function OnStandardEvent
		// from the event MyExampleEvent. Unsubscribing is safe to do
		// even if the function isn't subscribed.

		// Unsubscribe from global event.
		EventManager.Unsubscribe<MyExampleEvent>(OnStandardEvent);

		// Unsubscribe from local event.
		gameObject.Unsubscribe<MyExampleEvent>(OnStandardEvent);
}

	public void SendEvents()
	{
		// Examples of how to send the global and local events. If
		// subscribed it will invoke the function OnStandardEvent.

		// Send the global event.
		EventManager.SendEvent(new MyExampleEvent(54, null, true));

		// Send the local event.
		gameObject.SendEvent(new MyExampleEvent(11, gameObject, false));

		// Sends an event to the local event system of the GameObject
		// and all it's children (and it's children's children ect..)
		gameObject.SendEventDeep(new MyExampleEvent(8, gameObject, false));
	}

	private void OnStandardEvent(MyExampleEvent ev)
	{
		Debug.LogFormat(
			"Standard Event Triggered: Integer: {0} GameObject: {1} Was Global Event?: {2}",
			ev.anIntProperty,
			ev.anObjectProperty == null ? "null" : ev.anObjectProperty.name,
			ev.wasGlobal);
	}

	//
	// Event Send Modes
	//

	public void SendEventsWithEventMode()
	{
		// Sends the event immediately. All the functions will invoke during this call.
		// This is the default mode.
		EventManager.SendEvent(
			new MyExampleEvent(3, null, true),
			EventSendMode.Immediate);

		// Queues the event to trigger during the next fixed update tick.
		EventManager.SendEvent(
			new MyExampleEvent(5, null, true),
			EventSendMode.OnNextFixedUpdate);

		// Queues the event to trigger at the end of this frame during
		// LateUpdate. This is useful for events that fire during OnEnable/OnDisable
		// that might alter the active of a GameObject.
		EventManager.SendEvent(
			new MyExampleEvent(5, null, true),
			EventSendMode.OnLateUpdate);
	}

	public void ChangeDefaultSendModeToFixedUpdate()
	{
		// Changes the default behavior of how events are sent. The starting
		// default is EventSendMode.Immediate.
		EventManager.defaultSendMode = EventSendMode.OnNextFixedUpdate;
	}

	//
	// Terminable Functions
	//

	public void TerminableFunctions()
	{
		// Terminable functions allows an event to stop the processing of
		// all events after it. This works for both global and local
		// event systems.

		// Subscription
		EventManager.SubscribeTerminable<MyExampleEvent>(
			OnStandardEventTerminable);

		// Sending the event
		if (EventManager.SendEvent(new MyExampleEvent(2, null, true)))
		{
			// Send Event returned true? We terminated. Note we can only
			// know this if our send mode was Immediate.
			Debug.Log("Event terminated early!");
		}
		else
		{
			Debug.Log("Event didn't terminate, all functions were invoked!");
		}

		// Unsubscription
		EventManager.UnsubscribeTerminable<MyExampleEvent>(
			OnStandardEventTerminable);
	}

	private bool OnStandardEventTerminable(MyExampleEvent ev)
	{
		Debug.LogFormat(
			"Standard Event Triggered: Integer: {0} GameObject: {1} Was Global Event?: {2}",
			ev.anIntProperty,
			ev.anObjectProperty == null ? "null" : ev.anObjectProperty.name,
			ev.wasGlobal);

		// We arbitrarily decide to terminate if the int property is < 5.
		return ev.anIntProperty < 5;
	}

	//
	// Other Functions 
	//

	public void Reset()
	{
		// Reseting unsubscribes all functions from an event.

		// Resets global event system for specific event
		EventManager.Reset<MyExampleEvent>();

		// Reset all global events.
		EventManager.ResetGlobal();

		// Reset local event system
		gameObject.ResetEventSystem();

		// Reset all event systems everywhere
		EventManager.ResetAll();
	}

	public void HasSubscribers()
	{
		// Checks if the global event system has any subscribers 
		// for the event.
		bool hasSubscribers = EventManager.HasSubscribers<MyExampleEvent>();

		// Checks if the local event system has any subscribers.
		hasSubscribers = gameObject.HasSubscribers<MyExampleEvent>();
	}

	public void Initialization()
	{
		// Local event systems are initialized lazily so this isn't necessary
		// but setting up a local event system is costly and generates
		// garbage. Use this function if early initialization is desired.
		gameObject.InitializeEventSystem();
	}

	//
	// Event Attributes
	//

	// Placing these attributes on a function will automatically subscribe
	// the function to the event when the GameObject is enabled and 
	// automatically unsubscribe the function when the GameObject is
	// disabled. For these attributes to work the EventAttributeHandler
	// component must be on the GameObject.

	// Marks this function to automatically subscribe and unsubscribe from
	// the global event system when the GameObject is enabled and disabled.
	[GlobalEventListener]
	private void OnGlobalAttrEvent(MyExampleEvent ev)
	{
		Debug.LogFormat(
			"Global Attribute Event Triggered: Integer: {0} GameObject: {1} Was Global Event?: {2}",
			ev.anIntProperty,
			ev.anObjectProperty == null ? "null" : ev.anObjectProperty.name,
			ev.wasGlobal);
	}

	// Marks this function to automatically subscribe and unsubscribe from
	// the local event system when the GameObject is enabled and disabled.
	[LocalEventListener]
	private void OnLocalAttrEvent(MyExampleEvent ev)
	{
		Debug.LogFormat(
			"Local Attribute Event Triggered: Integer: {0} GameObject: {1} Was Global Event?: {2}",
			ev.anIntProperty,
			ev.anObjectProperty == null ? "null" : ev.anObjectProperty.name,
			ev.wasGlobal);
	}

	// Instead of attaching to the local event, functions with the 
	// ParentCompEventListener will walk up the parent tree until it
	// finds a parent with the specified component and subscribe
	// to its local event system.

	[ParentCompEventListener(typeof(Rigidbody), skipSelf = true)]
	private void OnParentCompAttrEvent(MyExampleEvent ev)
	{
		Debug.LogFormat(
			"Parent Component Attribute Event Triggered: Integer: {0} GameObject: {1} Was Global Event?: {2}",
			ev.anIntProperty,
			ev.anObjectProperty == null ? "null" : ev.anObjectProperty.name,
			ev.wasGlobal);
	}

	//
	// Handlers
	//

	public void UseSubscriptionHandles()
	{
		// Subscription handles make subscribing and unsubscribing more
		// efficient (avoids list searching). They are completely optional
		// and in most cases probably not worth the headache of maintaining.
		// But if a lot of subscriptions and unsubscriptions are occurring
		// then they may be desired. 

		// Global Events
		SubscriptionHandle<MyExampleEvent> globalHandle = 
			EventManager.GetSubscriptionHandle<MyExampleEvent>(OnStandardEvent);

		SubscriptionHandle<MyExampleEvent> globalTerminableHandle =
			EventManager.GetSubscriptionHandleTerminable<MyExampleEvent>(OnStandardEventTerminable);

		globalHandle.Subscribe();
		globalTerminableHandle.Subscribe();

		EventManager.SendEvent(new MyExampleEvent(8777, null, true));

		globalHandle.Unsubscribe();
		globalTerminableHandle.Unsubscribe();

		// Local Events
		SubscriptionHandle<MyExampleEvent> localHandle =
			gameObject.GetSubscriptionHandle<MyExampleEvent>(OnStandardEvent);

		SubscriptionHandle<MyExampleEvent> localTerminableHandle =
			gameObject.GetSubscriptionHandleTerminable<MyExampleEvent>(OnStandardEventTerminable);

		localHandle.Subscribe();
		localTerminableHandle.Subscribe();

		gameObject.SendEvent(new MyExampleEvent(8777, null, true));

		localHandle.Unsubscribe();
		localTerminableHandle.Unsubscribe();
	}

	public void UseLocalEventSendHandle()
	{
		// The local event system uses a dictionary to maintain all the
		// events. Using the EventHandle allows the system to avoid the
		// dictionary look up which can be nice if a large number of events
		// are being sent. This handle is also optional to use.

		EventHandle<MyExampleEvent> eventHandle = gameObject.GetEventHandle<MyExampleEvent>();
		eventHandle.SendEvent(new MyExampleEvent(-1, gameObject, false));
	}
}
