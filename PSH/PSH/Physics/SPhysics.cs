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

		public override int Priority => 100;

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

		/// <summary>
		/// This is where all the physics bodies are stored.
		/// </summary>
		public static CollisionGrid Grid = new CollisionGrid();
		
		/// <summary>
		/// Number of collision resolution iterations.
		/// </summary>
		public static int _iterations;

		bool _useParallel = true;

		public static Stopwatch _stopwatch = new Stopwatch();

		/// <summary>
		/// All the cached collisions are stored here.
		/// List of lists is required for multithreading.
		/// </summary>
		List<List<CachedCollision>> _cachedCollisions = new List<List<CachedCollision>>();

		public override void Create(Component component)
		{
			// Caching the position to reduce GetComponent calls.
			var physics = (CPhysics)component;
			var position = physics.Owner.GetComponent<CPosition>();
			physics.PositionComponent = position;
		}

		public override void FixedUpdate(List<Component> components)
		{
			_iterations = 0;
			_stopwatch.Start();

			// Updating the grid.
			Grid.Clear();
			for (var i = 0; i < components.Count; i += 1)
			{
				var physics = (CPhysics)components[i];

				Grid.Add(physics);
				physics.HadCollision = false;
			}
			// Updating the grid.



			// Clearing and resizing the cached collision lists.
			var count = Grid.FilledCells.Count - _cachedCollisions.Count;
			if (count < 0)
			{
				_cachedCollisions.RemoveRange(_cachedCollisions.Count + count, -count);
			}
			for (var i = 0; i < _cachedCollisions.Count; i += 1)
			{	
				_cachedCollisions[i].Clear();
			}
			for (var i = 0; i < count; i += 1)
			{
				_cachedCollisions.Add(new List<CachedCollision>());
			}
			// Clearing and resizing the cached collision lists.



			// Getting all intersections and manifolds.
			if (_useParallel)
			{
				Parallel.ForEach(
					Grid.FilledCells,
					(quad, state, index) => {
						CacheAllCollisions(_cachedCollisions[(int)index], quad);
					}
				);
			}
			else
			{
				for(var i = 0; i < Grid.FilledCells.Count; i += 1)
				{
					CacheAllCollisions(_cachedCollisions[i], Grid.FilledCells[i]);
				}
			}
			// Getting all intersections and manifolds.



			// Resolving collisions.
			for(var i = 0; i < _resolveIterations; i += 1) 
			{
				// Doing collision resolving multiple times over and over again
				// improves sim results.
				for(var l = 0; l < _cachedCollisions.Count; l += 1)
				{
					var list = _cachedCollisions[l];
					for(var c = 0; c < list.Count; c += 1)
					{
						ResolveCollision(list[c]);
					}
				}
			}
			// Resolving collisions.



			// Correcting positions.
			for (var l = 0; l < _cachedCollisions.Count; l += 1)
			{
				var list = _cachedCollisions[l];
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

			_stopwatch.Stop();
		}


		/// <summary>
		/// Caches all collisions to use them later.
		/// </summary>
		void CacheAllCollisions(List<CachedCollision> collisions, QuadTree quad)
		{
			var leaves = quad.Leaves;
			for (var leafId = 0; leafId < leaves.Count; leafId += 1)
			{
				var leaf = leaves[leafId];

				for (var i = 0; i < leaf.ItemsCount; i += 1)
				{
					var a = leaf.GetItem(i);

					for (var k = i + 1; k < leaf.ItemsCount; k += 1)
					{
						var b = leaf.GetItem(k);
						if (a.Ghost && b.Ghost) // Ghost bodies don't collide with other ghost bodies.
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
			var intersection = IntersectionSystem.CheckIntersection(a.Collider, b.Collider);

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
						// Secret sauce that makes platformer stacking work. Should point in the gravity direction.
						+ Vector2.Min(a.DirectionalElasticity, b.DirectionalElasticity) * manifold.Direction,
				};


				a.HadCollision = true;
				b.HadCollision = true;

				collisions.Add(collision);
			}
		}


		/// <summary>
		/// Applies forces on bodies to push them apart according to the manifold.
		/// </summary>
		void ResolveCollision(CachedCollision collision)
		{
			// Looks a bit weird, but apparently, it works faster without vector operations.
			// Yes, it's in a realm of microoptimizations, but this method runs A LOT of times per frame. 
			_iterations += 1;

			var a = collision.A;
			var b = collision.B;
			
			var dotProduct = (a.Speed.X - b.Speed.X) * collision.Manifold.Direction.X 
				+ (a.Speed.Y - b.Speed.Y) * collision.Manifold.Direction.Y;
			

			// Do not push, if shapes are separating.
			if (dotProduct < 0)
			{
				return;
			}
			
			var dot = dotProduct / collision.InvMassSum;

			var lx = collision.ElasticityDirection.X * dot;
			var ly = collision.ElasticityDirection.Y * dot;
			
			a.Speed = new Vector2(a.Speed.X - lx * a.InverseMass, a.Speed.Y - ly * a.InverseMass);
			b.Speed = new Vector2(b.Speed.X + lx * b.InverseMass, b.Speed.Y + ly * b.InverseMass);
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
			var c = new Color(88, 91, 130);

			GraphicsMgr.CurrentColor = c * 0.5f;
			physics.Collider.Draw(false);

			GraphicsMgr.CurrentColor = c;
			physics.Collider.Draw(true);
		}




		#region User methods.

		/// <summary>
		/// Returns the first physics body which intersects with given physics body.
		/// </summary>
		public static CPhysics GetCollision(CPhysics owner) =>
			GetCollision(owner.Collider, owner);


		/// <summary>
		/// Returns the first physics body which intersects with given 
		/// collider and is not the specified owner.
		/// </summary>
		public static CPhysics GetCollision(ICollider collider, CPhysics owner = null)
		{
			var topLeft = collider.Position - collider.HalfSize;
			var bottomRight = collider.Position + collider.HalfSize;

			var cells = Grid.GetFilledCellsInRange(topLeft, bottomRight);
			var leaves = new List<QuadTreeNode>();

			for(var i = 0; i < cells.Count; i += 1)
			{
				cells[i].GetLeavesInRange(leaves, topLeft, bottomRight);
			}

			for(var i = 0; i < leaves.Count; i += 1)
			{
				for (var k = 0; k < leaves[i].ItemsCount; k += 1)
				{
					var physics = leaves[i].GetItem(k);

					if (physics != owner && IntersectionSystem.CheckIntersection(collider, physics.Collider).Collided)
					{
						return physics;
					}
				}

				for (var k = 0; k < leaves[i].ImmovableItemsCount; k += 1)
				{
					var physics = leaves[i].GetImmovableItem(k);

					if (physics != owner && IntersectionSystem.CheckIntersection(collider, physics.Collider).Collided)
					{
						return physics;
					}
				}
			}

			return null;
		}


		/// <summary>
		/// Returns a list of all physics bodies which intersect with given physics body.
		/// </summary>
		public static List<CPhysics> GetAllCollisions(CPhysics owner) =>
			GetAllCollisions(owner.Collider, owner);


		/// <summary>
		/// Returns a list of all physics bodies which intersect with given 
		/// collider and are not the specified owner.
		/// </summary>
		public static List<CPhysics> GetAllCollisions(ICollider collider, CPhysics owner = null)
		{
			var topLeft = collider.Position - collider.HalfSize;
			var bottomRight = collider.Position + collider.HalfSize;

			var cells = Grid.GetFilledCellsInRange(topLeft, bottomRight);
			var leaves = new List<QuadTreeNode>();

			for (var i = 0; i < cells.Count; i += 1)
			{
				cells[i].GetLeavesInRange(leaves, topLeft, bottomRight);
			}

			var collisions = new List<CPhysics>();

			for (var i = 0; i < leaves.Count; i += 1)
			{
				for (var k = 0; k < leaves[i].ItemsCount; k += 1)
				{
					var physics = leaves[i].GetItem(k);

					if (physics != owner && IntersectionSystem.CheckIntersection(collider, physics.Collider).Collided)
					{
						collisions.Add(physics);
					}
				}

				for (var k = 0; k < leaves[i].ImmovableItemsCount; k += 1)
				{
					var physics = leaves[i].GetImmovableItem(k);

					if (physics != owner && IntersectionSystem.CheckIntersection(collider, physics.Collider).Collided)
					{
						collisions.Add(physics);
					}
				}
			}

			return collisions;
		}

		#endregion User methods.


	}
}
