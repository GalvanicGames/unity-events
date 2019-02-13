using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEvents;

public class CompareSystems : MonoBehaviour
{
	public int numberOfSubscribers;
	public int numberOfEvents;

	public bool sendEvent;

	private int _numberOfSubscribers;
	private EventEntity _entity;

	private CustomSampler _subscribeOldSampler;
	private CustomSampler _subscribeNewSampler;

	private CustomSampler _sendOldSampler;
	private CustomSampler _sendNewSampler;
	
	private void Start()
	{
		_subscribeOldSampler = CustomSampler.Create("Subscribe (Old)");
		_subscribeNewSampler = CustomSampler.Create("Subscribe (New)");

		_sendOldSampler = CustomSampler.Create("Send Event (Old)");
		_sendNewSampler = CustomSampler.Create("Send Event (New)");

		EventManager.defaultSendMode = EventSendMode.OnNextFixedUpdate;
	}

	private void Update()
	{
		if (_numberOfSubscribers != numberOfSubscribers)
		{
			_numberOfSubscribers = numberOfSubscribers;

			EventManager.ResetAll();
			EventManagerDO.ResetAll();
			
			List<Action<CompareEvent>> callbacks = new List<Action<CompareEvent>>();

			for (int i = 0; i < _numberOfSubscribers; i++)
			{
				int index = i;
				callbacks.Add(ev => { float f4 = ev.f1 + ev.f2 + ev.f3 + index;});
			}

			_subscribeOldSampler.Begin();
			
			for (int i = 0; i < _numberOfSubscribers; i++)
			{
				EventManager.Subscribe<CompareEvent>(callbacks[i]);
			}
			
			_subscribeOldSampler.End();

			_entity = EventEntity.CreateEntity();
			
			_subscribeNewSampler.Begin();

			for (int i = 0; i < _numberOfSubscribers; i++)
			{
				GlobalSimEventSystem.Subscribe<CompareEvent>(callbacks[i]);
			}
			
			_subscribeNewSampler.End();
		}

		if (sendEvent)
		{
			sendEvent = false;
			_sendOldSampler.Begin();
		
			for (int i = 0; i < numberOfEvents; i++)
			{
				EventManager.SendEvent(new CompareEvent(1f, 2f, 3f));
			}
		
			_sendOldSampler.End();
		
			_sendNewSampler.Begin();

			for (int i = 0; i < numberOfEvents; i++)
			{
				GlobalSimEventSystem.SendEvent(new CompareEvent(1f, 2f, 3f));
			}
		
			_sendNewSampler.End();
		}
	}

	private void OnCompareEvent(CompareEvent ev)
	{
		float f4 = ev.f1 + ev.f2 + ev.f3;
	}
}

public struct CompareEvent
{
	public float f1;
	public float f2;
	public float f3;

	public CompareEvent(float f1, float f2, float f3)
	{
		this.f1 = f1;
		this.f2 = f2;
		this.f3 = f3;
	}
}