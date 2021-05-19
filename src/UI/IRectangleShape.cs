using SFML.System;

namespace Example.UI
{
	internal interface IRectangleShape
	{
		Vector2f Position { get; }
		Vector2f Size { get; }
	}
}