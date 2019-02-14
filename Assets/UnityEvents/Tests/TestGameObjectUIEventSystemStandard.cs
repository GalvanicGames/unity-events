using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEventsInternal;
using Object = UnityEngine.Object;

namespace UnityEvents.Test
{
	public class TestGameObjectUIEventSystemStandard
	{
		private GameObject _gameObject;

		[SetUp]
		public void SetUp()
		{
			Time.timeScale = 0;
			_gameObject = new GameObject();
		}
		
		[TearDown]
		public void TearDown()
		{
			Time.timeScale = 1;
			
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
			
			_gameObject.SubscribeUI(callback);
			
			_gameObject.SendEventUI(new EvSimpleEvent());
			
			yield return null;
			
			Assert.IsTrue(value == 1);
			
			_gameObject.UnsubscribeUI(callback);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleSubscribersAndEvent()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };
			Action<EvSimpleEvent> callback2 = x => { value += 2; };
			
			_gameObject.SubscribeUI(callback);
			_gameObject.SubscribeUI(callback2);
			
			_gameObject.SendEventUI(new EvSimpleEvent());
			
			yield return null;
			
			Assert.IsTrue(value == 3);
			
			_gameObject.UnsubscribeUI(callback);
			_gameObject.UnsubscribeUI(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestTwoSubscribesOneUnsubscribeEvent()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };
			Action<EvSimpleEvent> callback2 = x => { value += 2; };
			
			_gameObject.SubscribeUI(callback);
			_gameObject.SubscribeUI(callback2);
			
			_gameObject.SendEventUI(new EvSimpleEvent());
			
			yield return null;
			
			Assert.IsTrue(value == 3);
			
			_gameObject.SendEventUI(new EvSimpleEvent());
			_gameObject.UnsubscribeUI(callback);
			
			yield return null;
			
			Assert.IsTrue(value == 5);
			_gameObject.UnsubscribeUI(callback2);

			EventManager.VerifyNoSubscribersAll();
		}

		[Test]
		public void TestLingeringSubscriber()
		{
			_gameObject.SubscribeUI<EvSimpleEvent>(ev => { });
			Assert.Throws<SubscriberStillListeningException<EvSimpleEvent, EvSimpleEvent>>(EventManager.VerifyNoSubscribersAll);
		}

		[UnityTest]
		public IEnumerator TestMultipleEvents()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };
			
			_gameObject.SubscribeUI(callback);
			
			_gameObject.SendEventUI(new EvSimpleEvent());
			_gameObject.SendEventUI(new EvSimpleEvent());
			
			yield return null;
			
			Assert.IsTrue(value == 2);
			
			_gameObject.SendEventUI(new EvSimpleEvent());
			yield return null;
			
			Assert.IsTrue(value == 3);
			
			_gameObject.SendEventUI(new EvSimpleEvent());
			yield return null;

			Assert.IsTrue(value == 4);
			
			_gameObject.UnsubscribeUI(callback);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleDifferentEvents()
		{
			int value1 = 0;
			int value2 = 0;
			
			Action<EvSimpleEvent> callback = x => { value1 += 1; };
			Action<EvSimpleEvent2> callback2 = x => { value2 += 2; };
			
			_gameObject.SubscribeUI(callback);
			_gameObject.SubscribeUI(callback2);
			
			_gameObject.SendEventUI(new EvSimpleEvent());
			
			yield return null;
			
			Assert.IsTrue(value1 == 1);
			
			_gameObject.SendEventUI(new EvSimpleEvent2());
			yield return null;
			
			Assert.IsTrue(value2 == 2);			
			_gameObject.UnsubscribeUI(callback);
			_gameObject.UnsubscribeUI(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
	}
}