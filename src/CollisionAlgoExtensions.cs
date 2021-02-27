using Collision;
using System.Collections.Generic;
using System.Linq;

namespace Example
{
	internal static class CollisionAlgoExtensions
	{
		public static HashSet<(ICollider, ICollider)> FindAllCollisions(this CollisionGrid<ICollider> collisionGrid, IColliderProvider scene)
		{
			var result = new HashSet<(ICollider, ICollider)>();
			collisionGrid.Clear();
			foreach (var collider in scene.Collider)
			{
				collisionGrid.Add(collider);
			}
			collisionGrid.FindAllCollisions((c1, c2) => CollisionDetection.ExactCollision(result, c1, c2));
			return result;
		}

		public static IEnumerable<ICollider> Flatten(this IEnumerable<(ICollider, ICollider)> collidingSetA) => collidingSetA.SelectMany((tuple) => new ICollider[] { tuple.Item1, tuple.Item2 }).Distinct();
	}
}
