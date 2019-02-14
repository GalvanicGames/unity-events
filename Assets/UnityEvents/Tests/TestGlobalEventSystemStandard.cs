using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEventsInternal;

namespace UnityEvents.Test
{
	public class TestGlobalEventSystemStandard
	{
		[TearDown]
		public void TearDown()
		{
			EventManager.ResetAll();
		}

		[UnityTest]
		public IEnumerator TestSimpleSubscribeAndEvent()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };
			
			GlobalEventSystem.Subscribe(callback);
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent());
			
			yield return new WaitForFixedUpdate();
			
			Assert.IsTrue(value == 1);
			
			GlobalEventSystem.Unsubscribe(callback);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleSubscribersAndEvent()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };
			Action<EvSimpleEvent> callback2 = x => { value += 2; };
			
			GlobalEventSystem.Subscribe(callback);
			GlobalEventSystem.Subscribe(callback2);
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent());
			
			yield return new WaitForFixedUpdate();
			
			Assert.IsTrue(value == 3);
			
			GlobalEventSystem.Unsubscribe(callback);
			GlobalEventSystem.Unsubscribe(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestTwoSubscribesOneUnsubscribeEvent()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };
			Action<EvSimpleEvent> callback2 = x => { value += 2; };
			
			GlobalEventSystem.Subscribe(callback);
			GlobalEventSystem.Subscribe(callback2);
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent());
			
			yield return new WaitForFixedUpdate();
			
			Assert.IsTrue(value == 3);
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent());
			GlobalEventSystem.Unsubscribe(callback);
			
			yield return new WaitForFixedUpdate();
			
			Assert.IsTrue(value == 5);
			GlobalEventSystem.Unsubscribe(callback2);

			EventManager.VerifyNoSubscribersAll();
		}

		[Test]
		public void TestLingeringSubscriber()
		{
			GlobalEventSystem.Subscribe<EvSimpleEvent>(ev => { });
			Assert.Throws<SubscriberStillListeningException<EvSimpleEvent, EvSimpleEvent>>(EventManager.VerifyNoSubscribersAll);
		}

		[UnityTest]
		public IEnumerator TestMultipleEvents()
		{
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };
			
			GlobalEventSystem.Subscribe(callback);
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent());
			GlobalEventSystem.SendEvent(new EvSimpleEvent());
			
			yield return new WaitForFixedUpdate();
			
			Assert.IsTrue(value == 2);
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent());
			yield return new WaitForFixedUpdate();
			
			Assert.IsTrue(value == 3);
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent());
			yield return new WaitForFixedUpdate();

			Assert.IsTrue(value == 4);
			
			GlobalEventSystem.Unsubscribe(callback);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleDifferentEvents()
		{
			int value1 = 0;
			int value2 = 0;
			
			Action<EvSimpleEvent> callback = x => { value1 += 1; };
			Action<EvSimpleEvent2> callback2 = x => { value2 += 2; };
			
			GlobalEventSystem.Subscribe(callback);
			GlobalEventSystem.Subscribe(callback2);
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent());
			
			yield return new WaitForFixedUpdate();
			
			Assert.IsTrue(value1 == 1);
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent2());
			yield return new WaitForFixedUpdate();
			
			Assert.IsTrue(value2 == 2);			
			GlobalEventSystem.Unsubscribe(callback);
			GlobalEventSystem.Unsubscribe(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
	}
}