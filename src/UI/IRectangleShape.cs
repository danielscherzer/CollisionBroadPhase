using SFML.System;

namespace UI
{
	interface IRectangleShape
	{
		Vector2f Position { get; }
		Vector2f Size { get; }
	}
}