using PSH.Collisions.Intersections;

namespace PSH.Collisions
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
