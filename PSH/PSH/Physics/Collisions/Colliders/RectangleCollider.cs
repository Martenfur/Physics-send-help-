using Microsoft.Xna.Framework;
using Monofoxe.Engine.Drawing;

namespace PSH.Physics.Collisions.Colliders
{
	public struct RectangleCollider : ICollider
	{
		public ColliderType ColliderType => ColliderType.Rectangle;
		public Vector2 Position { get; set; }
		
		public Vector2 HalfSize { get; set; }

		public bool Enabled { get; set; }

		public RectangleCollider(Vector2 position, Vector2 size)
		{
			Position = position;
			HalfSize = size / 2;
			Enabled = true;
		}

		public void Draw(bool isOutline) =>
			RectangleShape.DrawBySize(Position, HalfSize * 2, isOutline);
		
	}
}
