﻿using System;
using System.Collections.Generic;
using System.Linq;
using Zenseless.Spatial;

namespace Collision;

/// <summary>
/// Multi resolution collision grid 
/// The idea is to have a stack of collision grids with 2^level cells.
/// Each cell has a list of <seealso cref="TCollider"/>, whose center are located above it.
/// <seealso cref="TCollider"/> may overlap to neighboring cells. 
/// So <seealso cref="TCollider"/> are inserted in the highest level, were the cell size is bigger then the radius,
/// so only direct neighbors need to be considered.
/// Idea from Game Gems 2: Multi-Resolution Maps for Interaction Detection by Jan Svarovsky
/// Please note that Game Gems article contains an error in what neighbors have to be traversed (see code below for details)
/// Not 3, but 4 neighbors have to be traversed!
/// </summary>
internal class CollisionMultiGrid<TCollider> : ICollisionGrid<TCollider>, ICollisionMethodBroadPhase<TCollider> where TCollider : ICircle2dCollider
{
	public CollisionMultiGrid(int minLevel, int maxLevel, float minX, float minY, float size)
	{
		//create each grid level
		var cellCount = (int)MathF.Pow(2f, minLevel);
		for (int level = minLevel; level <= maxLevel; ++level, cellCount *= 2)
		{
			var grid = new Grid<List<TCollider>>(cellCount, cellCount);
			grid.ForEach(() => new List<TCollider>());
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

	public void Add(TCollider collider)
	{
		void AddToGridLevel(Grid<List<TCollider>> grid)
		{
			var unitX = (collider.CenterX - MinX) / Size;
			var unitY = (collider.CenterY - MinY) / Size;
			//calculate grid column and row
			var column = Math.Clamp((int)(unitX * grid.Columns), 0, grid.Columns - 1);
			var row = Math.Clamp((int)(unitY * grid.Rows), 0, grid.Rows - 1);
			grid[column, row].Add(collider);
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
		foreach (var (cellSize, grid) in multiGrid)
		{
			grid.ForEach(cell => cell.Clear());
		}
	}

	public void FindAllCollisions(Action<TCollider, TCollider> collisionHandler)
	{
		// from smallest objects to largest objects
		for (int level = MaxLevel; level >= MinLevel; --level)
		{
			var grid = GetGridLevel(level);
			for (int y = 0; y < grid.Rows; ++y)
			{
				for (int x = 0; x < grid.Columns; ++x)
				{
					CheckCell(level, x, y, collisionHandler);
				}
			}
		}
	}

	public IEnumerable<IReadOnlyGrid<List<TCollider>>> Grids => multiGrid.Select(level => level.Item2);

	public IReadOnlyGrid<List<TCollider>> GetGridLevel(int level) => multiGrid[level - MinLevel].Item2;

	private readonly List<(float cellSize, Grid<List<TCollider>>)> multiGrid = new();

	private (float cellSize, Grid<List<TCollider>> grid) GetLevel(int level) => multiGrid[level - MinLevel];

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
			void CheckCell(IReadOnlyList<TCollider> cellB)
			{
				foreach (var objB in cellB)
				{
					collisionHandler(obj, objB);
				}
			}

			//check 4 neighbors (of 8) for instance 3 with bigger y and one with just bigger x
			var existsBiggerX = x + 1 < grid.Columns;
			if (existsBiggerX)
			{
				CheckCell(grid[x + 1, y]);
			}
			if (y + 1 < grid.Rows)
			{
				CheckCell(grid[x, y + 1]);
				if (existsBiggerX)
				{
					CheckCell(grid[x + 1, y + 1]);
				}
				if (0 < x)
				{
					CheckCell(grid[x - 1, y + 1]);
				}
			}

			//check with cells with smaller level == bigger cell size
			//we only need to check with direct neighbors in each level
			for (int l = level - 1, shift = 1; l >= MinLevel; --l, ++shift)
			{
				var lowResGrid = GetGridLevel(l);
				var biggerX = x >> shift;
				var biggerY = y >> shift;
				var biggerCell = lowResGrid[biggerX, biggerY];
				CheckCell(biggerCell);
				var minX = Math.Max(0, biggerX - 1);
				var minY = Math.Max(0, biggerY - 1);
				var maxX = Math.Min(lowResGrid.Columns, biggerX + 2);
				var maxY = Math.Min(lowResGrid.Rows, biggerY + 2);
				for (int row = minY; row < maxY; ++row)
				{
					for (int column = minX; column < maxX; ++column)
					{
						CheckCell(lowResGrid[column, row]);
					}
				}
			}
		}
	}
}
