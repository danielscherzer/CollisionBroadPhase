using System.Linq;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Example
{
	static class Program
	{
		static void Main(string[] args)
		{
			var gameWindow = new OpenTK.GameWindow(); // required before RenderWindow construction for OpenGL use
			var size = (uint)(VideoMode.DesktopMode.Height * 0.8f);
			var window = new RenderWindow(new VideoMode(size, size), "Collision Grid");

			window.Closed += (_, __) => window.Close();
			window.SetKeyRepeatEnabled(false);
			window.SetVerticalSyncEnabled(true);

			var parameters = new CollisionParameters();
			var scene = new SceneAdapter(2000, 0.01f, 0.002f);
			var collisionDetection = new CollisionDetection(scene, parameters);
			scene.OnRegeneration += (_, __) => collisionDetection.Recreate(scene);
			Property.OnChange(() => parameters.CollisionMethod, _ => collisionDetection.Recreate(scene), false);
			//Bind.Property(() => parameters.DebugAlgo == parameters.Freeze);


			var ui = new Ui(window, parameters, collisionDetection);
			ui.AddPropertyGrid(scene);
			ui.AddPropertyGrid(parameters);
			ui.AddPropertyGrid(collisionDetection);

			var view = new View();
			window.Resized += (_, a) => view.Resize((int)a.Width, (int)a.Height);
			window.Resized += (_, a) => ui.Resize((int)a.Width, (int)a.Height);

			window.KeyPressed += (_, a) =>
			{
				if (Keyboard.Key.Escape == a.Code)
				{
					window.Close();
				}
			};

			var clock = new Clock();
			while (window.IsOpen)
			{
				window.DispatchEvents();
				var deltaTime = clock.Restart().AsSeconds();
				scene.Update(deltaTime);
				collisionDetection.Update(deltaTime);
				view.Draw(collisionDetection.GameObjects, collisionDetection.CollisionAlgoDifference.SelectMany((tuple) => new GameObject[] { tuple.Item1, tuple.Item2 }));

				window.PushGLStates();
				ui.Draw();
				window.PopGLStates();
				window.Display(); //buffer swap for double buffering and wait for next frame
			}
		}
	}
}
