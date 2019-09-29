using System;
using System.Collections.Generic;
using System.Diagnostics;
using Zenseless.Patterns;

namespace Example
{
	/// <summary>
	/// Class that handles the collision detection
	/// </summary>
	internal class CollisionDetection
	{
		public CollisionDetection(IGameObjectProvider scene, ICollisionParameters parameters)
		{
			this.parameters = parameters;
			this.scene = scene;
			Recreate(scene);
		}

		internal void Recreate(IGameObjectProvider scene)
		{
			CollisionCount = 0;

			var level = (int)Math.Ceiling(Math.Log(parameters.CellCount) / Math.Log(2.0));
			CollisionMultiGrid = new CollisionMultiGrid<GameObject>(level - 1, level, -1f, -1f, 2f);
			CollisionGrid = new CollisionGrid<GameObject>(-1f, -1f, 2f, 2f, parameters.CellCount, parameters.CellCount);

			//SAP only usable if iterative so do not add/delete GameObjects without adding/removing them from the SAP structure too!
			CollisionSAP.Clear();
			foreach (var gameObject in GameObjects)
			{
				CollisionSAP.Add(gameObject);
			}
			CollisionPersistentSAP.Clear();
			foreach (var gameObject in GameObjects)
			{
				CollisionPersistentSAP.Add(gameObject);
			}
			collisionTime.Clear();
		}

		//TODO: public int BroadPhaseCollisionCount { get; private set; }
		public int CollisionCount { get; private set; } = 10000;

		public float CollisionTimeMsec { get; private set; } = 5.0001f;

		internal IEnumerable<(GameObject, GameObject)> CollisionAlgoDifference { get; private set; } = new List<(GameObject, GameObject)>();

		internal IReadOnlyList<GameObject> GameObjects => scene.GameObjects;

		internal void Update(float frameTime)
		{
			if (parameters.CollisionDetection)
			{
				var stopWatch = new Stopwatch();
				stopWatch.Start();
				var collidingSet = FindCollisions(parameters.CollisionMethod);
				stopWatch.Stop();

				collisionTime.NewSample(stopWatch.Elapsed.TotalMilliseconds);
				CollisionTimeMsec = (float)Math.Round(collisionTime.SmoothedValue, 2);
				foreach (var (collider1, collider2) in collidingSet)
				{
					collider1.HandleCollision(collider2);
					collider2.HandleCollision(collider1);
				}
				CollisionCount = collidingSet.Count;

				if (parameters.DebugAlgo)
				{
					var diff = new HashSet<(GameObject, GameObject)>(GridCollision());
					diff.SymmetricExceptWith(collidingSet);
					CollisionAlgoDifference = diff;
				}
			}
		}

		private ICollisionParameters parameters;
		private ExponentialSmoothing collisionTime = new ExponentialSmoothing(0.01);
		private IGameObjectProvider scene;

		internal CollisionGrid<GameObject> CollisionGrid { get; private set; }
		internal CollisionMultiGrid<GameObject> CollisionMultiGrid { get; private set; }
		private CollisionSAP<GameObject> CollisionSAP { get; } = new CollisionSAP<GameObject>();
		private CollisionPersistentSAP<GameObject> CollisionPersistentSAP { get; } = new CollisionPersistentSAP<GameObject>();

		private IReadOnlyCollection<(GameObject, GameObject)> FindCollisions(CollisionMethodTypes type)
		{
			HashSet<(GameObject, GameObject)> result;
			switch (type)
			{
				case CollisionMethodTypes.BruteForce: result = BruteForceCollision(); break;
				case CollisionMethodTypes.Grid: result = GridCollision(); break;
				case CollisionMethodTypes.MultiGrid: result = MultiGridCollision(); break;
				case CollisionMethodTypes.SAP_X: result = SAPCollision(); break;
				case CollisionMethodTypes.PersistentSAP: result = PersistentSAPCollision(); break;
				default: result = new HashSet<(GameObject, GameObject)>(); break;
			}
			return result;
		}

		private HashSet<(GameObject, GameObject)> BruteForceCollision()
		{
			// a data structure that holds only distinct elements
			var collisions = new HashSet<(GameObject, GameObject)>();
			//Check all game objects for collision with any other game object. And add each colliding game object to the colliding set.
			for (int i = 0; i + 1 < GameObjects.Count; ++i)
			{
				for (int j = i + 1; j < GameObjects.Count; ++j)
				{
					TestForCollision(collisions, GameObjects[i], GameObjects[j]);
				}
			}
			return collisions;
		}

		private static void TestForCollision(HashSet<(GameObject, GameObject)> collidingSet, GameObject a, GameObject b)
		{
			if (a.Intersects(b))
			{
				collidingSet.Add(a.GetHashCode() < b.GetHashCode() ? (a, b) : (b, a));
			}
		}

		private HashSet<(GameObject, GameObject)> GridCollision()
		{
			CollisionGrid.Clear();
			foreach (var gameObject in GameObjects)
			{
				CollisionGrid.Add(gameObject);
			}
			var collisions = new HashSet<(GameObject, GameObject)>();
			CollisionGrid.FindAllCollisions((a, b) => TestForCollision(collisions, a, b));
			return collisions;
		}

		private HashSet<(GameObject, GameObject)> MultiGridCollision()
		{
			CollisionMultiGrid.Clear();
			foreach (var gameObject in GameObjects)
			{
				CollisionMultiGrid.Add(gameObject);
			}
			var collisions = new HashSet<(GameObject, GameObject)>();
			CollisionMultiGrid.FindAllCollisions((a, b) => TestForCollision(collisions, a, b));
			return collisions;
		}

		private HashSet<(GameObject, GameObject)> PersistentSAPCollision()
		{
			CollisionPersistentSAP.UpdateBounds();
			var collisions = new HashSet<(GameObject, GameObject)>();
			CollisionPersistentSAP.FindAllCollisions((a, b) => TestForCollision(collisions, a, b));
			return collisions;
		}

		private HashSet<(GameObject, GameObject)> SAPCollision()
		{
			CollisionSAP.UpdateBounds();
			var collisions = new HashSet<(GameObject, GameObject)>();
			CollisionSAP.FindAllCollisions((a, b) => TestForCollision(collisions, a, b));
			return collisions;
		}
	}
}
