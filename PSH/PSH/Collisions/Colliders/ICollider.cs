using Microsoft.Xna.Framework;

namespace PSH.Collisions.Colliders
{
	/// <summary>
	/// Colliders describe some shape or a structure which 
	/// can be tested against other colliders for intersection.
	/// </summary>
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
		
		/// <summary>
		/// DIsabled collider will not detect collisions.
		/// </summary>
		bool Enabled { get; set; }

		/// <summary>
		/// Renders the collider.
		/// NOTE: This is a debug-only method!
		/// </summary>
		void Draw(bool isOutline);
	}

	/// <summary>
	/// Type of the collider.
	/// Used for collision matrix.
	/// </summary>
	public enum ColliderType : int
	{
		Rectangle = 0,
		Circle = 1,
	}
}