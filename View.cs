using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Example
{
	/// <summary>
	/// Class that handles all the actual drawing using OpenGL.
	/// </summary>
	internal class View
	{
		public View()
		{
			GL.ClearColor(Color.Black);
		}

		/// <summary>
		/// Responsible for reacting on a window resize
		/// </summary>
		internal void Resize(int width, int height)
		{
			GL.Viewport(0, 0, width, height); // tell OpenGL to use the whole window for drawing
		}

		internal void Draw(IEnumerable<GameObject> enumerable)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.Color3(Color.Gray);
			foreach (var asteroid in enumerable)
			{
				DrawAsteroid(asteroid.Center, asteroid.Radius);
			}
		}

		private static readonly List<Vector2> asteroidPoints = CreateAsteroidPoints();

		/// <summary>
		/// Creates points along a circle, but for each point the radius of the circle is varied by a random variable.
		/// </summary>
		/// <returns></returns>
		private static List<Vector2> CreateAsteroidPoints()
		{
			const float PI2 = 2f * MathF.PI;
			var randomNumber = new Random(12);
			var count = 20;
			var delta = PI2 / count;
			var points = new List<Vector2>();
			for (float alpha = 0.0f; alpha < PI2; alpha += delta)
			{
				// step around the unit circle
				var x = MathF.Cos(alpha);
				var y = MathF.Sin(alpha);
				// change the radius with a random number to change the circle into an asteroid
				var radius = 0.9f + (float)randomNumber.NextDouble() * 0.1f;
				var pointOnUnitCircle = new Vector2(x, y);
				points.Add(radius * pointOnUnitCircle);
			}
			return points;
		}

		private static void DrawAsteroid(Vector2 center, float radius)
		{
			GL.Begin(PrimitiveType.TriangleFan);
			GL.Vertex2(center);
			foreach (var point in asteroidPoints)
			{
				GL.Vertex2(center + radius * point);
			}
			GL.Vertex2(center + radius * asteroidPoints[0]); //close border loop
			GL.End();
		}

		private void DrawQuad(Vector2 center, float radius)
		{
			GL.Begin(PrimitiveType.Quads);
			GL.Vertex2(center + new Vector2(-radius, -radius));
			GL.Vertex2(center + new Vector2(radius, -radius));
			GL.Vertex2(center + new Vector2(radius, radius));
			GL.Vertex2(center + new Vector2(-radius, radius));
			GL.End();
		}
	}
}
