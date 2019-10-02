using System;
using System.Collections.Generic;
using System.Diagnostics;
using Zenseless.Patterns;

namespace Example
{
	/// <summary>
	/// Class that handles the collision detection
	/// </summary>
	internal class CollisionDetection
	{
		public CollisionDetection(IGameObjectProvider scene, ICollisionParameters parameters)
		{
			this.scene = scene;
			Recreate(scene, parameters);
		}

		internal void Recreate(IGameObjectProvider scene, ICollisionParameters parameters)
		{
			CollisionCount = 0;

			//SAP only usable if iterative so do not add/delete GameObjects without adding/removing them from the SAP structure too!
			collisionTime.Clear();
			iterativeCollisionMethod = false;
			switch(parameters.CollisionMethod)
			{
				case CollisionMethodTypes.BruteForce:
					CollisionMethod = new CollisionBruteForce<GameObject>();
					iterativeCollisionMethod = true;
					break;
				case CollisionMethodTypes.Grid:
					CollisionMethod = new CollisionGrid<GameObject>(-1f, -1f, 2f, 2f, parameters.CellCount, parameters.CellCount);
					break;
				case CollisionMethodTypes.MultiGrid:
					var level = (int)Math.Ceiling(Math.Log(parameters.CellCount) / Math.Log(2.0));
					CollisionMethod = new CollisionMultiGrid<GameObject>(level - 1, level, -1f, -1f, 2f);
					break;
				case CollisionMethodTypes.SAP_X:
					CollisionMethod = new CollisionSAP<GameObject>();
					iterativeCollisionMethod = true;
					break;
				case CollisionMethodTypes.PersistentSAP:
					CollisionMethod = new CollisionPersistentSAP<GameObject>();
					iterativeCollisionMethod = true;
					break;
				default: throw new ArgumentOutOfRangeException($"Collision method {parameters.CollisionMethod} unknown");
			}
			if(iterativeCollisionMethod)
			{
				CollisionMethod.Clear();
				foreach (var gameObject in GameObjects)
				{
					CollisionMethod.Add(gameObject);
				}
			}
			OnRegeneration?.Invoke(this, EventArgs.Empty);
		}

		public int BroadPhaseCollisionCount { get; private set; } = 0;
		public int CollisionCount { get; private set; } = 10000;

		public float CollisionTimeMsec { get; private set; } = 5.0001f;

		private IReadOnlyList<GameObject> GameObjects => scene.GameObjects;

		internal HashSet<(GameObject, GameObject)> FindCollisions()
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			if (!iterativeCollisionMethod)
			{
				CollisionMethod.Clear();
				foreach (var gameObject in GameObjects)
				{
					CollisionMethod.Add(gameObject);
				}
			}
			var collidingSet = new HashSet<(GameObject, GameObject)>();
			BroadPhaseCollisionCount = 0;
			CollisionMethod.FindAllCollisions((a, b) => TestForCollision(collidingSet, a, b));
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

		private ExponentialSmoothing collisionTime = new ExponentialSmoothing(0.01);
		private IGameObjectProvider scene;
		private bool iterativeCollisionMethod;

		[UiIgnore]
		public ICollisionMethodBroadPhase<GameObject> CollisionMethod { get; private set; } = null;

		public event EventHandler OnRegeneration;

		private void TestForCollision(HashSet<(GameObject, GameObject)> collidingSet, GameObject a, GameObject b)
		{
			++BroadPhaseCollisionCount;
			if (a.Intersects(b))
			{
				collidingSet.Add(a.GetHashCode() < b.GetHashCode() ? (a, b) : (b, a));
			}
		}
	}
}
