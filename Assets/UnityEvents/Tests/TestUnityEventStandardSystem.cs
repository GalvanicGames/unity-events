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
			EventTarget target = EventTarget.CreateTarget();

			int value = 0;

			Action<EvSimpleEvent> callback = x => value += x.value;

			_system.Subscribe(target, callback);

			_system.QueueEvent(target, new EvSimpleEvent(10));
			_system.ProcessEvents();

			_system.Unsubscribe(target, callback);
			_system.VerifyNoSubscribers();

			_system.QueueEvent(target, new EvSimpleEvent(10));
			_system.ProcessEvents();

			Assert.IsTrue(value == 10);
		}

		[Test]
		public void TestOtherUnsubscribe()
		{
			EventTarget target1 = EventTarget.CreateTarget();
			EventTarget target2 = EventTarget.CreateTarget();

			int value = 0;

			Action<EvSimpleEvent> callback = x => value += x.value;

			_system.Subscribe(target1, callback);
			_system.Subscribe(target2, callback);
			_system.Unsubscribe(target2, callback);
			_system.QueueEvent(target1, new EvSimpleEvent(10));

			_system.ProcessEvents();
			_system.Unsubscribe(target1, callback);
			_system.VerifyNoSubscribers();

			Assert.IsTrue(value == 10);
		}

		[Test]
		public void TestMeUnsubscribe()
		{
			EventTarget target1 = EventTarget.CreateTarget();
			EventTarget target2 = EventTarget.CreateTarget();

			int value = 0;

			Action<EvSimpleEvent> callback = x => value += x.value;

			_system.Subscribe(target1, callback);
			_system.Subscribe(target2, callback);
			_system.Unsubscribe(target1, callback);
			_system.QueueEvent(target1, new EvSimpleEvent(10));

			_system.ProcessEvents();
			_system.Unsubscribe(target2, callback);
			_system.VerifyNoSubscribers();

			Assert.IsTrue(value == 0);
		}

		[Test]
		public void TestMultipleEvents()
		{
			EventTarget target = EventTarget.CreateTarget();

			int value = 0;

			Action<EvSimpleEvent> callback = x => value += x.value;

			_system.Subscribe(target, callback);

			_system.QueueEvent(target, new EvSimpleEvent(10));
			_system.QueueEvent(target, new EvSimpleEvent(10));
			_system.QueueEvent(target, new EvSimpleEvent(10));
			_system.QueueEvent(target, new EvSimpleEvent(10));
			_system.ProcessEvents();

			_system.Unsubscribe(target, callback);
			_system.VerifyNoSubscribers();

			_system.QueueEvent(target, new EvSimpleEvent(10));
			_system.ProcessEvents();

			Assert.IsTrue(value == 40);
		}

		[Test]
		public void TestMultipleEntities()
		{
			EventTarget target1 = EventTarget.CreateTarget();
			EventTarget target2 = EventTarget.CreateTarget();

			int value1 = 0;
			int value2 = 0;

			Action<EvSimpleEvent> callback = x => value1 += x.value;
			Action<EvSimpleEvent> callback2 = x => value2 += x.value;

			_system.Subscribe(target1, callback);
			_system.Subscribe(target2, callback2);
			_system.QueueEvent(target1, new EvSimpleEvent(10));
			_system.QueueEvent(target2, new EvSimpleEvent(30));

			_system.ProcessEvents();
			_system.Unsubscribe(target1, callback);
			_system.Unsubscribe(target2, callback2);
			_system.VerifyNoSubscribers();

			Assert.IsTrue(value1 == 10);
			Assert.IsTrue(value2 == 30);
		}

		[Test]
		public void TestMultipleSubscribes()
		{
			EventTarget target = EventTarget.CreateTarget();

			Action<EvSimpleEvent> callback = x => { };

			_system.Subscribe(target, callback);

			Assert.Throws<MultipleSubscriptionsException<EvSimpleEvent>>(() => _system.Subscribe(target, callback));
		}

		[Test]
		public void TestMultipleSubscribeUnsubscribesEvents()
		{
			EventTarget target1 = EventTarget.CreateTarget();
			EventTarget target2 = EventTarget.CreateTarget();

			int value1 = 0;
			int value2 = 0;

			Action<EvSimpleEvent> callback = x => value1 += x.value;
			Action<EvSimpleEvent> callback2 = x => value2 += x.value;

			_system.Subscribe(target1, callback);
			_system.Subscribe(target2, callback2);
			_system.Unsubscribe(target2, callback2);

			_system.QueueEvent(target1, new EvSimpleEvent(10));
			_system.QueueEvent(target2, new EvSimpleEvent(30));
			_system.ProcessEvents();

			Assert.IsTrue(value1 == 10);
			Assert.IsTrue(value2 == 0);

			_system.Subscribe(target2, callback2);

			_system.QueueEvent(target1, new EvSimpleEvent(10));
			_system.QueueEvent(target2, new EvSimpleEvent(30));
			_system.ProcessEvents();

			Assert.IsTrue(value1 == 20);
			Assert.IsTrue(value2 == 30);

			_system.Unsubscribe(target1, callback);

			_system.QueueEvent(target1, new EvSimpleEvent(10));
			_system.QueueEvent(target2, new EvSimpleEvent(30));
			_system.ProcessEvents();

			Assert.IsTrue(value1 == 20);
			Assert.IsTrue(value2 == 60);

			_system.Subscribe(target1, callback);

			_system.QueueEvent(target1, new EvSimpleEvent(10));
			_system.QueueEvent(target2, new EvSimpleEvent(30));
			_system.ProcessEvents();

			Assert.IsTrue(value1 == 30);
			Assert.IsTrue(value2 == 90);

			_system.Unsubscribe(target2, callback2);

			_system.QueueEvent(target1, new EvSimpleEvent(10));
			_system.QueueEvent(target2, new EvSimpleEvent(30));
			_system.ProcessEvents();

			Assert.IsTrue(value1 == 40);
			Assert.IsTrue(value2 == 90);

			_system.Unsubscribe(target1, callback);

			_system.QueueEvent(target1, new EvSimpleEvent(10));
			_system.QueueEvent(target2, new EvSimpleEvent(30));
			_system.ProcessEvents();

			_system.VerifyNoSubscribers();

			Assert.IsTrue(value1 == 40);
			Assert.IsTrue(value2 == 90);
		}

		[Test]
		public void TestUnblittableEvent()
		{
			Assert.Throws<EventTypeNotBlittableException>(() =>
			{
				EventHandlerStandard<UnblittableEvent> system = new EventHandlerStandard<UnblittableEvent>();
			});
		}
	}
}