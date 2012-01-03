using System;
using Framework.Core.Contexts;
using Framework.Core.Scenes;
using Framework.Core.Services;
using Framework.Gui;
using Game.Network.Clients;
using Game.Network.Clients.Settings;
using Game.Network.Servers;
using Game.Network.Servers.Settings;
using Game.World;
using Microsoft.Xna.Framework;
using Outworld.Scenes.InGame;
using Outworld.Settings.Global;

namespace Outworld.Scenes.MainMenu.ChildScenes
{
	public class NewGameScene : SceneBase
	{
		private GlobalSettings globalSettings;
		private IGameServer gameServer;
		private IGameClient gameClient;
		private bool startedLoading;

		private StackPanel menuOptions;

		private enum ButtonCommand
		{
			HostGame,
			JoinGame,
			Return
		}

		public override void Initialize(GameContext context)
		{
			base.Initialize(context);

			globalSettings = ServiceLocator.Get<GlobalSettings>();
			gameServer = ServiceLocator.Get<IGameServer>();
			gameClient = ServiceLocator.Get<IGameClient>();
		}

		public override void LoadContent()
		{
		}

		public override void UnloadContent()
		{
		}

		public void InitializeGui(GuiManager gui)
		{
			menuOptions = new StackPanel();
			menuOptions.HorizontalAlignment = HorizontalAlignment.Left;
			menuOptions.VerticalAlignment = VerticalAlignment.Center;
			menuOptions.Margin = new Thickness(100, 40, 0, 0);
			menuOptions.Width = 176;
			menuOptions.Height = 400;
			menuOptions.Spacing.Bottom = 32;
			gui.Elements.Add(menuOptions);

			for (int i = 0; i < 3; i++)
			{
				menuOptions.Children.Add(GetMenuOption(i));
			}

			gui.UpdateLayout();
		}

		private ImageButton GetMenuOption(int row)
		{
			var optionWidth = 175;
			var optionHeight = 29;
			int offsetY = 0;
			ButtonCommand buttonCommand;

			switch (row)
			{
				case 1:
					buttonCommand = ButtonCommand.JoinGame;
					offsetY = 58;
					break;

				case 2:
					buttonCommand = ButtonCommand.Return;
					offsetY = 115;
					break;

				default:
					buttonCommand = ButtonCommand.HostGame;
					break;
			}

			var buttonStates = new ButtonStates();
			buttonStates.Default = new Rectangle(0, offsetY, optionWidth, optionHeight);
			buttonStates.Highlighted = new Rectangle(176, offsetY, optionWidth, optionHeight);
			buttonStates.Focused = new Rectangle(352, offsetY, optionWidth, optionHeight);
			buttonStates.Pressed = buttonStates.Default;

			var result = new ImageButton(Context.Resources.Textures["MainMenu.NewGameOptions"], buttonStates);
			result.HorizontalAlignment = HorizontalAlignment.Left;
			result.VerticalAlignment = VerticalAlignment.Top;
			result.Click += MenuOptionOnClick;
			result.Tag = buttonCommand;

			return result;
		}

		private void MenuOptionOnClick(object sender, EventArgs eventArgs)
		{
			var button = (ImageButton)sender;

			switch ((ButtonCommand)button.Tag)
			{
				case ButtonCommand.HostGame:
					CreateHost();
					break;

				case ButtonCommand.JoinGame:
					((MainMenuScene)Parent).ShowScene(SceneType.JoinGame);
					break;

				case ButtonCommand.Return:
					((MainMenuScene)Parent).ShowScene(SceneType.Main);
					break;
			}
		}

		public override void Update(GameTime gameTime)
		{
			if (menuOptions.Visibility != Visibility.Visible)
			{
				return;
			}

			if (gameServer.IsStarted)
			{
				gameServer.Update(gameTime);
			}

			gameClient.Update(gameTime);

			if (gameClient.IsConnected)
			{
				if (!startedLoading)
				{
					startedLoading = true;
					GetGameSettings();
				}
			}
		}

		public override void Render(GameTime gameTime)
		{
		}

		private void CreateHost()
		{
			var settings = new GameServerSettings
			{
				MaximumConnections = 4,
				Port = globalSettings.Network.ServerPort,
			};

			settings.World.Gravity = globalSettings.World.Gravity;
			settings.World.Seed = globalSettings.World.Seed;

			gameServer.Initialize(settings);
			gameServer.Start();

			JoinHost(globalSettings.Network.ServerAddress);
		}

		private void JoinHost(string ip)
		{
			var settings = new GameClientSettings
			{
				ServerAddress = ip,
				ServerPort = globalSettings.Network.ClientPort
			};

			gameClient.Initialize(settings);
			gameClient.Connect();
		}

		private void GetGameSettings()
		{
			gameClient.GetGameSettingsCompleted += (s, e) => InitializeGameSettings(e);
			gameClient.GetGameSettings();
		}

		private void InitializeGameSettings(GameSettingsEventArgs e)
		{
			// Initialize the settings
			globalSettings.World.Seed = e.Seed;
			globalSettings.World.Gravity = e.Gravity;

			// Initialize the world
			gameClient.World = new WorldContext();
			gameClient.World.Initialize(Context, globalSettings.World.ViewDistance, globalSettings.World.Gravity, globalSettings.World.Seed);

			ExitScene();
		}

		private void ExitScene()
		{
			// Remove the parent scene (and automatically, all its children) and add the target scene
			Context.Scenes.Remove(Parent);

			// Before exiting the current scene, initialize the target scene
			Context.Scenes.Add(new InGameScene(), true);
		}
	}
}