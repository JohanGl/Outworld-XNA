using System;
using System.Text;
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
using Microsoft.Xna.Framework.Graphics;
using Outworld.Settings.Global;

namespace Outworld.Scenes.InGame
{
	public class NewGameScene : SceneBase
	{
		private GlobalSettings globalSettings;
		private IGameServer gameServer;
		private IGameClient gameClient;
		private bool startedLoading;
		private bool isHost;
		private GuiManager gui;

		// Menu and submenus
		private StackPanel menuItem_Main;
		private StackPanel menuItem_JoinHost;

		private TextBox textBoxIp;

		private enum ButtonCommand
		{
			Menu_JoinHost,
			Menu_Return,
			CreateHost,
			JoinHost,
			Quit
		}

		public override void Initialize(GameContext context)
		{
			base.Initialize(context);

			gui = new GuiManager(Context.Input, Context.Graphics.Device, Context.Graphics.SpriteBatch);

			gameServer = ServiceLocator.Get<IGameServer>();
			gameClient = ServiceLocator.Get<IGameClient>();

			isHost = false;
			startedLoading = false;

			globalSettings = ServiceLocator.Get<GlobalSettings>();
			InitializeInput();

			context.Input.Mouse.ShowCursor = true;
			context.Input.Mouse.AutoCenter = false;
		}

		private void InitializeInput()
		{
			Context.Input.Keyboard.ClearMappings();
		}

		private void InitializeGui()
		{
			// Create the dialog background
			var background = new Image(Context.Resources.Textures["Gui.Hud.Dialog"]);
			background.HorizontalAlignment = HorizontalAlignment.Center;
			background.VerticalAlignment = VerticalAlignment.Center;
			background.Opacity = 0.9f;
			gui.Elements.Add(background);

			// Create the dialog title
			var title = new TextBlock("Menu", Context.Resources.Fonts["Global.Default"]);
			title.HorizontalAlignment = HorizontalAlignment.Center;
			title.VerticalAlignment = VerticalAlignment.Center;
			title.Margin = new Thickness(0, 0, 140, 225);
			gui.Elements.Add(title);

			// Create the MainMenu
			menuItem_Main = new StackPanel();
			menuItem_Main.HorizontalAlignment = HorizontalAlignment.Center;
			menuItem_Main.VerticalAlignment = VerticalAlignment.Center;
			menuItem_Main.Margin.Bottom = 100;
			menuItem_Main.Width = 200;
			menuItem_Main.Height = 400;
			menuItem_Main.Spacing.Bottom = 10;
			menuItem_Main.Visibility = Visibility.Visible;
			gui.Elements.Add(menuItem_Main);

			// Add the buttons to the stackpanel
			menuItem_Main.Children.Add(CreateButton("Create host", ButtonCommand.CreateHost));
			menuItem_Main.Children.Add(CreateButton("Join host", ButtonCommand.Menu_JoinHost));
			menuItem_Main.Children.Add(CreateButton("Quit", ButtonCommand.Quit));

			// Create the SubMenu
			menuItem_JoinHost = new StackPanel();
			menuItem_JoinHost.HorizontalAlignment = HorizontalAlignment.Center;
			menuItem_JoinHost.VerticalAlignment = VerticalAlignment.Center;
			menuItem_JoinHost.Margin.Bottom = 100;
			menuItem_JoinHost.Width = 200;
			menuItem_JoinHost.Height = 400;
			menuItem_JoinHost.Spacing.Bottom = 10;
			menuItem_JoinHost.Visibility = Visibility.Hidden;
			gui.Elements.Add(menuItem_JoinHost);

			// Add textbox to the stackpanel
			var textBoxInfo = new TextBoxInfo()
			{
				MaxLength = 100,
				SpriteFont = Context.Resources.Fonts["Global.Default"]
			};
			textBoxIp = new TextBox("", textBoxInfo);
			textBoxIp.Opacity = 1.0f;
			menuItem_JoinHost.Children.Add(textBoxIp);

			// Add the buttons to the stackpanel
			menuItem_JoinHost.Children.Add(CreateButton("Join", ButtonCommand.JoinHost));
			menuItem_JoinHost.Children.Add(CreateButton("Return", ButtonCommand.Menu_Return));

			gui.UpdateLayout();
		}

		private Button CreateButton(string text, object tag)
		{
			var button = new Button(text,
									Context.Resources.Fonts["Global.Default"],
									Context.Resources.Textures["Gui.Hud.Buttons.Default"],
									Context.Resources.Textures["Gui.Hud.Buttons.Selected"],
									Context.Resources.Textures["Gui.Hud.Buttons.Pressed"]);
			button.Opacity = 0.6f;
			button.Tag = tag;
			button.Click += button_Click;

			return button;
		}

		private void button_Click(object sender, EventArgs e)
		{
			var tag = (sender as Button).Tag;
			var command = (ButtonCommand)tag;

			switch (command)
			{
				case ButtonCommand.CreateHost:
					CreateHost();
					break;

				case ButtonCommand.JoinHost:
					JoinHost(textBoxIp.Text);
					break;

				case ButtonCommand.Menu_JoinHost:
					Menu_JoinHost();
					break;

				case ButtonCommand.Menu_Return:
					Menu_Return();
					break;

				case ButtonCommand.Quit:
					Context.Game.Exit();
					break;
			}
		}

		private void Menu_JoinHost()
		{
			menuItem_Main.Visibility = Visibility.Hidden;
			menuItem_JoinHost.Visibility = Visibility.Visible;
		}

		private void Menu_Return()
		{
			menuItem_Main.Visibility = Visibility.Visible;
			menuItem_JoinHost.Visibility = Visibility.Hidden;
		}

		private void CreateHost()
		{
			isHost = true;

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

		public override void LoadContent()
		{
			var content = Context.Resources.Content;
			var resources = Context.Resources;

			resources.Textures.Add("Gui.Hud.Dialog", content.Load<Texture2D>(@"Gui\Scenes\InGame\Dialog"));
			resources.Textures.Add("Gui.Hud.Buttons.Default", content.Load<Texture2D>(@"Gui\Controls\Buttons\Default"));
			resources.Textures.Add("Gui.Hud.Buttons.Selected", content.Load<Texture2D>(@"Gui\Controls\Buttons\Selected"));
			resources.Textures.Add("Gui.Hud.Buttons.Pressed", content.Load<Texture2D>(@"Gui\Controls\Buttons\Pressed"));

			InitializeGui();
		}

		public override void UnloadContent()
		{
			var resources = Context.Resources;

			resources.Textures.Remove("Gui.Hud.Dialog");
			resources.Textures.Remove("Gui.Hud.Buttons.Default");
			resources.Textures.Remove("Gui.Hud.Buttons.Selected");
			resources.Textures.Remove("Gui.Hud.Buttons.Pressed");
		}

		public override void Update(GameTime gameTime)
		{
			gui.UpdateInput();

			if (isHost && gameServer.IsStarted)
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
			Context.Graphics.Device.Clear(Color.Black);

			gui.Render();
		}

		private void GetGameSettings()
		{
			gameClient.GetGameSettingsCompleted += (s, e) => InitializeGameSettings(e);
			gameClient.GetGameSettings();
		}

		private void InitializeGameSettings(GameSettingsEventArgs e)
		{
			// Initialize the settings
			var settings = ServiceLocator.Get<GlobalSettings>();
			settings.World.Seed = e.Seed;
			settings.World.Gravity = e.Gravity;

			// Initialize the world
			gameClient.World = new WorldContext();
			gameClient.World.Initialize(Context, settings.World.ViewDistance, settings.World.Gravity, settings.World.Seed);

			ExitScene();
		}

		private void ExitScene()
		{
			// Before exiting the current scene, initialize the target scene
			var targetScene = new InGameScene();
			targetScene.Initialize(Context);
			targetScene.LoadContent();

			// Remove the current scene and add the target scene
			Context.Scenes.Remove(this);
			Context.Scenes.Add(targetScene);
		}
	}
}