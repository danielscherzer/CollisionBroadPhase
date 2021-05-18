namespace Collision
{
	public interface IReadOnlyGrid<T>
	{
		T this[int column, int row] { get; }

		int Columns { get; }
		int Rows { get; }
	}
}