using System.ComponentModel;

namespace Example
{
	public enum CollisionMethodTypes { BruteForce, Grid, MultiGrid, SAP_X, PersistentSAP };

	interface IParameters : INotifyPropertyChanged
	{
		int CellCount { get; }
		bool CollisionDetection { get; }
		CollisionMethodTypes CollisionMethod { get; }
		bool DebugAlgo { get; }
		bool Freeze { get; }
		int ObjectCount { get; }
		float ObjectMinSize { get; }
		float ObjectSizeVariation { get; }
	}
}