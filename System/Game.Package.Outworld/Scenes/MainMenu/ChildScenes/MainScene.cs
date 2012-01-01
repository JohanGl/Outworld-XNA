using System;
using Framework.Core.Scenes;
using Framework.Gui;
using Microsoft.Xna.Framework;

namespace Outworld.Scenes.MainMenu.ChildScenes
{
	public class MainScene : SceneBase
	{
		private StackPanel menuOptions;

		private enum ButtonCommand
		{
			NewGame,
			LoadGame,
			Options,
			Credits,
			Quit
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
			menuOptions.Visibility = Visibility.Visible;
			gui.Elements.Add(menuOptions);

			for (int i = 0; i < 5; i++)
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
					buttonCommand = ButtonCommand.LoadGame;
					offsetY = 58;
					break;

				case 2:
					buttonCommand = ButtonCommand.Options;
					offsetY = 115;
					break;

				case 3:
					buttonCommand = ButtonCommand.Credits;
					offsetY = 173;
					break;

				case 4:
					buttonCommand = ButtonCommand.Quit;
					offsetY = 231;
					break;

				default:
					buttonCommand = ButtonCommand.NewGame;
					break;
			}

			var buttonStates = new ButtonStates();
			buttonStates.Default = new Rectangle(0, offsetY, optionWidth, optionHeight);
			buttonStates.Highlighted = new Rectangle(176, offsetY, optionWidth, optionHeight);
			buttonStates.Focused = new Rectangle(352, offsetY, optionWidth, optionHeight);
			buttonStates.Pressed = buttonStates.Default;

			var result = new ImageButton(Context.Resources.Textures["MainMenu.MainOptions"], buttonStates);
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
				case ButtonCommand.NewGame:
					((MainMenuScene)Parent).ShowScene(SceneType.NewGame);
					break;

				case ButtonCommand.LoadGame:
					break;

				case ButtonCommand.Options:
					break;

				case ButtonCommand.Credits:
					break;

				case ButtonCommand.Quit:
					Context.Game.Exit();
					break;
			}
		}

		public override void Update(GameTime gameTime)
		{
		}

		public override void Render(GameTime gameTime)
		{
		}
	}
}