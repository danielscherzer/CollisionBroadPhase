using Zenseless.Patterns.Property;

namespace Example;

internal class Parameters : NotifyPropertyChanged
{
	public bool CollisionDetection
	{
		get => _collisionDetection;
		set => Set(ref _collisionDetection, value);
	}

	public bool DebugAlgo
	{
		get => _debugAlgo;
		set => Set(ref _debugAlgo, value);
	}

	private bool _collisionDetection = true;
	private bool _debugAlgo = false;
}
