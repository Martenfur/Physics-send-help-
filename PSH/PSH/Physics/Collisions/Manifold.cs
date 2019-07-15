using Microsoft.Xna.Framework;

namespace PSH.Physics.Collisions
{
	public struct Manifold
	{
		public bool Collided;

		public float Depth;
		public Vector2 Direction;
	}
}
