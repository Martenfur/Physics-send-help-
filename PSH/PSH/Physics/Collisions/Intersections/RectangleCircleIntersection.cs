using Microsoft.Xna.Framework;
using PSH.Physics.Collisions.Colliders;
using System;


namespace PSH.Physics.Collisions.Intersections
{
	public struct RectangleCircleIntersection : IIntersection
	{
		public bool Collided {get; private set;}
		public ICollider A {get; private set;}
		public ICollider B {get; private set;}
		
		Vector2 _delta;

		Vector2 _closestCorner;

		bool _inside;

		Vector2 _normal;

		float _normalLengthSquared;


		public RectangleCircleIntersection(
			ICollider a, 
			ICollider b,
			bool collided,
			Vector2 delta,
			Vector2 closestCorner,
			bool inside,
			Vector2 normal,
			float normalLengthSquared
		)
		{
			A = a;
			B = b;
			Collided = collided;
			_delta = delta;
			_closestCorner = closestCorner;
			_inside = inside;
			_normal = normal;
			_normalLengthSquared = normalLengthSquared;
		}

		public Manifold GenerateManifold()
		{
			var manifold = new Manifold();
			
			var normalLength = 0f;

			if (_normalLengthSquared == 0)
			{
				_normal = Vector2.UnitX;
			}
			else
			{
				normalLength = (float)Math.Sqrt(_normalLengthSquared);
				_normal /= normalLength;
			}
			
			if (_inside)
			{
				manifold.Direction = -_normal;
				manifold.Depth = ((CircleCollider)B).Radius + normalLength;
			}
			else
			{
				manifold.Direction = _normal;
				manifold.Depth = ((CircleCollider)B).Radius - normalLength;
			}
			
			return manifold;
		}

	}
}
