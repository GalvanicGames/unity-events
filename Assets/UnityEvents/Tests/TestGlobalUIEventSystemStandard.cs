using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEventsInternal;

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

			GlobalUIEventSystem.Subscribe(callback);

			GlobalUIEventSystem.SendEvent(new EvSimpleEvent());

			yield return null;

			Assert.IsTrue(value == 1);

			GlobalUIEventSystem.Unsubscribe(callback);

			EventManager.VerifyNoSubscribersAll();
		}

		[UnityTest]
		public IEnumerator TestMultipleSubscribersAndEvent()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };
			Action<EvSimpleEvent> callback2 = x => { value += 2; };

			GlobalUIEventSystem.Subscribe(callback);
			GlobalUIEventSystem.Subscribe(callback2);

			GlobalUIEventSystem.SendEvent(new EvSimpleEvent());

			yield return null;

			Assert.IsTrue(value == 3);

			GlobalUIEventSystem.Unsubscribe(callback);
			GlobalUIEventSystem.Unsubscribe(callback2);

			EventManager.VerifyNoSubscribersAll();
		}

		[UnityTest]
		public IEnumerator TestTwoSubscribesOneUnsubscribeEvent()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };
			Action<EvSimpleEvent> callback2 = x => { value += 2; };

			GlobalUIEventSystem.Subscribe(callback);
			GlobalUIEventSystem.Subscribe(callback2);

			GlobalUIEventSystem.SendEvent(new EvSimpleEvent());

			yield return null;

			Assert.IsTrue(value == 3);

			GlobalUIEventSystem.SendEvent(new EvSimpleEvent());
			GlobalUIEventSystem.Unsubscribe(callback);

			yield return null;

			Assert.IsTrue(value == 5);
			GlobalUIEventSystem.Unsubscribe(callback2);

			EventManager.VerifyNoSubscribersAll();
		}

		[Test]
		public void TestLingeringSubscriber()
		{
			GlobalUIEventSystem.Subscribe<EvSimpleEvent>(ev => { });
			Assert.Throws<SubscriberStillListeningException<EvSimpleEvent, EvSimpleEvent>>(EventManager
				.VerifyNoSubscribersAll);
		}

		[UnityTest]
		public IEnumerator TestMultipleEvents()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };

			GlobalUIEventSystem.Subscribe(callback);

			GlobalUIEventSystem.SendEvent(new EvSimpleEvent());
			GlobalUIEventSystem.SendEvent(new EvSimpleEvent());

			yield return null;

			Assert.IsTrue(value == 2);

			GlobalUIEventSystem.SendEvent(new EvSimpleEvent());
			yield return null;

			Assert.IsTrue(value == 3);

			GlobalUIEventSystem.SendEvent(new EvSimpleEvent());
			yield return null;

			Assert.IsTrue(value == 4);

			GlobalUIEventSystem.Unsubscribe(callback);

			EventManager.VerifyNoSubscribersAll();
		}

		[UnityTest]
		public IEnumerator TestMultipleDifferentEvents()
		{
			int value1 = 0;
			int value2 = 0;

			Action<EvSimpleEvent> callback = x => { value1 += 1; };
			Action<EvSimpleEvent2> callback2 = x => { value2 += 2; };

			GlobalUIEventSystem.Subscribe(callback);
			GlobalUIEventSystem.Subscribe(callback2);

			GlobalUIEventSystem.SendEvent(new EvSimpleEvent());

			yield return null;

			Assert.IsTrue(value1 == 1);

			GlobalUIEventSystem.SendEvent(new EvSimpleEvent2());
			yield return null;

			Assert.IsTrue(value2 == 2);
			GlobalUIEventSystem.Unsubscribe(callback);
			GlobalUIEventSystem.Unsubscribe(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
	}
}