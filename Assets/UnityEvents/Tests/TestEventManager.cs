using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEvents.Test
{
	
	public class TestEventManager
	{
		[TearDown]
		public void TearDown()
		{
			EventManager.ResetAll();
		}

		[UnityTest] 
		public IEnumerator TestSimpleValue()
		{
			EventEntity entity = EventEntity.CreateEntity();
			
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += x.value; };
			
			EventManager.Subscribe(entity, callback, EventUpdateType.Update);

			EventManager.SendEvent(entity, new EvSimpleEvent(10), EventUpdateType.Update);
			Assert.IsTrue(value == 0);
			
			yield return null;
			
			Assert.IsTrue(value == 10);
		}

		[UnityTest]
		public IEnumerator TestSimpleValueJob()
		{
			EventEntity entity = EventEntity.CreateEntity();
			
			Action<TestJob> callback = x => { Assert.IsTrue(x.result == 10); };
			
			EventManager.SubscribeWithJob<TestJob, EvSimpleEvent>(entity, new TestJob(), callback, EventUpdateType.Update);
			EventManager.SendEvent(entity, new EvSimpleEvent(10), EventUpdateType.Update);

			yield return null;
		}
		
		[UnityTest]
		public IEnumerator TestUpdateEvents()
		{
			EventEntity entity = EventEntity.CreateEntity();
			
			Action<EvSimpleEvent> updateCallback = x =>
			{
				string stacktrace = Environment.StackTrace;
				Assert.IsTrue(stacktrace.Contains("EventManager.Update ()"));
			};
			
			Action<EvSimpleEvent> fixedUpdateCallback = x =>
			{
				string stacktrace = Environment.StackTrace;
				Assert.IsTrue(stacktrace.Contains("EventManager.FixedUpdate ()"));
			};
			
			Action<EvSimpleEvent> lateUpdateCallback = x =>
			{
				string stacktrace = Environment.StackTrace;
				Assert.IsTrue(stacktrace.Contains("EventManager.LateUpdate ()"));
			};
			
			EventManager.Subscribe(entity, updateCallback, EventUpdateType.Update);
			EventManager.Subscribe(entity, fixedUpdateCallback, EventUpdateType.FixedUpdate);
			EventManager.Subscribe(entity, lateUpdateCallback, EventUpdateType.LateUpdate);
			
			EventManager.SendEvent(entity, new EvSimpleEvent(), EventUpdateType.Update);
			EventManager.SendEvent(entity, new EvSimpleEvent(), EventUpdateType.FixedUpdate);
			EventManager.SendEvent(entity, new EvSimpleEvent(), EventUpdateType.LateUpdate);

			yield return new WaitForFixedUpdate();
			yield return null;
		}

		[UnityTest]
		public IEnumerator TestUpdateMismatchEvent()
		{
			EventEntity entity = EventEntity.CreateEntity();
			
			int value = 0;
			Action<EvSimpleEvent> callback = x => { value += 1; };
			
			EventManager.Subscribe(entity, callback, EventUpdateType.Update);
			EventManager.SendEvent(entity, new EvSimpleEvent(), EventUpdateType.LateUpdate);
			EventManager.SendEvent(entity, new EvSimpleEvent(), EventUpdateType.FixedUpdate);

			yield return null;
			
			Assert.IsTrue(value == 0);
			
			EventManager.SendEvent(entity, new EvSimpleEvent(), EventUpdateType.Update);

			yield return null;
			Assert.IsTrue(value == 1);
		}

		[UnityTest]
		public IEnumerator TestMultipleEntities()
		{
			int value1 = 0;
			int value2 = 0;
			
			Action<EvSimpleEvent> callback1 = x => { value1 += 1; };
			Action<EvSimpleEvent> callback2 = x => { value2 += 1; };
			
			EventEntity entity1 = EventEntity.CreateEntity();
			EventEntity entity2 = EventEntity.CreateEntity();
			
			EventManager.Subscribe(entity1, callback1, EventUpdateType.Update);
			EventManager.Subscribe(entity2, callback2, EventUpdateType.Update);
			
			EventManager.SendEvent(entity1, new EvSimpleEvent(), EventUpdateType.Update);

			yield return null;
			
			Assert.IsTrue(value1 == 1);
			Assert.IsTrue(value2 == 0);
			
			EventManager.SendEvent(entity2, new EvSimpleEvent(), EventUpdateType.Update);

			yield return null;

			Assert.IsTrue(value1 == 1);
			Assert.IsTrue(value2 == 1);
		}
		
		[UnityTest]
		public IEnumerator TestMultipleEntitiesJob()
		{
			EventEntity entity1 = EventEntity.CreateEntity();
			EventEntity entity2 = EventEntity.CreateEntity();
			
			Action<TestJob> callback1 = x => { Assert.IsTrue(x.result == 10); };
			Action<TestJob> callback2 = x => { Assert.IsTrue(x.result == 20); };
			
			EventManager.SubscribeWithJob<TestJob, EvSimpleEvent>(entity1, new TestJob(), callback1, EventUpdateType.Update);
			EventManager.SubscribeWithJob<TestJob, EvSimpleEvent>(entity2, new TestJob(), callback2, EventUpdateType.Update);
			
			EventManager.SendEvent(entity1, new EvSimpleEvent(10), EventUpdateType.Update);

			yield return null;
			
			EventManager.SendEvent(entity2, new EvSimpleEvent(20), EventUpdateType.Update);

			yield return null;
		}

		[UnityTest]
		public IEnumerator TestStandardVsJob()
		{
			int value = 0;
			EventEntity entity = EventEntity.CreateEntity();
			
			Action<TestJob> callback1 = x => { Assert.IsTrue(x.result == 10); };
			Action<EvSimpleEvent> callback2 = x => { value += x.value; };
			
			EventManager.SubscribeWithJob<TestJob, EvSimpleEvent>(entity, new TestJob(), callback1, EventUpdateType.Update);
			EventManager.Subscribe(entity, callback2, EventUpdateType.Update);
			
			EventManager.SendEvent(entity, new EvSimpleEvent(10), EventUpdateType.Update);

			yield return null;
			
			Assert.IsTrue(value == 10);
		}
		
		[UnityTest]
		public IEnumerator TestMultipleEntitiesStandardVsJob()
		{
			EventEntity entity1 = EventEntity.CreateEntity();
			EventEntity entity2 = EventEntity.CreateEntity();

			int value = 0;

			Action<TestJob> callback1 = x => { Assert.IsTrue(x.result == 10); };
			Action<EvSimpleEvent> callback2 = x => { value += x.value; };
			
			EventManager.SubscribeWithJob<TestJob, EvSimpleEvent>(entity1, new TestJob(), callback1, EventUpdateType.Update);
			EventManager.Subscribe(entity2, callback2, EventUpdateType.Update);

			EventManager.SendEvent(entity1, new EvSimpleEvent(10), EventUpdateType.Update);

			yield return null;
			
			Assert.IsTrue(value == 0);
			
			EventManager.SendEvent(entity2, new EvSimpleEvent(20), EventUpdateType.Update);

			yield return null;
			
			Assert.IsTrue(value == 20);
		}

	}
}
