using Microsoft.Xna.Framework;

namespace PSH.Physics.Collisions.Colliders
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
		
		// TODO: Add offsets.

		/// <summary>
		/// TODO: Implement.
		/// </summary>
		bool Enabled { get; set; }

		void Draw(bool isOutline);
	}

	public enum ColliderType : int
	{
		Rectangle = 0,
		Circle = 1,
	}
}