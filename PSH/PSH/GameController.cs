using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monofoxe.Engine;
using Monofoxe.Engine.Cameras;
using Monofoxe.Engine.Drawing;
using Monofoxe.Engine.ECS;
using Monofoxe.Engine.SceneSystem;
using PSH.Physics;
using PSH.Physics.Collisions;
using PSH.Physics.Collisions.Colliders;
using PSH.Test;
using Monofoxe.Engine.Utils;

namespace PSH
{
	public class GameController : Entity
	{
		Camera cam = new Camera(1200, 800);

		public GameController() : base(SceneMgr.GetScene("default")["default"])
		{
			GameMgr.MaxGameSpeed = 60;
			GameMgr.MinGameSpeed = 60; // Fixing framerate on 60.
			GameMgr.FixedUpdateRate = 1.0 / 60.0;

			cam.BackgroundColor = new Color(38, 38, 38);
			cam.Zoom = 1;//0.5f;
			GameMgr.WindowManager.CanvasSize = new Vector2(1200, 800);
			GameMgr.WindowManager.Window.AllowUserResizing = false;
			GameMgr.WindowManager.ApplyChanges();
			GameMgr.WindowManager.CenterWindow();
			GameMgr.WindowManager.CanvasMode = CanvasMode.Fill;
			
			GraphicsMgr.Sampler = SamplerState.PointClamp;
			
			IntersectionSystem.Init();

			CircleShape.CircleVerticesCount = 16;
			

			var v1 = new Vector2(200, 300);
			var v2 = new Vector2(450, -456);


			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			for(var i = 0; i < 1000000; i += 1)
			{
				var v = v1 - v2;
			}
			sw.Stop();



			var sw1 = new System.Diagnostics.Stopwatch();
			sw1.Start();
			for (var i = 0; i < 1000000; i += 1)
			{
				var v = new Vector2(v1.X - v2.X, v1.Y - v2.Y);
			}
			sw1.Stop();

			System.Console.WriteLine(sw.ElapsedTicks + " : " + sw1.ElapsedTicks);

			TimeKeeper.GlobalTimeMultiplier = 0.5f;
		}

		public override void Update()
		{
			if (Input.CheckButtonPress(Buttons.B) || Input.CheckButton(Buttons.M))
			{
				var p = new Player(Layer, cam.GetRelativeMousePosition());
				var phy = p.GetComponent<CPhysics>();
				phy.DirectionalElasticity = Vector2.UnitY;
				if (Input.CheckButtonPress(Buttons.B))
				{
					//phy.Ghost = true;
				}
			}
			if (Input.CheckButtonPress(Buttons.N))
			{
				var e = new Entity(Layer, "wall");

				var size = new Vector2(100, 500);

				if (Input.CheckButton(Buttons.LeftShift))
				{
					size = new Vector2(500, 100);
				}

				var collider = new RectangleCollider(cam.GetRelativeMousePosition(), size);

				e.AddComponent(new CPosition(collider.Position));
				var phy = new CPhysics(0);
				phy.Collider = collider;
				e.AddComponent(phy);
			}

		}

		public override void Draw()
		{
			
			var r = new RectangleCollider(new Vector2(300, 300), new Vector2(220, 220));
			
			var c = new CircleCollider(Input.MousePosition, 100);

			var collision = IntersectionSystem.CheckIntersection(r, c);//.RectangleCircle(r, c);
			var manifold = collision.GenerateManifold();
			
			GraphicsMgr.CurrentColor = Color.Orange;

			RectangleShape.DrawBySize(r.Position, r.HalfSize * 2, true);
			CircleShape.Draw(c.Position, c.Radius, true);

			if (collision.Collided)
			{
				GraphicsMgr.CurrentColor = Color.Red;

				LineShape.Draw(c.Position, c.Position + manifold.Direction * manifold.Depth);
			}
			
			GraphicsMgr.CurrentColor = Color.Beige * 0.3f;
			//SPhysics.Grid.Draw();

		}

	}
}