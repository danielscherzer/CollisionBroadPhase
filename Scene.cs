using System;
using System.Collections.Generic;
using System.Numerics;

namespace Example
{
	class Scene
	{
		public Scene(int objectCount, float objectMinSize, float objectSizeVariation)
		{
			GameObject NewAsteroid(float radius, Vector2 center) => new GameObject(center.X, center.Y, radius);

			var randomNumber = new Random(12);
			Vector2 RandomVector()
			{
				var x = randomNumber.NextDouble() * 2 - 1;
				var y = randomNumber.NextDouble() * 2 - 1;
				return new Vector2((float)x, (float)y);
			}

			while (gameObjects.Count < objectCount)
			{
				var center = RandomVector();
				var radius = (float)randomNumber.NextDouble();
				radius = MathF.Pow(radius, 8f); // more small ones than big ones
				radius = radius * objectSizeVariation + objectMinSize;
				var newAsteroid = NewAsteroid(radius, center);
				newAsteroid.Velocity = 0.1f * RandomVector();
				gameObjects.Add(newAsteroid);
			}
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
	}
}
