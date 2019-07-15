using Monofoxe.Engine.Utils;
using Monofoxe.Engine;
using Microsoft.Xna.Framework;
using System;


namespace PSH.Physics.Collisions
{
	public static class CollisionSystem
	{
		public static Manifold CheckCollision(ICollider collider1, ICollider collider2)
		{
			if (collider1 is RectangleCollider && collider2 is RectangleCollider)
			{
				return RectangleRectangle((RectangleCollider)collider1, (RectangleCollider)collider2);
			}
			if (collider1 is CircleCollider && collider2 is CircleCollider)
			{
				return CircleCircle((CircleCollider)collider1, (CircleCollider)collider2);
			}

			if (collider1 is RectangleCollider && collider2 is CircleCollider)
			{
				return RectangleCircle((RectangleCollider)collider1, (CircleCollider)collider2);
			}
			if (collider2 is RectangleCollider && collider1 is CircleCollider)
			{
				var c = RectangleCircle((RectangleCollider)collider2, (CircleCollider)collider1);
				c.Direction *= -1;
				return c;
			}

			return new Manifold();
		}

		public static Manifold RectangleCircle(RectangleCollider rectangle, CircleCollider circle)
		{
			var collision = new Manifold();


			var delta = circle.Position - rectangle.Position;

			var closest = new Vector2(
				MathHelper.Clamp(delta.X, -rectangle.HalfSize.X, rectangle.HalfSize.X),
				MathHelper.Clamp(delta.Y, -rectangle.HalfSize.Y, rectangle.HalfSize.Y)
			);

			var inside = false;
			
			if (delta == closest)
			{
				inside = true;
				
				if (rectangle.HalfSize.X - Math.Abs(delta.X) < rectangle.HalfSize.Y - Math.Abs(delta.Y))
				{
					if (closest.X > 0)
					{
						closest.X = rectangle.HalfSize.X;
					}
					else
					{
						closest.X = -rectangle.HalfSize.X;
					}
				}
				else
				{
					if (closest.Y > 0)
					{
						closest.Y = rectangle.HalfSize.Y;
					}
					else
					{
						closest.Y = -rectangle.HalfSize.Y;
					}
				}

			}

			var normal = delta - closest;
			var d = normal.LengthSquared();
			//
			if (d > circle.Radius * circle.Radius && !inside)
			{
				collision.Collided = false;
				return collision;
			}

			d = (float)Math.Sqrt(d);

			if (d == 0)
			{
				normal = Vector2.UnitX;
			}
			else
			{
				normal /= d;
			}


			if (inside)
			{	
				collision.Direction = -normal;
				
				collision.Depth = d + circle.Radius;
			}
			else
			{
				collision.Direction = normal;
				collision.Depth = circle.Radius - d;
			}
			collision.Collided = true;
			
			return collision;
		}


		public static Manifold RectangleRectangle(RectangleCollider collider1, RectangleCollider collider2)
		{
			var collision = new Manifold();

			var delta = collider2.Position - collider1.Position;
			
			var overlapX = collider1.HalfSize.X + collider2.HalfSize.X - Math.Abs(delta.X);

			if (overlapX > 0)
			{
				var overlapY = collider1.HalfSize.Y + collider2.HalfSize.Y - Math.Abs(delta.Y);

				if (overlapY > 0)
				{
					if (overlapX < overlapY)
					{
						// Point towards B knowing that n points from A to B
						if (delta.X < 0)
						{
							collision.Direction = new Vector2(-1, 0);
						}
						else
						{
							collision.Direction = new Vector2(1, 0);
						}

						collision.Depth = overlapX;
						
					}
					else
					{
						// Point towards B knowing that n points from A to B
						if (delta.Y < 0)
						{
							collision.Direction = new Vector2(0, -1);
						}
						else
						{
							collision.Direction = new Vector2(0, 1);
						}

						collision.Depth = overlapY;
						
					}
					collision.Collided = true;
					return collision;
				}

			}

			return new Manifold();
		}


		public static Manifold CircleCircle(CircleCollider collider1, CircleCollider collider2)
		{
			var collision = new Manifold();

			var delta = collider2.Position - collider1.Position;
			
			var rSum = collider1.Radius + collider2.Radius;
			rSum *= rSum;

			var lengthSqr = delta.LengthSquared();

			if (lengthSqr > rSum)
			{
				// No collision.
				collision.Collided = false;
				return collision;
			}

			collision.Collided = true;

			if (lengthSqr != 0)
			{
				var length = (float)Math.Sqrt(lengthSqr);

				collision.Depth = (float)Math.Sqrt(rSum) - length;

				collision.Direction = delta / length;
			}
			else
			{
				collision.Depth = collider1.Radius;
				collision.Direction = Vector2.UnitX;
			}

			return collision;
		}

		

	}
}
