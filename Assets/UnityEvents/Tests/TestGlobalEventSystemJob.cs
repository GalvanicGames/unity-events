using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEvents.Internal;

namespace UnityEvents.Test
{
	public class TestGlobalEventSystemJob
	{
		[TearDown]
		public void TearDown()
		{
			EventManager.ResetAll();
		}

		[UnityTest]
		public IEnumerator TestSimpleSubscribeAndEvent()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			
			GlobalEventSystem.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent(10));
			
			yield return new WaitForFixedUpdate();
			
			GlobalEventSystem.UnsubscribeWithJob<TestJob, EvSimpleEvent>(callback);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleSubscribersAndEvent()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			Action<TestJob> callback2 = x => { Assert.IsTrue(x.result == 10); };
			
			GlobalEventSystem.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			GlobalEventSystem.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback2);
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent(10));
			
			yield return new WaitForFixedUpdate();
			
			GlobalEventSystem.UnsubscribeWithJob<TestJob, EvSimpleEvent>(callback);
			GlobalEventSystem.UnsubscribeWithJob<TestJob, EvSimpleEvent>(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestTwoSubscribesOneUnsubscribeEvent()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			Action<TestResetJob> callback2 = x => { Assert.IsTrue(x.result == 10); };
			
			GlobalEventSystem.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			GlobalEventSystem.SubscribeWithJob<TestResetJob, EvSimpleEvent>(new TestResetJob(), callback2);
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent(10));
			
			yield return new WaitForFixedUpdate();
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent(10));
			GlobalEventSystem.UnsubscribeWithJob<TestJob, EvSimpleEvent>(callback);
			
			yield return new WaitForFixedUpdate();
			
			GlobalEventSystem.UnsubscribeWithJob<TestResetJob, EvSimpleEvent>(callback2);

			EventManager.VerifyNoSubscribersAll();
		}

		[Test]
		public void TestLingeringSubscriber()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			GlobalEventSystem.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			
			Assert.Throws<SubscriberStillListeningException<TestJob, EvSimpleEvent>>(EventManager.VerifyNoSubscribersAll);
		}

		[UnityTest]
		public IEnumerator TestMultipleEvents()
		{
			Action<TestResetJob> callback = x => { Assert.IsTrue(x.result == 10); };
			GlobalEventSystem.SubscribeWithJob<TestResetJob, EvSimpleEvent>(new TestResetJob(), callback);
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent(10));

			yield return new WaitForFixedUpdate();
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent(10));
			yield return new WaitForFixedUpdate();
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent(10));
			yield return new WaitForFixedUpdate();

			GlobalEventSystem.UnsubscribeWithJob<TestResetJob, EvSimpleEvent>(callback);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleDifferentEvents()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			Action<TestJob2> callback2 = x => { Assert.IsTrue(x.result == 20); };
			
			GlobalEventSystem.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			GlobalEventSystem.SubscribeWithJob<TestJob2, EvSimpleEvent2>(new TestJob2(), callback2);
			
			GlobalEventSystem.SendEvent(new EvSimpleEvent(10));
			
			yield return new WaitForFixedUpdate();
						
			GlobalEventSystem.SendEvent(new EvSimpleEvent2(20));
			yield return new WaitForFixedUpdate();
			
			GlobalEventSystem.UnsubscribeWithJob<TestJob, EvSimpleEvent>(callback);
			GlobalEventSystem.UnsubscribeWithJob<TestJob2, EvSimpleEvent2>(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
	}
}