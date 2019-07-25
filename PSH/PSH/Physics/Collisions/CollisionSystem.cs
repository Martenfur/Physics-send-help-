using Microsoft.Xna.Framework;
using PSH.Physics.Collisions.Colliders;
using PSH.Physics.Collisions.Intersections;
using System;


namespace PSH.Physics.Collisions
{
	public delegate IIntersection IntersectionDelegate(ICollider a, ICollider b, bool flipNormal);

	public static class CollisionSystem
	{
		/// <summary>
		/// Contains all allowed collider combinations.
		/// </summary>
		private static IntersectionDelegate[,] _intersectionMatrix;

		private const int _intersectionMatrixSize = 2;

		public static void Init()
		{
			/*
			 *   | r  | c  | p  |
			 * ------------------
			 * r | rr | rc | rp |
			 * ------------------
			 * c | xx | cc | cp |
			 * ------------------
			 * p | xx | xx | pp |
			 */

			_intersectionMatrix = new IntersectionDelegate[_intersectionMatrixSize, _intersectionMatrixSize];

			_intersectionMatrix[
				(int)ColliderType.Rectangle,
				(int)ColliderType.Rectangle
			] = RectangleRectangle;

			_intersectionMatrix[
				(int)ColliderType.Circle,
				(int)ColliderType.Circle
			] = CircleCircle;

			_intersectionMatrix[
				(int)ColliderType.Rectangle,
				(int)ColliderType.Circle
			] = RectangleCircle;
			
		}

		public static IIntersection CheckCollision(ICollider collider1, ICollider collider2)
		{
			/*
			 * Each collider type has its own unique index.
			 * We are taking them and retrieving appropriate
			 * collision function.
			 */
			var id1 = (int)collider1.ColliderType;
			var id2 = (int)collider2.ColliderType;

			// Maybe add a null check here to prevent crashes, if some function isn't implemented.
			// Though, this probably won't be needed.

			if (id2 < id1) // Only upper half of matrix is being used.
			{
				return _intersectionMatrix[id2, id1](collider2, collider1, true);
			}
			return _intersectionMatrix[id1, id2](collider1, collider2, false);
		}

		

		public static IIntersection RectangleRectangle(ICollider a, ICollider b, bool flipNormal)
		{
			var r1 = (RectangleCollider)a;
			var r2 = (RectangleCollider)b;

			var delta = r2.Position - r1.Position;
			
			var overlapX = r1.HalfSize.X + r2.HalfSize.X - Math.Abs(delta.X);

			if (overlapX > 0)
			{
				var overlapY = r1.HalfSize.Y + r2.HalfSize.Y - Math.Abs(delta.Y);

				if (overlapY > 0)
				{
					var collision = new RectangleRectangleIntersection(
						r1, 
						r2, 
						true, 
						delta, 
						new Vector2(overlapX, overlapY)
					);
					
					return collision;
				}

			}

			return new RectangleRectangleIntersection(
				r1,
				r2,
				false,
				Vector2.Zero,
				Vector2.Zero
			); 
		}

		
		public static IIntersection CircleCircle(ICollider a, ICollider b, bool flipNormal)
		{
			var c1 = (CircleCollider)a;
			var c2 = (CircleCollider)b;

			var delta = c2.Position - c1.Position;
			
			var rSumSqr = c1.Radius + c2.Radius;
			rSumSqr *= rSumSqr;

			var lengthSqr = delta.LengthSquared();

			if (lengthSqr > rSumSqr) // No collision.
			{
				return new CircleCircleIntersection(c1, c2, false, Vector2.Zero, 0, 0);
			}

			return new CircleCircleIntersection(c1, c2, true,	delta, lengthSqr, rSumSqr);
		}
		
		
		static float Clamp(float val, float min, float max)
		{
			if (val <= min)
			{
				return min;
			}
			if (val >= max)
			{
				return max;
			}
			return val;
		}

		public static IIntersection RectangleCircle(ICollider a, ICollider b, bool flipNormal)
		{
			var r = (RectangleCollider)a;
			var c = (CircleCollider)b;
			
			var delta = c.Position - r.Position;


			var closestCorner = new Vector2(
				Clamp(delta.X, -r.HalfSize.X, r.HalfSize.X),
				Clamp(delta.Y, -r.HalfSize.Y, r.HalfSize.Y)
			);

			var inside = false;
			
			if (delta == closestCorner)
			{
				inside = true;
				
				if (r.HalfSize.X - Math.Abs(delta.X) < r.HalfSize.Y - Math.Abs(delta.Y))
				{
					if (closestCorner.X > 0)
					{
						closestCorner.X = r.HalfSize.X;
					}
					else
					{
						closestCorner.X = -r.HalfSize.X;
					}
				}
				else
				{
					if (closestCorner.Y > 0)
					{
						closestCorner.Y = r.HalfSize.Y;
					}
					else
					{
						closestCorner.Y = -r.HalfSize.Y;
					}
				}

			}
			var normal = delta - closestCorner;
			var d = normal.LengthSquared();

			// An extreme corner case which most likely will never happen.
			// When d = 0, the circle's center point is right on the rectangle's outline.
			if (d == 0)
			{
				normal = -delta / delta.Length() * c.Radius / 2;
				d = c.Radius * c.Radius;
			}

			Console.WriteLine(inside + "; " + closestCorner);
			if (d > c.Radius * c.Radius && !inside)
			{
				return new RectangleCircleIntersection(r, c, false, Vector2.Zero, Vector2.Zero, false, Vector2.Zero, 0);
			}

			
			if (flipNormal)
			{
				normal *= -1;
			}
			return new RectangleCircleIntersection(r, c, true, delta, closestCorner, inside, normal, d);
		}
		



	}
}
