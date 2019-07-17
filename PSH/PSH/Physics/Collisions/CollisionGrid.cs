using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monofoxe.Engine.Drawing;
using Monofoxe.Engine;

namespace PSH.Physics.Collisions
{
	public class CollisionGrid
	{
		public List<CPhysics>[,] Cells;
		
		public readonly int Width = 10;
		public readonly int Height = 10;
		
		public readonly int CellSize = 32;

		Vector2 Position;
		
		public CollisionGrid()
		{
			Cells = new List<CPhysics>[Width, Height];
			for(var y = 0; y < Height; y += 1)
			{
				for(var x = 0; x < Width; x += 1)
				{
					Cells[x, y] = new List<CPhysics>();
				}
			}
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
						Cells[x, y].Add(physics);
					}
				}
			}
			
		}

		Point ToCellCoordinates(Vector2 position) =>
			((position - Position) / CellSize).ToPoint();
			
		bool InBounds(Point point) =>
			(point.X >= 0 && point.Y >= 0 && point.X < Width && point.Y < Height);

		bool InBounds(Point minPoint, Point maxPoint) =>
			(minPoint.X >= 0 && minPoint.Y >= 0 && maxPoint.X < Width && maxPoint.Y < Height);


		public bool Remove(CPhysics physics)
		{
			var delta = physics.Collider.Position - Position;
			var x = (int)(delta.X % CellSize);
			var y = (int)(delta.Y % CellSize);

			if (x >= 0 && y >= 0 && x < Width && y < Height)
			{
				return Cells[x, y].Remove(physics);
			}
			return false;
		}

		public void Clear()
		{
			for(var y = 0; y < Height; y += 1)
			{
				for(var x = 0; x < Width; x += 1)
				{
					Cells[x, y].Clear();
				}	
			}
		}


		public void Draw()
		{
			var size = new Vector2(Width, Height) * CellSize;

			Text.CurrentFont = Resources.Fonts.Arial;
			for(var y = 0; y < Height; y += 1)
			{
				for(var x = 0; x < Width; x += 1)
				{
					var center = Position + Vector2.One * CellSize / 2 + CellSize * new Vector2(x, y);
					RectangleShape.DrawBySize(center, Vector2.One * CellSize, true);
					Text.Draw(Cells[x, y].Count + "", center);
				}
			}
		}

	}
}
