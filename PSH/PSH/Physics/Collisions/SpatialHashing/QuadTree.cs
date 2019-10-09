using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace PSH.Physics.Collisions.SpatialHashing
{
	public class QuadTree
	{
		/// <summary>
		/// Center of the quadtree.
		/// </summary>
		public Vector2 Position;
		public Vector2 Size;

		QuadTreeNode _root;

		public List<QuadTreeNode> Leaves;


		public int Count {get; private set;}
		
		public QuadTree(Vector2 position, Vector2 size)
		{
			Position = position;
			Size = size;
			Leaves = new List<QuadTreeNode>();
			_root = new QuadTreeNode(this, position, size, 0);
		}

		public void Add(CPhysics item)
		{
			_root.Add(item);
			Count += 1;
		}
		

		public void Clear()
		{
			_root.Clear();
			Count = 0;
		}

		/// <summary>
		/// Draws a quadtree.
		/// NOTE: This is a debug-only method.
		/// </summary>
		public void Draw() =>
			_root.Draw();
		

		public void GetLeavesInRange(List<QuadTreeNode> list, Vector2 topLeft, Vector2 bottomRight) =>
			_root.GetLeavesInRange(list, topLeft, bottomRight);
		
		
		




	}
}
