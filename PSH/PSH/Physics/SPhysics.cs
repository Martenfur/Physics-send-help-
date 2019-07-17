using Microsoft.Xna.Framework;
using Monofoxe.Engine;
using Monofoxe.Engine.Drawing;
using Monofoxe.Engine.ECS;
using Monofoxe.Engine.Utils;
using PSH.Physics.Collisions;
using PSH.Physics.Collisions.Colliders;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PSH.Physics
{
	public class SPhysics : BaseSystem
	{
		public override Type ComponentType => _componentType;
		private Type _componentType = typeof(CPhysics);

		public override int Priority => 1;
		
		private const float _positionCorrection = 0.4f; // 0.2 - 0.8
		private const float _positionCorrectionSlack = 0.01f; // 0.01 - 0.1

		public static CollisionGrid Grid = new CollisionGrid();

		private int _iterations;

		public override void FixedUpdate(List<Component> components)
		{
			var sw = new Stopwatch();
			_iterations = 0;
			
			sw.Start();

			Grid.Clear();
			for (var i = 0; i < components.Count; i += 1)
			{
				var physics = (CPhysics)components[i];

				Grid.Add(physics);
			}
			for (var i = 0; i < components.Count; i += 1)
			{
				var physics = (CPhysics)components[i];

				physics.HadCollision = false;
			}

			foreach(var list in Grid.Cells)
			{
				for (var i = 0; i < list.Count - 1; i += 1)
				{
					var physics = list[i];
				

					for (var k = i + 1; k < list.Count; k += 1)
					{
						var otherPhysics = list[k];

						ResolveCollision(physics, otherPhysics);
					}
				}
			}

			for (var i = 0; i < components.Count; i += 1)
			{
				var physics = (CPhysics)components[i];

				var position = physics.Owner.GetComponent<CPosition>();

				position.Position += TimeKeeper.GlobalTime(physics.Speed);
				
				physics.Collider.Position = position.Position;
			}

			sw.Stop();

			GameMgr.WindowManager.WindowTitle = "fps: " + GameMgr.Fps 
				+ ", iterations: " + _iterations 
				+ ", time: " + sw.ElapsedTicks 
				+ ", bodies: " + components.Count;
			
		}

		void ResolveCollision(CPhysics obj1, CPhysics obj2)
		{
			var sw = 
			_iterations += 1;
			var collision = CollisionSystem.CheckCollision(obj1.Collider, obj2.Collider);

			var speedDelta = TimeKeeper.GlobalTime(obj1.Speed - obj2.Speed);


			if (!collision.Collided)
			{
				return;
			}
			obj1.HadCollision = true;
			obj2.HadCollision = true;

			var manifold = collision.GenerateManifold();

			var dotProduct = Vector2.Dot(speedDelta, manifold.Direction);

			// Do not push, if shapes are separating.
			if (dotProduct < 0)
			{
				return;
			}

			var invMassSum = obj1.InverseMass + obj2.InverseMass;
			var l = Vector2.Zero;

			if (invMassSum != 0)
			{
				// TODO: Add bounciness.
				l = (1 * manifold.Direction * dotProduct) / invMassSum; 
			}
			
			obj1.Speed -= l / (float)TimeKeeper.GlobalTime() * obj1.InverseMass;
			obj2.Speed += l / (float)TimeKeeper.GlobalTime() * obj2.InverseMass;

			PositionalCorrection(obj1, obj2, manifold);
		}

		/// <summary>
		/// Pushes bodies out of each other.
		/// </summary>
		void PositionalCorrection(CPhysics obj1, CPhysics obj2, Manifold manifold)
		{
			var invMassSum = obj1.InverseMass + obj2.InverseMass;
			if (invMassSum == 0)
			{
				return;
			}

			var correction = Math.Max(manifold.Depth - _positionCorrectionSlack, 0) / invMassSum * _positionCorrection * manifold.Direction;

			var pos1 = obj1.Owner.GetComponent<CPosition>();
			var pos2 = obj2.Owner.GetComponent<CPosition>();

			pos1.Position -= correction * obj1.InverseMass;
			pos2.Position += correction * obj2.InverseMass;
		}

		public override void Draw(Component component)
		{
			var physics = (CPhysics)component;
			var position = physics.Owner.GetComponent<CPosition>();

			if (physics.HadCollision)
			{
				GraphicsMgr.CurrentColor = Color.Red;
			}
			else
			{
				GraphicsMgr.CurrentColor = Color.White;
			}

			physics.Collider.Draw(true);
		}
		
	}
}
