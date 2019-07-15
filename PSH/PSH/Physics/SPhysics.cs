using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monofoxe.Engine.ECS;
using Monofoxe.Engine;
using Monofoxe.Engine.Drawing;
using Monofoxe.Engine.Utils;
using Microsoft.Xna.Framework;
using PSH.Physics.Collisions;


namespace PSH.Physics
{
	public class SPhysics : BaseSystem
	{
		public override Type ComponentType => _componentType;
		private Type _componentType = typeof(CPhysics);

		public override int Priority => 1;
		

		public override void FixedUpdate(List<Component> components)
		{

			
			for (var i = 0; i < components.Count; i += 1)
			{
				var physics = (CPhysics)components[i];

				if (physics.InverseMass == 0)
				{
					continue;
				}

				for (var k = 0; k < components.Count; k += 1)
				{
					if (i == k)
					{
						continue;
					}
					var otherPhysics = (CPhysics)components[k];

					ResolveCollision(physics, otherPhysics);
				}
			}

			for (var i = 0; i < components.Count; i += 1)
			{
				var physics = (CPhysics)components[i];

				var position = physics.Owner.GetComponent<CPosition>();

				position.Position += TimeKeeper.GlobalTime(physics.Speed);
				

				physics.Collider.Position = position.Position;
			}







		}

		void ResolveCollision(CPhysics obj1, CPhysics obj2)
		{
			var collision = CollisionSystem.CheckCollision(obj1.Collider, obj2.Collider);

			var speedDelta = TimeKeeper.GlobalTime(obj1.Speed - obj2.Speed);


			if (!collision.Collided)
			{
				return;
			}

			var dotProduct = Vector2.Dot(speedDelta, collision.Direction);

			// Do not push, if shapes are separating.
			if (dotProduct < 0)
			{
				return;
			}

			var l = (1 * collision.Direction * dotProduct) / (obj1.InverseMass + obj2.InverseMass); 
			
			obj1.Speed -= l / (float)TimeKeeper.GlobalTime() * obj1.InverseMass;
			obj2.Speed += l / (float)TimeKeeper.GlobalTime() * obj2.InverseMass;

			PositionalCorrection(obj1, obj2, collision);
		}

		void PositionalCorrection(CPhysics obj1, CPhysics obj2, Collision collision)
		{
			var percent = 0.4f; // 0.2 - 0.8
			var slack = 0.01f; // 0.01 - 0.1

			Vector2 correction = Math.Max(collision.Depth - slack, 0) / (obj1.InverseMass + obj2.InverseMass) * percent * collision.Direction;

			var pos1 = obj1.Owner.GetComponent<CPosition>();
			var pos2 = obj2.Owner.GetComponent<CPosition>();

			pos1.Position -= correction * obj1.InverseMass;
			pos2.Position += correction * obj2.InverseMass;
		}

		public override void Draw(Component component)
		{
			var physics = (CPhysics)component;
			var position = physics.Owner.GetComponent<CPosition>();

			GraphicsMgr.CurrentColor = Color.White;

			if (physics.Collider is RectangleCollider)
			{
				RectangleShape.DrawBySize(position.Position.FloorV(), physics.Collider.Size, false);
			}
			else
			{
				CircleShape.Draw(position.Position.FloorV(), physics.Collider.Size.X / 2, true);
			}
		}
		/*
		public static CPhysics GetFirstCollision(CPhysics solid, List<Component> components)
		{
			foreach(CPhysics otherSolid in components)
			{
				if (solid != otherSolid && CollisionSystem.CheckCollision(solid.Collider, otherSolid.Collider))
				{
					return otherSolid;
				}
			}
			return null;
		}*/
		
	}
}
