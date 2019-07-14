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

				//var position = physics.Owner.GetComponent<CPosition>();
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

				if (position.Position.Y > 500)
				{
					position.Position.Y = 500;
				}

				physics.Collider.Position = position.Position;
			}







		}


		void ResolveCollision1(CPhysics obj1, CPhysics obj2)
		{
			var collision = CollisionSystem.CheckCollision(obj1.Collider, obj2.Collider);

			if (!collision.Collided)
			{
				return;
			}
			
			var pos1 = obj1.Owner.GetComponent<CPosition>();
			var pos2 = obj2.Owner.GetComponent<CPosition>();
			
			var resVect = collision.Direction * collision.Depth / 2f;

			obj1.Speed -= resVect / (float)TimeKeeper.GlobalTime();
			obj2.Speed += resVect / (float)TimeKeeper.GlobalTime();

		}


		void ResolveCollision(CPhysics obj1, CPhysics obj2)
		{
			var collision = CollisionSystem.CheckCollision(obj1.Collider, obj2.Collider);

			var speedDelta = TimeKeeper.GlobalTime(obj1.Speed - obj2.Speed);


			if (!collision.Collided)
			{
				return;
			}

			var l = 1 * collision.Direction * (Vector2.Dot(speedDelta, collision.Direction)); 
			
			obj1.Speed -= l / (float)TimeKeeper.GlobalTime() / 2f;
			obj2.Speed += l / (float)TimeKeeper.GlobalTime() / 2f;

			PositionalCorrection(obj1, obj2, collision);
		}

		void PositionalCorrection(CPhysics obj1, CPhysics obj2, Collision collision)
		{
			float percent = 0.2f; // usually 20% to 80%

			Vector2 correction = collision.Depth * percent * collision.Direction;

			var pos1 = obj1.Owner.GetComponent<CPosition>();
			var pos2 = obj2.Owner.GetComponent<CPosition>();

			pos1.Position -= correction;
			pos2.Position += correction;
		}

		public override void Draw(Component component)
		{
			var physics = (CPhysics)component;
			var position = physics.Owner.GetComponent<CPosition>();

			GraphicsMgr.CurrentColor = Color.White;

			if (physics.Collider is RectangleCollider)
			{
				RectangleShape.DrawBySize(position.Position.FloorV(), physics.Collider.Size, true);
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
