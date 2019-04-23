using System;
using System.Collections.Generic;

namespace Example
{
	/// <summary>
	/// Sweep and Prune collision detection
	/// Not implemented just quick test, if it is usable for non iterative applications
	/// Result: too slow, but if used iteratively with insertion sort it could be rather fast
	/// </summary>
	public class CollisionSAP<TCollider>
	{
		internal void Clear()
		{
		}

		internal void Add(IBox2DCollider objectBounds)
		{
			if (!FirstTime) return;
			boundsX.Add(new Data(true, objectBounds));
			boundsX.Add(new Data(false, objectBounds));
		}

		internal void FindAllCollisions(Action<TCollider, TCollider> collisionHandler)
		{
			if (FirstTime)
			{
				boundsX.Sort();
			}
			else
			{
				InsertionSort();
			}
			FirstTime = false;
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

		private struct Data : IComparable<Data>
		{
			public bool IsMin { get; }
			public IBox2DCollider Collider { get; }
			public float Value { get; }

			public Data(bool isMin, IBox2DCollider collider)
			{
				IsMin = isMin;
				Collider = collider;
				Value = isMin ? collider.MinX : collider.MaxX;
			}

			public int CompareTo(Data other) => Value.CompareTo(other.Value);
		}

		private readonly List<Data> boundsX = new List<Data>();

		public bool FirstTime { get; private set; } = true;
	}
}
