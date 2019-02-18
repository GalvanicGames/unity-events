using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEvents.Internal;

namespace UnityEvents.Test
{
	public class TestGlobalUIEventSystemStandard
	{
		[SetUp]
		public void Setup()
		{
			Time.timeScale = 0;
		}

		[TearDown]
		public void TearDown()
		{
			Time.timeScale = 1;
			EventManager.ResetAll();
		}

		[UnityTest]
		public IEnumerator TestSimpleSubscribeAndEvent()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };

			GlobalEventSystem.SubscribeUI(callback);

			GlobalEventSystem.SendEventUI(new EvSimpleEvent());

			yield return null;

			Assert.IsTrue(value == 1);

			GlobalEventSystem.UnsubscribeUI(callback);

			EventManager.VerifyNoSubscribersAll();
		}

		[UnityTest]
		public IEnumerator TestMultipleSubscribersAndEvent()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };
			Action<EvSimpleEvent> callback2 = x => { value += 2; };

			GlobalEventSystem.SubscribeUI(callback);
			GlobalEventSystem.SubscribeUI(callback2);

			GlobalEventSystem.SendEventUI(new EvSimpleEvent());

			yield return null;

			Assert.IsTrue(value == 3);

			GlobalEventSystem.UnsubscribeUI(callback);
			GlobalEventSystem.UnsubscribeUI(callback2);

			EventManager.VerifyNoSubscribersAll();
		}

		[UnityTest]
		public IEnumerator TestTwoSubscribesOneUnsubscribeEvent()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };
			Action<EvSimpleEvent> callback2 = x => { value += 2; };

			GlobalEventSystem.SubscribeUI(callback);
			GlobalEventSystem.SubscribeUI(callback2);

			GlobalEventSystem.SendEventUI(new EvSimpleEvent());

			yield return null;

			Assert.IsTrue(value == 3);

			GlobalEventSystem.SendEventUI(new EvSimpleEvent());
			GlobalEventSystem.UnsubscribeUI(callback);

			yield return null;

			Assert.IsTrue(value == 5);
			GlobalEventSystem.UnsubscribeUI(callback2);

			EventManager.VerifyNoSubscribersAll();
		}

		[Test]
		public void TestLingeringSubscriber()
		{
			GlobalEventSystem.SubscribeUI<EvSimpleEvent>(ev => { });
			Assert.Throws<SubscriberStillListeningException<EvSimpleEvent, EvSimpleEvent>>(EventManager
				.VerifyNoSubscribersAll);
		}

		[UnityTest]
		public IEnumerator TestMultipleEvents()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };

			GlobalEventSystem.SubscribeUI(callback);

			GlobalEventSystem.SendEventUI(new EvSimpleEvent());
			GlobalEventSystem.SendEventUI(new EvSimpleEvent());

			yield return null;

			Assert.IsTrue(value == 2);

			GlobalEventSystem.SendEventUI(new EvSimpleEvent());
			yield return null;

			Assert.IsTrue(value == 3);

			GlobalEventSystem.SendEventUI(new EvSimpleEvent());
			yield return null;

			Assert.IsTrue(value == 4);

			GlobalEventSystem.UnsubscribeUI(callback);

			EventManager.VerifyNoSubscribersAll();
		}

		[UnityTest]
		public IEnumerator TestMultipleDifferentEvents()
		{
			int value1 = 0;
			int value2 = 0;

			Action<EvSimpleEvent> callback = x => { value1 += 1; };
			Action<EvSimpleEvent2> callback2 = x => { value2 += 2; };

			GlobalEventSystem.SubscribeUI(callback);
			GlobalEventSystem.SubscribeUI(callback2);

			GlobalEventSystem.SendEventUI(new EvSimpleEvent());

			yield return null;

			Assert.IsTrue(value1 == 1);

			GlobalEventSystem.SendEventUI(new EvSimpleEvent2());
			yield return null;

			Assert.IsTrue(value2 == 2);
			GlobalEventSystem.UnsubscribeUI(callback);
			GlobalEventSystem.UnsubscribeUI(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
	}
}