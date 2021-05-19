using Collision;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Collections.Generic;
using System.Linq;

namespace Example
{
	internal static class Program
	{
		private static void Main(string[] _)
		{
			var size = (uint)(VideoMode.DesktopMode.Height * 0.8f);
			var gameWindow = new GameWindow(GameWindowSettings.Default, new NativeWindowSettings { Profile = ContextProfile.Compatability })
			{
				IsVisible = false
			}; 
			// required before RenderWindow construction for OpenGL use
			var window = new RenderWindow(new VideoMode(size, size), "Collision Grid");

			window.Closed += (_1, _2) => window.Close();
			window.SetKeyRepeatEnabled(false);
			window.SetVerticalSyncEnabled(true);

			var scene = new SceneAdapter(8000, 0.007f, 0.004f);
			var parameters = new Parameters();
			var collisionDetection = new CollisionDetection(scene);
			scene.OnChange += (_1, _2) => collisionDetection.Update();

			var style = new LightStyle();

			var ui = new Ui(window, style);
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
			collisionDetection.OnUpdate += (_1, _2) => RecreateUi();

			var view = new View(style);
			window.Resized += (_1, a) => View.Resize((int)a.Width, (int)a.Height);
			//window.Resized += (_1, a) => ui.Resize((int)a.Width, (int)a.Height);

			bool drawUi = true;
			bool drawVisualization = true;
			window.KeyPressed += (_1, a) =>
			{
				switch(a.Code)
				{
					case Keyboard.Key.Escape: window.Close(); break;
					case Keyboard.Key.Tab: drawUi = !drawUi; break;
					case Keyboard.Key.V: drawVisualization = !drawVisualization; break;
				}
			};

			var refCollisionAlgo = new CollisionGrid<ICollider>(-1f, -1f, 2f, 2f, 32, 32);
			IEnumerable<ICollider> highlight = Enumerable.Empty<ICollider>();
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
						var result = refCollisionAlgo.FindAllCollisions(scene);
						collidingSet.SymmetricExceptWith(result);
						highlight = collidingSet.Flatten();
					}
					else
					{
						highlight = collidingSet.Flatten();
					}
				}

				view.Draw(scene.Collider, highlight, parameters.DebugAlgo);

				window.PushGLStates();
				if (drawVisualization) ui.DrawVisualization();
				if (drawUi) ui.DrawUi();
				window.PopGLStates();
				window.Display(); //buffer swap for double buffering and wait for next frame
			}
		}
	}
}
