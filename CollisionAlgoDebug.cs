using System.Collections.Generic;
using System.Linq;

namespace Example
{
	class CollisionAlgoDebug
	{
		public CollisionAlgoDebug()
		{
			collisionGrid = new CollisionGrid<GameObject>(-1f, -1f, 2f, 2f, 32, 32);
			result = new HashSet<(GameObject, GameObject)>();
		}

		public void Check(IGameObjectProvider scene, HashSet<(GameObject, GameObject)> collidingSet)
		{
			collisionGrid.Clear();
			foreach (var gameObject in scene.GameObjects)
			{
				collisionGrid.Add(gameObject);
			}
			void TestForCollision(GameObject a, GameObject b)
			{
				if (a.Intersects(b))
				{
					result.Add(a.GetHashCode() < b.GetHashCode() ? (a, b) : (b, a));
				}
			}
			collisionGrid.FindAllCollisions(TestForCollision);

			result.SymmetricExceptWith(collidingSet);
		}

		public IEnumerable<(GameObject, GameObject)> CollisionAlgoDifference => result;
		public IEnumerable<GameObject> Errors => result.SelectMany((tuple) => new GameObject[] { tuple.Item1, tuple.Item2 });

		private readonly CollisionGrid<GameObject> collisionGrid;
		private readonly HashSet<(GameObject, GameObject)> result;
	}
}
