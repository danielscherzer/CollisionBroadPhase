﻿using System;

namespace Example
{
	public interface ICollisionMethodBroadPhase<TCollider>
	{
		void Add(TCollider collider);
		void Clear();
		void FindAllCollisions(Action<TCollider, TCollider> collisionHandler);
	}
}