using System;

namespace Collision
{
	public class Grid<T> : IReadOnlyGrid<T>
	{
		public Grid(int columns, int rows)
		{
			if (0 == columns) throw new ArgumentOutOfRangeException(nameof(columns));
			if (0 == rows) throw new ArgumentOutOfRangeException(nameof(rows));
			Columns = columns;
			Rows = rows;
			Array = new T[columns * rows];
		}

		public T this[int column, int row]
		{
			get => Array[Index(column, row)];
			set => Array[Index(column, row)] = value;
		}

		public delegate void CellHandler(int column, int row, ref T value);
		public void ForEach(CellHandler cellHandler)
		{
			if (cellHandler is null) throw new ArgumentNullException(nameof(cellHandler));
			for (int row = 0; row < Rows; ++row)
			{
				for (int column = 0; column < Columns; ++column)
				{
					cellHandler(column, row, ref Array[Index(column, row)]);
				}
			}
		}

		public delegate void Initializer(ref T value);
		public void ForEach(Initializer initializer)
		{
			if (initializer is null) throw new ArgumentNullException(nameof(initializer));
			for (int i = 0; i < Array.Length; ++i)
			{
				initializer(ref Array[i]);
			}
		}

		public T[] Array { get; }
		public int Columns { get; }
		public int Rows { get; }

		private int Index(int column, int row) => column + Columns * row;
	}
}
