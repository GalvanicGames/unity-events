using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Profiling;

public class CompareJobSysVsReg : MonoBehaviour
{
	public int numberOfEntities;
	public int numberOfSubscribers;

	public bool sendEvent;

	[Space]
	public int loopAmount;

	private int _numberOfSubscribers;
	private int _numberOfEntities;

	private int _garbage;

	private int _resultStandard;
	private int _resultJobs;
	
	private UnityEventSystemDOP<TestEvent> _standardSystem;
	private UnityEventJobSystem<TestJob, TestEvent> _jobsSystem;

	private CustomSampler _standardSampler;
	private CustomSampler _jobsSampler;
	
	// Start is called before the first frame update
	private void Start()
	{
		_standardSystem = new UnityEventSystemDOP<TestEvent>();
		_jobsSystem = new UnityEventJobSystem<TestJob, TestEvent>();
		
		_standardSampler = CustomSampler.Create("Standard Process");
		_jobsSampler = CustomSampler.Create("Jobs Process");
	}

	private void OnDestroy()
	{
		if (_standardSystem != null)
		{
			_standardSystem.Dispose();
			_jobsSystem.Dispose();
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
			
			_standardSystem.Reset();
			_jobsSystem.Reset();

			_resultStandard = 0;
			_resultJobs = 0;

			for (int i = 0; i < _numberOfEntities; i++)
			{
				EventEntity entity = new EventEntity((uint) i);
				
				for (int j = 0; j < _numberOfSubscribers; j++)
				{
					int index = j;
					
					Action<TestEvent> callback = ev =>
					{
						_garbage = index;
						
						unchecked
						{
							for (int k = 0; k < ev.loopAmount; k++)
							{
								_resultStandard += ev.amountToAdd;
							}
						}
					};
						
					_standardSystem.Subscribe(entity, callback);
					
					_jobsSystem.Subscribe(
						entity, 
						new TestJob(),
						ev =>
						{
							_garbage = index;
							_resultJobs += ev.result;
						});
				}
			}
		}
		
		if (sendEvent)
		{
			sendEvent = false;

			for (int i = 0; i < _numberOfEntities; i++)
			{
				EventEntity entity = new EventEntity((uint) i);
				_standardSystem.QueueEvent(entity, new TestEvent(loopAmount, 1));
				_jobsSystem.QueueEvent(entity, new TestEvent(loopAmount, 1));
			}

			_standardSampler.Begin();
			_standardSystem.ProcessEvents();
			_standardSampler.End();
			
			_jobsSampler.Begin();
			_jobsSystem.ProcessEvents();
			_jobsSampler.End();
			
			Debug.Log(_resultStandard + " " + _resultJobs);
		}
	}

	private struct TestEvent
	{
		public int loopAmount;
		public int amountToAdd;

		public TestEvent(int loopAmount, int amountToAdd)
		{
			this.loopAmount = loopAmount;
			this.amountToAdd = amountToAdd;
		}
	}
	
	private struct TestJob : IJobForEvent<TestEvent>
	{
		public int result;

		public void ExecuteEvent(TestEvent ev)
		{
			result = 0;
			
			unchecked
			{
				for (int i = 0; i < ev.loopAmount; i++)
				{
					result += ev.amountToAdd;
				}
			}
		}
	}
}
