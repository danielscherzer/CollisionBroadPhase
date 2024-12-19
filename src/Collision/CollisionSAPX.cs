using System;
using System.Collections.Generic;

namespace Collision;

/// <summary>
/// Sweep and Prune collision detection in one axis direction
/// If built iteratively: fast if little movement (insertion sorting fast), degenerates with more movement (more sorting)
/// Do not add/delete GameObjects without adding/removing them from the SAP structure too!
/// </summary>
public class CollisionSAPX<TCollider> : ICollisionMethodBroadPhase<TCollider> where TCollider : IBox2DCollider
{
	public void Add(TCollider objectBounds)
	{
		InsertIntoSorted(boundsX, new LowerXBound(objectBounds));
		InsertIntoSorted(boundsX, new UpperXBound(objectBounds));
	}

	public void Clear()
	{
		boundsX.Clear();
	}

	private void UpdateBounds()
	{
		foreach (var bound in boundsX)
		{
			bound.UpdateValue();
		}
		InsertionSort(boundsX);
	}

	public void FindAllCollisions(Action<TCollider, TCollider> collisionHandler)
	{
		UpdateBounds();
		var activeBounds = new HashSet<TCollider>();
		foreach (var bound in boundsX)
		{
			switch (bound)
			{
				case LowerXBound lower:
					// only add starting colliders to active list
					activeBounds.Add(bound.Collider);
					break;
				case UpperXBound upper:
					// this collider has ended -> remove from active list
					var colliderA = bound.Collider;
					activeBounds.Remove(colliderA);
					// check for collision with all on active list
					foreach (var colliderB in activeBounds)
					{
						// do y-check then call exact collision handler
						if (colliderA.IntersectsY(colliderB))
						{
							collisionHandler(colliderA, colliderB);
						}
					}
					break;
			}
		}
	}

	private abstract class Bound : IComparable<Bound>
	{
		protected float _value;

		public TCollider Collider { get; }
		public float Value => _value;

		public Bound(TCollider collider)
		{
			Collider = collider;
			UpdateValue();
		}

		public abstract void UpdateValue();

		public int CompareTo(Bound other) => Value.CompareTo(other.Value);
	}

	private class LowerXBound : Bound
	{
		public LowerXBound(TCollider collider) : base(collider) { }

		public override void UpdateValue() => _value = Collider.MinX;
	}

	private class UpperXBound : Bound
	{
		public UpperXBound(TCollider collider) : base(collider) { }

		public override void UpdateValue() => _value = Collider.MaxX;
	}

	private readonly List<Bound> boundsX = new();

	private static void InsertIntoSorted(List<Bound> bounds, Bound bound)
	{
		var index = bounds.BinarySearch(bound);
		if (index < 0) index = ~index;
		bounds.Insert(index, bound);
	}

	/// <summary>
	/// O(n) for a nearly sorted list
	/// </summary>
	private static void InsertionSort(List<Bound> axsi)
	{
		for (int i = 1; i < axsi.Count; i++)
		{
			int j = i;
			while (j > 0)
			{
				if (0 < axsi[j - 1].CompareTo(axsi[j]))
				{
					var temp = axsi[j - 1];
					axsi[j - 1] = axsi[j];
					axsi[j] = temp;
					j--;
				}
				else
					break;
			}
		}
	}
}
