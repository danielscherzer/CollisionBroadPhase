using SFML.System;

namespace Example
{
	interface IRectangleShape
	{
		Vector2f Position { get; }
		Vector2f Size { get; }
	}
}