using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class CompareDifferentImps : MonoBehaviour
{
	public int numberOfEntities;
	public int numberOfSubscribers;
	public int numberOfEvents;

	public bool sendEvent;

	private int _numberOfSubscribers;
	private int _numberOfEntities;

	private int _testValue;
	private int _garbage;
	
	private UnityEventSystemDOP<CompareEvent> _system;

	private CustomSampler _standardProcessSampler;
	private CustomSampler _processQueueSampler;
	private CustomSampler _processMapParallelSampler;
	
	// Start is called before the first frame update
	private void Start()
	{
		_system = new UnityEventSystemDOP<CompareEvent>();
		
		_standardProcessSampler = CustomSampler.Create("Standard Process");
		_processQueueSampler = CustomSampler.Create("Process Queue");
		_processMapParallelSampler = CustomSampler.Create("Process Map Parallel");
	}

	private void OnDestroy()
	{
		if (_system != null)
		{
			_system.Dispose();
		}
	}

	// Update is called once per frame
	private void Update()
	{
		if (numberOfSubscribers != _numberOfSubscribers ||
			_numberOfEntities != numberOfEntities)
		{
			_numberOfSubscribers = numberOfSubscribers;
			_numberOfEntities = numberOfEntities;
			
			_system.Reset();

			for (int i = 0; i < _numberOfEntities; i++)
			{
				EventEntity entity = new EventEntity((uint) i);
				
				for (int j = 0; j < _numberOfSubscribers; j++)
				{
					int index = j;
					
					Action<CompareEvent> callback = ev =>
					{
						_testValue++;
						_garbage = index;
					};
						
					_system.Subscribe(entity, callback);
				}
			}
		}
		
		if (sendEvent)
		{
			sendEvent = false;

			for (int i = 0; i < _numberOfEntities; i++)
			{
				EventEntity entity = new EventEntity((uint) i);

				for (int j = 0; j < numberOfEvents; j++)
				{
					_system.QueueEvent(entity, new CompareEvent(1f, 2f, 3f));
				}
			}

			_standardProcessSampler.Begin();
			_system.ProcessEvents();
			_standardProcessSampler.End();
			
			Debug.Log("Standard Result: " + _testValue);
			_testValue = 0;
			
//			_processQueueSampler.Begin();
//			_system.ProcessEvents2();
//			_processQueueSampler.End();
//			
//			Debug.Log("Queue Result: " + _testValue);
//			_testValue = 0;
//						
//			_system.ResetEvents();
		}
	}
	
	private void OnCompareEvent(CompareEvent ev)
	{
		float f4 = ev.f1 + ev.f2 + ev.f3;
	}
}