using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Example
{
	static class Program
	{
		private static readonly List<IDisposable> disposeList = new List<IDisposable>();

		static void Main(string[] args)
		{
			var size = (uint)(VideoMode.DesktopMode.Height * 0.8f);
			var gameWindow = new OpenTK.GameWindow(); // required before RenderWindow construction for OpenGL use
			var window = new RenderWindow(new VideoMode(size, size), "Collision Grid");

			window.Closed += (_, __) => window.Close();
			window.SetKeyRepeatEnabled(false);
			window.SetVerticalSyncEnabled(true);

			var scene = new SceneAdapter(2000, 0.01f, 0.002f);
			var parameters = new CollisionAdapter(scene);
			var collisionDetection = new CollisionDetection(scene, parameters);
			scene.OnRegeneration += (_, __) => collisionDetection.Recreate(scene, parameters);
			Property.OnChange(() => parameters.CollisionMethod, _ => collisionDetection.Recreate(scene, parameters), false);
			//Bind.Property(() => parameters.DebugAlgo == parameters.Freeze);

			//Ui ui;
			//void RecreateUi()
			//{
			//	ui?.Dispose();
				var ui = new Ui(window, collisionDetection);
				ui.AddPropertyGrid(scene);
				ui.AddPropertyGrid(collisionDetection);
				ui.AddPropertyGrid(parameters);
			//}
			//RecreateUi();
			collisionDetection.OnRegeneration += (_, __) =>
			{
				disposeList.Add(ui);
				ui = new Ui(window, collisionDetection);
				ui.AddPropertyGrid(scene);
				ui.AddPropertyGrid(collisionDetection);
				ui.AddPropertyGrid(parameters);
			};

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

				view.Draw(scene.GameObjects, algoDebug.Errors);

				window.PushGLStates();
				ui.Draw();
				window.PopGLStates();
				foreach(var disposable in disposeList)
				{
					disposable.Dispose();
				}
				disposeList.Clear();
				window.Display(); //buffer swap for double buffering and wait for next frame
			}
		}
	}
}
