using System.Collections.Generic;
using UnityEngine;
using UnityEvents;


namespace UnityEventsTest
{
	public class EventTest : MonoBehaviour
	{
		public int numOfEvents;

		private int _counter = 0;

		private System.Action<MyEvent> _func;
		private List<EventHandle<MyEvent>> _handles;

		public struct MyEvent
		{

		}

		// Use this for initialization
		void Start()
		{
			_func = TestEventFunc;
			_handles = new List<EventHandle<MyEvent>>(numOfEvents);
			EventManager.defaultSendMode = EventSendMode.OnNextFixedUpdate;
		}

		// Update is called once per frame
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.T))
			{
				Subscribe();
				SendEvent();
				_unsubscribe = true;
			}
		}

		private void SendEvent()
		{
			gameObject.SendEvent(new MyEvent(), _handles[0]);
		}

		private void Subscribe()
		{
			for (int i = 0; i < numOfEvents; i++)
			{
				if (i > 0)
				{
					_handles.Add(gameObject.Subscribe(_func, _handles[0]));
				}
				else
				{
					_handles.Add(gameObject.Subscribe(_func));
				}
			}
		}

		private void Unsubscribe()
		{
			for (int i = 0; i < _handles.Count; i++)
			{
				gameObject.Unsubscribe(_handles[i]);
			}

			_handles.Clear();
		}

		private bool _unsubscribe;

		private void TestEventFunc(MyEvent ev)
		{
			if (_unsubscribe)
			{
				Unsubscribe();
				_unsubscribe = false;
			}

			_counter++;
		}
	}
}
