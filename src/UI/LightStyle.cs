using Example.UI;
using OpenTK.Mathematics;
using SFML.Graphics;

namespace Example
{
	public class LightStyle : IStyle
	{
		public Color4 Background { get; } = Color4.White;
		public Color4 ObjectFill { get; } = Color4.LightGray;
		public Color4 ObjectBorder { get; } = Color4.DarkGray;
		public Color4 Accent { get; } = Color4.Red;
		public Color[] VisualLevel { get; } = new Color[] { Color.Black, Color.Blue, Color.Yellow, Color.Green, Color.Magenta, Color.Red };
		public Color Text { get; } = Color.Black;
		public Color TextInactive { get; } = new Color(120, 120, 120);
		public Color UiFill { get; } = new Color(230, 230, 230);
		public Color UiOutline { get; } = new Color(60, 60, 80);
		public float UiOutlineThickness { get; } = 3f;
		public float UiLineSpacing { get; } = 1.2f;
	}
}
