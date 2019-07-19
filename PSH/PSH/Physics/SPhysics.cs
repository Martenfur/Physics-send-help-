using Microsoft.Xna.Framework;
using Monofoxe.Engine;
using Monofoxe.Engine.Drawing;
using Monofoxe.Engine.ECS;
using Monofoxe.Engine.Utils;
using PSH.Physics.Collisions;
using PSH.Physics.Collisions.Intersections;
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
		
		/// <summary>
		/// Rate of positional correction. Too little, and the objects will sink.
		/// Too much, and they will jitter. Recommended to be 0.2-0.8.
		/// </summary>
		private const float _positionCorrection = 0.4f;
		/// <summary>
		/// Poitional correction slack. Positional correction doesn't apply if bodies
		/// are sunk by this value. Recommended to be 0.01-0.1.
		/// </summary>
		private const float _positionCorrectionSlack = 0.01f;

		/// <summary>
		/// How many times per frame collision resolution will be performed.
		/// Bigger value = more accurate simulation, but also bigger performance hit.
		/// Recommended range: 1-20.
		/// </summary>
		private const int _resolveIterations = 1;


		public static CollisionGrid Grid = new CollisionGrid();

		private int _iterations;

		private bool _flipflop = false;

		public override void FixedUpdate(List<Component> components)
		{
			var sw = new Stopwatch();
			_iterations = 0;
			
			sw.Start();

			// Updating the grid.
			Grid.Clear();
			for (var i = 0; i < components.Count; i += 1)
			{
				var physics = (CPhysics)components[i];

				Grid.Add(physics);
				physics.HadCollision = false;
			}
			// Updating the grid.


			var dt = (float)TimeKeeper.GlobalTime();

			// Getting all intersections and manifolds.
			var cachedCollisions = new List<CachedCollision>();
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

							if ((physics.InverseMass + otherPhysics.InverseMass) == 0)
								continue;
							
							var intersection = CollisionSystem.CheckCollision(physics.Collider, otherPhysics.Collider);
							
							if (intersection.Collided)
							{
								var collision = new CachedCollision
								{
									A = physics,
									B = otherPhysics,
									Intersection = intersection,
									Manifold = intersection.GenerateManifold(),
									InvMassSum = physics.InverseMass + otherPhysics.InverseMass,
								};

								physics.HadCollision = true;
								otherPhysics.HadCollision = true;

								cachedCollisions.Add(collision);
							}
						}
					}
				}
			}
			// Getting all intersections and manifolds.

			// Resolving collisions.
			var cachedCollisionsA = cachedCollisions.ToArray();
			for (var i = 0; i < _resolveIterations; i += 1)
			{
				for(var c = 0; c < cachedCollisionsA.Length; c += 1)
				{
					ResolveCollision(cachedCollisionsA[c]);
				}
			}
			// Resolving collisions.

			// Correcting positions.
			foreach (var collsiison in cachedCollisionsA)
			{
				PositionalCorrection(collsiison);
			}
			// Correcting positions.

			// Updating positions.
			for (var i = 0; i < components.Count; i += 1)
			{
				var physics = (CPhysics)components[i];

				var position = physics.Owner.GetComponent<CPosition>();

				physics.Collider.Position += TimeKeeper.GlobalTime(physics.Speed);
				position.Position = physics.Collider.Position;
			}
			// Updating positions.

			sw.Stop();

			GameMgr.WindowManager.WindowTitle = "fps: " + GameMgr.Fps 
				+ ", iterations: " + _iterations 
				+ ", time: " + sw.ElapsedTicks 
				+ ", bodies: " + components.Count;
			
		}

		void ResolveCollision(CachedCollision collision)
		{
			_iterations += 1;

			var a = collision.A;
			var b = collision.B;
			
			var speedDelta = a.Speed - b.Speed;

			var dotProduct = Vector2.Dot(speedDelta, collision.Manifold.Direction);

			// Do not push, if shapes are separating.
			if (dotProduct < 0)
			{
				return;
			}
			
			// TODO: Add bounciness.
			var l = (1 * collision.Manifold.Direction * dotProduct) / collision.InvMassSum;
			
			a.Speed -= l * a.InverseMass;
			b.Speed += l * b.InverseMass;
		}

		/// <summary>
		/// Pushes bodies out of each other.
		/// </summary>
		void PositionalCorrection(CachedCollision collision)
		{
			var correction = Math.Max(collision.Manifold.Depth - _positionCorrectionSlack, 0) 
				/ collision.InvMassSum * _positionCorrection * collision.Manifold.Direction;
			
			collision.A.Collider.Position -= correction * collision.A.InverseMass;
			collision.B.Collider.Position += correction * collision.B.InverseMass;
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

		public static bool GetCollision(CPhysics owner, ICollider collider)
		{
			foreach (var quad in Grid.Cells)
			{
				foreach (var leaf in quad.GetLeaves())
				{
					for (var i = 0; i < leaf.Count; i += 1)
					{
						var physics = leaf[i];

						if (physics != owner && CollisionSystem.CheckCollision(collider, physics.Collider).Collided)
						{
							Console.WriteLine("Got one!");
							return true;
						}
					}
				}
			}
			return false;
		}


	}
}
