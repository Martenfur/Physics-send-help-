using Microsoft.Xna.Framework;
using PSH.Physics.Collisions.Colliders;

namespace PSH.Physics.Collisions.Intersections
{
	public struct RectangleRectangleIntersection : IIntersection
	{
		public bool Collided {get; private set;}
		public ICollider A {get; private set;}
		public ICollider B {get; private set;}

		Vector2 _overlap;

		Vector2 _delta;

		public RectangleRectangleIntersection(
			ICollider a, 
			ICollider b,
			bool collided, 
			Vector2 delta, 
			Vector2 overlap
		)
		{
			A = a;
			B = b;
			Collided = collided;
			_delta = delta;
			_overlap = overlap;
		}

		public Manifold GenerateManifold()
		{
			var manifold = new Manifold();

			if (_overlap.X < _overlap.Y)
			{
				// Point towards B knowing that n points from A to B
				if (_delta.X < 0)
				{
					manifold.Direction = new Vector2(-1, 0);
				}
				else
				{
					manifold.Direction = new Vector2(1, 0);
				}
				manifold.Depth = _overlap.X;
			}
			else
			{
				// Point towards B knowing that n points from A to B
				if (_delta.Y < 0)
				{
					manifold.Direction = new Vector2(0, -1);
				}
				else
				{
					manifold.Direction = new Vector2(0, 1);
				}
				manifold.Depth = _overlap.Y;
			}
			return manifold;
		}

	}
}
