using System;
using NUnit.Framework;
using UnityEvents.Internal;

namespace UnityEvents.Test
{
	public class TestEventTarget
	{
		[Test]
		public void TestGameObjectRange()
		{
			int minId = int.MinValue;
			int maxId = int.MaxValue;
			int allBits = unchecked((int)0xffffffff);
			
			// This represents the range for gameobjects, should only be the first 32 bits of a ulong
			EventTarget minTarget = new EventTarget(minId);
			EventTarget maxTarget = new EventTarget(maxId);
			EventTarget allBitsTarget = new EventTarget(allBits);

			ulong zeroedBits = 0xffffffff;
			zeroedBits <<= 32;
			
			Assert.IsTrue((minTarget.id & zeroedBits) == 0);
			Assert.IsTrue((maxTarget.id & zeroedBits) == 0);
			Assert.IsTrue((allBitsTarget.id & zeroedBits) == 0);
			Assert.IsTrue((EventTarget.CreateTarget().id & zeroedBits) != 0);
		}

		[Test]
		public void TestReservation()
		{
			EventTargetReservation reservation = EventTarget.ReserveTargets(3);
			EventTarget target1 = reservation.GetEntityTarget(0);
			EventTarget target2 = reservation.GetEntityTarget(1);
			EventTarget target3 = reservation.GetEntityTarget(2);
			
			Assert.IsFalse(target1.Equals(target2));
			Assert.IsFalse(target2.Equals(target3));
			Assert.IsFalse(target1.Equals(target3));

			Assert.Throws<IndexOutOfReservedTargetsException>(() => reservation.GetEntityTarget(-1));
			Assert.Throws<IndexOutOfReservedTargetsException>(() => reservation.GetEntityTarget(3));
		}

		[Test]
		public void TestOverflow()
		{
			Assert.Throws<OverflowException>(() => EventTarget.ReserveTargets(ulong.MaxValue));
		}
	}
}
