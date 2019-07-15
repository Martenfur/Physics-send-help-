using Microsoft.Xna.Framework;
using System;
using Monofoxe.Engine.Drawing;

namespace PSH.Physics.Collisions.Colliders
{
	public struct CircleCollider : ICollider
	{
		public ColliderType ColliderType => ColliderType.Circle;
		public Vector2 Position { get; set; }
		

		public Vector2 HalfSize 
		{ 
			get => Vector2.One * Radius; 
			set => throw new NotImplementedException(); 
		}

		public float Radius;

		public bool Enabled { get; set; }

		public CircleCollider(Vector2 position, float radius)
		{
			Position = position;
			Radius = radius;
			Enabled = true;
		}

		public void Draw(bool isOutline) =>
			CircleShape.Draw(Position, Radius, isOutline);
	}
}
