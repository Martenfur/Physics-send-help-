using Microsoft.Xna.Framework;
using System;

namespace PSH.Physics.Collisions
{
	public struct CircleCollider : ICollider
	{
		public ColliderType ColliderType => ColliderType.Rectangle;
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
	}
}
