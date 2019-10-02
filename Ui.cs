using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using Zenseless.Patterns;

namespace Example
{
	class Ui : Disposable
	{
		public Ui(RenderWindow window, CollisionDetection model)
		{
			font = new Font("Content/sansation.ttf");
			this.window = window;
			
			switch (model.CollisionMethod)
			{
				case CollisionMultiGrid<GameObject> collisionMethod:
					var colors = new Color[] { Color.White, Color.Blue, Color.Yellow, Color.Green, Color.Magenta, Color.Red };
					for (int level = collisionMethod.MaxLevel, colorId = 0; level >= collisionMethod.MinLevel; --level, ++colorId)
					{
						var gridLevel = collisionMethod.GetGridLevel(level);
						var color = colors[colorId % colors.Length];
						var uiGrid = new PullUiGrid((uint)gridLevel.GetLength(0), (uint)gridLevel.GetLength(1)
							, new Vector2f(0, 0), (Vector2f)window.Size, color, font
							, (col, row) => GetCellString(gridLevel, col, row));

						drawables.Add(uiGrid);
					}
					break;
				case CollisionGrid<GameObject> collisionMethod:
					var grid = new PullUiGrid((uint)collisionMethod.CellCountX, (uint)collisionMethod.CellCountY
						, new Vector2f(0, 0), (Vector2f)window.Size, Color.White
						, font, (col, row) => GetCellString(collisionMethod.GetGrid(), col, row));
					drawables.Add(grid);
					break;
				default:
					break;
			}
		}

		public void AddPropertyGrid(object obj)
		{
			var propGrid = new UiPropertyGrid(window, new Vector2f(10, currentY + 10), font);
			propGrid.AddProperties(obj);
			drawables.Add(propGrid);
			currentY = propGrid.Position.Y + propGrid.Size.Y;
		}

		public void Clear()
		{
			drawables.Clear();
		}

		internal void Resize(int width, int height)
		{
		}

		public void Draw()
		{
			foreach(var drawable in drawables)
			{
				window.Draw(drawable);
			}
		}

		protected override void DisposeResources()
		{
			foreach (var drawable in drawables)
			{
				(drawable as IDisposable)?.Dispose();
			}
			font.Dispose();
		}

		private float currentY = 0f;
		private readonly Font font;
		private RenderWindow window;
		private List<Drawable> drawables = new List<Drawable>();

		private string GetCellString(IReadOnlyList<object>[,] grid, int column, int row)
		{
			var count = grid[column, row].Count;
			return 0 == count ? "" : count.ToString();
		}
	}
}
