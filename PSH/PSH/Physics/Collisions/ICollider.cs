using Microsoft.Xna.Framework;
using System;

namespace PSH.Physics.Collisions
{
	public interface ICollider
	{
		ColliderType ColliderType { get; }

		/// <summary>
		/// Center point at current frame.
		/// NOTE: This property has to be set manually 
		/// before checking collision.
		/// </summary>
		Vector2 Position { get; set; }
		
		/// <summary>
		/// AABB size.
		/// </summary>
		Vector2 HalfSize { get; set; }
		
		bool Enabled { get; set; }
	}

	public enum ColliderType : int
	{
		Rectangle = 0,
	}
}