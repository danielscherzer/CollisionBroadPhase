using System;
using Zenseless.Spatial;

namespace Collision;

internal static class GridHelper
{
	public static void ForEach<T>(this Grid<T> grid, Func<T> initializer)
	{
		ArgumentNullException.ThrowIfNull(initializer);
		for (int row = 0; row < grid.Rows; ++row)
		{
			for (int column = 0; column < grid.Columns; ++column)
			{
				grid[column, row] = initializer();
			}
		}
	}

	public static void ForEach<T>(this Grid<T> grid, Action<T> cellProcessor)
	{
		ArgumentNullException.ThrowIfNull(cellProcessor);
		for (int row = 0; row < grid.Rows; ++row)
		{
			for (int column = 0; column < grid.Columns; ++column)
			{
				cellProcessor(grid[column, row]);
			}
		}
	}
}
