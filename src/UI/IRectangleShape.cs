using SFML.System;

namespace UI
{
	internal interface IRectangleShape
	{
		Vector2f Position { get; }
		Vector2f Size { get; }
	}
}