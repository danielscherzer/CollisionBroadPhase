using Example.UI;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using Zenseless.Patterns;
using Zenseless.Spatial;

namespace Example
{
	internal class Ui : Disposable
	{
		public Ui(RenderWindow window, IStyle style)
		{
			font = new Font("Content/sansation.ttf");
			window.MouseButtonReleased += Window_MouseButtonReleased;
			this.window = window;
			this.style = style;
		}

		private void Window_MouseButtonReleased(object sender, MouseButtonEventArgs e)
		{
			foreach (var propGrid in propGrids)
			{
				if (propGrid.ChangeValueAt(e.X, e.Y, e.Button != Mouse.Button.Left)) return; //possible prop grid is removed -> so stop
			}
		}

		public void AddCountGrid<T>(IReadOnlyGrid<List<T>> grid)
		{
			var uiGrid = new PullUiGrid((uint)grid.Columns, (uint)grid.Rows
				, new Vector2f(0, 0), (Vector2f)window.Size, style.VisualLevel[currentColorId], font
				, (col, row) => GetCellString(grid, col, row));

			drawables.Add(uiGrid);

			++currentColorId;
			currentColorId %= style.VisualLevel.Length;
		}

		public void AddPropertyGrid(object obj)
		{
			var propGrid = new UiPropertyGrid(new Vector2f(10, currentY + 10), font, style);
			propGrid.AddProperties(obj);
			propGrids.Add(propGrid);
			currentY = propGrid.Position.Y + propGrid.Size.Y;
		}

		public void Clear()
		{
			drawables.Clear();
			propGrids.Clear();
			currentY = 0f;
			currentColorId = 0;
		}

		//internal void Resize(int width, int height)
		//{
		//}

		public void DrawUi()
		{
			foreach (var propGrid in propGrids)
			{
				window.Draw(propGrid);
			}
		}

		public void DrawVisualization()
		{
			foreach (var drawable in drawables)
			{
				window.Draw(drawable);
			}
		}

		protected override void DisposeResources()
		{
			window.MouseButtonReleased -= Window_MouseButtonReleased;

			foreach (var drawable in drawables)
			{
				(drawable as IDisposable)?.Dispose();
			}
			font.Dispose();
		}

		private float currentY = 0f;
		private int currentColorId = 0;
		private readonly Font font;
		private readonly RenderWindow window;
		private readonly IStyle style;
		private readonly List<Drawable> drawables = new();
		private readonly List<UiPropertyGrid> propGrids = new();

		private static string GetCellString<T>(IReadOnlyGrid<List<T>> grid, int column, int row)
		{
			var count = grid[column, row].Count;
			return 0 == count ? "" : count.ToString();
		}
	}
}
