using System;
using NUnit.Framework;

namespace UnityEvents.Test
{
	public class TestEventEntity
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
	}
}
