using Collision;

namespace Example
{
	public interface ICollider : ICircle2dCollider, IBox2DCollider
	{
		void HandleCollision(ICollider other);
	}
}
