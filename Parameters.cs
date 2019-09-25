using Zenseless.Patterns;

namespace Example
{
	class Parameters : NotifyPropertyChanged, IParameters
	{
		[Increment(1000)]
		public int ObjectCount
		{
			get => _objectCount;
			set => SetNotify(ref _objectCount, value);
		}

		[Increment(0.001f)]
		public float ObjectMinSize
		{
			get => _objectMinSize;
			set => SetNotify(ref _objectMinSize, value);
		}

		[Increment(0.001f)]
		public float ObjectSizeVariation
		{
			get => _objectSizeVariation;
			set => SetNotify(ref _objectSizeVariation, value);
		}

		[Increment(8)]
		public int CellCount
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

		public bool Freeze
		{
			get => _freeze;
			set => SetNotify(ref _freeze, value);
		}

		private bool _collisionDetection = true;
		private int _cellCount = 32;
		private int _objectCount = 2000;
		private CollisionMethodTypes _collisionMethod = CollisionMethodTypes.MultiGrid;
		private bool _debugAlgo = false;
		private bool _freeze = true;
		private float _objectMinSize = 0.01f;
		private float _objectSizeVariation = 0.002f;
	}
}
