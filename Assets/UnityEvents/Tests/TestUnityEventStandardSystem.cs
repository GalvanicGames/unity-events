using System;
using NUnit.Framework;
using UnityEventsInternal;

namespace UnityEvents.Test
{
	public class TestUnityEventStandardSystem
	{
		private EventHandlerStandard<EvSimpleEvent> _system;

		[SetUp]
		public void SetUp()
		{
			_system = new EventHandlerStandard<EvSimpleEvent>();
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

			int value = 0;

			Action<EvSimpleEvent> callback = x => value += x.value;

			_system.Subscribe(entity, callback);

			_system.QueueEvent(entity, new EvSimpleEvent(10));
			_system.ProcessEvents();

			_system.Unsubscribe(entity, callback);
			_system.VerifyNoSubscribers();

			_system.QueueEvent(entity, new EvSimpleEvent(10));
			_system.ProcessEvents();

			Assert.IsTrue(value == 10);
		}

		[Test]
		public void TestOtherUnsubscribe()
		{
			EventEntity entity1 = EventEntity.CreateEntity();
			EventEntity entity2 = EventEntity.CreateEntity();

			int value = 0;

			Action<EvSimpleEvent> callback = x => value += x.value;

			_system.Subscribe(entity1, callback);
			_system.Subscribe(entity2, callback);
			_system.Unsubscribe(entity2, callback);
			_system.QueueEvent(entity1, new EvSimpleEvent(10));

			_system.ProcessEvents();
			_system.Unsubscribe(entity1, callback);
			_system.VerifyNoSubscribers();

			Assert.IsTrue(value == 10);
		}

		[Test]
		public void TestMeUnsubscribe()
		{
			EventEntity entity1 = EventEntity.CreateEntity();
			EventEntity entity2 = EventEntity.CreateEntity();

			int value = 0;

			Action<EvSimpleEvent> callback = x => value += x.value;

			_system.Subscribe(entity1, callback);
			_system.Subscribe(entity2, callback);
			_system.Unsubscribe(entity1, callback);
			_system.QueueEvent(entity1, new EvSimpleEvent(10));

			_system.ProcessEvents();
			_system.Unsubscribe(entity2, callback);
			_system.VerifyNoSubscribers();

			Assert.IsTrue(value == 0);
		}

		[Test]
		public void TestMultipleEvents()
		{
			EventEntity entity = EventEntity.CreateEntity();

			int value = 0;

			Action<EvSimpleEvent> callback = x => value += x.value;

			_system.Subscribe(entity, callback);

			_system.QueueEvent(entity, new EvSimpleEvent(10));
			_system.QueueEvent(entity, new EvSimpleEvent(10));
			_system.QueueEvent(entity, new EvSimpleEvent(10));
			_system.QueueEvent(entity, new EvSimpleEvent(10));
			_system.ProcessEvents();

			_system.Unsubscribe(entity, callback);
			_system.VerifyNoSubscribers();

			_system.QueueEvent(entity, new EvSimpleEvent(10));
			_system.ProcessEvents();

			Assert.IsTrue(value == 40);
		}

		[Test]
		public void TestMultipleEntities()
		{
			EventEntity entity1 = EventEntity.CreateEntity();
			EventEntity entity2 = EventEntity.CreateEntity();

			int value1 = 0;
			int value2 = 0;

			Action<EvSimpleEvent> callback = x => value1 += x.value;
			Action<EvSimpleEvent> callback2 = x => value2 += x.value;

			_system.Subscribe(entity1, callback);
			_system.Subscribe(entity2, callback2);
			_system.QueueEvent(entity1, new EvSimpleEvent(10));
			_system.QueueEvent(entity2, new EvSimpleEvent(30));

			_system.ProcessEvents();
			_system.Unsubscribe(entity1, callback);
			_system.Unsubscribe(entity2, callback2);
			_system.VerifyNoSubscribers();

			Assert.IsTrue(value1 == 10);
			Assert.IsTrue(value2 == 30);
		}

		[Test]
		public void TestMultipleSubscribes()
		{
			EventEntity entity = EventEntity.CreateEntity();

			Action<EvSimpleEvent> callback = x => { };

			_system.Subscribe(entity, callback);

			Assert.Throws<MultipleSubscriptionsException<EvSimpleEvent>>(() => _system.Subscribe(entity, callback));
		}

		[Test]
		public void TestMultipleSubscribeUnsubscribesEvents()
		{
			EventEntity entity1 = EventEntity.CreateEntity();
			EventEntity entity2 = EventEntity.CreateEntity();

			int value1 = 0;
			int value2 = 0;

			Action<EvSimpleEvent> callback = x => value1 += x.value;
			Action<EvSimpleEvent> callback2 = x => value2 += x.value;

			_system.Subscribe(entity1, callback);
			_system.Subscribe(entity2, callback2);
			_system.Unsubscribe(entity2, callback2);

			_system.QueueEvent(entity1, new EvSimpleEvent(10));
			_system.QueueEvent(entity2, new EvSimpleEvent(30));
			_system.ProcessEvents();

			Assert.IsTrue(value1 == 10);
			Assert.IsTrue(value2 == 0);

			_system.Subscribe(entity2, callback2);

			_system.QueueEvent(entity1, new EvSimpleEvent(10));
			_system.QueueEvent(entity2, new EvSimpleEvent(30));
			_system.ProcessEvents();

			Assert.IsTrue(value1 == 20);
			Assert.IsTrue(value2 == 30);

			_system.Unsubscribe(entity1, callback);

			_system.QueueEvent(entity1, new EvSimpleEvent(10));
			_system.QueueEvent(entity2, new EvSimpleEvent(30));
			_system.ProcessEvents();

			Assert.IsTrue(value1 == 20);
			Assert.IsTrue(value2 == 60);

			_system.Subscribe(entity1, callback);

			_system.QueueEvent(entity1, new EvSimpleEvent(10));
			_system.QueueEvent(entity2, new EvSimpleEvent(30));
			_system.ProcessEvents();

			Assert.IsTrue(value1 == 30);
			Assert.IsTrue(value2 == 90);

			_system.Unsubscribe(entity2, callback2);

			_system.QueueEvent(entity1, new EvSimpleEvent(10));
			_system.QueueEvent(entity2, new EvSimpleEvent(30));
			_system.ProcessEvents();

			Assert.IsTrue(value1 == 40);
			Assert.IsTrue(value2 == 90);

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