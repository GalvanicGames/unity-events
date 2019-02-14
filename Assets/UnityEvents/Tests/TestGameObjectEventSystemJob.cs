using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEventsInternal;
using Object = UnityEngine.Object;

namespace UnityEvents.Test
{
	public class TestGameObjectEventSystemJob
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
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			
			_gameObject.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			
			_gameObject.SendEvent(new EvSimpleEvent(10));
			
			yield return new WaitForFixedUpdate();
			
			_gameObject.UnsubscribeWithJob<TestJob, EvSimpleEvent>(callback);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleSubscribersAndEvent()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			Action<TestJob> callback2 = x => { Assert.IsTrue(x.result == 10); };
			
			_gameObject.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			_gameObject.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback2);
			
			_gameObject.SendEvent(new EvSimpleEvent(10));
			
			yield return new WaitForFixedUpdate();
			
			_gameObject.UnsubscribeWithJob<TestJob, EvSimpleEvent>(callback);
			_gameObject.UnsubscribeWithJob<TestJob, EvSimpleEvent>(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestTwoSubscribesOneUnsubscribeEvent()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			Action<TestResetJob> callback2 = x => { Assert.IsTrue(x.result == 10); };
			
			_gameObject.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			_gameObject.SubscribeWithJob<TestResetJob, EvSimpleEvent>(new TestResetJob(), callback2);
			
			_gameObject.SendEvent(new EvSimpleEvent(10));
			
			yield return new WaitForFixedUpdate();
			
			_gameObject.SendEvent(new EvSimpleEvent(10));
			_gameObject.UnsubscribeWithJob<TestJob, EvSimpleEvent>(callback);
			
			yield return new WaitForFixedUpdate();
			
			_gameObject.UnsubscribeWithJob<TestResetJob, EvSimpleEvent>(callback2);

			EventManager.VerifyNoSubscribersAll();
		}

		[Test]
		public void TestLingeringSubscriber()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			_gameObject.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			
			Assert.Throws<SubscriberStillListeningException<TestJob, EvSimpleEvent>>(EventManager.VerifyNoSubscribersAll);
		}

		[UnityTest]
		public IEnumerator TestMultipleEvents()
		{
			Action<TestResetJob> callback = x => { Assert.IsTrue(x.result == 10); };
			_gameObject.SubscribeWithJob<TestResetJob, EvSimpleEvent>(new TestResetJob(), callback);
			
			_gameObject.SendEvent(new EvSimpleEvent(10));

			yield return new WaitForFixedUpdate();
			
			_gameObject.SendEvent(new EvSimpleEvent(10));
			yield return new WaitForFixedUpdate();
			
			_gameObject.SendEvent(new EvSimpleEvent(10));
			yield return new WaitForFixedUpdate();

			_gameObject.UnsubscribeWithJob<TestResetJob, EvSimpleEvent>(callback);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleDifferentEvents()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			Action<TestJob2> callback2 = x => { Assert.IsTrue(x.result == 20); };
			
			_gameObject.SubscribeWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			_gameObject.SubscribeWithJob<TestJob2, EvSimpleEvent2>(new TestJob2(), callback2);
			
			_gameObject.SendEvent(new EvSimpleEvent(10));
			
			yield return new WaitForFixedUpdate();
						
			_gameObject.SendEvent(new EvSimpleEvent2(20));
			yield return new WaitForFixedUpdate();
			
			_gameObject.UnsubscribeWithJob<TestJob, EvSimpleEvent>(callback);
			_gameObject.UnsubscribeWithJob<TestJob2, EvSimpleEvent2>(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
	}
}