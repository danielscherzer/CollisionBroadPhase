using System;
using System.Collections.Generic;
using System.Numerics;

namespace Example
{
	class Scene
	{
		public Scene(int objectCount, float objectMinSize, float objectSizeVariation)
		{
			randomNumber = new Random(12);
			while (gameObjects.Count < objectCount)
			{
				gameObjects.Add(CreateAsteroid(objectMinSize, objectSizeVariation));
			}
		}

		private GameObject CreateAsteroid(float objectMinSize, float objectSizeVariation)
		{
			Vector2 RandomVector()
			{
				var x = randomNumber.NextDouble() * 2 - 1;
				var y = randomNumber.NextDouble() * 2 - 1;
				return new Vector2((float)x, (float)y);
			}

			var center = RandomVector();
			var radius = (float)randomNumber.NextDouble();
			radius = MathF.Pow(radius, 8f); // more small ones than big ones
			radius = radius * objectSizeVariation + objectMinSize;
			var newAsteroid = new GameObject(center.X, center.Y, radius)
			{
				Velocity = 0.1f * RandomVector()
			};
			return newAsteroid;
		}

		public IReadOnlyList<GameObject> GameObjects => gameObjects;

		/// <summary>
		/// Scene update. Should be called once a frame: Moves all objects.
		/// <param name="frameTime">Time in seconds since the last update.</param>
		/// </summary>
		public void Update(float frameTime)
		{
			foreach (var obj in gameObjects)
			{
				obj.Update(frameTime);
			}
		}

		private List<GameObject> gameObjects = new List<GameObject>();
		private readonly Random randomNumber;
	}
}
