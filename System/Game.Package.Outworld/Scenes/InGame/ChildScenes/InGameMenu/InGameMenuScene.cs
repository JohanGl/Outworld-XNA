using System.Collections.Generic;
using Framework.Core.Contexts;
using Framework.Core.Scenes;
using Framework.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Outworld.Scenes.InGame.ChildScenes.InGameMenu
{
	public class InGameMenuScene : SceneBase
	{
		private enum ButtonCommand
		{
			Return,
			Respawn,
			Quit
		}

		private GuiManager gui;
		private List<Button> buttons;
		private int buttonIndex = 0;

		public override void Initialize(GameContext context)
		{
			base.Initialize(context);

			gui = new GuiManager(Context.Input, Context.Graphics.Device, Context.Graphics.SpriteBatch);
			buttons = new List<Button>();

			// Enable the mouse in this scene
			Context.Input.Mouse.AutoCenter = false;
			context.Input.Mouse.ShowCursor = true;
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
			// Leave menu
			if (Context.Input.GamePadState[Buttons.Back].WasJustPressed)
			{
				ExitScene();
			}

			UpdateInputs();
		}

		public override void Render(GameTime gameTime)
		{
			gui.Render();
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
			var title = new TextBlock("Ingame Menu", Context.Resources.Fonts["Global.Default"]);
			title.HorizontalAlignment = HorizontalAlignment.Center;
			title.VerticalAlignment = VerticalAlignment.Center;
			title.Margin = new Thickness(0, 0, 140, 225);
			gui.Elements.Add(title);

			// Create the button stackpanel
			var stackPanel = new StackPanel();
			stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
			stackPanel.VerticalAlignment = VerticalAlignment.Center;
			stackPanel.Margin.Bottom = 100;
			stackPanel.Width = 200;
			stackPanel.Height = 400;
			stackPanel.Spacing.Bottom = 10;
			gui.Elements.Add(stackPanel);

			// Add the buttons to the stackpanel
			stackPanel.Children.Add(CreateButton("Return", ButtonCommand.Return));
			stackPanel.Children.Add(CreateButton("Respawn", ButtonCommand.Respawn));
			stackPanel.Children.Add(CreateButton("Quit", ButtonCommand.Quit));

			buttons.Add(stackPanel.Children[0] as Button);
			buttons.Add(stackPanel.Children[1] as Button);
			buttons.Add(stackPanel.Children[2] as Button);

			// Create the buttons
			//AddButton("Return", new Thickness(0, 0, 90, 180), ButtonCommand.Return);
			//AddButton("Respawn", new Thickness(0, 0, 90, 130), ButtonCommand.Respawn);
			//AddButton("Quit", new Thickness(0, 0, 90, 80), ButtonCommand.Quit);

			gui.UpdateLayout();

			UpdateFocus();
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

		private void button_Click(object sender, System.EventArgs e)
		{
			var tag = (sender as Button).Tag;
			var command = (ButtonCommand)tag;

			switch (command)
			{
				case ButtonCommand.Return:
					ExitScene();
					break;

				case ButtonCommand.Respawn:
					(Parent as InGameScene).Respawn();
					ExitScene();
					break;

				case ButtonCommand.Quit:
					Context.Game.Exit();
					break;
			}
		}

		private void UpdateInputs()
		{
			

			// Select previous GUI-item
			if (Context.Input.GamePadState[Buttons.LeftThumbstickUp].WasJustPressed ||
				Context.Input.GamePadState[Buttons.DPadUp].WasJustPressed ||
				Context.Input.Keyboard.KeyboardState[Keys.Up].WasJustPressed)
			{
				buttonIndex--;

				if (buttonIndex < 0)
				{
					buttonIndex = buttons.Count - 1;
				}

				UpdateFocus();
			}

			// Select next GUI-item
			if (Context.Input.GamePadState[Buttons.LeftThumbstickDown].WasJustPressed || 
				Context.Input.GamePadState[Buttons.DPadDown].WasJustPressed ||
				Context.Input.Keyboard.KeyboardState[Keys.Down].WasJustPressed)
			{
				buttonIndex++;

				if (buttonIndex >= buttons.Count)
				{
					buttonIndex = 0;
				}

				UpdateFocus();
			}

			// GUI-item pressed
			if (Context.Input.GamePadState[Buttons.A].WasJustPressed ||
				Context.Input.Keyboard.KeyboardState[Keys.Space].WasJustPressed)
			{
				PressSelectedButton();
			}
		}

		private void PressSelectedButton()
		{
			buttons[buttonIndex].Press();
		}

		private void UpdateFocus()
		{
			// Clear all button states
			foreach (var button in buttons)
			{
				button.IsFocused = false;
			}

			buttons[buttonIndex].IsFocused = true;

			//gui.FocusElement(buttons[buttonIndex]);
		}

		private void ExitScene()
		{
			// Disable the mouse when returning to "in game" mode
			Context.Input.Mouse.ShowCursor = false;
			Context.Input.Mouse.AutoCenter = true;

			Context.Scenes.Remove(this);
		}
	}
}