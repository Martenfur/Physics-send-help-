using Microsoft.Xna.Framework;
using PSH.Physics.Collisions;
using PSH.Physics.Collisions.Intersections;

namespace PSH.Physics
{
	/// <summary>
	/// Information about collision.
	/// </summary>
	struct CachedCollision
	{
		public CPhysics A;
		public CPhysics B;

		public IIntersection Intersection;
		public Manifold Manifold;

		public float InvMassSum;

		/// <summary>
		/// Min elasticity multiplied by the manifold direction.
		/// </summary>
		public Vector2 ElasticityDirection;
	}
}
