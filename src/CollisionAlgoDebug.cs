using Collision;
using System.Collections.Generic;
using System.Linq;

namespace Example
{
	class CollisionAlgoDebug
	{
		public CollisionAlgoDebug()
		{
			collisionGrid = new CollisionGrid<ICollider>(-1f, -1f, 2f, 2f, 32, 32);
			result = new HashSet<(ICollider, ICollider)>();
		}

		public void Check(IColliderProvider scene, HashSet<(ICollider, ICollider)> collidingSet)
		{
			collisionGrid.Clear();
			foreach (var collider in scene.Collider)
			{
				collisionGrid.Add(collider);
			}
			void TestForCollision(ICollider a, ICollider b)
			{
				if (a.Intersects(b))
				{
					result.Add(a.GetHashCode() < b.GetHashCode() ? (a, b) : (b, a));
				}
			}
			collisionGrid.FindAllCollisions(TestForCollision);

			result.SymmetricExceptWith(collidingSet);
		}

		public IEnumerable<(ICollider, ICollider)> CollisionAlgoDifference => result;
		public IEnumerable<ICollider> Errors => result.SelectMany((tuple) => new ICollider[] { tuple.Item1, tuple.Item2 });

		private readonly CollisionGrid<ICollider> collisionGrid;
		private readonly HashSet<(ICollider, ICollider)> result;
	}
}
