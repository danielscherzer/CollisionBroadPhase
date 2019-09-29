using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;
using System.Linq;

namespace Example
{
	class Ui
	{
		public Ui(RenderWindow window, ICollisionParameters parameters, CollisionDetection model)
		{
			this.window = window;
			this.parameters = parameters;
			this.model = model;

			var collMultiGrid = model.CollisionMultiGrid;
			var colors = new Color[] { Color.White, Color.Blue, Color.Yellow , Color.Green, Color.Magenta, Color.Red };
			for(int level = collMultiGrid.MaxLevel, colorId = 0; level >= collMultiGrid.MinLevel; --level, ++colorId)
			{
				var gridLevel = collMultiGrid.GetGridLevel(level);
				var color = colors[colorId % colors.Length];
				uiGrids.Add(level, new UiGrid((uint)gridLevel.GetLength(0), (uint)gridLevel.GetLength(1), new Vector2f(0, 0), (Vector2f)window.Size, color));
			}
			uiGrid = new UiGrid((uint)model.CollisionGrid.CellCountX, (uint)model.CollisionGrid.CellCountY, new Vector2f(0, 0), (Vector2f)window.Size, Color.White);
		}

		public void AddPropertyGrid(object obj)
		{
			var last = propertyGrids.LastOrDefault();
			var y = last is null ? 10 : last.Position.Y + last.Size.Y + 10;
			propertyGrids.Add(new UiPropertyGrid(window, new Vector2f(10, y), obj));
		}

		internal void Resize(int width, int height)
		{
		}

		public void Draw()
		{
			switch(parameters.CollisionMethod)
			{
				case CollisionMethodTypes.MultiGrid:
					foreach (var (level, uiGrid) in uiGrids)
					{
						UpdateGrid(uiGrid, model.CollisionMultiGrid.GetGridLevel(level));
						window.Draw(uiGrid);
					}
					break;
				case CollisionMethodTypes.Grid:
					UpdateGrid(uiGrid, model.CollisionGrid.GetGrid());
					window.Draw(uiGrid);
					break;
				default:
					break;
			}
			foreach(var propertyGrid in propertyGrids)
			{
				propertyGrid.Update();
				propertyGrid.Draw();
			}
		}

		private RenderWindow window;
		private readonly ICollisionParameters parameters;
		private CollisionDetection model;
		private List<UiPropertyGrid> propertyGrids = new List<UiPropertyGrid>();
		private readonly UiGrid uiGrid;
		private readonly Dictionary<int, UiGrid> uiGrids = new Dictionary<int, UiGrid>();

		private void UpdateGrid(UiGrid uiGrid, IReadOnlyList<GameObject>[,] grid)
		{
			for (int column = 0; column < uiGrid.Columns; ++column)
			{
				for (int row = 0; row < uiGrid.Rows; ++row)
				{
					var count = grid[column, row].Count;
					uiGrid[column, row] = 0 == count ? "" : count.ToString();
				}
			}
		}
	}
}
