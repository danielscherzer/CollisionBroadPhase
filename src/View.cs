using Collision;
using Example.UI;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Example
{
	/// <summary>
	/// Class that handles all the actual drawing using OpenGL.
	/// </summary>
	internal class View
	{
		public View(IStyle style)
		{
			GL.ClearColor(style.Background);
			var asteroidPoints = CreateAsteroidPoints();
			asteroidVertexCount = asteroidPoints.Count;
			var array = asteroidPoints.ToArray(); //create an array (data is guarantied to be consecutive in memory
			int byteSize = Vector2.SizeInBytes * array.Length; // calculate size in bytes of circle points

			vertexArray = GL.GenVertexArray(); // create a vertex array object for interpreting our buffer data (circle points)
			GL.BindVertexArray(vertexArray); // activate vertex array; from now on state is stored;

			//for version 4.5 and or arb_direct_state_access
			//GL.CreateBuffers(1, out uint circleBuffer); // create a buffer on the graphics card
			//GL.NamedBufferStorage(circleBuffer, byteSize, array, 0); // copy circle points into buffer CPU -> GPU

			var circleBuffer = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, circleBuffer); // activate buffer
			GL.BufferData(BufferTarget.ArrayBuffer, byteSize, array, BufferUsageHint.StaticDraw);

			GL.EnableVertexAttribArray(0); // activate this vertex attribute for the active vertex array
			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0); // specify what our buffer contains
			GL.BindVertexArray(0); // deactivate vertex array; state storing is stopped;
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // deactivate buffer; just to be on the cautious side;
			Style = style;
		}

		/// <summary>
		/// Responsible for reacting on a window resize
		/// </summary>
		internal static void Resize(int width, int height)
		{
			GL.Viewport(0, 0, width, height); // tell OpenGL to use the whole window for drawing
		}

		internal void Draw(IEnumerable<ICircle2dCollider> gameObjects, IEnumerable<ICircle2dCollider> highlightGameObjects, bool fillHighlighted = false)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.Color4(Style.ObjectFill);
			GL.BindVertexArray(vertexArray); // activate vertex array
			foreach (var asteroid in gameObjects)
			{
				DrawAsteroid(asteroid.CenterX, asteroid.CenterY, asteroid.Radius);
			}
			GL.Color4(Style.ObjectBorder);
			foreach (var asteroid in gameObjects)
			{
				DrawAsteroid(asteroid.CenterX, asteroid.CenterY, asteroid.Radius, PrimitiveType.LineLoop);
			}
			GL.Color4(Style.Accent);
			foreach (var error in highlightGameObjects)
			{
				DrawAsteroid(error.CenterX, error.CenterY, error.Radius, fillHighlighted ? PrimitiveType.TriangleFan : PrimitiveType.LineLoop);
			}
			GL.BindVertexArray(0); // deactivate vertex array
		}

		private readonly int vertexArray;
		private readonly int asteroidVertexCount;

		public IStyle Style { get; }

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

		private void DrawAsteroid(float centerX, float centerY, float radius, PrimitiveType primitiveType = PrimitiveType.TriangleFan)
		{
			GL.PushMatrix();
			GL.Translate(centerX, centerY, 0f);
			GL.Scale(radius, radius, radius);
			GL.DrawArrays(primitiveType, 0, asteroidVertexCount); // draw with vertex array data
			GL.PopMatrix();
		}
	}
}
