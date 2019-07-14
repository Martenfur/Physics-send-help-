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
		float _speed = 200;

		static RandomExt _r = new RandomExt();

		public Player(Layer layer, Vector2 position) : base(layer)
		{
			
			var collider = new RectangleCollider(
				position, 
				new Vector2(_r.Next(32, 64), _r.Next(32, 64))
			);
			
			/*
			var collider = new CircleCollider(
				position,
				_r.Next(16, 48)
			);*/
			_speed = _r.Next(100, 200);
			
			AddComponent(new CPosition(position));
			AddComponent(new CPhysics{Collider = collider});
		}

		public override void FixedUpdate()
		{
			
			var physics = GetComponent<CPhysics>();

			if (Input.CheckButton(Buttons.MouseLeft))
			{
				var position = GetComponent<CPosition>();

				var dir = GameMath.Direction(position.Position, Input.MousePosition);
				// TODO: Change input arg to double.
				physics.Speed = _speed * GameMath.DirectionToVector2((float)dir);
			}
			else
			{
				physics.Speed = Vector2.Zero;
			}

		}

	}
}
