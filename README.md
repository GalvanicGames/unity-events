# Unity Events 2.0
=======================
An efficient code focused typed publisher/subscriber event system. Supports global event system and per object event systems that send deferred events to be processed at a later time. Allows regular callback events and multithreaded jobs that trigger on events.

####Obtain!####
[Releases](https://github.com/GalvanicGames/unity-events/releases)

If you'd like the most up to date version (which is the most cool), then pull the repo or download it [here](https://github.com/GalvanicGames/unity-events/archive/master.zip) and copy the files in Assets to your project's Assets folder.

## Setup

Once the Unity Events asset has been imported into the project then the event system is ready to be used.

## Simple Usage

### Events ###
Events in Unity Events are unmanaged structs that that are created by publishers and processed by subscribers.

```csharp
// This is the example struct that will be used throughout all the examples
public struct MyExampleEvent
{
	// Can pass ANY unmanaged information in these events!
	public int intValue;
	public float floatValue;

	public MyExampleEvent(int intValue, float floatValue)
	{
		this.intValue = intValue;
		this.floatValue = floatValue;
	}
}
```

As previously mentioned, Events HAVE to be unmanaged types. This means primitives and structs that only contain primitives. This is done to force "good" habits by only sending copyable data. Since events aren't processed immediately references can be stale or even point to "null" Unity objects. If a reference still needs to be sent (say to a GameObject or a ScriptableObject), create a look up database and send an ID in the event.

If an array/list needs to be sent then consider using something like [ValueTypeLists](https://gist.github.com/cjddmut/cb43af3ee191af78363f41a3188c0f7b).

### Global Event Systems ###

### Local Event Systems ###

## Event Systems

A Unity Events event system works fairly simply. It will maintain a list of subscribed functions to each strongly typed event. When an event is sent through a system, it will invoke each function that is listening for that event. There are two different kinds of event systems, global and local.

### Global Event System ###

The global event system is an event system that is considered special and not tied to any GameObject. Any class can subscribe, unsubscribe, or send an event through the global event system through the static class EventManager. The global event system is not tied to any GameObject and thus will persist through scenes. Subscribed classes are responsible for unsubscribing themselves.

### Local Event Systems ###

Local event systems are tied to GameObjects. Each GameObject can have its own event system that processes it's own subscribed functions and events. Sending an event to a GameObject's local event system will only invoke the functions that subscribed to that system. A GameObject's local event system is destroyed along with the GameObject.

## Standard Use

### Example Event ###

All events used by Unity Events are required to be structs. This allows creation of them on the fly without worrying about garbage generation. Because structs are being used as events, any data set to the struct can be set as the event. This does also mean any shallow modifying of the event by a subscribed function will not have any meaning to subsequent functions.

```csharp
// This is the example struct that will be used throughout all the examples
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
```
### Example Listener ###

A listener is a subscribed function that will be invoked when a specific event is sent through a system. The standard listener function has no return type and must accept the event struct as its only argument.

```csharp
private void ListenerFunction(MyExampleEvent ev)
{
	Debug.LogFormat(
		"ListenerFunction Invoked: Integer: {0} GameObject: {1} Was Global Event?: {2}",
		ev.anIntProperty,
		ev.anObjectProperty == null ? "null" : ev.anObjectProperty.name,
		ev.wasGlobal);
}
```

### Subscribing ###

A class must subscribe a listener to an event system in order for it to be invoked when an event occurs.

```csharp
// Examples of how to subscribe to global and local events. We are
// subscribing to the event MyExampleEvent which will invoke
// the function ListenerFunction when the global or local event
// is sent. Note that nothing prevents a function from subscribing
// multiple times.

// Global subscription
EventManager.Subscribe<MyExampleEvent>(ListenerFunction);

// Local subscription
gameObject.Subscribe<MyExampleEvent>(ListenerFunction);
```
### Unsubscribing ###

If a class does not want a function to be invoked anymore, it will unsubscribe the listener from the event system.

```chsarp
// Examples of how to unsubscribe to global and local events.
// Specifically it will unsubscribe the function ListenerFunction
// from the event MyExampleEvent. Unsubscribing is safe to do
// even if the function isn't subscribed.
//
// NOTE! If unsubscribe is called when an event is invoking then
// it will wait to unsubscribe until the event has finished
// invoking.

// Unsubscribe from global event.
EventManager.Unsubscribe<MyExampleEvent>(ListenerFunction);

// Unsubscribe from local event.
gameObject.Unsubscribe<MyExampleEvent>(ListenerFunction);
```

### Sending Events ###

Sending an event to invoke listeners is as simple as creating the struct, populating its values, and sending it to the event system.

```csharp
// Examples of how to send the global and local events. If
// subscribed it will invoke the function ListenerFunction.

// Send the global event.
EventManager.SendEvent(new MyExampleEvent(54, null, true));

// Send the local event.
gameObject.SendEvent(new MyExampleEvent(11, gameObject, false));

// Sends an event to the local event system of the GameObject
// and all it's children (and it's children's children ect..)
gameObject.SendEventDeep(new MyExampleEvent(8, gameObject, false));
```

### Event Send Modes ###

Optionally there are multiple different kinds of send modes for events that change when the listeners are invoked.

```csharp
public enum EventSendMode
{
	// Send functions using the default mode which can be set.
	Default,

	// Invoke the listeners immediately. This is the default if left unchanged.
	Immediate,

	// Invoke the listeners on the next fixed update.
	OnNextFixedUpdate,

	// Invoke the listeners at the end of this frame during LateUpdate().
	OnLateUpdate
}
```

The default sending mode can be set on EventManager.

```csharp
// Changes the default behavior of how events are sent. The starting
// default is EventSendMode.Immediate.
EventManager.defaultSendMode = EventSendMode.OnNextFixedUpdate;
```

Optionally it can be set during a specific SendEvent (for both global and local event systems).

```csharp
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
```

### Terminable Listeners ###

There is a second type of listener which is the terminable listener. This is a subscribed function that can stop all subsequent listeners from invoking. It is the same as the standard listener except it returns a bool to indicate if it should terminate or not.

```csharp
private bool ListenerFunctionTerminable(MyExampleEvent ev)
{
	Debug.LogFormat(
		"Listener Function Terminable Invoked: Integer: {0} GameObject: {1} Was Global Event?: {2}",
		ev.anIntProperty,
		ev.anObjectProperty == null ? "null" : ev.anObjectProperty.name,
		ev.wasGlobal);

	// We arbitrarily decide to terminate if the int property is < 5.
	return ev.anIntProperty < 5;
}
```

Subscribing and unsubscribing terminable listeners works very similarly to regular listeners.

```csharp
// Terminable functions allows an event to stop the processing of
// all events after it. This works for both global and local
// event systems.

// Subscription
EventManager.SubscribeTerminable<MyExampleEvent>(
	ListenerFunctionTerminable);

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
	ListenerFunctionTerminable);
```

### Other Functions ###

These are some other utility functions that can be used on global and local event systems.

**Resetting**

Resetting an event system means unsubscribing all listeners.

```csharp
// Resets global event system for specific event
EventManager.Reset<MyExampleEvent>();

// Reset all global events.
EventManager.ResetGlobal();

// Reset local event system
gameObject.ResetEventSystem();

// Reset all event systems everywhere
EventManager.ResetAll();
```

**Has Subscribers?**

Checks the system if the event has any active listeners.

```csharp
// Checks if the global event system has any subscribers 
// for the event.
bool hasSubscribers = EventManager.HasSubscribers<MyExampleEvent>();

// Checks if the local event system has any subscribers.
hasSubscribers = gameObject.HasSubscribers<MyExampleEvent>();
```

**Unsubscribe Target**

Unsubcribes all listeners from the event system who are functions of 'target'. This literally checks the function's target object.

```csharp
// Unsubscribes all functions associated with the object passed in.
EventManager.UnsubscribeTarget(this);
gameObject.UnsubscribeTarget(this);
```

**Initilization**

Local event systems are lazily initialized. This initilization includes creating components that are added to the GameObject. This isn't necessarily efficient. If the lazy initialization doesn't work in a specific case then it can be forced to initialize up front.

```csharp
gameObject.InitializeEventSystem();
```

## Event Attributes

Event attributes are a way to allow automatic subscription and unsubscription for a MonoBahvior when it is enabled and disabled. In order for these attributes to work, the component EventAttributeHandler must be present on the GameObject.

### GlobalEventListener ###

Automatically subscribe and unsubscribe a listener to the global event system when the MonoBehaviour is enabled and disabled.

```csharp
[GlobalEventListener]
private void GlobalAttrListener(MyExampleEvent ev)
{
	Debug.LogFormat(
		"Global Attribute Listener Invoked: Integer: {0} GameObject: {1} Was Global Event?: {2}",
		ev.anIntProperty,
		ev.anObjectProperty == null ? "null" : ev.anObjectProperty.name,
		ev.wasGlobal);
}
```

### LocalEventListener ###

Similarly, automatically subscribe and unsubscribe a listener to the MonoBehaviour's GameObject when it is enabled and disabled.

```csharp
[LocalEventListener]
private void OnLocalAttrListener(MyExampleEvent ev)
{
	Debug.LogFormat(
		"Local Attribute Event Listener: Integer: {0} GameObject: {1} Was Global Event?: {2}",
		ev.anIntProperty,
		ev.anObjectProperty == null ? "null" : ev.anObjectProperty.name,
		ev.wasGlobal);
}
```

### ParentCompEventListener ###

Alternatively, if you have a child MonoBheaviour that wants to subscribe to a parent's local event system. This attribute will search up the parents for a specific component and once found will automatically subscribe and unsubscribe when it is enabled and disabled. The example uses RigidBody component as an example, but this could be any custom component. Set the optional parameter skipSelf to true to ignore the current GameObject and immediately look at parent.

```csharp
[ParentCompEventListener(typeof(Rigidbody), skipSelf = true)]
private void OnParentCompAttrListener(MyExampleEvent ev)
{
	Debug.LogFormat(
		"Parent Component Attribute Listener Invoked: Integer: {0} GameObject: {1} Was Global Event?: {2}",
		ev.anIntProperty,
		ev.anObjectProperty == null ? "null" : ev.anObjectProperty.name,
		ev.wasGlobal);
}
```

## Event Handles

Event handles are an optional element of the event system that allows for more efficient interaction with the global and local event systems. This will allow the event systems to bypass various dictionary lookup costs and list searching but comes with the overhead of needing to maintain handles. If a ton of interactions are occuring with noticeable performance impact then these can help, otherwise they can be ignored.

### Subscription Handles ###

Subscription handles are relevant to both the gloval and local event systems. They allow subscription and unsubscription to be more efficient by avoiding list searching. Obtain a handle and use it to call subscribe and unsubscribe.

```csharp
// Global Events
SubscriptionHandle<MyExampleEvent> globalHandle = 
	EventManager.GetSubscriptionHandle<MyExampleEvent>(ListenerFunction);

SubscriptionHandle<MyExampleEvent> globalTerminableHandle =
	EventManager.GetSubscriptionHandleTerminable<MyExampleEvent>(ListenerFunctionTerminable);

globalHandle.Subscribe();
globalTerminableHandle.Subscribe();

EventManager.SendEvent(new MyExampleEvent(8777, null, true));

globalHandle.Unsubscribe();
globalTerminableHandle.Unsubscribe();

// Local Events
SubscriptionHandle<MyExampleEvent> localHandle =
	gameObject.GetSubscriptionHandle<MyExampleEvent>(ListenerFunction);

SubscriptionHandle<MyExampleEvent> localTerminableHandle =
	gameObject.GetSubscriptionHandleTerminable<MyExampleEvent>(ListenerFunctionTerminable);

localHandle.Subscribe();
localTerminableHandle.Subscribe();

gameObject.SendEvent(new MyExampleEvent(8777, null, true));

localHandle.Unsubscribe();
localTerminableHandle.Unsubscribe();
```

The event attributes use these handles underneath so subscriptions and unsubscriptions from the attibutes gain these perks.

### Event Handle ###

Using an event handle allows a local event system to avoid a dictionary lookup each time an event is sent. This does not apply to the
global event system.

```csharp
EventHandle<MyExampleEvent> eventHandle = gameObject.GetEventHandle<MyExampleEvent>();
eventHandle.SendEvent(new MyExampleEvent(-1, gameObject, false));
```
