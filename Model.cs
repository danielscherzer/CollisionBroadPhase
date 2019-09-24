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
		public Model()
		{
			ObjectCount = 2000;
			CellCount = 32;
		}
		public enum CollisionMethodTypes { BruteForce, Grid, MultiGrid, SAP_X, PersistentSAP };

		private Scene scene;

		[Increment(1000)]
		public int ObjectCount
		{
			get => scene.ObjectCount;
			set
			{
				scene = new Scene();
				scene.ObjectCount = value;
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
		}

		public int CellCount
		{
			get => CollisionGrid.CellCountX;
			set
			{
				var level = (int)Math.Ceiling(Math.Log(value) / Math.Log(2.0));
				var cells = (int)Math.Pow(2, level);
				CollisionMultiGrid = new CollisionMultiGrid<GameObject>(level - 1, level, -1f, -1f, 2f);
				CollisionGrid = new CollisionGrid<GameObject>(-1f, -1f, 2f, 2f, value, value);
			}
		}

		public bool CollisionDetection
		{
			get => _collisionDetection;
			set
			{
				_collisionDetection = value;
				CollisionCount = 0;
			}
		}
		private bool _collisionDetection = true;

		public int CollisionCount { get; private set; }

		public float CollisionTimeMsec { get; private set; }

		public CollisionMethodTypes CollisionMethod
		{
			get => _collisionMethod; set
			{
				_collisionMethod = value;
				collisionTime.Clear();
			}
		}

		public bool Freeze { get; set; } = true;

		public bool DebugAlgo { get; set; } = false;

		internal IEnumerable<(GameObject, GameObject)> CollisionAlgoDifference { get; private set; } = new List<(GameObject, GameObject)>();

		internal IReadOnlyList<GameObject> GameObjects => scene.GameObjects;

		/// <summary>
		/// Game logic update. Should be called once a frame: Moves all objects and resolves collision.
		/// <param name="frameTime">Time in seconds since the last update.</param>
		/// </summary>
		internal void Update(float frameTime)
		{
			scene.Update(Freeze ? 0f : frameTime);
			if (CollisionDetection)
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

		private ExponentialSmoothing collisionTime = new ExponentialSmoothing(0.01);
		private CollisionMethodTypes _collisionMethod = CollisionMethodTypes.SAP_X;

		internal CollisionGrid<GameObject> CollisionGrid { get; private set; }
		internal CollisionMultiGrid<GameObject> CollisionMultiGrid { get; private set; }
		private CollisionSAP<GameObject> CollisionSAP { get; } = new CollisionSAP<GameObject>();
		private CollisionPersistentSAP<GameObject> CollisionPersistentSAP { get; } = new CollisionPersistentSAP<GameObject>();

		private IReadOnlyCollection<(GameObject, GameObject)> FindCollisions()
		{
			HashSet<(GameObject, GameObject)> result;
			switch (CollisionMethod)
			{
				case CollisionMethodTypes.BruteForce: result = BruteForceCollision(); break;
				case CollisionMethodTypes.Grid: result = GridCollision(); break;
				case CollisionMethodTypes.MultiGrid: result = MultiGridCollision(); break;
				case CollisionMethodTypes.SAP_X: result = SAPCollision(); break;
				case CollisionMethodTypes.PersistentSAP: result = PersistentSAPCollision(); break;
				default: result = new HashSet<(GameObject, GameObject)>(); break;
			}
			if (!DebugAlgo) return result;

			var diff = new HashSet<(GameObject, GameObject)>(GridCollision());
			diff.SymmetricExceptWith(result);
			CollisionAlgoDifference = diff;
			if (0 != diff.Count)
			{
				Freeze = true;
			}
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

		private static List<GameObject> CreateAsteroids(int count, float minSize, float variation)
		{
			var gameObjects = new List<GameObject>(count);
			GameObject NewAsteroid(float radius, Vector2 center) => new GameObject(center.X, center.Y, radius);

			var randomNumber = new Random(12);
			Vector2 RandomVector()
			{
				var x = randomNumber.NextDouble() * 2 - 1;
				var y = randomNumber.NextDouble() * 2 - 1;
				return new Vector2((float)x, (float)y);
			}

			while (gameObjects.Count < count)
			{
				var center = RandomVector();
				var radius = (float)randomNumber.NextDouble();
				radius = MathF.Pow(radius, 8f); // more small ones than big ones
				radius = radius * variation + minSize;
				var newAsteroid = NewAsteroid(radius, center);
				newAsteroid.Velocity = 0.1f * RandomVector();
				gameObjects.Add(newAsteroid);
			}
			return gameObjects;
		}

		private static void HandleCollision(GameObject a, GameObject b)
		{
			var diff = a.Center - b.Center;
			a.Velocity = Vector2.Normalize(diff) * a.Velocity.Length();
		}
	}
}
