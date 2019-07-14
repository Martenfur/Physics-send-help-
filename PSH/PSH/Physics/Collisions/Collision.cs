using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace PSH.Physics.Collisions
{
	public struct Collision
	{
		public bool Collided;

		public float Depth;
		public Vector2 Direction;

		public CPhysics Obj1;
		public CPhysics Obj2;
		
	}
}
