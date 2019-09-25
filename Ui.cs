using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;

namespace Example
{
	class Ui
	{
		public Ui(RenderWindow window, IParameters parameters, Model model)
		{
			this.window = window;
			this.parameters = parameters;
			this.model = model;
			
			uiPropertyGridModel = new UiPropertyGrid(window, new Vector2f(window.Size.X - 500, 10), model);
			uiPropertyGridParams = new UiPropertyGrid(window, new Vector2f(10, 10), parameters);

			var collMultiGrid = model.CollisionMultiGrid;
			var colors = new Color[] { Color.White, Color.Blue, Color.Yellow , Color.Green, Color.Magenta, Color.Red };
			for(int level = collMultiGrid.MaxLevel, colorId = 0; level >= collMultiGrid.MinLevel; --level, ++colorId)
			{
				var gridLevel = collMultiGrid.GetGridLevel(level);
				var color = colors[colorId % colors.Length];
				uiGrids.Add(level, new UiGrid((uint)gridLevel.GetLength(0), (uint)gridLevel.GetLength(1), new Vector2f(10, 0), (Vector2f)window.Size, color));
			}
			uiGrid = new UiGrid((uint)model.CollisionGrid.CellCountX, (uint)model.CollisionGrid.CellCountY, new Vector2f(0, 0), (Vector2f)window.Size, Color.White);
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
			uiPropertyGridModel.Update();
			uiPropertyGridModel.Draw();
			uiPropertyGridParams.Update();
			uiPropertyGridParams.Draw();
		}

		private RenderWindow window;
		private readonly IParameters parameters;
		private Model model;
		private readonly UiPropertyGrid uiPropertyGridModel;
		private readonly UiPropertyGrid uiPropertyGridParams;
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
