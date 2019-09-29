using System.Collections.Generic;

namespace Example
{
	public interface IGameObjectProvider
	{
		IReadOnlyList<GameObject> GameObjects { get; }
	}
}