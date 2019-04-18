using System;
using System.Collections.Generic;

namespace Example
{
	/// <summary>
	/// Multi resolution collision grid 
	/// The idea is to have a stack of collision grids with 2^level cells.
	/// Each cell has a list of <seealso cref="ICircle2dCollider"/>, whose center are located above it.
	/// <seealso cref="ICircle2dCollider"/> may overlap to neighboring cells. 
	/// So <seealso cref="ICircle2dCollider"/> are inserted in the highest level, were the cell size is bigger then the radius,
	/// so only direct neighbors need to be considered.
	/// Idea from Game Gems 2: Multi-Resolution Maps for Interaction Detection by Jan Svarovsky
	/// </summary>
	class CollisionMultiGrid
	{
		public CollisionMultiGrid(int gridLevels, float minX, float minY, float size)
		{
			//create each grid level
			int cellCount = 1;
			var cellSize = size;
			for (int level = 0; level < gridLevels; ++level, cellCount *= 2)
			{
				var grid = new List<ICircle2dCollider>[cellCount, cellCount];
				for (int y = 0; y < cellCount; ++y)
				{
					for (int x = 0; x < cellCount; ++x)
					{
						grid[x, y] = new List<ICircle2dCollider>();
					}
				}
				multiGrid.Add((cellSize, grid));
				cellSize /= 2f;
			}

			GridLevels = gridLevels;
			MinX = minX;
			MinY = minY;
			Size = size;
		}

		public void Add(ICircle2dCollider collider)
		{
			// find highest multiGrid level with cell size >= radius
			for (int level = multiGrid.Count - 1; level >= 0; --level)
			{
				var (cellSize, grid) = multiGrid[level];
				//if small enough -> insert into this level
				if (collider.Radius <= cellSize)
				{
					var unitX = (collider.CenterX - MinX) / Size;
					var unitY = (collider.CenterY - MinY) / Size;
					var columns = grid.GetLength(0);
					var rows = grid.GetLength(1);
					//calculate grid column and row
					var column = Math.Clamp((int)(unitX * columns), 0, columns - 1);
					var row    = Math.Clamp((int)(unitY *    rows), 0,    rows - 1);
					grid[column, row].Add(collider);
					break;
				}
			}
		}

		public void Clear()
		{
			foreach(var (cellSize, grid) in multiGrid)
			{
				foreach(var cell in grid)
				{
					cell.Clear();
				}
			}
		}

		public void FindCollision(Action<ICircle2dCollider, ICircle2dCollider> collisionHandler)
		{
			// from smallest objects to largest objects
			for (int level = multiGrid.Count - 1; level >= 0; --level)
			{
				var (_, grid) = multiGrid[level];
				for (int y = 0; y < grid.GetLength(1); ++y)
				{
					for (int x = 0; x < grid.GetLength(0); ++x)
					{
						CheckCell(level, x, y, collisionHandler);
					}
				}
			}
		}

		public IReadOnlyList<ICircle2dCollider>[,] GetGridLevel(int level) => multiGrid[level].Item2;

		private void CheckCell(int level, int x, int y, Action<ICircle2dCollider, ICircle2dCollider> collisionHandler)
		{
			var (_, grid) = multiGrid[level];
			var cell = grid[x, y];
			CheckCell(cell, collisionHandler);
			//check only with objects later in list
			for (int i = 0; i < cell.Count; ++i)
			{
				var obj = cell[i];
				//check each collider against every other collider in the cell
				for (int j = i + 1; j < cell.Count; ++j)
				{
					collisionHandler(obj, cell[j]);
				}
				//check with containing cells with smaller level == bigger cell size
				//we only need to check with the containing cells, because bigger neighbors are checked 
				for(int l = 0; l < level; ++l)
				{
					var (_, lowResGrid) = multiGrid[l];
					var biggerCell = lowResGrid[ x >> 1, y >> 1];
					foreach(var element in biggerCell)
					{
						collisionHandler(obj, element);
					}
				}
				//check neighbors to east, south and southeast
			}
		}

		private void CheckCell(List<ICircle2dCollider> cell, Action<ICircle2dCollider, ICircle2dCollider> collisionHandler)
		{
		}

		private List<(float, List<ICircle2dCollider>[,])> multiGrid = new List<(float, List<ICircle2dCollider>[,])>();

		public int GridLevels { get; }
		public float MinX { get; }
		public float MinY { get; }
		public float Size { get; }
	}
}
