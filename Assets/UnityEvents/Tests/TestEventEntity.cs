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
			EventEntity minEntity = new EventEntity(minId);
			EventEntity maxEntity = new EventEntity(maxId);
			EventEntity allBitsEntity = new EventEntity(allBits);

			ulong zeroedBits = 0xffffffff;
			zeroedBits <<= 32;
			
			Assert.IsTrue((minEntity.id & zeroedBits) == 0);
			Assert.IsTrue((maxEntity.id & zeroedBits) == 0);
			Assert.IsTrue((allBitsEntity.id & zeroedBits) == 0);
			Assert.IsTrue((EventEntity.CreateEntity().id & zeroedBits) != 0);
		}
	}
}
