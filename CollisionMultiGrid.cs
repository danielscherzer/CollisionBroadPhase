using System;
using System.Collections.Generic;

namespace Example
{
	/// <summary>
	/// Multi resolution collision grid 
	/// The idea is to have a stack of collision grids with 2^level cells.
	/// Each cell has a list of <seealso cref="TCollider"/>, whose center are located above it.
	/// <seealso cref="TCollider"/> may overlap to neighboring cells. 
	/// So <seealso cref="TCollider"/> are inserted in the highest level, were the cell size is bigger then the radius,
	/// so only direct neighbors need to be considered.
	/// Idea from Game Gems 2: Multi-Resolution Maps for Interaction Detection by Jan Svarovsky
	/// </summary>
	class CollisionMultiGrid<TCollider> where TCollider : class
	{
		public CollisionMultiGrid(int minLevel, int maxLevel, float minX, float minY, float size)
		{
			//create each grid level
			var cellCount = (int)MathF.Pow(2f, minLevel);
			for (int level = minLevel; level <= maxLevel; ++level, cellCount *= 2)
			{
				var grid = new List<TCollider>[cellCount, cellCount];
				for (int y = 0; y < cellCount; ++y)
				{
					for (int x = 0; x < cellCount; ++x)
					{
						grid[x, y] = new List<TCollider>();
					}
				}
				var cellSize = 2f / MathF.Pow(2, level);
				multiGrid.Add((cellSize, grid));
			}

			MinLevel = minLevel;
			MaxLevel = maxLevel;
			MinX = minX;
			MinY = minY;
			Size = size;
		}

		public int MinLevel { get; }
		public int MaxLevel { get; }
		public float MinX { get; }
		public float MinY { get; }
		public float Size { get; }

		public void Add(ICircle2dCollider collider)
		{
			void AddToGridLevel(List<TCollider>[,] grid)
			{
				var unitX = (collider.CenterX - MinX) / Size;
				var unitY = (collider.CenterY - MinY) / Size;
				var columns = grid.GetLength(0);
				var rows = grid.GetLength(1);
				//calculate grid column and row
				var column = Math.Clamp((int)(unitX * columns), 0, columns - 1);
				var row = Math.Clamp((int)(unitY * rows), 0, rows - 1);
				grid[column, row].Add(collider as TCollider);
			}

			// find highest multiGrid level with cell size >= object size
			for (int level = MaxLevel; level > MinLevel; --level)
			{
				var (cellSize, grid) = GetLevel(level);
				//if small enough -> insert into this level
				if (collider.Radius * 2f <= cellSize)
				{
					AddToGridLevel(grid);
					return;
				}
			}
			//too big for smaller levels -> add to biggest 
			AddToGridLevel(GetLevel(MinLevel).grid);
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

		public void FindCollision(Action<TCollider, TCollider> collisionHandler)
		{
			// from smallest objects to largest objects
			for (int level = MaxLevel; level >= MinLevel; --level)
			{
				var grid = GetGridLevel(level);
				for (int y = 0; y < grid.GetLength(1); ++y)
				{
					for (int x = 0; x < grid.GetLength(0); ++x)
					{
						CheckCell(level, x, y, collisionHandler);
					}
				}
			}
		}

		public IReadOnlyList<TCollider>[,] GetGridLevel(int level) => multiGrid[level - MinLevel].Item2;

		private List<(float, List<TCollider>[,])> multiGrid = new List<(float, List<TCollider>[,])>();

		private (float cellSize, List<TCollider>[,] grid) GetLevel(int level) => multiGrid[level - MinLevel];

		private void CheckCell(int level, int x, int y, Action<TCollider, TCollider> collisionHandler)
		{
			var grid = GetGridLevel(level);
			var cell = grid[x, y];
			//check only with objects later in list
			for (int i = 0; i + 1 < cell.Count; ++i)
			{
				var obj = cell[i];
				//check each collider against every other collider in the cell
				for (int j = i + 1; j < cell.Count; ++j)
				{
					collisionHandler(obj, cell[j]);
				}
			}
			foreach (var obj in cell)
			{
				//check 4 neighbors (of 8) for instance 3 with bigger y  and one with just bigger x
				var existsBiggerX = x + 1 < grid.GetLength(0);
				if (existsBiggerX)
				{
					var biggerXcell = grid[x + 1, y];
					foreach(var objB in biggerXcell)
					{
						collisionHandler(obj, objB);
					}
				}
				if (y + 1 < grid.GetLength(1))
				{
					var biggerYcell = grid[x, y + 1];
					foreach (var objB in biggerYcell)
					{
						collisionHandler(obj, objB);
					}
					if(existsBiggerX)
					{
						var bothBiggerCell = grid[x + 1, y + 1];
						foreach (var objB in bothBiggerCell)
						{
							collisionHandler(obj, objB);
						}
					}
					if(0 < x)
					{
						var neighbor = grid[x - 1, y + 1];
						foreach (var objB in neighbor)
						{
							collisionHandler(obj, objB);
						}
					}
				}
				//check with containing cells with smaller level == bigger cell size
				//we only need to check with the containing cells, because bigger neighbors are checked 
				//for(int l = MinLevel; l < level; ++l)
				//{
				//	var lowResGrid = GetGridLevel(l);
				//	var biggerCell = lowResGrid[ x >> 1, y >> 1];
				//	foreach(var element in biggerCell)
				//	{
				//		collisionHandler(obj, element);
				//	}
				//}
			}
		}
	}
}
