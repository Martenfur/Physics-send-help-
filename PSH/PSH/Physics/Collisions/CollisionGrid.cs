using Microsoft.Xna.Framework;
using Monofoxe.Engine.Drawing;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PSH.Physics.Collisions
{
	public class CollisionGrid
	{
		public QuadTree[,] _cells;

		/// <summary>
		/// Contains all the cells which have something in them.
		/// </summary>
		public List<QuadTree> FilledCells;

		public readonly int Width = 10;
		public readonly int Height = 10;
		
		public readonly int CellSize = 512;

		/// <summary>
		/// Top-left corner of the grid.
		/// </summary>
		public Vector2 Position;
		
		public CollisionGrid()
		{
			_cells = new QuadTree[Width, Height];
			for(var y = 0; y < Height; y += 1)
			{
				for(var x = 0; x < Width; x += 1)
				{
					_cells[x, y] = new QuadTree(Position + new Vector2(x, y) * CellSize + Vector2.One * CellSize / 2, Vector2.One * CellSize);
				}
			}

			FilledCells = new List<QuadTree>();
		}


		public void Add(CPhysics physics)
		{
			var minPoint = ToCellCoordinates(physics.Collider.Position - physics.Collider.HalfSize);
			var maxPoint = ToCellCoordinates(physics.Collider.Position + physics.Collider.HalfSize);
			
			for(var y = minPoint.Y; y <= maxPoint.Y; y += 1)
			{
				for(var x = minPoint.X; x <= maxPoint.X; x += 1)
				{
					if (InBounds(new Point(x, y)))
					{
						_cells[x, y].Add(physics);
						if (!FilledCells.Contains(_cells[x, y]))
						{
							FilledCells.Add(_cells[x, y]);
						}
					}
				}
			}
			
		}
		
		public void Clear()
		{
			for(var y = 0; y < Height; y += 1)
			{
				for(var x = 0; x < Width; x += 1)
				{
					_cells[x, y].Clear();
				}
			}
		}

		/// <summary>
		/// Converts regular position to the grid cell coordinates.
		/// </summary>
		Point ToCellCoordinates(Vector2 position) =>
			((position - Position) / CellSize).ToPoint();

		/// <summary>
		/// Checks if given cell coordinates are in bounds.
		/// </summary>
		bool InBounds(Point point) =>
			(point.X >= 0 && point.Y >= 0 && point.X < Width && point.Y < Height);

		/// <summary>
		/// Checks if given range cell coordinates are in bounds.
		/// </summary>
		bool InBounds(Point minPoint, Point maxPoint) =>
			(minPoint.X >= 0 && minPoint.Y >= 0 && maxPoint.X < Width && maxPoint.Y < Height);


		/// <summary>
		/// Returns a list of non-empty cells in given range or regular coordinates. 
		/// </summary>
		public List<QuadTree> GetFilledCellsInRange(Vector2 topLeft, Vector2 bottomRight)
		{
			var minPoint = ToCellCoordinates(topLeft);
			var maxPoint = ToCellCoordinates(bottomRight);
			
			var cells = new List<QuadTree>();

			for (var y = minPoint.Y; y <= maxPoint.Y; y += 1)
			{
				for (var x = minPoint.X; x <= maxPoint.X; x += 1)
				{
					if (InBounds(new Point(x, y)) && _cells[x, y].Count > 0)
					{
						cells.Add(_cells[x, y]);
					}
				}
			}

			return cells;
		}


		/// <summary>
		/// Renders the grid and its cells.
		/// NOTE: This method should be used only for debug purposes.
		/// </summary>
		public void Draw()
		{
			var size = new Vector2(Width, Height) * CellSize;

			Text.CurrentFont = Resources.Fonts.Arial;
			for(var y = 0; y < Height; y += 1)
			{
				for(var x = 0; x < Width; x += 1)
				{
					var center = Position + Vector2.One * CellSize / 2 + CellSize * new Vector2(x, y);
					_cells[x, y].Draw();
				}
			}
		}

	}
}
