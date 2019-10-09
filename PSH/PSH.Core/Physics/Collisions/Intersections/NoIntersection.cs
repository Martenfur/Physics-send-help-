using PSH.Physics.Collisions.Colliders;

namespace PSH.Physics.Collisions.Intersections
{
	/// <summary>
	/// Used when there is no intersection.
	/// </summary>
	public class NoIntersection : IIntersection
	{
		public bool Collided => false;
		public ICollider A => null;
		public ICollider B => null;

		public void Setup() {}

		public Manifold GenerateManifold() => default(Manifold); 
	}
}
