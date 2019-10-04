using System.Collections.Generic;

namespace Example
{
	interface ICollisionGrid<TCollider>
	{
		IEnumerable<IReadOnlyList<TCollider>[,]> Grids { get; }
	}
}