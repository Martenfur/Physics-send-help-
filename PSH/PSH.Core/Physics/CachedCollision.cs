using Microsoft.Xna.Framework;
using PSH.Physics.Collisions;
using PSH.Physics.Collisions.Intersections;

namespace PSH.Physics
{
	/// <summary>
	/// Contains information about collision. 
	/// Used to cache collisions for SPhysics.
	/// </summary>
	struct CachedCollision
	{
		public CPhysics A;
		public CPhysics B;

		public IIntersection Intersection;
		public Manifold Manifold;

		/// <summary>
		/// Sum of inverse masses.
		/// </summary>
		public float InvMassSum;

		/// <summary>
		/// Min elasticity multiplied by the manifold direction.
		/// </summary>
		public Vector2 ElasticityDirection;
	}
}
