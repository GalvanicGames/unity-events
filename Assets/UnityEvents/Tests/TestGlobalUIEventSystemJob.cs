using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEventsInternal;

namespace UnityEvents.Test
{
	public class TestGlobalUIEventSystemJob
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
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			
			GlobalEventSystem.SubscribeUIWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			
			GlobalEventSystem.SendEventUI(new EvSimpleEvent(10));
			
			yield return null;
			
			GlobalEventSystem.UnsubscribeUIWithJob<TestJob, EvSimpleEvent>(callback);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleSubscribersAndEvent()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			Action<TestJob> callback2 = x => { Assert.IsTrue(x.result == 10); };
			
			GlobalEventSystem.SubscribeUIWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			GlobalEventSystem.SubscribeUIWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback2);
			
			GlobalEventSystem.SendEventUI(new EvSimpleEvent(10));
			
			yield return null;
			
			GlobalEventSystem.UnsubscribeUIWithJob<TestJob, EvSimpleEvent>(callback);
			GlobalEventSystem.UnsubscribeUIWithJob<TestJob, EvSimpleEvent>(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestTwoSubscribesOneUnsubscribeEvent()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			Action<TestResetJob> callback2 = x => { Assert.IsTrue(x.result == 10); };
			
			GlobalEventSystem.SubscribeUIWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			GlobalEventSystem.SubscribeUIWithJob<TestResetJob, EvSimpleEvent>(new TestResetJob(), callback2);
			
			GlobalEventSystem.SendEventUI(new EvSimpleEvent(10));
			
			yield return null;
			
			GlobalEventSystem.SendEventUI(new EvSimpleEvent(10));
			GlobalEventSystem.UnsubscribeUIWithJob<TestJob, EvSimpleEvent>(callback);
			
			yield return null;
			
			GlobalEventSystem.UnsubscribeUIWithJob<TestResetJob, EvSimpleEvent>(callback2);

			EventManager.VerifyNoSubscribersAll();
		}

		[Test]
		public void TestLingeringSubscriber()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			GlobalEventSystem.SubscribeUIWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			
			Assert.Throws<SubscriberStillListeningException<TestJob, EvSimpleEvent>>(EventManager.VerifyNoSubscribersAll);
		}

		[UnityTest]
		public IEnumerator TestMultipleEvents()
		{
			Action<TestResetJob> callback = x => { Assert.IsTrue(x.result == 10); };
			GlobalEventSystem.SubscribeUIWithJob<TestResetJob, EvSimpleEvent>(new TestResetJob(), callback);
			
			GlobalEventSystem.SendEventUI(new EvSimpleEvent(10));

			yield return null;
			
			GlobalEventSystem.SendEventUI(new EvSimpleEvent(10));
			yield return null;
			
			GlobalEventSystem.SendEventUI(new EvSimpleEvent(10));
			yield return null;

			GlobalEventSystem.UnsubscribeUIWithJob<TestResetJob, EvSimpleEvent>(callback);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleDifferentEvents()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			Action<TestJob2> callback2 = x => { Assert.IsTrue(x.result == 20); };
			
			GlobalEventSystem.SubscribeUIWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			GlobalEventSystem.SubscribeUIWithJob<TestJob2, EvSimpleEvent2>(new TestJob2(), callback2);
			
			GlobalEventSystem.SendEventUI(new EvSimpleEvent(10));
			
			yield return null;
						
			GlobalEventSystem.SendEventUI(new EvSimpleEvent2(20));
			yield return null;
			
			GlobalEventSystem.UnsubscribeUIWithJob<TestJob, EvSimpleEvent>(callback);
			GlobalEventSystem.UnsubscribeUIWithJob<TestJob2, EvSimpleEvent2>(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
	}
}