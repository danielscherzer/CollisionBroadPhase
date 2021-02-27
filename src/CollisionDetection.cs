using Collision;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UI;
using Zenseless.Patterns;

namespace Example
{
	/// <summary>
	/// Class that handles the collision detection
	/// </summary>
	public class CollisionDetection : NotifyPropertyChanged
	{
		public CollisionDetection(IColliderProvider scene)
		{
			this.scene = scene;
			Update();
		}

		[UiValueChangeFunction(8)]
		public uint CellCount
		{
			get => _cellCount;
			set => SetNotify(ref _cellCount, value, cellCount => Update());
		}

		public enum CollisionMethodTypes { BruteForce, Grid, PersistentSAP, SAP_X, MultiGrid };
		public CollisionMethodTypes CollisionMethod
		{
			get => _collisionMethod;
			set => SetNotify(ref _collisionMethod, value, method => Update());
		}

		internal void Update()
		{
			CollisionCount = 0;
			//SAP only usable if iterative so do not add/delete GameObjects without adding/removing them from the SAP structure too!
			collisionTime.Clear();
			iterativeCollisionMethod = false;
			switch(CollisionMethod)
			{
				case CollisionMethodTypes.BruteForce:
					Algorithm = new CollisionBruteForce<ICollider>();
					iterativeCollisionMethod = true;
					break;
				case CollisionMethodTypes.Grid:
					Algorithm = new CollisionGrid<ICollider>(-1f, -1f, 2f, 2f, CellCount, CellCount);
					break;
				case CollisionMethodTypes.MultiGrid:
					var level = (int)Math.Ceiling(Math.Log(CellCount) / Math.Log(2.0));
					Algorithm = new CollisionMultiGrid<ICollider>(level - 1, level, -1f, -1f, 2f);
					break;
				case CollisionMethodTypes.SAP_X:
					Algorithm = new CollisionSAPX<ICollider>();
					iterativeCollisionMethod = true;
					break;
				case CollisionMethodTypes.PersistentSAP:
					Algorithm = new CollisionPersistentSAP<ICollider>();
					iterativeCollisionMethod = true;
					break;
				default: throw new ArgumentOutOfRangeException($"Collision method {CollisionMethod} unknown");
			}
			if(iterativeCollisionMethod)
			{
				AddSceneObjects();
			}
			OnUpdate?.Invoke(this, EventArgs.Empty);
		}

		public int CollisionCount { get; private set; } = 10000;
		public float CollisionTimeMsec { get; private set; } = 5.0001f;
		[UiIgnore]
		public ICollisionMethodBroadPhase<ICollider> Algorithm { get; private set; } = null;
		public event EventHandler OnUpdate;

		public HashSet<(ICollider, ICollider)> FindCollisions()
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			if (!iterativeCollisionMethod)
			{
				AddSceneObjects();
			}
			var collidingSet = new HashSet<(ICollider, ICollider)>();
			Algorithm.FindAllCollisions((a, b) => ExactCollision(collidingSet, a, b));
			stopWatch.Stop();

			collisionTime.NewSample(stopWatch.Elapsed.TotalMilliseconds);
			CollisionTimeMsec = (float)Math.Round(collisionTime.SmoothedValue, 2);
			foreach (var (collider1, collider2) in collidingSet)
			{
				collider1.HandleCollision(collider2);
				collider2.HandleCollision(collider1);
			}
			CollisionCount = collidingSet.Count;
			return collidingSet;
		}

		private readonly ExponentialSmoothing collisionTime = new ExponentialSmoothing(0.01);
		private readonly IColliderProvider scene;
		private bool iterativeCollisionMethod;
		private CollisionMethodTypes _collisionMethod = CollisionMethodTypes.Grid;
		private uint _cellCount = 32;

		private void AddSceneObjects()
		{
			Algorithm.Clear();
			foreach (var gameObject in scene.Collider)
			{
				Algorithm.Add(gameObject);
			}
		}

		public static void ExactCollision(HashSet<(ICollider, ICollider)> collidingSet, ICollider a, ICollider b)
		{
			if (a.Intersects(b))
			{
				collidingSet.Add(a.GetHashCode() < b.GetHashCode() ? (a, b) : (b, a));
			}
		}
	}
}
