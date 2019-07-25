using Microsoft.Xna.Framework;
using Monofoxe.Engine.ECS;
using PSH.Physics.Collisions.Colliders;

namespace PSH.Physics
{
	public class CPhysics : Component
	{
		public Vector2 Speed;
		public float Mass 
		{
			get
			{
				if (InverseMass == 0)
				{
					return 0;
				}
				return 1f / InverseMass;
			}
			set 
			{
				if (value == 0)
				{
					InverseMass = 0;
				}
				else
				{
					InverseMass = 1f / value;
				}
			}
		}
		public float InverseMass {get; private set;}

		/// <summary>
		/// Immovable bodies can't be influenced by other bodies.
		/// </summary>
		public bool Immovable => (InverseMass == 0);


		public ICollider Collider;


		public bool HadCollision = false;

		/// <summary>
		/// Ghost bodies don't collide with other ghost bodies.
		/// </summary>
		public bool Ghost = false;

		/// <summary>
		/// Bounciness of the body.
		/// </summary>
		public float Elasticity = 0f;

		public Vector2 DirectionalElasticity = Vector2.Zero;


		public CPosition PositionComponent;
		
		public CPhysics()
		{
			Visible = true;
		}
	}
}
