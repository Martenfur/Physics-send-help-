using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monofoxe.Engine;
using Monofoxe.Engine.Drawing;
using Monofoxe.Engine.ECS;
using Monofoxe.Engine.SceneSystem;
using Monofoxe.Engine.Cameras;
using Resources.Sprites;
using PSH.Test;
using PSH.Physics.Collisions;
using PSH.Physics;


namespace PSH
{
	public class GameController : Entity
	{
		Camera cam = new Camera(800, 600);

		public GameController() : base(SceneMgr.GetScene("default")["default"])
		{
			GameMgr.MaxGameSpeed = 60;
			GameMgr.MinGameSpeed = 60; // Fixing framerate on 60.
			GameMgr.FixedUpdateRate = 1.0 / 60.0;

			cam.BackgroundColor = new Color(38, 38, 38);

			GameMgr.WindowManager.CanvasSize = new Vector2(800, 600);
			GameMgr.WindowManager.Window.AllowUserResizing = false;
			GameMgr.WindowManager.ApplyChanges();
			GameMgr.WindowManager.CenterWindow();
			GameMgr.WindowManager.CanvasMode = CanvasMode.Fill;
			
			GraphicsMgr.Sampler = SamplerState.PointClamp;
		}
		
		public override void Update()
		{
			if (Input.CheckButtonPress(Buttons.B))
			{
				new Player(Layer, Input.MousePosition);
			}
			if (Input.CheckButtonPress(Buttons.N))
			{
				var e = new Entity(Layer, "wall");

				var size = new Vector2(32, 500);

				if (Input.CheckButton(Buttons.LeftShift))
				{
					size = new Vector2(500, 32);
				}

				var collider = new RectangleCollider(Input.MousePosition, size);

				e.AddComponent(new CPosition(Input.MousePosition));
				e.AddComponent(new CPhysics { Collider = collider, Mass = 0});
			}

		}

		public override void Draw()
		{
			//Default.Monofoxe.Draw(new Vector2(400, 300), Default.Monofoxe.Origin);

			var r = new RectangleCollider(new Vector2(300, 300), new Vector2(100, 100));
			
			var c = new CircleCollider(Input.MousePosition, 100);

			var collision = CollisionSystem.RectangleCircle(r, c);

			/*
			GraphicsMgr.CurrentColor = Color.Orange;

			RectangleShape.DrawBySize(r.Position, r.Size, true);
			CircleShape.Draw(c.Position, c.Radius, true);

			if (collision.Collided)
			{
				GraphicsMgr.CurrentColor = Color.Red;

				LineShape.Draw(c.Position, c.Position + collision.Direction * collision.Depth);

			}
			*/

		}

	}
}