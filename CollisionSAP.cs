using System;
using System.Collections.Generic;

namespace Example
{
	/// <summary>
	/// Sweep and Prune collision detection
	/// Not fully implemented just quick test, if it is usable for non iterative applications
	/// Result: Too slow if rebuilt each frame.
	/// If built iteratively: fast if little movement (insertion sorting fast), degenerates with more movement (more sorting)
	/// Do not add/delete GameObjects without adding/removing them from the SAP structure too!
	/// </summary>
	public class CollisionSAP<TCollider> where TCollider : IBox2DCollider
	{
		internal void Add(TCollider objectBounds)
		{
			InsertIntoSorted(new LowerXBound(objectBounds));
			InsertIntoSorted(new UpperXBound(objectBounds));
		}

		private void InsertIntoSorted(Bound bound)
		{
			var index = boundsX.BinarySearch(bound);
			if (index < 0) index = ~index;
			boundsX.Insert(index, bound);
		}

		internal void UpdateBounds()
		{
			foreach(var bound in boundsX)
			{
				bound.UpdateValue();
			}
			InsertionSort();
		}

		internal void FindAllCollisions(Action<TCollider, TCollider> collisionHandler)
		{
			activeBounds.Clear();
			//TODO: find active pairs (do we need a second boundsY list?)
			foreach(var bound in boundsX)
			{
				switch(bound)
				{
					case LowerXBound lower:
						// only add starting colliders to active list
						activeBounds.Add(bound.Collider);
						break;
					case UpperXBound upper:
						// this collider has ended -> remove from active list
						activeBounds.Remove(bound.Collider);
						// check for collision with all on active list
						foreach (var collider in activeBounds)
						{
							collisionHandler(bound.Collider, collider);
						}
						break;
				}
			}
		}

		/// <summary>
		/// O(n) for a nearly sorted list
		/// </summary>
		private void InsertionSort()
		{
			for (int i = 1; i < boundsX.Count; i++)
			{
				int j = i;
				while (j > 0)
				{
					if (0 < boundsX[j - 1].CompareTo(boundsX[j]))
					{
						var temp = boundsX[j - 1];
						boundsX[j - 1] = boundsX[j];
						boundsX[j] = temp;
						j--;
					}
					else
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

			abstract public void UpdateValue();

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

		private readonly List<Bound> boundsX = new List<Bound>();
		private readonly HashSet<TCollider> activeBounds = new HashSet<TCollider>();
	}
}
