using Zenseless.Patterns;

namespace Example
{
	class Parameters : NotifyPropertyChanged
	{
		public bool CollisionDetection
		{
			get => _collisionDetection;
			set => SetNotify(ref _collisionDetection, value);
		}

		public bool DebugAlgo
		{
			get => _debugAlgo;
			set => SetNotify(ref _debugAlgo, value);
		}

		private bool _collisionDetection = true;
		private bool _debugAlgo = false;
	}
}
