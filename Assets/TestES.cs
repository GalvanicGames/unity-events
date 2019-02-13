using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

public class TestES : MonoBehaviour
{
	public bool doTest;

	private EventEntity _entity1;
	private EventEntity _entity2;
	
	private void Start()
	{
		_entity1 = EventEntity.CreateEntity();
		_entity2 = EventEntity.CreateEntity();
		
		EventManagerDO.Subscribe<EvEvent1>(_entity1, OnEvent1, EventUpdateType.FixedUpdate);
		EventManagerDO.Subscribe<EvEvent2>(_entity1, OnEvent2, EventUpdateType.FixedUpdate);
		EventManagerDO.Subscribe<EvEvent1>(_entity1, OnEvent12, EventUpdateType.FixedUpdate);
		EventManagerDO.Subscribe<EvEvent1>(_entity1, OnEvent1, EventUpdateType.FixedUpdate);
//		
		EventManagerDO.Subscribe<EvEvent2>(_entity2, OnEvent2, EventUpdateType.FixedUpdate);
		EventManagerDO.Subscribe<EvEvent1>(_entity2, OnEvent12, EventUpdateType.FixedUpdate);
		EventManagerDO.Subscribe<EvEvent1>(_entity2, OnEvent1, EventUpdateType.FixedUpdate);

	}
	
	// Start is called before the first frame update
	private void Update()
	{
		if (doTest)
		{
			doTest = false;
			
			EventManagerDO.SendEvent(_entity1, new EvEvent1(), EventUpdateType.FixedUpdate);
			EventManagerDO.SendEvent(_entity2, new EvEvent1(), EventUpdateType.FixedUpdate);
			EventManagerDO.SendEvent(_entity1, new EvEvent2(), EventUpdateType.FixedUpdate);
			EventManagerDO.SendEvent(_entity2, new EvEvent2(), EventUpdateType.FixedUpdate);
		}
	}

	private void OnEvent1(EvEvent1 ev)
	{
		Debug.Log("Event 1");
	}
	
	private void OnEvent12(EvEvent1 ev)
	{
		Debug.Log("Event 1-2");
	}
	
	private void OnEvent2(EvEvent2 ev)
	{
		Debug.Log("Event 2");
	}
}

public struct EvEvent1
{
	
}

public struct EvEvent2
{
	
}