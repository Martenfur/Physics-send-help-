using Microsoft.Xna.Framework;
using PSH.Collisions.Colliders;
using PSH.Collisions.Intersections;
using System;


namespace PSH.Collisions
{
	public delegate IIntersection IntersectionDelegate(ICollider a, ICollider b, bool flipNormal);

	/// <summary>
	/// Contains methods for checking if various colliders intersect.
	/// </summary>
	public static class IntersectionSystem
	{
		/// <summary>
		/// Contains all allowed collider combinations.
		/// </summary>
		private static IntersectionDelegate[,] _intersectionMatrix;

		private const int _intersectionMatrixSize = 2;

		private static BatchReturnPool<CircleCircleIntersection> _circleCircleIntersectionPool;
		private static BatchReturnPool<RectangleCircleIntersection> _rectangleCircleIntersectionPool;
		private static BatchReturnPool<RectangleRectangleIntersection> _rectangleRectangleIntersectionPool;


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



			_circleCircleIntersectionPool = new BatchReturnPool<CircleCircleIntersection>(1024);
			_rectangleCircleIntersectionPool = new BatchReturnPool<RectangleCircleIntersection>(1024);
			_rectangleRectangleIntersectionPool = new BatchReturnPool<RectangleRectangleIntersection>(1024);
		}

		public static void Update()
		{
			_circleCircleIntersectionPool.ReturnAll();
			_rectangleCircleIntersectionPool.ReturnAll();
			_rectangleRectangleIntersectionPool.ReturnAll();
		}


		/// <summary>
		/// Checks if two colliders intersect.
		/// </summary>
		public static IIntersection CheckIntersection(ICollider collider1, ICollider collider2)
		{
			if (!collider1.Enabled || !collider2.Enabled)
			{
				return new NoIntersection();
			}

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
				// Colliders of two different types need their normal to be flipped for 
				// collision resolving to work correctly.
				return _intersectionMatrix[id2, id1](collider2, collider1, true);
			}
			return _intersectionMatrix[id1, id2](collider1, collider2, false);
		}



		static IIntersection RectangleRectangle(ICollider a, ICollider b, bool flipNormal)
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
					var collision = _rectangleRectangleIntersectionPool[_rectangleRectangleIntersectionPool.Take()];
					collision.Setup(
						r1,
						r2,
						true,
						delta,
						new Vector2(overlapX, overlapY)
					);

					return collision;
				}

			}

			var collision1 = _rectangleRectangleIntersectionPool[_rectangleRectangleIntersectionPool.Take()];
			collision1.Setup(
				r1,
				r2,
				false,
				Vector2.Zero,
				Vector2.Zero
			);
			return collision1;
		}


		static IIntersection CircleCircle(ICollider a, ICollider b, bool flipNormal)
		{
			var c1 = (CircleCollider)a;
			var c2 = (CircleCollider)b;

			var delta = c2.Position - c1.Position;

			var rSumSqr = c1.Radius + c2.Radius;
			rSumSqr *= rSumSqr;

			var lengthSqr = delta.LengthSquared();

			if (lengthSqr > rSumSqr) // No collision.
			{
				var collision = _circleCircleIntersectionPool[_circleCircleIntersectionPool.Take()];
				collision.Setup(c1, c2, false, Vector2.Zero, 0, 0);
				return collision;
			}
;
			var collision1 = _circleCircleIntersectionPool[_circleCircleIntersectionPool.Take()];
			collision1.Setup(c1, c2, true, delta, lengthSqr, rSumSqr);
			return collision1;
		}


		static IIntersection RectangleCircle(ICollider a, ICollider b, bool flipNormal)
		{
			var r = (RectangleCollider)a;
			var c = (CircleCollider)b;

			var delta = c.Position - r.Position;


			var closestCorner = new Vector2(
				MathHelper.Clamp(delta.X, -r.HalfSize.X, r.HalfSize.X),
				MathHelper.Clamp(delta.Y, -r.HalfSize.Y, r.HalfSize.Y)
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

			if (d > c.Radius * c.Radius && !inside)
			{
				var collision = _rectangleCircleIntersectionPool[_rectangleCircleIntersectionPool.Take()];
				collision.Setup(r, c, false, Vector2.Zero, Vector2.Zero, false, Vector2.Zero, 0);
				return collision;
			}


			if (flipNormal)
			{
				normal *= -1;
			}

			var collision1 = _rectangleCircleIntersectionPool[_rectangleCircleIntersectionPool.Take()];
			collision1.Setup(r, c, true, delta, closestCorner, inside, normal, d);
			return collision1;
		}




	}
}
