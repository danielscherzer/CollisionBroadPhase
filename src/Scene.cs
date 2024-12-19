using Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Example;

internal class Scene
{
	public Scene(int objectCount, float objectMinRadius, float objectRadiusVariation)
	{
		randomNumber = new Random(12);
		GameObject CreateAsteroid(float centerX, float centerY)
		{
			var radius = (float)randomNumber.NextDouble();
			radius = MathF.Pow(radius, 8f); // more small ones than big ones
			radius = (radius * objectRadiusVariation) + objectMinRadius;
			var newAsteroid = new GameObject(centerX, centerY, radius)
			{
				Velocity = RandomVelocity
			};
			return newAsteroid;
		}

		// need row/column number that at least allows grid to contain <objectCount> elements
		var countSqrt = (int)MathF.Ceiling(MathF.Sqrt(objectCount));
		var circles = new List<Circle>(objectCount);

		var delta = 2f / countSqrt;
		var min = -1f + (0.5f * delta);
		// create a grid of non overlapping minimal sized objects
		for (int i = 0; i < objectCount; ++i)
		{
			var column = i % countSqrt;
			var row = i / countSqrt;
			var x = min + (delta * column);
			var y = min + (delta * row);
			var circle = new Circle(x, y, objectMinRadius);
			circles.Add(circle);
		}
		// try to grow them as long as no overlap
		gameObjects = circles
			.OrderBy(item => randomNumber.Next())
			.Select(circle => CreateAsteroid(circle.CenterX, circle.CenterY))
			.ToList();
	}

	public IReadOnlyList<GameObject> GameObjects => gameObjects;

	public Vector2 RandomVelocity => 0.1f * RandomVector();

	/// <summary>
	/// Scene update. Should be called once a frame: Moves all objects.
	/// <param name="frameTime">Time in seconds since the last update.</param>
	/// </summary>
	public void Update(float frameTime) => gameObjects.ForEach(obj => obj.Update(frameTime));

	private struct Circle : ICircle2dCollider
	{
		public Circle(float centerX, float centerY, float radius)
		{
			CenterX = centerX;
			CenterY = centerY;
			Radius = radius;
		}

		public float CenterX { get; set; }
		public float CenterY { get; set; }
		public float Radius { get; set; }
	}

	private readonly List<GameObject> gameObjects = new();
	private readonly Random randomNumber;

	private Vector2 RandomVector()
	{
		var x = (randomNumber.NextDouble() * 2) - 1;
		var y = (randomNumber.NextDouble() * 2) - 1;
		return new Vector2((float)x, (float)y);
	}
}
