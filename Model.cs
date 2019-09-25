using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Zenseless.Patterns;

namespace Example
{
	/// <summary>
	/// Class that handles the game logic
	/// </summary>
	internal class Model
	{
		public Model(IParameters parameters)
		{
			this.parameters = parameters;
			parameters.PropertyChanged += (s, e) => Change(e.PropertyName);
			Recreate();
		}

		internal void Recreate()
		{
			CollisionCount = 0;
			scene = new Scene(parameters.ObjectCount, parameters.ObjectMinSize, parameters.ObjectSizeVariation);

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

		/// <summary>
		/// Game logic update. Should be called once a frame: Moves all objects and resolves collision.
		/// <param name="frameTime">Time in seconds since the last update.</param>
		/// </summary>
		internal void Update(float frameTime)
		{
			frameTime = 1f / 60f; //TODO: check constant movement delta
			scene.Update(parameters.Freeze ? 0f : frameTime);
			if (parameters.CollisionDetection)
			{
				var stopWatch = new Stopwatch();
				stopWatch.Start();
				var collidingSet = FindCollisions();
				stopWatch.Stop();
				collisionTime.NewSample(stopWatch.Elapsed.TotalMilliseconds);
				CollisionTimeMsec = (float)Math.Round(collisionTime.SmoothedValue, 2);
				foreach (var (collider1, collider2) in collidingSet)
				{
					HandleCollision(collider1, collider2);
					HandleCollision(collider2, collider1);
				}
				CollisionCount = collidingSet.Count;
			}
		}

		private IParameters parameters;
		private ExponentialSmoothing collisionTime = new ExponentialSmoothing(0.01);
		private Scene scene;

		private void Change(string propertyName)
		{
			var lightParams = new string[] { nameof(parameters.Freeze), nameof(parameters.CollisionDetection) };
			if (!lightParams.Contains(propertyName))
			{
				Recreate();
			}
		}

		internal CollisionGrid<GameObject> CollisionGrid { get; private set; }
		internal CollisionMultiGrid<GameObject> CollisionMultiGrid { get; private set; }
		private CollisionSAP<GameObject> CollisionSAP { get; } = new CollisionSAP<GameObject>();
		private CollisionPersistentSAP<GameObject> CollisionPersistentSAP { get; } = new CollisionPersistentSAP<GameObject>();

		private IReadOnlyCollection<(GameObject, GameObject)> FindCollisions()
		{
			HashSet<(GameObject, GameObject)> result;
			switch (parameters.CollisionMethod)
			{
				case CollisionMethodTypes.BruteForce: result = BruteForceCollision(); break;
				case CollisionMethodTypes.Grid: result = GridCollision(); break;
				case CollisionMethodTypes.MultiGrid: result = MultiGridCollision(); break;
				case CollisionMethodTypes.SAP_X: result = SAPCollision(); break;
				case CollisionMethodTypes.PersistentSAP: result = PersistentSAPCollision(); break;
				default: result = new HashSet<(GameObject, GameObject)>(); break;
			}
			if (!parameters.DebugAlgo) return result;

			var diff = new HashSet<(GameObject, GameObject)>(GridCollision());
			diff.SymmetricExceptWith(result);
			CollisionAlgoDifference = diff;
			return result;
		}

		private HashSet<(GameObject, GameObject)> BruteForceCollision()
		{
			// a data structure that holds only distinct elements
			var collidingSet = new HashSet<(GameObject, GameObject)>();
			//Check all game objects for collision with any other game object. And add each colliding game object to the colliding set.
			for (int i = 0; i + 1 < GameObjects.Count; ++i)
			{
				for (int j = i + 1; j < GameObjects.Count; ++j)
				{
					TestForCollision(collidingSet, GameObjects[i], GameObjects[j]);
				}
			}
			return collidingSet;
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
			CollisionMultiGrid.FindCollision((a, b) => TestForCollision(collisions, a, b));
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

		private static void HandleCollision(GameObject a, GameObject b)
		{
			var diff = a.Center - b.Center;
			a.Velocity = Vector2.Normalize(diff) * a.Velocity.Length();
		}
	}
}
