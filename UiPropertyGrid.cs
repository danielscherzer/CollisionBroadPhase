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
			background.Size += 2f * new Vector2f(border, 0f);
		}

		private void ExpandBackground(FloatRect bounds)
		{
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
					if (value is bool && !(setMethod is null))
					{
						property.SetValue(instance, !(bool)value);
					}
				}
			}
		}
	}
}
