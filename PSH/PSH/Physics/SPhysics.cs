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
			for(var i = 0; i < components.Count; i += 1)
			{
				var physics = (CPhysics)components[i];

				var position = physics.Owner.GetComponent<CPosition>();

				position.Position += TimeKeeper.GlobalTime(physics.Speed);

				physics.Collider.Position = position.Position;
			}

			for (var i = 0; i < components.Count; i += 1)
			{
				var physics = (CPhysics)components[i];

				var position = physics.Owner.GetComponent<CPosition>();

				if (GetFirstCollision(physics, components) != null)
				{
					var delta = position.Position - position.OldPosition;

					var xDelta = new Vector2(delta.X, 0);
					var yDelta = new Vector2(0, delta.Y);


					if (MoveOutside(yDelta, physics, components))
					{
						position.Position = physics.Collider.Position;
					}
					else
					{
						if (MoveOutside(xDelta, physics, components))
						{
							position.Position = physics.Collider.Position;
						}
						else
						{
							MoveOutside(delta, physics, components, false);
							
							position.Position = physics.Collider.Position;
							
						}
					}


				}

			}
		}

		bool MoveOutside(Vector2 delta, CPhysics physics, List<Component> components, bool resetPosition = true)
		{
			var e = -delta.GetSafeNormalize();

			var oldPosition = physics.Collider.Position;

			for (var i = 0; i < delta.Length(); i += 1)
			{
				physics.Collider.Position += e;

				if (GetFirstCollision(physics, components) == null)
				{
					return true;
				}
			}
			if (resetPosition)
			{
				physics.Collider.Position = oldPosition;
			}

			return false;
		}

		bool MoveOutsideSpecial(Vector2 delta, CPhysics physics, List<Component> components, bool resetPosition = true)
		{
			var e = -delta.GetSafeNormalize();

			var oldPosition = physics.Collider.Position;

			var collider = GetFirstCollision(physics, components);

			for (var i = 0; i < Math.Max(delta.Length(), physics.Collider.Size.Length()); i += 1)
			{
				physics.Collider.Position += e;

				if (CollisionSystem.CheckCollision(physics.Collider, collider.Collider))
				{
					return true;
				}
			}
			if (resetPosition)
			{
				physics.Collider.Position = oldPosition;
			}

			return false;
		}


		public override void Draw(Component component)
		{
			var physics = (CPhysics)component;
			var position = physics.Owner.GetComponent<CPosition>();

			GraphicsMgr.CurrentColor = Color.White;

			RectangleShape.DrawBySize(position.OldPosition.FloorV(), physics.Collider.Size / 2, true);
			RectangleShape.DrawBySize(position.Position.FloorV(), physics.Collider.Size, true);
		}

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
		}
		
	}
}
