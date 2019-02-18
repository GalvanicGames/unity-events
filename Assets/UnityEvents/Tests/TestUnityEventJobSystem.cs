using System;
using NUnit.Framework;
using UnityEventsInternal;

namespace UnityEvents.Test
{
	public class TestUnityEventJobSystem
	{
		private EventHandlerJob<TestJob, EvSimpleEvent> _handler;

		[SetUp]
		public void SetUp()
		{
			_handler = new EventHandlerJob<TestJob, EvSimpleEvent>();
		}

		[TearDown]
		public void TearDown()
		{
			_handler.Reset();
			_handler.Dispose();
		}

		[Test]
		public void TestSimpleEvent()
		{
			EventTarget target = EventTarget.CreateTarget();

			int value = 10;

			Action<TestJob> callback = x => { Assert.IsTrue(x.result == value); };

			_handler.Subscribe(target, new TestJob(value), callback);

			_handler.QueueEvent(target, new EvSimpleEvent(10));
			value = 20;
			_handler.ProcessEvents();

			_handler.Unsubscribe(target, callback);
			_handler.VerifyNoSubscribers();

			_handler.QueueEvent(target, new EvSimpleEvent(10));
			_handler.ProcessEvents();
		}

		[Test]
		public void TestOtherUnsubscribe()
		{
			EventTarget target1 = EventTarget.CreateTarget();
			EventTarget target2 = EventTarget.CreateTarget();

			int value = 5;

			Action<TestJob> callback = x => { Assert.IsTrue(x.result == value); };
			Action<TestJob> callback2 = x => { Assert.Fail(); };

			_handler.Subscribe(target1, new TestJob(value), callback);
			_handler.Subscribe(target2, new TestJob(value), callback2);
			_handler.Unsubscribe(target2, callback2);
			_handler.QueueEvent(target1, new EvSimpleEvent(10));

			value = 15;
			_handler.ProcessEvents();
			_handler.Unsubscribe(target1, callback);
			_handler.VerifyNoSubscribers();
		}

		[Test]
		public void TestMeUnsubscribe()
		{
			EventTarget target1 = EventTarget.CreateTarget();
			EventTarget target2 = EventTarget.CreateTarget();

			int value = -1;

			Action<TestJob> callback = x => { Assert.Fail(); };

			_handler.Subscribe(target1, new TestJob(value), callback);
			_handler.Subscribe(target2, new TestJob(value), callback);
			_handler.Unsubscribe(target1, callback);
			_handler.QueueEvent(target1, new EvSimpleEvent(10));

			_handler.ProcessEvents();
			_handler.Unsubscribe(target2, callback);
			_handler.VerifyNoSubscribers();

			Assert.IsTrue(value == -1);
		}

		[Test]
		public void TestMultipleEvents()
		{
			EventTarget target = EventTarget.CreateTarget();

			int value = -1;

			Action<TestJob> callback = x => { Assert.IsTrue(x.result == value); };

			_handler.Subscribe(target, new TestJob(value), callback);

			// Should warn!
			_handler.QueueEvent(target, new EvSimpleEvent(10));
			_handler.QueueEvent(target, new EvSimpleEvent(10));
			_handler.QueueEvent(target, new EvSimpleEvent(10));
			_handler.QueueEvent(target, new EvSimpleEvent(10));

			// Only one event is accepted
			value = 9;
			_handler.ProcessEvents();

			_handler.Unsubscribe(target, callback);
			_handler.VerifyNoSubscribers();

			_handler.QueueEvent(target, new EvSimpleEvent(10));
			_handler.ProcessEvents();
		}

		[Test]
		public void TestMultipleSubscribes()
		{
			EventTarget target = EventTarget.CreateTarget();

			Action<TestJob> callback = x => { };

			_handler.Subscribe(target, new TestJob(), callback);

			Assert.Throws<MultipleSubscriptionsException<TestJob>>(() =>
				_handler.Subscribe(target, new TestJob(), callback));
		}

		[Test]
		public void TestMultipleEntities()
		{
			EventTarget target1 = EventTarget.CreateTarget();
			EventTarget target2 = EventTarget.CreateTarget();

			int value1 = 1;
			int value2 = 11;

			Action<TestJob> callback = x => { Assert.IsTrue(x.result == value1); };
			Action<TestJob> callback2 = x => { Assert.IsTrue(x.result == value2); };

			_handler.Subscribe(target1, new TestJob(value1), callback);
			_handler.Subscribe(target2, new TestJob(value2), callback2);
			_handler.QueueEvent(target1, new EvSimpleEvent(10));
			_handler.QueueEvent(target2, new EvSimpleEvent(30));

			value1 = 11;
			value2 = 41;

			_handler.ProcessEvents();
			_handler.Unsubscribe(target1, callback);
			_handler.Unsubscribe(target2, callback2);
			_handler.VerifyNoSubscribers();
		}

		[Test]
		public void TestMultipleSubscribeUnsubscribesEvents()
		{
			EventTarget target1 = EventTarget.CreateTarget();
			EventTarget target2 = EventTarget.CreateTarget();

			int value1 = 0;
			int value2 = 0;

			Action<TestJob> callback = x => { Assert.IsTrue(x.result == value1); };
			Action<TestJob> callback2 = x => { Assert.IsTrue(x.result == value2); };

			_handler.Subscribe(target1, new TestJob(value1), callback);
			_handler.Subscribe(target2, new TestJob(value2), callback2);
			_handler.Unsubscribe(target2, callback2);

			_handler.QueueEvent(target1, new EvSimpleEvent(10));
			_handler.QueueEvent(target2, new EvSimpleEvent(30));

			value1 = 10;
			Assert.IsTrue(value2 == 0);
			_handler.ProcessEvents();

			_handler.Subscribe(target2, new TestJob(value2), callback2);

			_handler.QueueEvent(target1, new EvSimpleEvent(10));
			_handler.QueueEvent(target2, new EvSimpleEvent(30));

			value1 = 20;
			value2 = 30;
			_handler.ProcessEvents();

			_handler.Unsubscribe(target1, callback);

			_handler.QueueEvent(target1, new EvSimpleEvent(10));
			_handler.QueueEvent(target2, new EvSimpleEvent(30));

			Assert.IsTrue(value1 == 20);
			value2 = 60;
			_handler.ProcessEvents();

			_handler.Subscribe(target1, new TestJob(value1), callback);

			_handler.QueueEvent(target1, new EvSimpleEvent(10));
			_handler.QueueEvent(target2, new EvSimpleEvent(30));

			value1 = 30;
			value2 = 90;
			_handler.ProcessEvents();

			_handler.Unsubscribe(target2, callback2);

			_handler.QueueEvent(target1, new EvSimpleEvent(10));
			_handler.QueueEvent(target2, new EvSimpleEvent(30));

			value1 = 40;
			Assert.IsTrue(value2 == 90);
			_handler.ProcessEvents();

			_handler.Unsubscribe(target1, callback);

			_handler.QueueEvent(target1, new EvSimpleEvent(10));
			_handler.QueueEvent(target2, new EvSimpleEvent(30));
			_handler.ProcessEvents();

			_handler.VerifyNoSubscribers();

			Assert.IsTrue(value1 == 40);
			Assert.IsTrue(value2 == 90);
		}
		
		[Test]
		public void TestUnblittableEvent()
		{
			Assert.Throws<JobTypeNotBlittableException>(() =>
			{
				EventHandlerJob<TestUnblittableJob, EvSimpleEvent> system = 
					new EventHandlerJob<TestUnblittableJob, EvSimpleEvent>();
			});
			
			Assert.Throws<EventTypeNotBlittableException>(() =>
			{
				EventHandlerJob<TestBlittableJobForUnblittable, UnblittableEvent> system = 
					new EventHandlerJob<TestBlittableJobForUnblittable, UnblittableEvent>();
			});
		}
	}
}