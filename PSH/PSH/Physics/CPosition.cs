using Microsoft.Xna.Framework;
using Monofoxe.Engine.ECS;

namespace PSH.Physics
{
	public class CPosition : Component
	{
		public Vector2 Position;
		public Vector2 OldPosition;

		public CPosition(Vector2 position)
		{
			Position = position;
			OldPosition = position;
		}
	}
}
