using System.ComponentModel;

namespace Example
{
	public enum CollisionMethodTypes { BruteForce, SAP_X, Grid, PersistentSAP, MultiGrid };

	interface ICollisionParameters : INotifyPropertyChanged
	{
		int CellCount { get; }
		bool CollisionDetection { get; }
		CollisionMethodTypes CollisionMethod { get; }
		bool DebugAlgo { get; }
	}
}