using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEvents.Internal;
using Object = UnityEngine.Object;

namespace UnityEvents.Test
{
	public class TestGameObjectEventSystemStandard
	{
		private GameObject _gameObject;

		[SetUp]
		public void SetUp()
		{
			_gameObject = new GameObject();
		}
		
		[TearDown]
		public void TearDown()
		{
			if (_gameObject != null)
			{
				Object.Destroy(_gameObject);
			}
			
			EventManager.ResetAll();
		}

		[UnityTest]
		public IEnumerator TestSimpleSubscribeAndEvent()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };
			
			_gameObject.Subscribe(callback);
			
			_gameObject.SendEvent(new EvSimpleEvent());
			
			yield return new WaitForFixedUpdate();
			
			Assert.IsTrue(value == 1);
			
			_gameObject.Unsubscribe(callback);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleSubscribersAndEvent()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };
			Action<EvSimpleEvent> callback2 = x => { value += 2; };
			
			_gameObject.Subscribe(callback);
			_gameObject.Subscribe(callback2);
			
			_gameObject.SendEvent(new EvSimpleEvent());
			
			yield return new WaitForFixedUpdate();
			
			Assert.IsTrue(value == 3);
			
			_gameObject.Unsubscribe(callback);
			_gameObject.Unsubscribe(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestTwoSubscribesOneUnsubscribeEvent()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };
			Action<EvSimpleEvent> callback2 = x => { value += 2; };
			
			_gameObject.Subscribe(callback);
			_gameObject.Subscribe(callback2);
			
			_gameObject.SendEvent(new EvSimpleEvent());
			
			yield return new WaitForFixedUpdate();
			
			Assert.IsTrue(value == 3);
			
			_gameObject.SendEvent(new EvSimpleEvent());
			_gameObject.Unsubscribe(callback);
			
			yield return new WaitForFixedUpdate();
			
			Assert.IsTrue(value == 5);
			_gameObject.Unsubscribe(callback2);

			EventManager.VerifyNoSubscribersAll();
		}

		[Test]
		public void TestLingeringSubscriber()
		{
			_gameObject.Subscribe<EvSimpleEvent>(ev => { });
			Assert.Throws<SubscriberStillListeningException<EvSimpleEvent, EvSimpleEvent>>(EventManager.VerifyNoSubscribersAll);
		}

		[UnityTest]
		public IEnumerator TestMultipleEvents()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };
			
			_gameObject.Subscribe(callback);
			
			_gameObject.SendEvent(new EvSimpleEvent());
			_gameObject.SendEvent(new EvSimpleEvent());
			
			yield return new WaitForFixedUpdate();
			
			Assert.IsTrue(value == 2);
			
			_gameObject.SendEvent(new EvSimpleEvent());
			yield return new WaitForFixedUpdate();
			
			Assert.IsTrue(value == 3);
			
			_gameObject.SendEvent(new EvSimpleEvent());
			yield return new WaitForFixedUpdate();

			Assert.IsTrue(value == 4);
			
			_gameObject.Unsubscribe(callback);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleDifferentEvents()
		{
			int value1 = 0;
			int value2 = 0;
			
			Action<EvSimpleEvent> callback = x => { value1 += 1; };
			Action<EvSimpleEvent2> callback2 = x => { value2 += 2; };
			
			_gameObject.Subscribe(callback);
			_gameObject.Subscribe(callback2);
			
			_gameObject.SendEvent(new EvSimpleEvent());
			
			yield return new WaitForFixedUpdate();
			
			Assert.IsTrue(value1 == 1);
			
			_gameObject.SendEvent(new EvSimpleEvent2());
			yield return new WaitForFixedUpdate();
			
			Assert.IsTrue(value2 == 2);			
			_gameObject.Unsubscribe(callback);
			_gameObject.Unsubscribe(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
	}
}