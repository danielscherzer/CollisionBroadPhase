using System.Collections.Generic;

namespace Collision
{
	interface ICollisionGrid<TCollider>
	{
		IEnumerable<IReadOnlyList<TCollider>[,]> Grids { get; }
	}
}