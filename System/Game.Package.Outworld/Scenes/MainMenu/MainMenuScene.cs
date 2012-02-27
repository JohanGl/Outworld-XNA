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
	public enum SceneType
	{
		Main = 0,
		NewGame,
		JoinGame
	}

	public class MainMenuScene : SceneBase
	{
		private GuiManager gui;
		private IAudioHandler audioHandler;
		private TextBlock title;
		private bool isSoundEnabled;
		private bool autoStartAsHost;

		public MainMenuScene(bool autoStartAsHost = false)
		{
			this.autoStartAsHost = autoStartAsHost;
		}

		public override void LoadContent()
		{
			var content = Context.Resources.Content;
			var resources = Context.Resources;

			resources.Textures.Add("MainMenu.Background", content.Load<Texture2D>(@"Gui\Scenes\MainMenu\Background"));
			resources.Textures.Add("MainMenu.Separator", content.Load<Texture2D>(@"Gui\Scenes\MainMenu\Separator"));
			resources.Textures.Add("MainMenu.MainOptions", content.Load<Texture2D>(@"Gui\Scenes\MainMenu\MainOptions"));
			resources.Textures.Add("MainMenu.NewGameOptions", content.Load<Texture2D>(@"Gui\Scenes\MainMenu\NewGameOptions"));
			resources.Textures.Add("MainMenu.JoinGameOptions", content.Load<Texture2D>(@"Gui\Scenes\MainMenu\JoinGameOptions"));
			resources.Textures.Add("MainMenu.TextBox", content.Load<Texture2D>(@"Gui\Controls\TextBoxes\TextBox"));

			resources.Fonts.Add("Hud", content.Load<SpriteFont>(@"Fonts\Moire"));

			audioHandler.LoadSound("ButtonPress", @"Audio\Sounds\Gui\ButtonPress01");
			audioHandler.LoadSound("ButtonHighlight", @"Audio\Sounds\Gui\ButtonPress01");

			InitializeGui();

			isSoundEnabled = true;

			// Initialize all child scenes
			var mainScene = new MainScene();
			Context.Scenes.AddChild(this, mainScene, true);
			mainScene.InitializeGui(gui);

			var newGameScene = new NewGameScene();
			Context.Scenes.AddChild(this, newGameScene, true);
			newGameScene.InitializeGui(gui);

			var joinGameScene = new JoinGameScene();
			Context.Scenes.AddChild(this, joinGameScene, true);
			joinGameScene.InitializeGui(gui);

			ShowScene(SceneType.Main);

			if (autoStartAsHost)
			{
				newGameScene.StartAsHost();
			}
		}

		public override void UnloadContent()
		{
			var resources = Context.Resources;

			resources.Textures.Remove("MainMenu.Background");
			resources.Textures.Remove("MainMenu.Separator");
			resources.Textures.Remove("MainMenu.MainOptions");
			resources.Textures.Remove("MainMenu.NewGameOptions");
			resources.Textures.Remove("MainMenu.JoinGameOptions");
			resources.Textures.Remove("MainMenu.TextBox");

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

			title = new TextBlock("MAIN MENU", Context.Resources.Fonts["Hud"]);
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

			gui.UpdateLayout();
			gui.ElementStateChanged += GuiOnElementStateChanged;
		}

		private void GuiOnElementStateChanged(object sender, ElementStateChangeArgs e)
		{
			if (!(sender is ImageButton) || !isSoundEnabled)
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

		public void ShowScene(SceneType type)
		{
			// Hide all child scenes
			foreach (var scene in SceneChildren)
			{
				scene.HasFocus = false;
			}

			// Hide all child-scene controls (the first four are static and shouldnt be hidden)
			for (int i = 4; i < gui.Elements.Count; i++)
			{
				gui.Elements[i].Visibility = Visibility.Hidden;
			}

			switch (type)
			{
				case SceneType.Main:
					title.Text = "MAIN MENU";
					gui.Elements[4].Visibility = Visibility.Visible;
					break;

				case SceneType.NewGame:
					title.Text = "NEW GAME";
					gui.Elements[5].Visibility = Visibility.Visible;
					break;

				case SceneType.JoinGame:
					title.Text = "JOIN GAME";
					gui.Elements[6].Visibility = Visibility.Visible;
					break;
			}
		}

		public void DisableSound()
		{
			isSoundEnabled = false;
		}
	}
}