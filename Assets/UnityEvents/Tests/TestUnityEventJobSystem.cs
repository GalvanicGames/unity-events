using System;
using NUnit.Framework;
using UnityEventsInternal;

namespace UnityEvents.Test
{
	public class TestUnityEventJobSystem
	{
		private UnityEventJobSystem<TestJob, EvSimpleEvent> _system;

		[SetUp]
		public void SetUp()
		{
			_system = new UnityEventJobSystem<TestJob, EvSimpleEvent>();
		}

		[TearDown]
		public void TearDown()
		{
			_system.Reset();
			_system.Dispose();
		}

		[Test]
		public void TestSimpleEvent()
		{
			EventEntity entity = EventEntity.CreateEntity();

			int value = 10;

			Action<TestJob> callback = x => { Assert.IsTrue(x.result == value); };

			_system.Subscribe(entity, new TestJob(value), callback);

			_system.QueueEvent(entity, new EvSimpleEvent(10));
			value = 20;
			_system.ProcessEvents();

			_system.Unsubscribe(entity, callback);
			_system.VerifyNoSubscribers();

			_system.QueueEvent(entity, new EvSimpleEvent(10));
			_system.ProcessEvents();
		}

		[Test]
		public void TestOtherUnsubscribe()
		{
			EventEntity entity1 = EventEntity.CreateEntity();
			EventEntity entity2 = EventEntity.CreateEntity();

			int value = 5;

			Action<TestJob> callback = x => { Assert.IsTrue(x.result == value); };
			Action<TestJob> callback2 = x => { Assert.Fail(); };

			_system.Subscribe(entity1, new TestJob(value), callback);
			_system.Subscribe(entity2, new TestJob(value), callback2);
			_system.Unsubscribe(entity2, callback2);
			_system.QueueEvent(entity1, new EvSimpleEvent(10));

			value = 15;
			_system.ProcessEvents();
			_system.Unsubscribe(entity1, callback);
			_system.VerifyNoSubscribers();
		}

		[Test]
		public void TestMeUnsubscribe()
		{
			EventEntity entity1 = EventEntity.CreateEntity();
			EventEntity entity2 = EventEntity.CreateEntity();

			int value = -1;

			Action<TestJob> callback = x => { Assert.Fail(); };

			_system.Subscribe(entity1, new TestJob(value), callback);
			_system.Subscribe(entity2, new TestJob(value), callback);
			_system.Unsubscribe(entity1, callback);
			_system.QueueEvent(entity1, new EvSimpleEvent(10));

			_system.ProcessEvents();
			_system.Unsubscribe(entity2, callback);
			_system.VerifyNoSubscribers();

			Assert.IsTrue(value == -1);
		}

		[Test]
		public void TestMultipleEvents()
		{
			EventEntity entity = EventEntity.CreateEntity();

			int value = -1;

			Action<TestJob> callback = x => { Assert.IsTrue(x.result == value); };

			_system.Subscribe(entity, new TestJob(value), callback);

			// Should warn!
			_system.QueueEvent(entity, new EvSimpleEvent(10));
			_system.QueueEvent(entity, new EvSimpleEvent(10));
			_system.QueueEvent(entity, new EvSimpleEvent(10));
			_system.QueueEvent(entity, new EvSimpleEvent(10));

			// Only one event is accepted
			value = 9;
			_system.ProcessEvents();

			_system.Unsubscribe(entity, callback);
			_system.VerifyNoSubscribers();

			_system.QueueEvent(entity, new EvSimpleEvent(10));
			_system.ProcessEvents();
		}

		[Test]
		public void TestMultipleSubscribes()
		{
			EventEntity entity = EventEntity.CreateEntity();

			Action<TestJob> callback = x => { };

			_system.Subscribe(entity, new TestJob(), callback);

			Assert.Throws<MultipleSubscriptionsException<TestJob>>(() =>
				_system.Subscribe(entity, new TestJob(), callback));
		}

		[Test]
		public void TestMultipleEntities()
		{
			EventEntity entity1 = EventEntity.CreateEntity();
			EventEntity entity2 = EventEntity.CreateEntity();

			int value1 = 1;
			int value2 = 11;

			Action<TestJob> callback = x => { Assert.IsTrue(x.result == value1); };
			Action<TestJob> callback2 = x => { Assert.IsTrue(x.result == value2); };

			_system.Subscribe(entity1, new TestJob(value1), callback);
			_system.Subscribe(entity2, new TestJob(value2), callback2);
			_system.QueueEvent(entity1, new EvSimpleEvent(10));
			_system.QueueEvent(entity2, new EvSimpleEvent(30));

			value1 = 11;
			value2 = 41;

			_system.ProcessEvents();
			_system.Unsubscribe(entity1, callback);
			_system.Unsubscribe(entity2, callback2);
			_system.VerifyNoSubscribers();
		}

		[Test]
		public void TestMultipleSubscribeUnsubscribesEvents()
		{
			EventEntity entity1 = EventEntity.CreateEntity();
			EventEntity entity2 = EventEntity.CreateEntity();

			int value1 = 0;
			int value2 = 0;

			Action<TestJob> callback = x => { Assert.IsTrue(x.result == value1); };
			Action<TestJob> callback2 = x => { Assert.IsTrue(x.result == value2); };

			_system.Subscribe(entity1, new TestJob(value1), callback);
			_system.Subscribe(entity2, new TestJob(value2), callback2);
			_system.Unsubscribe(entity2, callback2);

			_system.QueueEvent(entity1, new EvSimpleEvent(10));
			_system.QueueEvent(entity2, new EvSimpleEvent(30));

			value1 = 10;
			Assert.IsTrue(value2 == 0);
			_system.ProcessEvents();

			_system.Subscribe(entity2, new TestJob(value2), callback2);

			_system.QueueEvent(entity1, new EvSimpleEvent(10));
			_system.QueueEvent(entity2, new EvSimpleEvent(30));

			value1 = 20;
			value2 = 30;
			_system.ProcessEvents();

			_system.Unsubscribe(entity1, callback);

			_system.QueueEvent(entity1, new EvSimpleEvent(10));
			_system.QueueEvent(entity2, new EvSimpleEvent(30));

			Assert.IsTrue(value1 == 20);
			value2 = 60;
			_system.ProcessEvents();

			_system.Subscribe(entity1, new TestJob(value1), callback);

			_system.QueueEvent(entity1, new EvSimpleEvent(10));
			_system.QueueEvent(entity2, new EvSimpleEvent(30));

			value1 = 30;
			value2 = 90;
			_system.ProcessEvents();

			_system.Unsubscribe(entity2, callback2);

			_system.QueueEvent(entity1, new EvSimpleEvent(10));
			_system.QueueEvent(entity2, new EvSimpleEvent(30));

			value1 = 40;
			Assert.IsTrue(value2 == 90);
			_system.ProcessEvents();

			_system.Unsubscribe(entity1, callback);

			_system.QueueEvent(entity1, new EvSimpleEvent(10));
			_system.QueueEvent(entity2, new EvSimpleEvent(30));
			_system.ProcessEvents();

			_system.VerifyNoSubscribers();

			Assert.IsTrue(value1 == 40);
			Assert.IsTrue(value2 == 90);
		}
	}
}