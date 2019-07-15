using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monofoxe.Engine.ECS;
using Monofoxe.Engine;
using Monofoxe.Engine.SceneSystem;
using Monofoxe.Engine.Utils;
using Microsoft.Xna.Framework;
using PSH.Physics.Collisions;
using PSH.Physics;


namespace PSH.Test
{
	public class Player : Entity
	{
		float _speed = 300;

		static RandomExt _r = new RandomExt();

		public Player(Layer layer, Vector2 position) : base(layer)
		{
			ICollider collider;

			if (_r.Next(2) == 0)
			{
				collider = new RectangleCollider(
					position, 
					new Vector2(_r.Next(32, 64), _r.Next(32, 64))
				);
			}
			else
			{
				collider = new CircleCollider(
					position,
					_r.Next(16, 48)
				);
			}

			_speed = 200;//_r.Next(100, 2000);
			
			AddComponent(new CPosition(position));
			AddComponent(new CPhysics{Collider = collider, Mass = 1});
		}

		public override void FixedUpdate()
		{
			
			var physics = GetComponent<CPhysics>();

			if (Input.CheckButton(Buttons.MouseLeft))
			{
				var position = GetComponent<CPosition>();

				var dir = 90;//GameMath.Direction(position.Position, Input.MousePosition);
				// TODO: Change input arg to double.

				physics.Speed = _speed * GameMath.DirectionToVector2((float)dir) * Vector2.UnitY;
			}
			else
			{
				//physics.Speed = Vector2.Zero;
			}

			var ddir = 90;//GameMath.Direction(position.Position, Input.MousePosition);
									 
			physics.Speed += 10 * GameMath.DirectionToVector2((float)ddir) * Vector2.UnitY;
			if (physics.Speed.Y > _speed)
			{
				physics.Speed.Y = _speed;
			}
		}

	}
}
