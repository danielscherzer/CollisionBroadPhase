using System.Collections.Generic;

namespace Collision
{
	internal interface ICollisionGrid<TCollider>
	{
		IEnumerable<IReadOnlyGrid<List<TCollider>>> Grids { get; }
	}
}