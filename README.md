# Unity Events 2.0 #
A performant code focused strongly typed publisher/subscriber event system. Supports global event system and per GameObject event systems that send deferred events to be processed at a later time. Allows regular callback events and multithreaded jobs that trigger on events.

Custom event system can be created for more control if the supplied ones aren't sufficient.

Uses Unity's new Job System and the burst compiler. Both of those features are considered in preview and experimental. Use at your own risk!

#### Obtain! ####
[Releases](https://github.com/GalvanicGames/unity-events/releases)

If you'd like the most up to date version (which is the most cool), then pull the repo or download it [here](https://github.com/GalvanicGames/unity-events/archive/master.zip) and copy the files in Assets to your project's Assets folder.

## Setup
Once the Unity Events asset has been imported into the project then the event system is ready to be used.

## Examples
There are multiple [simple]() and [advanced]() examples in the repository and can be looked at for guidance.

As a simple example here is how an event can be sent to a Global event system and a GameObject's local event system.
```csharp
// I have to be an unmanaged type! Need references? Use an id and have a lookup database system.
private struct EvExampleEvent
{
  public int exampleValue;

  public EvExampleEvent(int exampleValue)
  {
    this.exampleValue = exampleValue;
  }
}

// The callback that will be invoked on an event
private void OnExampleEvent(EvExampleEvent ev)
{
  Debug.Log("Event received! Value: " + ev.exampleValue);
}

private void OnEnable()
{
  // Subscribes to the global event system, handles events in FixedUpdate
  GlobalEventSystem.Subscribe<EvExampleEvent>(OnExampleEvent);

  // Subscribes to THIS GameObject's event system! Also Fixed Update
  gameObject.Subscribe<EvExampleEvent>(OnExampleEvent);
}

public void SendEvents()
{
  // Send an event to the global event system, will be processed in the next FixedUpdate
  GlobalEventSystem.SendEvent(new EvExampleEvent(10));

  // Send an event to a specific GameObject, only listeners subscribed to that gameobject will get
  // this event. Also will be processed in the next FixedUpdate
  gameObject.SendEvent(new EvExampleEvent(99));
}

```

## Dropped Features
Unity Events 2.0 was rebuilt with performance and flexibility more in mind. Because of this some features of the original version of Unity Events have been dropped. If these features are important than the previous version of Unity Events can be found [here]().
