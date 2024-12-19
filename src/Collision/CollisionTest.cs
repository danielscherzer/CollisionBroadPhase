namespace Collision;

public static class CollisionTest
{
	public static bool IntersectsX(this IBox2DCollider a, IBox2DCollider b)
	{
		bool noXintersect = (a.MaxX <= b.MinX) || (a.MinX >= b.MaxX);
		return !noXintersect;
	}

	public static bool IntersectsY(this IBox2DCollider a, IBox2DCollider b)
	{
		bool noYintersect = (a.MaxY <= b.MinY) || (a.MinY >= b.MaxY);
		return !noYintersect;
	}
	/// <summary>
	/// Test for intersection of two rectangles (excluding borders)
	/// </summary>
	/// <param name="a">A rectangle</param>
	/// <param name="b">A second rectangle</param>
	/// <returns>true if the two rectangles overlap</returns>
	//public static bool Intersects(this IBox2DCollider a, IBox2DCollider b)
	//{
	//	bool noXintersect = (a.MaxX <= b.MinX) || (a.MinX >= b.MaxX);
	//	bool noYintersect = (a.MaxY <= b.MinY) || (a.MinY >= b.MaxY);
	//	return !(noXintersect || noYintersect);
	//}

	public static bool Intersects(this ICircle2dCollider a, ICircle2dCollider b)
	{
		var rSum = a.Radius + b.Radius;
		var diffX = a.CenterX - b.CenterX;
		var diffY = a.CenterY - b.CenterY;
		return rSum * rSum > (diffX * diffX) + (diffY * diffY);
	}

}
