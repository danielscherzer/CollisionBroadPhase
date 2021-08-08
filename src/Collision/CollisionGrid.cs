using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Zenseless.Spatial;

namespace Collision
{
	public class CollisionGrid<TCollider> : ICollisionGrid<TCollider>, ICollisionMethodBroadPhase<TCollider> where TCollider : IBox2DCollider
	{
		public int CellCountX => _cells.Columns;
		public int CellCountY => _cells.Rows;
		public Vector2 CellSize { get; }
		public float MinX { get; }
		public float MinY { get; }

		public IEnumerable<IReadOnlyGrid<List<TCollider>>> Grids
		{
			get
			{
				yield return _cells;
			}
		}

		public CollisionGrid(float minX, float minY, float sizeX, float sizeY, int cellCountX, int cellCountY)
		{
			MinX = minX;
			MinY = minY;
			_cells = new Grid<List<TCollider>>(cellCountX, cellCountY);
			if (0 >= sizeX) throw new ArgumentOutOfRangeException(nameof(sizeX));
			if (0 >= sizeY) throw new ArgumentOutOfRangeException(nameof(sizeY));
			CellSize = new Vector2(sizeX / cellCountX, sizeY / cellCountY);
			_cells.ForEach(() => new List<TCollider>());
		}

		public void Add(TCollider collider)
		{
			// Convert the object's AABB to integer grid coordinates.
			// Objects outside of the grid are clamped to the edge.
			int minX = Math.Max((int)Math.Floor((collider.MinX - MinX) / CellSize.X), 0);
			int maxX = Math.Min((int)Math.Floor((collider.MaxX - MinX) / CellSize.X), CellCountX - 1);
			int minY = Math.Max((int)Math.Floor((collider.MinY - MinY) / CellSize.Y), 0);
			int maxY = Math.Min((int)Math.Floor((collider.MaxY - MinY) / CellSize.Y), CellCountY - 1);

			// Loop over the cells the object overlaps and insert the object into each.
			for (int y = minY; y <= maxY; ++y)
			{
				for (int x = minX; x <= maxX; ++x)
				{
					_cells[x, y].Add(collider);
				}
			}
		}

		public void Clear() => _cells.ForEach(cell => cell.Clear());

		public void FindAllCollisions(Action<TCollider, TCollider> collisionHandler)
		{
			var partitions = Partitioner.Create(0, _cells.AsReadOnly.Count);
			//Parallel.ForEach(partitions, range =>
			//{
			//	for (int i = range.Item1; i < range.Item2; i++)
			//	{
			//		CheckCell(collisionHandler, _cells.Array[i]);
			//	}
			//});
			_cells.ForEach(cell => CheckCell(collisionHandler, cell));
		}

		private static void CheckCell(Action<TCollider, TCollider> collisionHandler, IReadOnlyList<TCollider> cell)
		{
			for (int i = 0; i + 1 < cell.Count; ++i)
			{
				//check each collider against every other collider
				for (int j = i + 1; j < cell.Count; ++j)
				{
					collisionHandler(cell[i], cell[j]);
				}
			}
		}

		private readonly Grid<List<TCollider>> _cells;
	}
}
