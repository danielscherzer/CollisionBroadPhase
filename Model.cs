using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Example
{
	/// <summary>
	/// Class that handles the game logic
	/// </summary>
	internal class Model
	{
		public Model()
		{
			CreateAsteroids(gameObjects, 1000);
			CollisionGrid = new CollisionMultiGrid(6, -1f, -1f, 2f);
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

				foreach (var collider in collidingSet)
				{
					collider.HandleCollision();
				}
				CollidingObjects = collidingSet.Count;
			}
		}

		private IReadOnlyCollection<GameObject> FindCollisions()
		{
			if (UseCollissionGrid)
			{
				CollisionGrid.Clear();
				foreach (var gameObject in gameObjects)
				{
					CollisionGrid.Add(gameObject);
				}
				return new List<GameObject>();
			}
			else
			{
				var collidingSet = new HashSet<GameObject>(); // a data structure that holds only distinct elements
				//Check all game objects for collision with any other game object. And add each colliding game object to the colliding set.
				for (int i = 0; i + 1 < gameObjects.Count; ++i)
				{
					for (int j = i + 1; j < gameObjects.Count; ++j)
					{
						if (gameObjects[i].Intersects(gameObjects[j]))
						{
							collidingSet.Add(gameObjects[i]);
							collidingSet.Add(gameObjects[j]);
						}
					}
				}
				return collidingSet;
			}
		}

		private List<GameObject> gameObjects = new List<GameObject>();
		private bool _collisionDetection = true;
		internal CollisionMultiGrid CollisionGrid { get; }

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
		public bool UseCollissionGrid { get; set; }
		public bool Freeze { get; set; }

		private static void CreateAsteroids(List<GameObject> gameObjects, uint count)
		{
			GameObject NewAsteroid(float radius, Vector2 center)
			{
				var asteroid = new GameObject(center.X, center.Y, radius);
				asteroid.OnCollision += () =>
				{
					asteroid.Velocity = -asteroid.Velocity;
				};
				return asteroid;
			}

			bool IntersectsAny(GameObject anotherObj)
			{
				foreach (var obj in gameObjects)
				{
					if (obj.Intersects(anotherObj)) return true;
				}
				return false;
			}

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
				var newAsteroid = NewAsteroid(0.005f, center);
				if (IntersectsAny(newAsteroid)) continue;
				newAsteroid.Velocity = 0.1f / MathF.Sqrt(2) * RandomVector();
				gameObjects.Add(newAsteroid);
			}
		}
	}
}
