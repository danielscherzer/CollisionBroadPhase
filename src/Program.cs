using SFML.Graphics;
using SFML.System;
using SFML.Window;

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
			//Property.OnChange(() => parameters.CollisionMethod, _ => collisionDetection.Recreate(), false);
			//Bind.Property(() => parameters.DebugAlgo == parameters.Freeze);

			var ui = new Ui(window);
			void RecreateUi()
			{
				ui.Clear();
				ui.AddPropertyGrid(parameters);
				ui.AddPropertyGrid(scene);
				ui.AddPropertyGrid(collisionDetection);
				switch (collisionDetection.Algorithm)
				{
					case CollisionMultiGrid<ICollider> collisionMethod:
						for (int level = collisionMethod.MaxLevel; level >= collisionMethod.MinLevel; --level)
						{
							ui.AddCountGrid(collisionMethod.GetGridLevel(level));
						}
						break;
					case CollisionGrid<ICollider> collisionMethod:
						ui.AddCountGrid(collisionMethod.GetGrid());
						break;
					default:
						break;
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
