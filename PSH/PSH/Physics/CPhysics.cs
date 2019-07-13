using Microsoft.Xna.Framework;
using Monofoxe.Engine.ECS;
using PSH.Physics.Collisions;

namespace PSH.Physics
{
	public class CPhysics : Component
	{
		public Vector2 Speed;
		public ICollider Collider;

		public CPhysics()
		{
			Visible = true;
		}
	}
}
