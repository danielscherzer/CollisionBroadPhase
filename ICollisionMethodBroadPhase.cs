using System;

namespace Example
{
	public interface ICollisionMethodBroadPhase<TCollider>
	{
//		void Add(IBox2DCollider objectBounds);
		void Clear();
		void FindAllCollisions(Action<TCollider, TCollider> collisionHandler);
	}
}