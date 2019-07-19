using PSH.Physics.Collisions;
using PSH.Physics.Collisions.Intersections;

namespace PSH.Physics
{
	struct CachedCollision
	{
		public CPhysics A;
		public CPhysics B;

		public IIntersection Intersection;
		public Manifold Manifold;

		public float InvMassSum;
	}
}
