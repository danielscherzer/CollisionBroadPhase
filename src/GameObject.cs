using System.Numerics;

namespace Example;

/// <summary>
/// Base class for all game objects. It uses a circle based intersection test.
/// </summary>
public class GameObject(float centerX, float centerY, float radius) : ICollider
{
	public Vector2 Center { get; private set; } = new Vector2(centerX, centerY);

	public float Radius { get; private set; } = radius;

	public Vector2 Velocity { get; set; } = Vector2.Zero;

	public float CenterX => Center.X;

	public float CenterY => Center.Y;

	public float MinX => CenterX - Radius;

	public float MaxX => CenterX + Radius;

	public float MinY => CenterY - Radius;

	public float MaxY => CenterY + Radius;

	public void HandleCollision(ICollider other)
	{
		var diff = Center - (other as GameObject).Center;
		Velocity = Vector2.Normalize(diff) * Velocity.Length();
	}

	/// <summary>
	/// Responsible for moving the game object. Should be called once a frame.
	/// <param name="frameTime">Time in seconds since the last update.</param>
	/// </summary>
	public virtual void Update(float frameTime)
	{
		var abs = Vector2.Abs(Center);
		//bounce off window edges
		if (abs.X > 1f || abs.Y > 1f)
		{
			Velocity = -Velocity;
		}
		Center += frameTime * Velocity;
	}
}
