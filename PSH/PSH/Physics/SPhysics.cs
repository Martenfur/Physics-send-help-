using Microsoft.Xna.Framework;
using Monofoxe.Engine;
using Monofoxe.Engine.Drawing;
using Monofoxe.Engine.ECS;
using Monofoxe.Engine.Utils;
using PSH.Physics.Collisions;
using PSH.Physics.Collisions.Colliders;
using PSH.Physics.Collisions.Intersections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;


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
		private int _resolveIterations = 20;


		public static CollisionGrid Grid = new CollisionGrid();

		private int _iterations;

		
		bool _useParallel = true;

		public override void Create(Component component)
		{
			// Caching the position to reduce GetComponent calls.
			var physics = (CPhysics)component;
			var position = physics.Owner.GetComponent<CPosition>();
			physics.PositionComponent = position;
		}

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

			// Getting all intersections and manifolds.
			var quads = new List<QuadTree>();
			foreach (var quad in Grid.Cells) // TODO: Replace.
			{
				if (quad.Count > 0)
				{
					quads.Add(quad);
				}
			}

			
			var cachedCollisions = new List<List<CachedCollision>>();

			for(var i = 0; i < quads.Count; i += 1)
			{
				cachedCollisions.Add(new List<CachedCollision>());
			}
			
			if (_useParallel)
			{
				Parallel.ForEach(
					quads,
					(quad, state, index) => {
						GetCollisions(cachedCollisions[(int)index], quad);
					}
				);
			}
			else
			{
				for(var i = 0; i < quads.Count; i += 1)
				{
					GetCollisions(cachedCollisions[i], quads[i]);
				}
			}
			
			// Getting all intersections and manifolds.

			// Resolving collisions.
			for(var i = 0; i < _resolveIterations; i += 1) 
			{
				// Doing collision resolving multiple times over and over again
				// improves sim results.
				for(var l = 0; l < cachedCollisions.Count; l += 1)
				{
					var list = cachedCollisions[l];
					for(var c = 0; c < list.Count; c += 1)
					{
						ResolveCollision(list[c]);
					}
				}
			}
			// Resolving collisions.

			// Correcting positions.
			for (var l = 0; l < cachedCollisions.Count; l += 1)
			{
				var list = cachedCollisions[l];
				for (var c = 0; c < list.Count; c += 1)
				{
					PositionalCorrection(list[c]);
				}
			}
			// Correcting positions.

			// Updating positions.
			for (var i = 0; i < components.Count; i += 1)
			{
				var physics = (CPhysics)components[i];
				
				physics.Collider.Position += TimeKeeper.GlobalTime(physics.Speed);
				physics.PositionComponent.Position = physics.Collider.Position;
			}
			// Updating positions.

			sw.Stop();

			GameMgr.WindowManager.WindowTitle = "fps: " + GameMgr.Fps
				+ ", iterations: " + _iterations
				+ ", time: " + sw.ElapsedTicks
				+ ", bodies: " + components.Count
				+ ", parallel: " + _useParallel;

		}


		void GetCollisions(List<CachedCollision> collisions, QuadTree quad)
		{
			var leaves = quad.GetLeaves();
			for (var leafId = 0; leafId < leaves.Count; leafId += 1)
			{
				var leaf = leaves[leafId];

				for (var i = 0; i < leaf.ItemsCount; i += 1)
				{
					var a = leaf.GetItem(i);

					for (var k = i + 1; k < leaf.ItemsCount; k += 1)
					{
						var b = leaf.GetItem(k);
						if (a.Ghost && b.Ghost)
						{
							continue;
						}

						CacheCollision(collisions, a, b);
					}

					for (var k = 0; k < leaf.ImmovableItemsCount; k += 1)
					{
						CacheCollision(collisions, a, leaf.GetImmovableItem(k));
					}

				}
			}
		}

		void CacheCollision(List<CachedCollision> collisions, CPhysics a, CPhysics b)
		{
			var intersection = CollisionSystem.CheckCollision(a.Collider, b.Collider);

			if (intersection.Collided)
			{
				// Calculating some collision data right away, since it will be reused multiple times.
				var manifold = intersection.GenerateManifold();
				var collision = new CachedCollision
				{
					A = a,
					B = b,
					Intersection = intersection,
					Manifold = manifold,
					InvMassSum = a.InverseMass + b.InverseMass,
					
					ElasticityDirection = (1 + Math.Min(a.Elasticity, b.Elasticity)) * manifold.Direction 
						// Secret sauce that makes platformer stacking work.
						+ Vector2.Min(a.DirectionalElasticity, b.DirectionalElasticity) * manifold.Direction,
				};


				a.HadCollision = true;
				b.HadCollision = true;

				collisions.Add(collision);
			}
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
			
			var l = (collision.ElasticityDirection * dotProduct) / collision.InvMassSum;

			a.Speed -= l * a.InverseMass;
			b.Speed += l * b.InverseMass;
		}

		/// <summary>
		/// Pushes the bodies out of each other.
		/// Due to float errors just assigning speeds is not enough.
		/// Positional correction eliminates most of the ovelapping.
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

			if (false)//physics.HadCollision)
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

			//physics.Collider.Draw(true);

		}

		public static bool GetCollision(CPhysics owner, ICollider collider)
		{
			// TODO: Replace this with something decent.
			foreach (var quad in Grid.Cells)
			{
				foreach (var leaf in quad.GetLeaves())
				{
					for (var i = 0; i < leaf.ItemsCount; i += 1)
					{
						var physics = leaf.GetItem(i);

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
