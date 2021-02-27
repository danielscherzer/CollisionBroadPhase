using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zenseless.Patterns;

namespace UI
{
	internal class UiPropertyGrid : Disposable, Drawable, IRectangleShape
	{
		public UiPropertyGrid(Vector2f position, Font font)
		{
			border = 5f;
			background = new RectangleShape
			{
				FillColor = new Color(30, 30, 30),
				OutlineColor = new Color(60, 60, 80),
				OutlineThickness = 3,
				Position = position,
			};
			textBlueprint = new Text("test", font)
			{
				LineSpacing = 1.2f,
				Position = background.Position + new Vector2f(border, border),
			};
		}

		public void AddProperties(object obj)
		{
			var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.CanRead && p.GetCustomAttribute<UiIgnoreAttribute>() is null);
			var position = textBlueprint.Position;
			//labels
			foreach (var property in properties)
			{
				var text = new Text(textBlueprint) { DisplayedString = property.Name };
				text.Position = position;
				position.Y += text.CharacterSize * text.LineSpacing;
				texts.Add(text);
				ExpandBackground(text.GetGlobalBounds());
			}
			background.Size += 2f * new Vector2f(border, border);
			position = textBlueprint.Position;
			position.X = background.Position.X + background.Size.X;
			//values
			foreach (var property in properties)
			{
				var text = new Text(textBlueprint) { DisplayedString = property.GetValue(obj)?.ToString() };
				if (property.GetSetMethod() is null)
				{
					text.FillColor = new Color(180, 180, 180);
				}
				text.Position = position;
				position.Y += text.CharacterSize * text.LineSpacing;
				texts.Add(text);
				this.properties.Add((text, property, obj));
				ExpandBackground(text.GetGlobalBounds());
			}
		}

		public void Draw(RenderTarget target, RenderStates states)
		{
			Update();
			target.Draw(background, states);
			foreach (var drawable in texts) target.Draw(drawable, states);
		}

		public Vector2f Position => background.Position;
		public Vector2f Size => background.Size;

		protected override void DisposeResources()
		{
			properties.Clear();
			foreach (var text in texts)
			{
				text.Dispose();
			}
			texts.Clear();
			textBlueprint.Dispose();
			background.Dispose();
		}

		private readonly float border;
		private readonly Text textBlueprint;
		private readonly RectangleShape background;
		private readonly List<Text> texts = new List<Text>();
		private readonly List<(Text, PropertyInfo, object)> properties = new List<(Text, PropertyInfo, object)>();

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

		private void Update()
		{
			foreach (var (text, property, instance) in properties)
			{
				text.DisplayedString = property.GetValue(instance)?.ToString();
				ExpandBackground(text.GetGlobalBounds());
			}
		}

		public bool ChangeValueAt(int x, int y, bool invert)
		{
			if (!background.GetGlobalBounds().Contains(x, y)) return false;
			(PropertyInfo property, object instance) = GetClickedProperty(x, y); // do this first because value change could call this.Dispose()
			if (property is null) return false;
			var value = property.GetValue(instance);
			var valueChangeFunction = property.GetCustomAttributes<UiValueChangeFunctionAttribute>().Append(new UiValueChangeFunctionAttribute()).First();
			valueChangeFunction = invert ? valueChangeFunction.Inverted() : valueChangeFunction;
			var delta = property.GetCustomAttributes<UiValueChangeFunctionAttribute>().Select(attr => attr.Constant).Append(1.0).First();
			delta = invert ? -delta : delta;
			switch (value)
			{
				case bool boolValue:
					property.SetValue(instance, !boolValue);
					break;
				case Enum enumValue:
					var possibleValues = Enum.GetValues(enumValue.GetType());
					var maxVal = possibleValues.Length - 1;
					var val = Convert.ToInt32(enumValue);
					val += (int)valueChangeFunction.Constant;
					property.SetValue(instance, Math.Clamp(val, 0, maxVal));
					break;
				case int intValue:
					intValue = (int)valueChangeFunction.NewValue(intValue);
					property.SetValue(instance, intValue);
					break;
				case uint uintValue:
					uintValue = (uint)valueChangeFunction.NewValue(uintValue);
					property.SetValue(instance, uintValue);
					break;
				case float floatValue:
					floatValue = (float)valueChangeFunction.NewValue(floatValue);
					property.SetValue(instance, floatValue);
					break;
				case double doubleValue:
					doubleValue = valueChangeFunction.NewValue(doubleValue);
					property.SetValue(instance, doubleValue);
					break;
			}
			return true;
		}

		private (PropertyInfo, object) GetClickedProperty(int x, int y)
		{
			foreach (var (text, property, instance) in properties)
			{
				if (!property.CanWrite) continue;
				if (text.GetGlobalBounds().Contains(x, y))
				{
					var setMethod = property.GetSetMethod();
					if (setMethod is null) continue;
					return (property, instance);
				}
			}
			return (null, null);
		}
	}
}
