﻿using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Example
{
	class UiPropertyGrid
	{
		public UiPropertyGrid(RenderWindow window, Vector2f position, object obj)
		{
			window.MouseButtonReleased += Window_MouseButtonReleased;
			window.KeyReleased += Window_KeyReleased;
			this.window = window ?? throw new ArgumentNullException(nameof(window));
			border = 5f;
			background = new RectangleShape
			{
				FillColor = new Color(30, 30, 30),
				OutlineColor = new Color(60, 60, 80),
				OutlineThickness = 3,
				Position = position,
			};
			var font = new Font("Content/sansation.ttf");
			textBlueprint = new Text("test", font)
			{
				LineSpacing = 1.2f,
				Position = background.Position + new Vector2f( border, border),
			};
			AddProperties(obj);
		}

		public void Draw()
		{
			window.Draw(background);
			foreach (var drawable in drawables) window.Draw(drawable);
		}

		public Vector2f Size => background.Size;

		public void Update()
		{
			foreach(var (text, property, instance) in properties)
			{
				text.DisplayedString = property.GetValue(instance)?.ToString();
			}
		}

		private readonly float border;
		private readonly Text textBlueprint;
		private readonly RectangleShape background;
		private readonly RenderWindow window;
		private readonly List<Drawable> drawables = new List<Drawable>();
		private readonly List<(Text, PropertyInfo, object)> properties = new List<(Text, PropertyInfo, object)>();

		private void AddProperties(object obj)
		{
			var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead);
			var position = textBlueprint.Position;
			foreach (var property in properties)
			{
				var text = new Text(textBlueprint) { DisplayedString = property.Name };
				text.Position = position;
				position.Y += text.CharacterSize * text.LineSpacing;
				drawables.Add(text);
				ExpandBackground(text.GetGlobalBounds());
			}
			background.Size += 2f * new Vector2f(border, border);
			position = textBlueprint.Position;
			position.X = background.Position.X + background.Size.X;
			foreach (var property in properties)
			{
				var text = new Text(textBlueprint) { DisplayedString = property.GetValue(obj)?.ToString() };
				if (property.GetSetMethod() is null)
				{
					text.FillColor = new Color(180, 180, 180);
				}
				text.Position = position;
				position.Y += text.CharacterSize * text.LineSpacing;
				drawables.Add(text);
				this.properties.Add((text, property, obj));
				ExpandBackground(text.GetGlobalBounds());
			}
		}

		private void ExpandBackground(FloatRect bounds)
		{
			bounds.Width += 2f * border;
			var combi = Combine(new FloatRect(background.Position, background.Size), bounds);
			//background.Position = new Vector2f(combi.Left, combi.Top); //only size should grow
			background.Size = new Vector2f(combi.Width, combi.Height);
		}

		private FloatRect Combine(FloatRect a, FloatRect b)
		{
			var left = Math.Min(a.Left, b.Left);
			var top = Math.Min(a.Top, b.Top);
			var right = Math.Max(a.Left + a.Width, b.Left + b.Width);
			var bottom = Math.Max(a.Top + a.Height, b.Top + b.Height);
			return new FloatRect(left, top, right - left, bottom - top);
		}

		private void Window_KeyReleased(object sender, KeyEventArgs e)
		{
		}

		private void Window_MouseButtonReleased(object sender, MouseButtonEventArgs e)
		{
			if (!background.GetGlobalBounds().Contains(e.X, e.Y)) return;
			foreach (var (text, property, instance) in properties)
			{
				if (!property.CanWrite) continue;
				if (text.GetGlobalBounds().Contains(e.X, e.Y))
				{
					var value = property.GetValue(instance);
					var setMethod = property.GetSetMethod();
					if (setMethod is null) continue;
					var increment = property.GetCustomAttributes<IncrementAttribute>().Select(attr => attr.Value).Append(1.0).First();
					increment = e.Button == Mouse.Button.Left ? increment : -increment;
					switch (value)
					{
						case bool boolValue:
							property.SetValue(instance, !boolValue);
							InvalidateBackgroundSize();
							break;
						case Enum enumValue:
							var possibleValues = Enum.GetValues(enumValue.GetType());
							var maxVal = possibleValues.Length - 1;
							var val = Convert.ToInt32(enumValue);
							val += (int)increment;
							property.SetValue(instance, Math.Clamp(val, 0, maxVal));
							InvalidateBackgroundSize();
							break;
						case int intValue:
							intValue += (int)increment;
							property.SetValue(instance, intValue);
							InvalidateBackgroundSize();
							break;
						case uint uintValue:
							uintValue = (uint)((int)uintValue + increment);
							property.SetValue(instance, uintValue);
							InvalidateBackgroundSize();
							break;
						case float floatValue:
							floatValue += (float)increment;
							property.SetValue(instance, floatValue);
							InvalidateBackgroundSize();
							break;
					}
				}
			}
		}

		private void InvalidateBackgroundSize()
		{
			foreach (var (text, property, instance) in properties)
			{
				text.DisplayedString = property.GetValue(instance)?.ToString();
				ExpandBackground(text.GetGlobalBounds());
			}
		}
	}
}
