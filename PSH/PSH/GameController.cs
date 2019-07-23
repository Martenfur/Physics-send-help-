﻿using Microsoft.Xna.Framework;
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
			
			CollisionSystem.Init();

			CircleShape.CircleVerticesCount = 16;


			var s1 = 8f;
			var s2 = 10f;
			var l = 3f;
			var ma = 20f;
			var mb = 15f;
			var dt = 0.2f;

			var ss1 = s1 - l * ma;
			var ss2 = s2 + l * mb;

			var ds1 = dt * (ss1 - ss2);
			var dsp = dt * (s1 - s2);
			var ds2 = dsp - dt * l * (ma + mb);

			System.Console.WriteLine("d1: " + ds1 + " d2: " + ds2);

		}

		public override void Update()
		{
			if (Input.CheckButtonPress(Buttons.B) || Input.CheckButton(Buttons.M))
			{
				var p = new Player(Layer, cam.GetRelativeMousePosition());
				if (Input.CheckButtonPress(Buttons.B))
				{
					var phy = p.GetComponent<CPhysics>();
					phy.Elasticity = 1;
				}
			}
			if (Input.CheckButtonPress(Buttons.N))
			{
				var e = new Entity(Layer, "wall");

				var size = new Vector2(32, 500);

				if (Input.CheckButton(Buttons.LeftShift))
				{
					size = new Vector2(500, 32);
				}

				var collider = new RectangleCollider(cam.GetRelativeMousePosition(), size);

				e.AddComponent(new CPosition(collider.Position));
				e.AddComponent(new CPhysics { Collider = collider, Mass = 0});
			}

		}

		public override void Draw()
		{
			//Default.Monofoxe.Draw(new Vector2(400, 300), Default.Monofoxe.Origin);

			var r = new RectangleCollider(new Vector2(300, 300), new Vector2(100, 100));
			
			var c = new CircleCollider(Input.MousePosition, 100);

			//var collision = CollisionSystem.RectangleCircle(r, c);

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
			GraphicsMgr.CurrentColor = Color.Beige * 0.7f;
			//SPhysics.Grid.Draw();

		}

	}
}