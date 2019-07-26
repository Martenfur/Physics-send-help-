using Microsoft.Xna.Framework;
using Monofoxe.Engine;
using Monofoxe.Engine.ECS;
using Monofoxe.Engine.SceneSystem;
using Monofoxe.Engine.Utils;
using PSH.Physics;
using PSH.Physics.Collisions.Colliders;


namespace PSH.Test
{
	public class Body : Entity
	{
		float _speed = 300;

		float _maxFallSpeed = 3000;


		static RandomExt _r = new RandomExt();

		public Body(Layer layer, Vector2 position) : base(layer)
		{
			ICollider collider;

			if (true)//_r.Next(2) == 0)
			{
				collider = new RectangleCollider(
					position, 
					new Vector2(_r.Next(32, 64), _r.Next(32, 64)) / 2f
				);
			}
			else
			{
				collider = new CircleCollider(
					position,
					_r.Next(16, 48) / 2f
				);
			}
			
			AddComponent(new CPosition(position));
			var phy = new CPhysics(1);
			phy.Collider = collider;
			AddComponent(phy);
		}

		public override void FixedUpdate()
		{
			
			var physics = GetComponent<CPhysics>();

			if (Input.CheckButtonPress(Buttons.MouseLeft))
			{
				var position = GetComponent<CPosition>();

				var dir = GameMath.Direction(position.Position, Input.MousePosition);
				// TODO: Change input arg to double.

				physics.Speed = _speed * GameMath.DirectionToVector2((float)dir);
			}

			var ddir = 90;//GameMath.Direction(position.Position, Input.MousePosition);
			
			
			var collider = new RectangleCollider(
				physics.Collider.Position + Vector2.UnitY * (physics.Collider.HalfSize.Y + 1),
				new Vector2(physics.Collider.HalfSize.X * 2 - 2, 1)
			);

			//if (SPhysics.GetCollision(collider, physics) == null)
			{
				physics.Speed += 10 * GameMath.DirectionToVector2((float)ddir) * Vector2.UnitY;
			}
			
			if (physics.Speed.Y > _maxFallSpeed)
			{
				physics.Speed.Y = _maxFallSpeed;
			}

			if (physics.PositionComponent.Position.Y > 1000)
			{
				DestroyEntity();
			}
		}

	}
}
