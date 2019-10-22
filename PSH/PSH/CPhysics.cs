using Microsoft.Xna.Framework;
using Monofoxe.Engine.ECS;
using PSH.Collisions;
using PSH.Collisions.Colliders;
using System.Collections.Generic;

namespace PSH
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
		}
		public readonly float InverseMass;

		/// <summary>
		/// Immovable bodies can't be influenced by other bodies.
		/// 0 mass will make the body immovable.
		/// </summary>
		public bool Immovable => (InverseMass == 0);


		public ICollider Collider;

		

		/// <summary>
		/// Ghost bodies don't collide with other ghost bodies.
		/// </summary>
		public bool Ghost = false;

		/// <summary>
		/// Bounciness of the body.
		/// </summary>
		public float Elasticity = 0f;

		/// <summary>
		/// Bounciness of the body that works in a single direction.
		/// Good for platformers.
		/// </summary>
		public Vector2 DirectionalElasticity = Vector2.Zero;
		// TODO: Probably remove?

		/// <summary>
		/// Tells if there was a collision on this frame.
		/// </summary>
		public bool HadCollision = false;
		
		/// <summary>
		/// A list of collisions for this frame.
		/// </summary>
		public List<Collision> Collisions;

		public CPosition PositionComponent;

		/// <summary>
		/// Mass can only be assigned at the start.
		/// 0 mass will make the body immovable.
		/// </summary>
		public CPhysics(float mass)
		{
			if (mass == 0)
			{
				InverseMass = 0;
			}
			else
			{
				InverseMass = 1f / mass;
			}
			Visible = true;
		}
	}
}
