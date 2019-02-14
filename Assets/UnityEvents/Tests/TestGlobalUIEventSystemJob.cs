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
			
			GlobalUIEventSystem.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			
			GlobalUIEventSystem.SendEvent(new EvSimpleEvent(10));
			
			yield return null;
			
			GlobalUIEventSystem.UnsubscribeWithJob<TestJob, EvSimpleEvent>(callback);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleSubscribersAndEvent()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			Action<TestJob> callback2 = x => { Assert.IsTrue(x.result == 10); };
			
			GlobalUIEventSystem.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			GlobalUIEventSystem.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback2);
			
			GlobalUIEventSystem.SendEvent(new EvSimpleEvent(10));
			
			yield return null;
			
			GlobalUIEventSystem.UnsubscribeWithJob<TestJob, EvSimpleEvent>(callback);
			GlobalUIEventSystem.UnsubscribeWithJob<TestJob, EvSimpleEvent>(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestTwoSubscribesOneUnsubscribeEvent()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			Action<TestResetJob> callback2 = x => { Assert.IsTrue(x.result == 10); };
			
			GlobalUIEventSystem.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			GlobalUIEventSystem.SubscribeWithJob<TestResetJob, EvSimpleEvent>(new TestResetJob(), callback2);
			
			GlobalUIEventSystem.SendEvent(new EvSimpleEvent(10));
			
			yield return null;
			
			GlobalUIEventSystem.SendEvent(new EvSimpleEvent(10));
			GlobalUIEventSystem.UnsubscribeWithJob<TestJob, EvSimpleEvent>(callback);
			
			yield return null;
			
			GlobalUIEventSystem.UnsubscribeWithJob<TestResetJob, EvSimpleEvent>(callback2);

			EventManager.VerifyNoSubscribersAll();
		}

		[Test]
		public void TestLingeringSubscriber()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			GlobalUIEventSystem.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			
			Assert.Throws<SubscriberStillListeningException<TestJob, EvSimpleEvent>>(EventManager.VerifyNoSubscribersAll);
		}

		[UnityTest]
		public IEnumerator TestMultipleEvents()
		{
			Action<TestResetJob> callback = x => { Assert.IsTrue(x.result == 10); };
			GlobalUIEventSystem.SubscribeWithJob<TestResetJob, EvSimpleEvent>(new TestResetJob(), callback);
			
			GlobalUIEventSystem.SendEvent(new EvSimpleEvent(10));

			yield return null;
			
			GlobalUIEventSystem.SendEvent(new EvSimpleEvent(10));
			yield return null;
			
			GlobalUIEventSystem.SendEvent(new EvSimpleEvent(10));
			yield return null;

			GlobalUIEventSystem.UnsubscribeWithJob<TestResetJob, EvSimpleEvent>(callback);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleDifferentEvents()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			Action<TestJob2> callback2 = x => { Assert.IsTrue(x.result == 20); };
			
			GlobalUIEventSystem.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			GlobalUIEventSystem.SubscribeWithJob<TestJob2, EvSimpleEvent2>(new TestJob2(), callback2);
			
			GlobalUIEventSystem.SendEvent(new EvSimpleEvent(10));
			
			yield return null;
						
			GlobalUIEventSystem.SendEvent(new EvSimpleEvent2(20));
			yield return null;
			
			GlobalUIEventSystem.UnsubscribeWithJob<TestJob, EvSimpleEvent>(callback);
			GlobalUIEventSystem.UnsubscribeWithJob<TestJob2, EvSimpleEvent2>(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
	}
}