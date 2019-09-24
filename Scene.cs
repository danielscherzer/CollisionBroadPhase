using System;
using System.Collections.Generic;
using System.Numerics;

namespace Example
{
	class Scene
	{
		/// <summary>
		/// Scene update. Should be called once a frame: Moves all objects.
		/// <param name="frameTime">Time in seconds since the last update.</param>
		/// </summary>
		internal void Update(float frameTime)
		{
			foreach (var obj in gameObjects)
			{
				obj.Update(frameTime);
			}
		}

		[Increment(1000)]
		internal int ObjectCount
		{
			get => gameObjects.Count;
			set
			{
				gameObjects = CreateAsteroids(value, 0.01f, 0.002f);
			}
		}

		internal IReadOnlyList<GameObject> GameObjects => gameObjects;

		private List<GameObject> gameObjects = new List<GameObject>();

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

	}
}
