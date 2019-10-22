using Microsoft.Xna.Framework;
using PSH.Collisions.Colliders;
using System;

namespace PSH.Collisions.Intersections
{
	public class CircleCircleIntersection : IIntersection
	{
		public bool Collided {get; private set;}
		public ICollider A {get; private set;}
		public ICollider B {get; private set;}

		Vector2 _delta;

		float _lengthSqr;

		float _rSumSqr;

		public void Setup(
			ICollider a, 
			ICollider b,
			bool collided, 
			Vector2 delta, 
			float lengthSqr,
			float rSumSqr
		)
		{
			A = a;
			B = b;
			Collided = collided;
			_delta = delta;
			_lengthSqr = lengthSqr;
			_rSumSqr = rSumSqr;
		}

		public Manifold GenerateManifold()
		{
			var manifold = new Manifold();

			if (_lengthSqr != 0)
			{
				var length = (float)Math.Sqrt(_lengthSqr);

				manifold.Depth = (float)Math.Sqrt(_rSumSqr) - length;

				manifold.Direction = _delta / length;
			}
			else
			{
				manifold.Depth = A.HalfSize.X;
				manifold.Direction = Vector2.UnitX;
			}
			
			return manifold;
		}

	}
}
