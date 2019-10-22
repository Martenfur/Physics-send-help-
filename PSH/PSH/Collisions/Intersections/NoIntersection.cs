using PSH.Collisions.Colliders;

namespace PSH.Collisions.Intersections
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
