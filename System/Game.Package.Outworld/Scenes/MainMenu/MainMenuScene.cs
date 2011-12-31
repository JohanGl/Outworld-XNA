using System;
using Framework.Audio;
using Framework.Core.Contexts;
using Framework.Core.Scenes;
using Framework.Core.Services;
using Framework.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Outworld.Scenes.MainMenu.ChildScenes;

namespace Outworld.Scenes.MainMenu
{
	public class MainMenuScene : SceneBase
	{
		private GuiManager gui;
		private StackPanel menuOptions;
		private IAudioHandler audioHandler;

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
			var content = Context.Resources.Content;
			var resources = Context.Resources;

			resources.Textures.Add("MainMenu.Background", content.Load<Texture2D>(@"Gui\Scenes\MainMenu\Background"));
			resources.Textures.Add("MainMenu.Separator", content.Load<Texture2D>(@"Gui\Scenes\MainMenu\Separator"));
			resources.Textures.Add("MainMenu.MenuOptions", content.Load<Texture2D>(@"Gui\Scenes\MainMenu\MenuOptions"));

			resources.Fonts.Add("Hud", content.Load<SpriteFont>(@"Fonts\Moire"));

			audioHandler.LoadSound("ButtonPress", @"Audio\Sounds\Gui\ButtonPress01");
			audioHandler.LoadSound("ButtonHighlight", @"Audio\Sounds\Gui\ButtonPress01");

			InitializeGui();
		}

		public override void UnloadContent()
		{
			var resources = Context.Resources;

			resources.Textures.Remove("MainMenu.Background");
			resources.Textures.Remove("MainMenu.Separator");
			resources.Textures.Remove("MainMenu.MenuOptions");

			resources.Fonts.Remove("Hud");

			audioHandler.UnloadContent();
		}

		public override void Initialize(GameContext context)
		{
			base.Initialize(context);

			gui = new GuiManager(Context.Input, Context.Graphics.Device, Context.Graphics.SpriteBatch);

			InitializeInput();

			audioHandler = ServiceLocator.Get<IAudioHandler>();

			context.Input.Mouse.ShowCursor = true;
			context.Input.Mouse.AutoCenter = false;

			// Initialize all child scenes
			//Context.Scenes.AddChild(this, new NewGameScene(), true);
		}

		private void InitializeInput()
		{
			Context.Input.Keyboard.ClearMappings();
		}

		private void InitializeGui()
		{
			// Create the dialog background
			var background = new Image(Context.Resources.Textures["MainMenu.Background"]);
			background.HorizontalAlignment = HorizontalAlignment.Left;
			background.VerticalAlignment = VerticalAlignment.Top;
			background.Width = Context.View.Area.Width;
			background.Height = Context.View.Area.Height;
			gui.Elements.Add(background);

			var title = new TextBlock("MAIN MENU", Context.Resources.Fonts["Hud"]);
			title.HorizontalAlignment = HorizontalAlignment.Left;
			title.VerticalAlignment = VerticalAlignment.Top;
			title.Margin = new Thickness(50, 50, 0, 0);
			gui.Elements.Add(title);

			var separatorTop = new Image(Context.Resources.Textures["MainMenu.Separator"]);
			separatorTop.HorizontalAlignment = HorizontalAlignment.Left;
			separatorTop.VerticalAlignment = VerticalAlignment.Top;
			separatorTop.Width = Context.View.Area.Width - 100;
			separatorTop.Height = 2;
			separatorTop.Margin = new Thickness(50, 50 + 40, 0, 0);
			gui.Elements.Add(separatorTop);

			var separatorBottom = new Image(Context.Resources.Textures["MainMenu.Separator"]);
			separatorBottom.HorizontalAlignment = HorizontalAlignment.Left;
			separatorBottom.VerticalAlignment = VerticalAlignment.Bottom;
			separatorBottom.Width = Context.View.Area.Width - 100;
			separatorBottom.Height = 2;
			separatorBottom.Margin = new Thickness(50, 0, 0, 92);
			gui.Elements.Add(separatorBottom);

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
			gui.ElementStateChanged += GuiOnElementStateChanged;
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

			var result = new ImageButton(Context.Resources.Textures["MainMenu.MenuOptions"], buttonStates);
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

		private void GuiOnElementStateChanged(object sender, ElementStateChangeArgs e)
		{
			if (!(sender is ImageButton))
			{
				return;
			}

			if (e.State == ElementState.Focused)
			{
				audioHandler.PlaySound("ButtonPress");
			}
			else if (e.State == ElementState.Highlighted)
			{
				audioHandler.PlaySound("ButtonHighlight", 0.25f);
			}
		}

		public override void Update(GameTime gameTime)
		{
			gui.UpdateInput();
			audioHandler.Update(gameTime);
		}

		public override void Render(GameTime gameTime)
		{
			Context.Graphics.Device.Clear(Color.Black);

			gui.Render();
		}
	}
}