using System.Collections.Generic;
using Zenseless.Spatial;

namespace Collision;

internal interface ICollisionGrid<TCollider>
{
	IEnumerable<IReadOnlyGrid<List<TCollider>>> Grids { get; }
}