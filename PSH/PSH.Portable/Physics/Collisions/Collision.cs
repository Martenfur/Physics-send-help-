using PSH.Physics.Collisions.Intersections;

namespace PSH.Physics.Collisions
{
	/// <summary>
	/// Contains information about collision.
	/// </summary>
	public struct Collision
	{
		public CPhysics Other;
		public Manifold Manifold;
	}
}
