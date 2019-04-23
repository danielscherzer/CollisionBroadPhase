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
			CreateAsteroids(gameObjects, 1000, 0.005f, 0.03f);
			CollisionMultiGrid = new CollisionMultiGrid<GameObject>(4, 4, -1f, -1f, 2f);
			CollisionGrid = new CollisionGrid<GameObject>(-1f, -1f, 2f, 2f, 16, 16);
		}

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
				CollisionTime = stopWatch.ElapsedMilliseconds;

				foreach (var (collider1, collider2) in collidingSet)
				{
					HandleCollision(collider1, collider2);
					HandleCollision(collider2, collider1);
				}
				CollidingObjects = collidingSet.Count;
			}
		}

		private IReadOnlyCollection<(GameObject, GameObject)> FindCollisions()
		{
			var multiGridCollision = MultiGridCollision();
			var gridCollision = GridCollision();
			var diff = new HashSet<(GameObject, GameObject)>(gridCollision);
			diff.SymmetricExceptWith(multiGridCollision);
			CollGridDebug = diff;
			if(0 != diff.Count)
			{
				Freeze = true;
			}
			if (UseCollissionGrid)
			{
				return multiGridCollision;
			}
			else
			{
				return BruteForceCollision();
			}
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

		private List<GameObject> gameObjects = new List<GameObject>();
		private bool _collisionDetection = true;
		internal CollisionMultiGrid<GameObject> CollisionMultiGrid { get; }
		internal CollisionGrid<GameObject> CollisionGrid { get; }
		internal IReadOnlyCollection<(GameObject, GameObject)> CollGridDebug { get; private set; } = new List<(GameObject, GameObject)>();

		public int ObjectCount => gameObjects.Count;
		public bool CollisionDetection
		{
			get => _collisionDetection;
			set
			{
				_collisionDetection = value;
				CollidingObjects = 0;
				CollisionTime = 0;
			}
		}
		public int CollidingObjects { get; private set; }
		public long CollisionTime { get; private set; }
		public bool UseCollissionGrid { get; set; } = true;
		public bool Freeze { get; set; }

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
				newAsteroid.Velocity = 0.91f / MathF.Sqrt(2) * RandomVector();
				gameObjects.Add(newAsteroid);
			}
		}

		private void HandleCollision(GameObject a, GameObject b)
		{
			var diff = a.Center - b.Center;
			a.Velocity = diff;
		}
	}
}
