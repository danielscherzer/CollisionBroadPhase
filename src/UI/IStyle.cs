using OpenTK.Mathematics;
using SFML.Graphics;

namespace Example.UI;

public interface IStyle
{
	Color4 Background { get; }
	Color4 Accent { get; }
	Color[] VisualLevel { get; }
	Color4 ObjectBorder { get; }
	Color4 ObjectFill { get; }
	Color Text { get; }
	Color TextInactive { get; }
	Color UiFill { get; }
	Color UiOutline { get; }
	float UiOutlineThickness { get; }
	float UiLineSpacing { get; }
}