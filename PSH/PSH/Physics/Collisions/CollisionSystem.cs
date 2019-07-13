using Monofoxe.Engine.Utils;

namespace PSH.Physics.Collisions
{
	public static class CollisionSystem
	{
		public static bool CheckCollision(ICollider collider1, ICollider collider2)
		{
			var rectangle1 = (RectangleCollider)collider1;
			var rectangle2 = (RectangleCollider)collider2;

			return GameMath.RectangleInRectangleBySize(
				rectangle1.Position, rectangle1.Size,
				rectangle2.Position, rectangle2.Size
			);
		}
		

	}
}
