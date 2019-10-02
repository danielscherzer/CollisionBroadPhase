using Zenseless.Patterns;

namespace Example
{
	class CollisionAdapter : NotifyPropertyChanged, ICollisionParameters
	{
		public CollisionAdapter(IGameObjectProvider scene)
		{
			//collisionDetection = new CollisionDetection(scene);
		}

		[UiIncrement(8)]
		public int CellCount //TODO: react to change
		{
			get => _cellCount;
			set => SetNotify(ref _cellCount, value);
		}

		public bool CollisionDetection
		{
			get => _collisionDetection;
			set => SetNotify(ref _collisionDetection, value);
		}

		public CollisionMethodTypes CollisionMethod
		{
			get => _collisionMethod;
			set => SetNotify(ref _collisionMethod, value);
		}

		public bool DebugAlgo
		{
			get => _debugAlgo;
			set => SetNotify(ref _debugAlgo, value);
		}

		private bool _collisionDetection = true;
		private int _cellCount = 32;
		private CollisionMethodTypes _collisionMethod = CollisionMethodTypes.PersistentSAP;
		private bool _debugAlgo = false;
	}
}
