using System.Collections.Generic;

namespace Example;

public interface IColliderProvider
{
	IReadOnlyList<ICollider> Collider { get; }
}