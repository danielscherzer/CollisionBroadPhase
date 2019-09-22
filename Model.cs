using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Example
{
	/// <summary>
	/// Class that handles the game logic
	/// </summary>
	internal class Model
	{
		public Model()
		{
			var objects = 2000u;
			var level = 5;
			CreateAsteroids(gameObjects, objects, 0.01f, 0.002f);
			var cells = (int)Math.Pow(2, level);
			CollisionMultiGrid = new CollisionMultiGrid<GameObject>(level - 1, level, -1f, -1f, 2f);
			CollisionGrid = new CollisionGrid<GameObject>(-1f, -1f, 2f, 2f, cells, cells);
			//SAP only usable if iterative so do not add/delete GameObjects without adding/removing them from the SAP structure too!
			CollisionSAP = new CollisionSAP<GameObject>();
			foreach (var gameObject in gameObjects)
			{
				CollisionSAP.Add(gameObject);
			}
		}

		public enum CollisionMethodTypes { BruteForce, Grid, MultiGrid, SAP_X };
		public int ObjectCount => gameObjects.Count;
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
		public double CollisionTimeMsec { get; private set; }
		public CollisionMethodTypes CollisionMethod { get; set; } = CollisionMethodTypes.MultiGrid;
		public bool Freeze { get; set; } = true;
		public bool DebugAlgo { get; set; } = false;
		internal IEnumerable<(GameObject, GameObject)> CollisionAlgoDifference { get; private set; } = new List<(GameObject, GameObject)>();

		internal IEnumerable<GameObject> GetGameObjects()
		{
			return gameObjects;
		}

		/// <summary>
		/// Game logic update. Should be called once a frame: Moves all objects and resolves collision.
		/// <param name="frameTime">Time in seconds since the last update.</param>
		/// </summary>
		internal void Update(float frameTime)
		{
			var deltaTime = Freeze ? 0f : frameTime;
			foreach (var obj in gameObjects)
			{
				obj.Update(deltaTime);
			}
			if (CollisionDetection)
			{
				var stopWatch = new Stopwatch();
				stopWatch.Start();
				var collidingSet = FindCollisions();
				stopWatch.Stop();
				CollisionTimeMsec = stopWatch.Elapsed.TotalMilliseconds;
				foreach (var (collider1, collider2) in collidingSet)
				{
					HandleCollision(collider1, collider2);
					HandleCollision(collider2, collider1);
				}
				CollisionCount = collidingSet.Count;
			}
		}

		private List<GameObject> gameObjects = new List<GameObject>();
		internal CollisionGrid<GameObject> CollisionGrid { get; }
		internal CollisionMultiGrid<GameObject> CollisionMultiGrid { get; }
		private CollisionSAP<GameObject> CollisionSAP { get; }

		private IReadOnlyCollection<(GameObject, GameObject)> FindCollisions()
		{
			HashSet<(GameObject, GameObject)> result;
			switch (CollisionMethod)
			{
				case CollisionMethodTypes.BruteForce: result = BruteForceCollision(); break;
				case CollisionMethodTypes.Grid: result = GridCollision(); break;
				case CollisionMethodTypes.MultiGrid: result = MultiGridCollision(); break;
				case CollisionMethodTypes.SAP_X: result = SAPCollision(); break;
				default: result = new HashSet<(GameObject, GameObject)>(); break;
			}
			if(!DebugAlgo) return result;

			var diff = new HashSet<(GameObject, GameObject)>(BruteForceCollision());
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
			var collidingSet = new HashSet<(GameObject,GameObject)>(); // a data structure that holds only distinct elements
			//Check all game objects for collision with any other game object. And add each colliding game object to the colliding set.
			for (int i = 0; i + 1 < gameObjects.Count; ++i)
			{
				for (int j = i + 1; j < gameObjects.Count; ++j)
				{
					TestForCollision(collidingSet, gameObjects[i], gameObjects[j]);
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
			foreach(var gameObject in gameObjects)
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
			foreach (var gameObject in gameObjects)
			{
				CollisionMultiGrid.Add(gameObject);
			}
			var collisions = new HashSet<(GameObject, GameObject)>();
			CollisionMultiGrid.FindCollision((a, b) => TestForCollision(collisions, a, b));
			return collisions;
		}

		private HashSet<(GameObject, GameObject)> SAPCollision()
		{
			CollisionSAP.UpdateBounds();
			var collisions = new HashSet<(GameObject, GameObject)>();
			CollisionSAP.FindAllCollisions((a, b) => TestForCollision(collisions, a, b));
			return collisions;
		}

		private static void CreateAsteroids(List<GameObject> gameObjects, uint count, float minSize, float variation)
		{
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
		}

		private static void HandleCollision(GameObject a, GameObject b)
		{
			var diff = a.Center - b.Center;
			a.Velocity = diff.Normalized() * a.Velocity.Length;
		}
	}
}
