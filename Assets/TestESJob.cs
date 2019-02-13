using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class TestESJob : MonoBehaviour
{
	public int testCount;

	public bool test;
	public bool newEntity;
	
	private static int _testCount;

	private CustomSampler _samplerSubscribe;
	private CustomSampler _samplerUnsubscribe;
	private EventEntity _entity;
	
	private UnityEventSystemDOP<TestEvent> _system;

	private Action<TestEvent> _callback;

	private void Start()
	{
		_system = new UnityEventSystemDOP<TestEvent>();
		_samplerSubscribe = CustomSampler.Create("Subscribe");
		_samplerUnsubscribe = CustomSampler.Create("Unsubscribe");
		
		_entity = EventEntity.CreateEntity();
		_callback = OnEvent;
		
		_system.Subscribe(_entity, x => _testCount++);
	}

	private void OnDestroy()
	{
		if (_system != null)
		{
			_system.Dispose();
		}
	}

	private void Update()
	{
		if (test)
		{
			test = !test;

			for (int i = 0; i < testCount; i++)
			{
				_system.QueueEvent(_entity, new TestEvent());
			}
			
			_system.ProcessEvents();
			
			Debug.Log(_testCount);

			_testCount = 0;
		}
	}

	private void OnEvent(TestEvent ev)
	{
		
	}

	private struct TestEvent
	{
		
	}
}