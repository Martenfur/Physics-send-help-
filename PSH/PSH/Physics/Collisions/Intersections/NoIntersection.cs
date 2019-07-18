using PSH.Physics.Collisions.Colliders;

namespace PSH.Physics.Collisions.Intersections
{
	/// <summary>
	/// Used when there is no intersection.
	/// </summary>
	public struct NoIntersection : IIntersection
	{
		public bool Collided => false;
		public ICollider A => null;
		public ICollider B => null;

		public CPhysics CachedA { get; set; }
		public CPhysics CachedB { get; set; }

		public Manifold Manifold {get; set;}

		public Manifold GenerateManifold() => new Manifold();
	}
}
