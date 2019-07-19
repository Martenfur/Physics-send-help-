using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monofoxe.Engine.Drawing;

namespace PSH.Physics.Collisions
{
	public class QuadTreeNode : IEnumerable<CPhysics>
	{
		const int _itemLimit = 5;
		const int _depthLimit = 4;

		readonly int _depth;

		QuadTreeNode[] _childNodes;

		public bool IsLeaf {get; private set;} = true; 

		List<CPhysics> _items = new List<CPhysics>();

		public Vector2 Position;
		public Vector2 Size;


		Vector2[] _rotation = new Vector2[]
		{
			new Vector2(1, -1),
			new Vector2(1, 1),
			new Vector2(-1, 1),
			new Vector2(-1, -1),
		};


		public QuadTreeNode(Vector2 position, Vector2 size, int depth)
		{
			Position = position;
			Size = size;
			_depth = depth;
		}

		public void Add(CPhysics item)
		{
			if (IsLeaf)
			{
				_items.Add(item);

				if (_items.Count > _itemLimit && _depth < _depthLimit)
				{
					Split();
				}
			}
			else
			{
				AddToChildren(item);
			}
		}
		
		public void Clear()
		{
			_childNodes = null;
			_items.Clear();
			IsLeaf = true;
		}

		public void Draw()
		{
			RectangleShape.DrawBySize(Position, Size, true);
			if (IsLeaf)
			{
				Text.Draw(_items.Count + "", Position);
			}
			else
			{
				for(var i = 0; i < 4; i += 1)
				{
					_childNodes[i].Draw();
				}
			}
		}

		void Split()
		{
			IsLeaf = false;

			_childNodes = new QuadTreeNode[4];

			for(var i = 0; i < 4; i += 1)
			{
				_childNodes[i] = new QuadTreeNode(Position + _rotation[i] * Size / 4f, Size / 2f, _depth + 1);
			}

			foreach(var item in _items)
			{
				AddToChildren(item);
			}


		}

		void AddToChildren(CPhysics item)
		{
			var minPoint = item.Collider.Position - item.Collider.HalfSize;
			var maxPoint = item.Collider.Position + item.Collider.HalfSize;

			var top = (minPoint.Y < Position.Y);
			var bottom = (maxPoint.Y >= Position.Y);
			var left = (minPoint.X < Position.X);
			var right = (maxPoint.X >= Position.X);

			if (top && right)
			{
				_childNodes[0].Add(item);
			}
			if (bottom && right)
			{
				_childNodes[1].Add(item);
			}
			if (bottom && left)
			{
				_childNodes[2].Add(item);
			}
			if (top && left)
			{
				_childNodes[3].Add(item);
			}
		}

		public void GetLeaves(List<QuadTreeNode> leaves)
		{
			if (IsLeaf)
			{
				leaves.Add(this);
			}
			else
			{
				for(var i = 0; i < 4; i += 1)
				{
					_childNodes[i].GetLeaves(leaves);
				}
			}
		}

		public CPhysics this[int i]
		{
			get => _items[i];
			set => _items[i] = value; 
		}

		public int Count => _items.Count;

		public IEnumerator<CPhysics> GetEnumerator() =>
			_items.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() =>
			 _items.GetEnumerator();
		
	}
}
