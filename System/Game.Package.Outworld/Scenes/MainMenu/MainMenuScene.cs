using Framework.Core.Contexts;
using Framework.Core.Scenes;
using Framework.Core.Services;
using Framework.Gui;
using Game.Network.Clients;
using Game.Network.Servers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Outworld.Settings.Global;

namespace Outworld.Scenes.MainMenu
{
	public class MainMenuScene : SceneBase
	{
		private GuiManager gui;
		private StackPanel menuOptions;

		public override void LoadContent()
		{
			var content = Context.Resources.Content;
			var resources = Context.Resources;

			resources.Textures.Add("MainMenu.Background", content.Load<Texture2D>(@"Gui\Scenes\MainMenu\Background"));
			resources.Textures.Add("MainMenu.Separator", content.Load<Texture2D>(@"Gui\Scenes\MainMenu\Separator"));
			resources.Textures.Add("MainMenu.MenuOptions", content.Load<Texture2D>(@"Gui\Scenes\MainMenu\MenuOptions"));

			resources.Fonts.Add("Hud", content.Load<SpriteFont>(@"Fonts\Moire"));

			InitializeGui();
		}

		public override void UnloadContent()
		{
			var resources = Context.Resources;

			resources.Textures.Remove("MainMenu.Background");
			resources.Textures.Remove("MainMenu.Separator");
			resources.Textures.Remove("MainMenu.MenuOptions");

			resources.Fonts.Remove("Hud");
		}

		public override void Initialize(GameContext context)
		{
			base.Initialize(context);

			gui = new GuiManager(Context.Input, Context.Graphics.Device, Context.Graphics.SpriteBatch);

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
			menuOptions.VerticalAlignment = VerticalAlignment.Top;
			menuOptions.Margin = new Thickness(100, 170, 0, 0);
			menuOptions.Width = 176;
			menuOptions.Height = 400;
			menuOptions.Spacing.Bottom = 32;
			menuOptions.Visibility = Visibility.Visible;
			gui.Elements.Add(menuOptions);

			var menuOption = GetMenuOption(0);
			menuOptions.Children.Add(menuOption);

			menuOption = GetMenuOption(1);
			menuOptions.Children.Add(menuOption);

			menuOption = GetMenuOption(2);
			menuOptions.Children.Add(menuOption);

			menuOption = GetMenuOption(3);
			menuOptions.Children.Add(menuOption);

			menuOption = GetMenuOption(4);
			menuOptions.Children.Add(menuOption);

			gui.UpdateLayout();
		}

		private ImageButton GetMenuOption(int row)
		{
			var texture = Context.Resources.Textures["MainMenu.MenuOptions"];
			var optionWidth = 175;
			var optionHeight = 29;
			int offsetY = 0;

			switch (row)
			{
				case 1:
					offsetY = 58;
					break;

				case 2:
					offsetY = 115;
					break;

				case 3:
					offsetY = 173;
					break;

				case 4:
					offsetY = 231;
					break;
			}

			var defaultState = new Rectangle(0, offsetY, optionWidth, optionHeight);
			var hoverState = new Rectangle(176, offsetY, optionWidth, optionHeight);
			var focusedState = new Rectangle(352, offsetY, optionWidth, optionHeight);

			var result = new ImageButton(Context.Resources.Textures["MainMenu.MenuOptions"], defaultState, hoverState, focusedState);
			result.HorizontalAlignment = HorizontalAlignment.Left;
			result.VerticalAlignment = VerticalAlignment.Top;

			return result;
		}

		public override void Update(GameTime gameTime)
		{
			gui.UpdateInput();
		}

		public override void Render(GameTime gameTime)
		{
			Context.Graphics.Device.Clear(Color.Black);

			gui.Render();
		}
	}
}