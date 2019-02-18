using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEvents.Internal;
using Object = UnityEngine.Object;

namespace UnityEvents.Test
{
	public class TestGameObjectUIEventSystemJob
	{
		private GameObject _gameObject;

		[SetUp]
		public void SetUp()
		{
			_gameObject = new GameObject();
			Time.timeScale = 0;
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
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			
			_gameObject.SubscribeUIWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			
			_gameObject.SendEventUI(new EvSimpleEvent(10));
			
			yield return null;
			
			_gameObject.UnsubscribeUIWithJob<TestJob, EvSimpleEvent>(callback);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleSubscribersAndEvent()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			Action<TestJob> callback2 = x => { Assert.IsTrue(x.result == 10); };
			
			_gameObject.SubscribeUIWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			_gameObject.SubscribeUIWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback2);
			
			_gameObject.SendEventUI(new EvSimpleEvent(10));
			
			yield return null;
			
			_gameObject.UnsubscribeUIWithJob<TestJob, EvSimpleEvent>(callback);
			_gameObject.UnsubscribeUIWithJob<TestJob, EvSimpleEvent>(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestTwoSubscribesOneUnsubscribeEvent()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			Action<TestResetJob> callback2 = x => { Assert.IsTrue(x.result == 10); };
			
			_gameObject.SubscribeUIWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			_gameObject.SubscribeUIWithJob<TestResetJob, EvSimpleEvent>(new TestResetJob(), callback2);
			
			_gameObject.SendEventUI(new EvSimpleEvent(10));
			
			yield return null;
			
			_gameObject.SendEventUI(new EvSimpleEvent(10));
			_gameObject.UnsubscribeUIWithJob<TestJob, EvSimpleEvent>(callback);
			
			yield return null;
			
			_gameObject.UnsubscribeUIWithJob<TestResetJob, EvSimpleEvent>(callback2);

			EventManager.VerifyNoSubscribersAll();
		}

		[Test]
		public void TestLingeringSubscriber()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			_gameObject.SubscribeUIWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			
			Assert.Throws<SubscriberStillListeningException<TestJob, EvSimpleEvent>>(EventManager.VerifyNoSubscribersAll);
		}

		[UnityTest]
		public IEnumerator TestMultipleEvents()
		{
			Action<TestResetJob> callback = x => { Assert.IsTrue(x.result == 10); };
			_gameObject.SubscribeUIWithJob<TestResetJob, EvSimpleEvent>(new TestResetJob(), callback);
			
			_gameObject.SendEventUI(new EvSimpleEvent(10));

			yield return null;
			
			_gameObject.SendEventUI(new EvSimpleEvent(10));
			yield return null;
			
			_gameObject.SendEventUI(new EvSimpleEvent(10));
			yield return null;

			_gameObject.UnsubscribeUIWithJob<TestResetJob, EvSimpleEvent>(callback);

			EventManager.VerifyNoSubscribersAll();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleDifferentEvents()
		{
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			Action<TestJob2> callback2 = x => { Assert.IsTrue(x.result == 20); };
			
			_gameObject.SubscribeUIWithJob<TestJob, EvSimpleEvent>(new TestJob(), callback);
			_gameObject.SubscribeUIWithJob<TestJob2, EvSimpleEvent2>(new TestJob2(), callback2);
			
			_gameObject.SendEventUI(new EvSimpleEvent(10));
			
			yield return null;
						
			_gameObject.SendEventUI(new EvSimpleEvent2(20));
			yield return null;
			
			_gameObject.UnsubscribeUIWithJob<TestJob, EvSimpleEvent>(callback);
			_gameObject.UnsubscribeUIWithJob<TestJob2, EvSimpleEvent2>(callback2);

			EventManager.VerifyNoSubscribersAll();
		}
	}
}