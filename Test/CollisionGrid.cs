using Collision;
using Example;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CollisionBroadPhase.Test
{
	[TestClass]
	public class CollisionGrid
	{
		[TestMethod]
		public void CellCount0()
		{
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => new CollisionGrid<GameObject>(-1, -1, 2, 2, 0, 1));
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => new CollisionGrid<GameObject>(-1, -1, 2, 2, 1, 0));
		}

		[TestMethod]
		public void SizeNegative()
		{
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => new CollisionGrid<GameObject>(-1, -1, -2, 2, 1, 1));
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => new CollisionGrid<GameObject>(-1, -1, 2, -2, 1, 1));
		}

		[TestMethod]
		public void Size0()
		{
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => new CollisionGrid<GameObject>(-1, -1, 2, 0, 1, 1));
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => new CollisionGrid<GameObject>(-1, -1, 0, 2, 1, 1));
		}

		[TestMethod]
		public void CellCount()
		{
			for (uint cellCountX = 1u; cellCountX < 20; ++cellCountX)
			{
				for (uint cellCountY = 1u; cellCountY < 20; ++cellCountY)
				{
					var grid = new CollisionGrid<GameObject>(-1, -1, 2, 2, cellCountX, cellCountY);
					Assert.AreEqual((uint)grid.GetGrid().Length, cellCountX * cellCountY);
				}
			}
		}

		[TestMethod]
		public void FindAllCollisions()
		{
			var grid = new CollisionGrid<GameObject>(-1, -1, 2, 2, 1, 1);
			var expected = new HashSet<(GameObject, GameObject)>();
			grid.FindAllCollisions((_, __) => Assert.Fail());
			var a = new GameObject(0, 0, 0.01f);
			grid.Add(a);
			grid.FindAllCollisions((_, __) => Assert.Fail());
			grid.Add(a);
			expected.Add((a, a));
			var result = new HashSet<(GameObject, GameObject)>();
			grid.FindAllCollisions((c1, c2) => Assert.AreEqual((a, a), (c1, c2)));
			grid.FindAllCollisions((c1, c2) => result.Add((c1, c2)));
			result.SymmetricExceptWith(expected);
			//Assert.AreEqual(expected, result);
			var b = new GameObject(1, 1, 0.01f);
			grid.Add(b);
			//grid.FindAllCollisions((c1, c2) => Assert.AreEqual((a, a), (c1, c2)));
			grid.Add(b);
			//grid.FindAllCollisions((c1, c2) => Assert.AreEqual((a, a), (c1, c2)));
		}
	}
}
