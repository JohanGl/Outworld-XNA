using System;
using System.Collections.Generic;
using Framework.Core.Contexts;
using Framework.Core.Scenes;
using Framework.Core.Services;
using Framework.Gui;
using Framework.Gui.Events;
using Game.Network.Clients;
using Game.Network.Clients.Settings;
using Game.World;
using Microsoft.Xna.Framework;
using Outworld.Scenes.Debug.Models;
using Outworld.Scenes.InGame;
using Outworld.Settings.Global;

namespace Outworld.Scenes.MainMenu.ChildScenes
{
	public class JoinGameScene : SceneBase
	{
		private GlobalSettings globalSettings;
		private IGameClient gameClient;
		private bool startedLoading;
		private Panel panel;
		private StackPanel menuOptions;
		private TextBox textBoxIp;
		private GuiManager gui;

		private enum ButtonCommand
		{
			Connect,
			Return
		}

		public override void Initialize(GameContext context)
		{
			base.Initialize(context);

			globalSettings = ServiceLocator.Get<GlobalSettings>();
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
			this.gui = gui;

			panel = new Panel();
			panel.HorizontalAlignment = HorizontalAlignment.Left;
			panel.VerticalAlignment = VerticalAlignment.Top;
			panel.Width = Context.View.Area.Width;
			panel.Height = Context.View.Area.Height;
			gui.Elements.Add(panel);

			menuOptions = new StackPanel();
			menuOptions.HorizontalAlignment = HorizontalAlignment.Left;
			menuOptions.VerticalAlignment = VerticalAlignment.Center;
			menuOptions.Margin = new Thickness(100, 40, 0, 0);
			menuOptions.Width = 176;
			menuOptions.Height = 400;
			menuOptions.Spacing.Bottom = 32;
			panel.Children.Add(menuOptions);

			// Add the address textbox to the stackpanel
			var textBoxInfo = new TextBoxInfo()
			{
				MaxLength = 100,
				SpriteFont = Context.Resources.Fonts["Global.Normal"],
				Background = Context.Resources.Textures["MainMenu.TextBox"]
			};
			textBoxIp = new TextBox("", textBoxInfo);
			textBoxIp.HorizontalAlignment = HorizontalAlignment.Left;
			textBoxIp.VerticalAlignment = VerticalAlignment.Top;
			textBoxIp.Width = 220;
			textBoxIp.Text = "";
			textBoxIp.EnterKeyDown += textBoxIp_EnterKeyDown;
			menuOptions.Children.Add(textBoxIp);

			for (int i = 0; i < 2; i++)
			{
				menuOptions.Children.Add(GetMenuOption(i));
			}

			gui.UpdateLayout();
		}

		private void textBoxIp_EnterKeyDown(object sender, TextBoxEventArgs e)
		{
			gui.ClearFocus();
			menuOptions.Children[1].SetFocus(true);
			MenuOptionOnClick(menuOptions.Children[1], EventArgs.Empty);
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
					buttonCommand = ButtonCommand.Return;
					offsetY = 58;
					break;

				default:
					buttonCommand = ButtonCommand.Connect;
					break;
			}

			var buttonStates = new ButtonStates();
			buttonStates.Default = new Rectangle(0, offsetY, optionWidth, optionHeight);
			buttonStates.Highlighted = new Rectangle(176, offsetY, optionWidth, optionHeight);
			buttonStates.Focused = new Rectangle(352, offsetY, optionWidth, optionHeight);
			buttonStates.Pressed = buttonStates.Default;

			var result = new ImageButton(Context.Resources.Textures["MainMenu.JoinGameOptions"], buttonStates);
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
				case ButtonCommand.Connect:
					JoinHost(textBoxIp.Text);
					break;

				case ButtonCommand.Return:
					((MainMenuScene)Parent).ShowScene(SceneType.NewGame);
					break;
			}
		}

		public override void Update(GameTime gameTime)
		{
			if (panel.Visibility != Visibility.Visible)
			{
				return;
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
			// Remove the parent scene (and automatically, all its children) and add the target scene
			Context.Scenes.Remove(Parent);

			// Before exiting the current scene, initialize the target scene
			Context.Scenes.Add(new InGameScene(), true);
		}
	}
}