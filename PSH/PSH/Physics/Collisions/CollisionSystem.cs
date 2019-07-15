using Monofoxe.Engine.Utils;
using Monofoxe.Engine;
using Microsoft.Xna.Framework;
using System;


namespace PSH.Physics.Collisions
{
	public static class CollisionSystem
	{
		public static Collision CheckCollision(ICollider collider1, ICollider collider2)
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

			return new Collision();
		}

		public static Collision RectangleCircle(RectangleCollider rectangle, CircleCollider circle)
		{
			var collision = new Collision();


			var delta = circle.Position - rectangle.Position;

			var closest = new Vector2(
				MathHelper.Clamp(delta.X, -rectangle.Size.X / 2f, rectangle.Size.X / 2f),
				MathHelper.Clamp(delta.Y, -rectangle.Size.Y / 2f, rectangle.Size.Y / 2f)
			);

			var inside = false;
			
			if (delta == closest)
			{
				inside = true;
				
				if (rectangle.Size.X / 2f - Math.Abs(delta.X) < rectangle.Size.Y / 2f - Math.Abs(delta.Y))
				{
					if (closest.X > 0)
					{
						closest.X = rectangle.Size.X / 2f;
					}
					else
					{
						closest.X = -rectangle.Size.X / 2f;
					}
				}
				else
				{
					if (closest.Y > 0)
					{
						closest.Y = rectangle.Size.Y / 2f;
					}
					else
					{
						closest.Y = -rectangle.Size.Y / 2f;
					}
				}

			}

			var normal = delta - closest;
			var d = normal.LengthSquared();

			if (d > circle.Radius * circle.Radius && !inside)
			{
				collision.Collided = false;
				return collision;
			}

			d = (float)Math.Sqrt(d);

			if (inside)
			{
				collision.Direction = -normal / d;
				collision.Depth = d + circle.Radius;
			}
			else
			{
				collision.Direction = normal / d;
				collision.Depth = circle.Radius - d;
			}
			collision.Collided = true;
			
			return collision;
		}


		public static Collision RectangleRectangle(RectangleCollider collider1, RectangleCollider collider2)
		{
			var collision = new Collision();

			var delta = collider2.Position - collider1.Position;

			
			var overlapX = (collider1.Size.X + collider2.Size.X) / 2f - Math.Abs(delta.X);

			if (overlapX > 0)
			{
				var overlapY = (collider1.Size.Y + collider2.Size.Y) / 2f - Math.Abs(delta.Y);

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

			return new Collision();
		}


		public static Collision CircleCircle(CircleCollider collider1, CircleCollider collider2)
		{
			var collision = new Collision();

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
