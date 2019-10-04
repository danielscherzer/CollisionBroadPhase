namespace Example
{
	using System;
	using System.Collections.Generic;
	using System.Numerics;

	public class CollisionGrid<TCollider> : ICollisionGrid<TCollider>, ICollisionMethodBroadPhase<TCollider> where TCollider : IBox2DCollider
	{
		public int CellCountX { get; }
		public int CellCountY { get; }
		public Vector2 CellSize { get; }
		public float MinX { get; }
		public float MinY { get; }

		public IEnumerable<IReadOnlyList<TCollider>[,]> Grids
		{
			get
			{
				yield return GetGrid();
			}
		}

		public CollisionGrid(float minX, float minY, float sizeX, float sizeY, uint cellCountX, uint cellCountY)
		{
			if (0 == cellCountX) throw new ArgumentOutOfRangeException(nameof(cellCountX));
			if (0 == cellCountY) throw new ArgumentOutOfRangeException(nameof(cellCountY));
			if (0 >= sizeX) throw new ArgumentOutOfRangeException(nameof(sizeX));
			if (0 >= sizeY) throw new ArgumentOutOfRangeException(nameof(sizeY));
			CellSize = new Vector2(sizeX / cellCountX, sizeY / cellCountY);

			cells = new List<TCollider>[cellCountX, cellCountY];
			for (int y = 0; y < cellCountY; ++y)
			{
				for (int x = 0; x < cellCountX; ++x)
				{
					cells[x, y] = new List<TCollider>();
				}
			}

			MinX = minX;
			MinY = minY;
			CellCountX = (int)cellCountX;
			CellCountY = (int)cellCountY;
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
					cells[x, y].Add(collider);
				}
			}
		}

		public void Clear()
		{
			foreach (var cell in cells)
			{
				cell.Clear();
			}
		}

		public void FindAllCollisions(Action<TCollider, TCollider> collisionHandler)
		{
			foreach(var cell in cells)
			{
				CheckCell(collisionHandler, cell);
			}
		}

		private static void CheckCell(Action<TCollider, TCollider> collisionHandler, List<TCollider> cell)
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

		public IReadOnlyList<TCollider> this[int x, int y] { get { return cells[x, y]; } }
		public IReadOnlyList<TCollider>[,] GetGrid() => cells;

		private readonly List<TCollider>[,] cells;
	}
}
