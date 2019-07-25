using Microsoft.Xna.Framework;

namespace PSH.Physics.Collisions
{
	/// <summary>
	/// Collision manifold. 
	/// Contains data about collision direction and depth.
	/// </summary>
	public struct Manifold
	{
		public float Depth;
		public Vector2 Direction;
	}
}
