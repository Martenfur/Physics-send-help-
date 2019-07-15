﻿using Microsoft.Xna.Framework;
using Monofoxe.Engine.ECS;
using PSH.Physics.Collisions;

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

		public ICollider Collider;

		public CPhysics()
		{
			Visible = true;
		}
	}
}
