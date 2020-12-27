using System;
using System.Collections.Generic;

namespace Collision
{
	class CollisionBruteForce<TCollider> : ICollisionMethodBroadPhase<TCollider>
	{

		public void Add(TCollider collider)
		{
			colliders.Add(collider);
		}

		public void Clear()
		{
			colliders.Clear();
		}

		public void FindAllCollisions(Action<TCollider, TCollider> collisionHandler)
		{
			//Check all colliders for collision with any other game object. And add each colliding game object to the colliding set.
			for (int i = 0; i + 1 < colliders.Count; ++i)
			{
				for (int j = i + 1; j < colliders.Count; ++j)
				{
					collisionHandler(colliders[i], colliders[j]);
				}
			}
		}

		private readonly List<TCollider> colliders = new List<TCollider>();
	}
}
