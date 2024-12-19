using Example.UI;
using OpenTK.Mathematics;
using SFML.Graphics;

namespace Example;

public class DarkStyle : IStyle
{
	public Color4 Background { get; } = Color4.Black;
	public Color4 ObjectFill { get; } = Color4.Gray;
	public Color4 ObjectBorder { get; } = Color4.DarkGray;
	public Color4 Accent { get; } = Color4.Red;
	public Color[] VisualLevel { get; } = new Color[] { Color.White, Color.Blue, Color.Yellow, Color.Green, Color.Magenta, Color.Red };
	public Color Text { get; } = Color.White;
	public Color TextInactive { get; } = new Color(180, 180, 180);
	public Color UiFill { get; } = new Color(30, 30, 30);
	public Color UiOutline { get; } = new Color(60, 60, 80);
	public float UiOutlineThickness { get; } = 3f;
	public float UiLineSpacing { get; } = 1.2f;
}
