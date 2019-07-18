using PSH.Physics.Collisions.Colliders;

namespace PSH.Physics.Collisions.Intersections
{
	public interface IIntersection
	{
		bool Collided {get;}
		
		ICollider A {get;}

		ICollider B {get;}

		CPhysics CachedA { get; set; }

		CPhysics CachedB { get; set; }

		Manifold Manifold { get; set; }

		Manifold GenerateManifold();
	}
}
