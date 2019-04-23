using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SFML.Graphics;
using SFML.System;

namespace Example
{
	class Ui
	{
		public Ui(RenderWindow window, Model model)
		{
			this.window = window;
			this.model = model;
			uiPropertyGrid = new UiPropertyGrid(window, new Vector2f(10, 10), model);

			var collGrid = model.CollisionMultiGrid;
			var colors = new Color[] { Color.Red, Color.Magenta, Color.Green, Color.Yellow, Color.White};
			for(int level = collGrid.MaxLevel; level >= collGrid.MinLevel; --level)
			{
				var gridLevel = collGrid.GetGridLevel(level);
				var color = colors[level % colors.Length];
				uiGrids.Add(level, new UiGrid((uint)gridLevel.GetLength(0), (uint)gridLevel.GetLength(1), new Vector2f(0, 0), (Vector2f)window.Size, color));
			}
		}

		public void Draw()
		{
			if (model.UseCollissionGrid)
			{
				var sum = 0;
				foreach(var (level, uiGrid) in uiGrids)
				{
					sum += UpdateGrid(uiGrid, level);
					window.Draw(uiGrid);
				}
				Debug.Assert(model.ObjectCount == sum);
			}
			uiPropertyGrid.Update();
			uiPropertyGrid.Draw();
		}

		private RenderWindow window;
		private Model model;
		private readonly Dictionary<int, UiGrid> uiGrids = new Dictionary<int, UiGrid>();
		private readonly UiPropertyGrid uiPropertyGrid;

		private int UpdateGrid(UiGrid uiGrid, int level)
		{
			var sum = 0;
			var levelCounts = GetLevelCounts(level);
			for (int column = 0; column < uiGrid.Columns; ++column)
			{
				for (int row = 0; row < uiGrid.Rows; ++row)
				{
					var count = levelCounts[column, row];
					sum += count;
					uiGrid[column, row] = count.ToString();
				}
			}
			return sum;
		}

		internal int[,] GetLevelCounts(int level)
		{
			var gridLevel = model.CollisionMultiGrid.GetGridLevel(level);
			var levelCounts = new int[gridLevel.GetLength(0), gridLevel.GetLength(1)];
			for (int y = 0; y < levelCounts.GetLength(1); ++y)
			{
				for (int x = 0; x < levelCounts.GetLength(0); ++x)
				{
					levelCounts[x, y] = gridLevel[x, y].Count;
				}
			}
			return levelCounts;
		}
	}
}
