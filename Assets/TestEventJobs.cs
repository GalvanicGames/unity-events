using System;
using Unity.Burst;
using UnityEngine;

public class TestEventJobs : MonoBehaviour
{
	public int subscriberCount;

	[Space]
	public int loopAmount;

	public int amountToAdd;

	[Space]
	public bool runTest;

	private UnityEventJobSystem<TestJob, TestEvent> _system;
	private int _subscriberCount;

	private int _totalResult;
	private int _garbage;

	private void Start()
	{
		_system = new UnityEventJobSystem<TestJob, TestEvent>();
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
		if (subscriberCount != _subscriberCount)
		{
			_subscriberCount = subscriberCount;

			_system.Reset();


			EventEntity entity = new EventEntity(0);

			for (int i = 0; i < _subscriberCount; i++)
			{
				int index = i;

				Action<TestJob> onCompleteCallback = job =>
				{
					_totalResult += job.result;
					_garbage = index;
					
					Debug.Log("Complete: " + job.result);
				};

				_system.Subscribe(entity, new TestJob(),  onCompleteCallback);
				Debug.Log("Subscribe");
			}
		}

		if (runTest)
		{
			runTest = false;

			TestEvent ev = new TestEvent();
			ev.loopAmount = loopAmount;
			ev.amountToAdd = amountToAdd;
			
			_system.QueueEvent(new EventEntity(0), ev);
			_system.ProcessEvents();
			
			Debug.Log(_totalResult);
			_totalResult = 0;
		}
	}

	private struct TestEvent
	{
		public int loopAmount;
		public int amountToAdd;
	}

	[BurstCompile]
	private struct TestJob : IJobForEvent<TestEvent>
	{
		public int result;

		public void ExecuteEvent(TestEvent ev)
		{
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