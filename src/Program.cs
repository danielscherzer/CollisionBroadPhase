﻿using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Linq;

namespace Example
{
	static class Program
	{
		static void Main(string[] args)
		{
			var size = (uint)(VideoMode.DesktopMode.Height * 0.8f);
			var gameWindow = new OpenTK.GameWindow(); // required before RenderWindow construction for OpenGL use
			var window = new RenderWindow(new VideoMode(size, size), "Collision Grid");

			window.Closed += (_, __) => window.Close();
			window.SetKeyRepeatEnabled(false);
			window.SetVerticalSyncEnabled(true);

			var scene = new SceneAdapter(2000, 0.01f, 0.002f);
			var parameters = new Parameters();
			var collisionDetection = new CollisionDetection(scene);
			scene.OnChange += (_, __) => collisionDetection.Update();

			var ui = new Ui(window);
			void RecreateUi()
			{
				ui.Clear();
				ui.AddPropertyGrid(parameters);
				ui.AddPropertyGrid(scene);
				ui.AddPropertyGrid(collisionDetection);
				if (collisionDetection.Algorithm is ICollisionGrid<ICollider> collGrid)
				{
					foreach (var grid in collGrid.Grids.Reverse())
					{
						ui.AddCountGrid(grid);
					}
				}
			}
			RecreateUi();
			collisionDetection.OnUpdate += (_, __) => RecreateUi();

			var view = new View();
			window.Resized += (_, a) => view.Resize((int)a.Width, (int)a.Height);
			//window.Resized += (_, a) => ui.Resize((int)a.Width, (int)a.Height);

			window.KeyPressed += (_, a) =>
			{
				if (Keyboard.Key.Escape == a.Code)
				{
					window.Close();
				}
			};

			CollisionAlgoDebug algoDebug = new CollisionAlgoDebug();

			var clock = new Clock();
			while (window.IsOpen)
			{
				window.DispatchEvents();
				var deltaTime = clock.Restart().AsSeconds();
				scene.Update(deltaTime);

				if (parameters.CollisionDetection)
				{
					var collidingSet = collisionDetection.FindCollisions();
					if (parameters.DebugAlgo)
					{
						algoDebug.Check(scene, collidingSet);
					}
				}

				view.Draw(scene.Collider, algoDebug.Errors);

				window.PushGLStates();
				ui.Draw();
				window.PopGLStates();
				window.Display(); //buffer swap for double buffering and wait for next frame
			}
		}
	}
}
