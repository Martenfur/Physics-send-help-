using Microsoft.Xna.Framework;
using Monofoxe.Engine;
using Monofoxe.Engine.Drawing;
using Monofoxe.Engine.ECS;
using Monofoxe.Engine.Utils;
using PSH.Physics.Collisions;
using PSH.Physics.Collisions.Intersections;
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

		private bool _flipflop = false;

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
				physics.HadCollision = false;
			}

			var intersections = new List<IIntersection>();
			foreach(var quad in Grid.Cells)
			{
				foreach (var leaf in quad.GetLeaves())
				{
					for (var i = 0; i < leaf.Count - 1; i += 1)
					{
						var physics = leaf[i];
	
						for (var k = i + 1; k < leaf.Count; k += 1)
						{
							var otherPhysics = leaf[k];
							var intersection = CollisionSystem.CheckCollision(physics.Collider, otherPhysics.Collider);
							intersection.CachedA = physics;
							intersection.CachedB = otherPhysics;

							if (intersection.Collided)
							{
								intersection.GenerateManifold();
								intersections.Add(intersection);
							}
						}
					}
				}
			}

			for (var t = 0; t < 10; t += 1)
			{
				foreach(var i in intersections)
				{
					ResolveCollision(i);
				}
			}

			foreach (var i in intersections)
			{
				PositionalCorrection(i.CachedA, i.CachedB, i);
			}

			for (var i = 0; i < components.Count; i += 1)
			{
				var physics = (CPhysics)components[i];

				var position = physics.Owner.GetComponent<CPosition>();

				physics.Collider.Position += TimeKeeper.GlobalTime(physics.Speed);
				position.Position = physics.Collider.Position;
			}

			sw.Stop();

			GameMgr.WindowManager.WindowTitle = "fps: " + GameMgr.Fps 
				+ ", iterations: " + _iterations 
				+ ", time: " + sw.ElapsedTicks 
				+ ", bodies: " + components.Count;
			
		}

		void ResolveCollision(IIntersection i)
		{
			_iterations += 1;

			var obj1 = i.CachedA;
			var obj2 = i.CachedB;

			var speedDelta = TimeKeeper.GlobalTime(obj1.Speed - obj2.Speed);
			
			obj1.HadCollision = true;
			obj2.HadCollision = true;
			

			var dotProduct = Vector2.Dot(speedDelta, i.Manifold.Direction);

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
				l = (1 * i.Manifold.Direction * dotProduct) / invMassSum; 
				l /= (float)TimeKeeper.GlobalTime();
			}
			
			obj1.Speed -= l * obj1.InverseMass;
			obj2.Speed += l * obj2.InverseMass;
		}

		/// <summary>
		/// Pushes bodies out of each other.
		/// </summary>
		void PositionalCorrection(CPhysics obj1, CPhysics obj2, IIntersection i)
		{
			var invMassSum = obj1.InverseMass + obj2.InverseMass;
			if (invMassSum == 0)
			{
				return;
			}

			var correction = Math.Max(i.Manifold.Depth - _positionCorrectionSlack, 0) / invMassSum * _positionCorrection * i.Manifold.Direction;
			
			obj1.Collider.Position -= correction * obj1.InverseMass;
			obj2.Collider.Position += correction * obj2.InverseMass;
		}

		public override void Draw(Component component)
		{
			var physics = (CPhysics)component;
			var position = physics.Owner.GetComponent<CPosition>();

			if (physics.HadCollision)
			{
				GraphicsMgr.CurrentColor = Color.Red * 0.5f;
			}
			else
			{
				GraphicsMgr.CurrentColor = Color.White * 0.5f;
			}

			physics.Collider.Draw(false);

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
