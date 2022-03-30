using System;
using System.Collections.Generic;

namespace Collision
{
	/// <summary>
	/// Sweep and Prune collision detection
	/// If built iteratively: fast if little movement (insertion sorting fast), degenerates with more movement (more sorting)
	/// Do not add/delete GameObjects without adding/removing them from the SAP structure too!
	/// </summary>
	public class CollisionPersistentSAP<TCollider> : ICollisionMethodBroadPhase<TCollider> where TCollider : IBox2DCollider
	{
		private int newlyAdded = 0;
		public void Add(TCollider objectBounds)
		{
			boundsX.Add(new LowerXBound(objectBounds));
			boundsX.Add(new UpperXBound(objectBounds));
			boundsY.Add(new LowerYBound(objectBounds));
			boundsY.Add(new UpperYBound(objectBounds));
			++newlyAdded;
		}

		public void Clear()
		{
			boundsX.Clear();
			boundsY.Clear();
			fullOverlaps.Clear();
		}

		private void UpdateBounds()
		{
			foreach (var bound in boundsX)
			{
				bound.UpdateValue();
			}

			foreach (var bound in boundsY)
			{
				bound.UpdateValue();
			}
		}

		public void FindAllCollisions(Action<TCollider, TCollider> collisionHandler)
		{
			UpdateBounds();
			if (1000 < newlyAdded)
			{
				fullOverlaps.Clear();
				QuickSortAxis(boundsX, CollisionTest.IntersectsY);
				QuickSortAxis(boundsY, CollisionTest.IntersectsX);
			}
			else
			{
				InsertionSortAxis(boundsX, CollisionTest.IntersectsY);
				InsertionSortAxis(boundsY, CollisionTest.IntersectsX);
			}
			newlyAdded = 0;

			foreach (var overlapPair in fullOverlaps)
			{
				collisionHandler(overlapPair.A, overlapPair.B);
			}

		}

		private abstract class Bound : IComparable<Bound>
		{
			protected float _value;

			public TCollider Collider { get; }
			public float Value => _value;

			public abstract bool IsLowerBound { get; }

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

			public override bool IsLowerBound => true;

			public override void UpdateValue() => _value = Collider.MinX;
		}

		private class UpperXBound : Bound
		{
			public UpperXBound(TCollider collider) : base(collider) { }

			public override bool IsLowerBound => false;

			public override void UpdateValue() => _value = Collider.MaxX;
		}

		private class LowerYBound : Bound
		{
			public LowerYBound(TCollider collider) : base(collider) { }

			public override bool IsLowerBound => true;

			public override void UpdateValue() => _value = Collider.MinY;
		}

		private class UpperYBound : Bound
		{
			public UpperYBound(TCollider collider) : base(collider) { }

			public override bool IsLowerBound => false;

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

				//return (other.A.Equals(A) && other.B.Equals(B)) || 
				//	/other.A.Equals(B) && other.B.Equals(A));

			}

			public override int GetHashCode()
			{
				return A.GetHashCode() + B.GetHashCode();
			}
		}

		private readonly List<Bound> boundsX = new List<Bound>();
		private readonly List<Bound> boundsY = new List<Bound>();
		private readonly HashSet<OverlapPair> fullOverlaps = new HashSet<OverlapPair>();

		/// <summary>
		/// O(n) for a nearly sorted list
		/// </summary>
		private void InsertionSortAxis(List<Bound> bounds, Func<IBox2DCollider, IBox2DCollider, bool> checkOtherAxis)
		{
			for (int j = 1; j < bounds.Count; j++)
			{
				var bound = bounds[j];
				float key = bound.Value;

				int i = j - 1;

				while (i >= 0 && bounds[i].Value > key)
				{
					var swapBound = bounds[i];

					if (bound.IsLowerBound && !swapBound.IsLowerBound)
					{
						if (checkOtherAxis(swapBound.Collider, bound.Collider))
						{
							fullOverlaps.Add(new OverlapPair(swapBound.Collider, bound.Collider));
						}
					}

					if (!bound.IsLowerBound && swapBound.IsLowerBound)
					{
						fullOverlaps.Remove(new OverlapPair(swapBound.Collider, bound.Collider));
					}

					bounds[i + 1] = swapBound;
					--i;
				}
				bounds[i + 1] = bound;
			}
		}

		/// <summary>
		/// O(n log(n))
		/// </summary>
		private void QuickSortAxis(List<Bound> bounds, Func<IBox2DCollider, IBox2DCollider, bool> checkOtherAxis)
		{
			int Comparer(Bound a, Bound b)
			{
				return (a.Value > b.Value) ? 1 : ((b.Value > a.Value) ? -1 : 0);
			}

			bounds.Sort(Comparer);
			var activeBounds = new HashSet<TCollider>(); // only called if many objects added at once
			foreach (var bound in bounds)
			{
				if (bound.IsLowerBound)
				{
					foreach (var collider in activeBounds)
					{
						if (checkOtherAxis(collider, bound.Collider))
						{
							fullOverlaps.Add(new OverlapPair(collider, bound.Collider));
						}
					}
					activeBounds.Add(bound.Collider);
				}
				else
				{
					activeBounds.Remove(bound.Collider);
				}
			}
		}
	}
}
