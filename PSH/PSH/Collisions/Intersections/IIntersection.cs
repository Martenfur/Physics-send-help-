using PSH.Collisions.Colliders;

namespace PSH.Collisions.Intersections
{
	public interface IIntersection
	{
		bool Collided {get;}
		
		ICollider A {get;}

		ICollider B {get;}
		
		Manifold GenerateManifold();
	}
}
