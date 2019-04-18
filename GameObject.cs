using OpenTK;
using System;

namespace Example
{
	/// <summary>
	/// Base class for all game objects. It uses a circle based intersection test.
	/// </summary>
	internal class GameObject : ICircle2dCollider
	{
		public GameObject(float centerX, float centerY, float radius)
		{
			Center = new Vector2(centerX, centerY);
			Radius = radius;
		}

		public Vector2 Center { get; private set; }

		public float Radius { get; private set; }

		public Vector2 Velocity { get; set; } = Vector2.Zero;

		public float CenterX => Center.X;

		public float CenterY => Center.Y;

		public event Action OnCollision;

		public void HandleCollision() => OnCollision?.Invoke();

		public bool Intersects(GameObject obj)
		{
			var rSum = Radius + obj.Radius;
			var diff = Center - obj.Center;
			return rSum * rSum > diff.LengthSquared;
		}

		/// <summary>
		/// Responsible for moving the game object. Should be called once a frame.
		/// <param name="frameTime">Time in seconds since the last update.</param>
		/// </summary>
		public virtual void Update(float frameTime)
		{
			Center += frameTime * Velocity;
			//wrap movement at window edges
			var c = Center;
			if (Center.X - Radius > 1) c.X = -1f - Radius;
			if (Center.X + Radius < -1) c.X = 1f + Radius;
			if (Center.Y - Radius > 1) c.Y = -1f - Radius;
			if (Center.Y + Radius < -1) c.Y = 1f + Radius;
			Center = c;
		}
	}
}
