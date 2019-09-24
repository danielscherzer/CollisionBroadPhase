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
		public void Add(TCollider objectBounds)
		{
			InsertIntoSorted(boundsX, new LowerXBound(objectBounds));
			InsertIntoSorted(boundsX, new UpperXBound(objectBounds));
			InsertIntoSorted(boundsY, new LowerYBound(objectBounds));
			InsertIntoSorted(boundsY, new UpperYBound(objectBounds));
		}

		public void Clear()
		{
			boundsX.Clear();
			boundsY.Clear();
			fullOverlaps.Clear();
		}

		internal void UpdateBounds()
		{
			foreach(var bound in boundsX)
			{
				bound.UpdateValue();
			}
			InsertionSort(boundsX);

			foreach (var bound in boundsY)
			{
				bound.UpdateValue();
			}
			InsertionSort(boundsY);
		}

		internal void FindAllCollisions(Action<TCollider, TCollider> collisionHandler)
		{
			var activeBounds = new HashSet<TCollider>();
			//var debug = new List<int>();
			//activeBounds.Clear();
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
						var colliderA = bound.Collider;
						activeBounds.Remove(colliderA);
	//					debug.Add(activeBounds.Count);
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

		private class LowerYBound : Bound
		{
			public LowerYBound(TCollider collider) : base(collider) { }

			public override void UpdateValue() => _value = Collider.MinY;
		}

		private class UpperYBound : Bound
		{
			public UpperYBound(TCollider collider) : base(collider) { }

			public override void UpdateValue() => _value = Collider.MaxY;
		}

		private struct OverlapPair : IEquatable<OverlapPair>
		{
			public TCollider A, B;

			public OverlapPair(TCollider a, TCollider b)
			{
				A = a;
				B = b;
			}

			public bool Equals(OverlapPair other)
			{
				return (ReferenceEquals(A, other.A) && ReferenceEquals(B, other.B)) ||
					(ReferenceEquals(A, other.B) && ReferenceEquals(B, other.A));
			}

			public override int GetHashCode()
			{
				return A.GetHashCode() + B.GetHashCode();
			}
		}

		private readonly List<Bound> boundsX = new List<Bound>();
		private readonly List<Bound> boundsY = new List<Bound>();
		private readonly HashSet<OverlapPair> fullOverlaps = new HashSet<OverlapPair>();

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
}
