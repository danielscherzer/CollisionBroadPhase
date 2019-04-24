using System;
using System.Collections.Generic;

namespace Example
{
	/// <summary>
	/// Sweep and Prune collision detection
	/// Not fully implemented just quick test, if it is usable for non iterative applications
	/// Result: Too slow if rebuilt each frame.
	/// If built iteratively: fast if little movement, degenerates with more movement (more sorting)
	/// Do not add/delete GameObjects without adding/removing them from the SAP structure too!
	/// </summary>
	public class CollisionSAP<TCollider>
	{
		internal void Add(IBox2DCollider objectBounds)
		{
			InsertSorted(new LowerXBound(objectBounds));
			InsertSorted(new UpperXBound(objectBounds));
		}

		private void InsertSorted(Bound bound)
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
			//TODO: find active pairs (do we need a second boundsY list?)
		}

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

			public IBox2DCollider Collider { get; }
			public float Value => _value;

			public Bound(IBox2DCollider collider)
			{
				Collider = collider;
				UpdateValue();
			}

			abstract public void UpdateValue();

			public int CompareTo(Bound other) => Value.CompareTo(other.Value);
		}

		private class LowerXBound : Bound
		{
			public LowerXBound(IBox2DCollider collider) : base(collider)
			{
			}

			public override void UpdateValue()
			{
				_value = Collider.MinX;
			}
		}

		private class UpperXBound : Bound
		{
			public UpperXBound(IBox2DCollider collider) : base(collider)
			{
			}

			public override void UpdateValue()
			{
				_value = Collider.MaxX;
			}
		}

		private readonly List<Bound> boundsX = new List<Bound>();
	}
}
