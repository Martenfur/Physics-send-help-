using Microsoft.Xna.Framework;
using Monofoxe.Engine.Drawing;
using System.Collections.Generic;

namespace PSH.Physics.Collisions.SpatialHashing
{
	public class QuadTreeNode
	{
		/// <summary>
		/// If the node is a leaf, contains more items 
		/// and limit allows and hasn't reached deoth limit, it will split.
		/// </summary>
		const int _itemLimit = 5;
		/// <summary>
		/// Leaves deeper than the limit cannot split anymore.
		/// </summary>
		const int _depthLimit = 3;

		/// <summary>
		/// Depth of the node.
		/// </summary>
		readonly int _depth;

		readonly QuadTreeNode[] _childNodes = new QuadTreeNode[4];

		/// <summary>
		/// Tells if the node is a leaf. Only leaves can contain items.
		/// </summary>
		public bool IsLeaf {get; private set;} = true; 

		/// <summary>
		/// List of all items with non-zero mass. 
		/// </summary>
		List<CPhysics> _items = new List<CPhysics>();

		/// <summary>
		/// List of all items with zero mass.
		/// </summary>
		List<CPhysics> _immovableItems = new List<CPhysics>();


		/// <summary>
		/// Center position of the node.
		/// </summary>
		public Vector2 Position {get; private set;}

		/// <summary>
		/// Size of the node.
		/// </summary>
		public Vector2 Size {get; private set;}


		Vector2[] _rotation = new Vector2[]
		{
			new Vector2(1, -1),
			new Vector2(1, 1),
			new Vector2(-1, 1),
			new Vector2(-1, -1),
		};

		public int ItemsCount => _items.Count;
		public int ImmovableItemsCount => _immovableItems.Count;

		QuadTree _owner;

		public QuadTreeNode(QuadTree owner, Vector2 position, Vector2 size, int depth)
		{
			Position = position;
			Size = size;
			_depth = depth;
			_owner = owner;

			_owner.Leaves.Add(this);
		}

		public void Add(CPhysics item)
		{
			if (IsLeaf)
			{
				if (item.Immovable)
				{
					_immovableItems.Add(item);
				}
				else
				{
					_items.Add(item);
				}

				if ((ItemsCount + ImmovableItemsCount) > _itemLimit && _depth < _depthLimit)
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
			for(var i = 0; i < 4; i += 1)
			{
				_childNodes[i] = null;
			}
			_items.Clear();
			_immovableItems.Clear();
			IsLeaf = true;

			_owner.Leaves.Clear();
			_owner.Leaves.Add(this);
		}


		public CPhysics GetItem(int i) => _items[i];

		public CPhysics GetImmovableItem(int i) => _immovableItems[i];

		void Split()
		{
			IsLeaf = false;
			
			for(var i = 0; i < 4; i += 1)
			{
				_childNodes[i] = new QuadTreeNode(_owner, Position + _rotation[i] * Size / 4f, Size / 2f, _depth + 1);
			}

			for(var i = 0; i < _items.Count; i += 1)
			{
				AddToChildren(_items[i]);
			}
			for (var i = 0; i < _immovableItems.Count; i += 1)
			{
				AddToChildren(_immovableItems[i]);
			}

			_owner.Leaves.Remove(this);
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

		public void GetLeavesInRange(List<QuadTreeNode> list, Vector2 topLeft, Vector2 bottomRight)
		{
			if (IsLeaf)
			{
				list.Add(this);
			}
			else
			{
				var top = (topLeft.Y < Position.Y);
				var bottom = (bottomRight.Y >= Position.Y);
				var left = (topLeft.X < Position.X);
				var right = (bottomRight.X >= Position.X);

				if (top && right)
				{
					_childNodes[0].GetLeavesInRange(list, topLeft, bottomRight);
				}
				if (bottom && right)
				{
					_childNodes[1].GetLeavesInRange(list, topLeft, bottomRight);
				}
				if (bottom && left)
				{
					_childNodes[2].GetLeavesInRange(list, topLeft, bottomRight);
				}
				if (top && left)
				{
					_childNodes[3].GetLeavesInRange(list, topLeft, bottomRight);
				}
			}
		}

		/// <summary>
		/// Draws a quadtree's node.
		/// NOTE: This is a debug-only method.
		/// </summary>
		public void Draw()
		{
			if (IsLeaf)
			{
				RectangleShape.DrawBySize(Position, Size, true);
				Text.Draw((_items.Count + _immovableItems.Count) + "", Position);
			}
			else
			{
				for (var i = 0; i < 4; i += 1)
				{
					_childNodes[i].Draw();
				}
			}
		}

	}
}
