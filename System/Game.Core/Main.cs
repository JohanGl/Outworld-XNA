using System;
using System.Threading;
using Framework.Core.Contexts;
using Framework.Core.Diagnostics.Logging;
using Framework.Core.Helpers.Input;
using Game.Core.Utils;
using Game.GamePackages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game.Core
{
	/// <summary>
	/// Main class of the application
	/// </summary>
	public class Main : Microsoft.Xna.Framework.Game
	{
		private GameContext context;
		private FpsCounter fpsCounter;
		private GamePackageManager gamePackageManager;

		/// <summary>
		/// Default constructor
		/// </summary>
		public Main()
		{
			// Initialize the default application settings
			Window.Title = "";

			// Set the name of the current thread which our application is running at
			Thread.CurrentThread.Name = "Game.Core";

			// Initialize the logger
			Logger.Outputs.Add(new FileOutputSource("log.txt"));

			// Initialize the game context used throughout the application containing shared resources
			context = new GameContext(this);

			// Graphics (first part)
			context.Graphics.DeviceManager = new GraphicsDeviceManager(this);
			context.Graphics.DeviceManager.PreferredBackBufferWidth = context.View.Area.Width;
			context.Graphics.DeviceManager.PreferredBackBufferHeight = context.View.Area.Height;
			//context.Graphics.DeviceManager.IsFullScreen = true;
			context.Graphics.DeviceManager.ApplyChanges();

			// View
			context.View.SafeArea = context.Graphics.DeviceManager.GraphicsDevice.Viewport.TitleSafeArea;

			// Content
			Content.RootDirectory = "Content";
			context.Resources.Content = Content;

			// Initialize the fps counter
			fpsCounter = new FpsCounter(context);
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			Logger.Log<Main>(LogLevel.Info, "Initialize");

			// Graphics (second part)
			context.Graphics.Device = context.Graphics.DeviceManager.GraphicsDevice;
			context.Graphics.SpriteBatch = new SpriteBatch(context.Graphics.Device);
			context.Graphics.Effect = new BasicEffect(context.Graphics.Device);
			(context.Graphics.Effect as BasicEffect).VertexColorEnabled = true;
			(context.Graphics.Effect as BasicEffect).TextureEnabled = true;
			(context.Graphics.Effect as BasicEffect).LightingEnabled = false;

			// Initialize the game package
			gamePackageManager = new GamePackageManager();
			gamePackageManager.Initialize();
			gamePackageManager.GamePackage.Initialize(context);

			// Scenes
			context.Scenes.Initialize(context);

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load all of your content
		/// </summary>
		protected override void LoadContent()
		{
			Logger.Log<Main>(LogLevel.Info, "LoadContent");
			context.Scenes.LoadContent();
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload all content
		/// </summary>
		protected override void UnloadContent()
		{
			Logger.Log<Main>(LogLevel.Info, "UnloadContent");
			context.Scenes.UnloadContent();
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world, checking for collisions, gathering input, and playing audio
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
			{
				Exit();
			}

			// Update the input states
			context.Input.Update();

			// Update the scenes
			context.Scenes.Update(gameTime);

			// Update the game
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values</param>
		protected override void Draw(GameTime gameTime)
		{
			// Render the scene
			context.Scenes.Render(gameTime);

			// Render the game
			base.Draw(gameTime);

			fpsCounter.Update(gameTime);
		}
	}

#if WINDOWS || XBOX

	static class Program
	{
		#if !XBOX
		[STAThreadAttribute]
		#endif
		static void Main(string[] args)
		{
			Logger.Log<Main>(LogLevel.Info, "Startup");

			using (var game = new Main())
			{
				game.Run();
			}

			Logger.Log<Main>(LogLevel.Info, "Shutdown");
		}
	}

#endif
}