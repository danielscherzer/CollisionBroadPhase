using Collision;
using Example;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CollisionBroadPhase.Test
{
	[TestClass]
	public class CollisionGridTests
	{
		[TestMethod]
		public void CellCount0()
		{
			Assert.ThrowsException<ArgumentException>(() => new CollisionGrid<GameObject>(-1, -1, 2, 2, 0, 1));
			Assert.ThrowsException<ArgumentException>(() => new CollisionGrid<GameObject>(-1, -1, 2, 2, 1, 0));
		}

		[TestMethod]
		public void SizeNegative()
		{
			Assert.ThrowsException<ArgumentException>(() => new CollisionGrid<GameObject>(-1, -1, -2, 2, 1, 1));
			Assert.ThrowsException<ArgumentException>(() => new CollisionGrid<GameObject>(-1, -1, 2, -2, 1, 1));
		}

		[TestMethod]
		public void Size0()
		{
			Assert.ThrowsException<ArgumentException>(() => new CollisionGrid<GameObject>(-1, -1, 2, 0, 1, 1));
			Assert.ThrowsException<ArgumentException>(() => new CollisionGrid<GameObject>(-1, -1, 0, 2, 1, 1));
		}

		[TestMethod]
		public void CellCount()
		{
			for (int cellCountX = 1; cellCountX < 20; ++cellCountX)
			{
				for (int cellCountY = 1; cellCountY < 20; ++cellCountY)
				{
					var grid = new CollisionGrid<GameObject>(-1, -1, 2, 2, cellCountX, cellCountY);
					Assert.AreEqual(grid.CellCountX * grid.CellCountY, cellCountX * cellCountY);
				}
			}
		}

		[TestMethod]
		public void FindAllCollisionsSingleObject()
		{
			var grid = new CollisionGrid<GameObject>(-1, -1, 2, 2, 1, 1);
			grid.FindAllCollisions((_, __) => Assert.Fail());

			var a = new GameObject(0, 0, 0.01f);
			grid.Add(a);
			grid.FindAllCollisions((_, __) => Assert.Fail());

			grid.Add(a);
			grid.FindAllCollisions((c1, c2) => Assert.AreEqual((a, a), (c1, c2)));

			var expected = new HashSet<(ICollider, ICollider)> { (a, a) };
			var result = new HashSet<(ICollider, ICollider)>();
			grid.FindAllCollisions((c1, c2) => result.Add((c1, c2)));
			CollectionAssert.AreEqual(expected.ToList(), result.ToList());
		}

		private static void AddOrdererdPair(HashSet<(ICollider, ICollider)> collidingSet, ICollider a, ICollider b)
		{
			collidingSet.Add(a.GetHashCode() < b.GetHashCode() ? (a, b) : (b, a));
		}

		private static void ExactCollision(HashSet<(ICollider, ICollider)> collidingSet, ICollider a, ICollider b)
		{
			if (a.Intersects(b))
			{
				AddOrdererdPair(collidingSet, a, b);
			}
		}

		private static string Print(HashSet<(ICollider, ICollider)> collisionPairs) => string.Join(',', collisionPairs);

		private static IEnumerable<object[]> GetGameObjects()
		{
			var a = new GameObject(0, 0, 0.1f);
			yield return new object[] { new GameObject[] { a, new GameObject(1, 1, 0.1f) }, Array.Empty<(int, int)>() };
			yield return new object[] { new GameObject[] { a, new GameObject(0.01f, 0, 0.1f) }, new (int, int)[] { (0, 1) } };
			yield return new object[] { new GameObject[] { a, new GameObject(0.19f, 0, 0.1f) }, new (int, int)[] { (0, 1) } };
			yield return new object[] { new GameObject[] { a, new GameObject(0.2f, 0, 0.1f) }, Array.Empty<(int, int)>() };
		}

		[DataTestMethod()]
		[DynamicData(nameof(GetGameObjects), DynamicDataSourceType.Method)]
		public void FindAllCollisions(GameObject[] gameObjects, (int, int)[] collidingIds)
		{
			var grid = new CollisionGrid<GameObject>(-1, -1, 2, 2, 1, 1);
			foreach (var gameObject in gameObjects) grid.Add(gameObject);

			var expected = new HashSet<(ICollider, ICollider)>();
			var result = new HashSet<(ICollider, ICollider)>();
			foreach ((int a, int b) in collidingIds) AddOrdererdPair(expected, gameObjects[a], gameObjects[b]);
			grid.FindAllCollisions((c1, c2) => CollisionDetection.ExactCollision(result, c1, c2));
			CollectionAssert.AreEqual(expected.ToList(), result.ToList(), $"\n\tExpected={Print(expected)}\n\tActual={Print(result)}");
		}
	}
}
