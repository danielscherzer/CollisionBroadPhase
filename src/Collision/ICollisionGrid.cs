using System.Collections.Generic;

namespace Collision
{
	internal interface ICollisionGrid<TCollider>
	{
		IEnumerable<IReadOnlyList<TCollider>[,]> Grids { get; }
	}
}