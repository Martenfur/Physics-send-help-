﻿using Microsoft.Xna.Framework;

namespace PSH.Physics.Collisions
{
	public struct RectangleCollider : ICollider
	{
		public ColliderType ColliderType => ColliderType.Rectangle;
		public Vector2 Position { get; set; }
		
		public Vector2 Size { get; set; }

		public bool Enabled { get; set; }

		public RectangleCollider(Vector2 position, Vector2 size)
		{
			Position = position;
			Size = size;
			Enabled = true;
		}
	}
}
