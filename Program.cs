using System;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Example
{
	class Program
	{
		static void Main(string[] args)
		{
			var gameWindow = new OpenTK.GameWindow(); // required before RenderWindow construction for OpenGL use
			var size = (uint)(VideoMode.DesktopMode.Height * 0.8f);
			var window = new RenderWindow(new VideoMode(size, size), "Collision Grid");

			window.Closed += (_, __) => window.Close();
			window.SetKeyRepeatEnabled(false);
			window.SetVerticalSyncEnabled(true);

			var parameters = new Parameters();
			var model = new Model(parameters);

			var ui = new Ui(window, parameters, model);

			var view = new View(model);
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
				model.Update(deltaTime);
				view.Draw();

				window.PushGLStates();
				ui.Draw();
				window.PopGLStates();
				window.Display(); //buffer swap for double buffering and wait for next frame
			}
		}
	}
}
