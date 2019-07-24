using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace PSH.Physics.Collisions
{
	public class QuadTree
	{
		public Vector2 Position;
		public Vector2 Size;

		QuadTreeNode _root;
		
		public int Count {get; private set;}
		

		public QuadTree(Vector2 position, Vector2 size)
		{
			Position = position;
			Size = size;
			_root = new QuadTreeNode(position, size, 0);
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

		public void Draw()
		{
			_root.Draw();
		}


		public List<QuadTreeNode> GetLeaves()
		{
			var leaves = new List<QuadTreeNode>();
			_root.GetLeaves(leaves);
			return leaves;
		}




	}
}
