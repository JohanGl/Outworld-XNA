using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Core.Contexts;
using Framework.Gui.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Framework.Gui
{
	public class ChatBox : UIElement
	{
		public event EventHandler<TextBoxEventArgs> EnterKeyDown;

		public SpriteFont Font;

		// The text shown in the ChatBox
		private int numberOfLines;
		private TextBlock[] textBlocks;
		private int textHistoryIndex;
		private Queue<string> textHistory;

		// The input-control
		private TextBox textBox;

		// MouseStates
		private MouseState currentMouseState;
		private MouseState lastMouseState;

		public ChatBox(int width, int height, int numberOfrows = 5, SpriteFont font = null)
		{
			Initialize();

			Font = font;

			// Initialize child control TextBox
			var textBoxInfo = new TextBoxInfo
			{
				MaxLength = 50,
				SpriteFont = font
			};
			textBox = new TextBox("", textBoxInfo);
			textBox.EnterKeyDown += textBox_EnterKeyDown;

			// Initialize child controls textBlocks
			numberOfLines = numberOfrows;
			textBlocks = new TextBlock[numberOfLines];
			for (int i = 0; i < numberOfLines; i++)
			{
				textBlocks[i] = new TextBlock("", font);
			}

			Height = height;
			Width = width;
		}

		private void textBox_EnterKeyDown(object sender, TextBoxEventArgs e)
		{
			if (EnterKeyDown != null)
			{
				EnterKeyDown(this, e);
			}
		}

		private void Initialize()
		{
			textHistory = new Queue<string>(500);
			textHistoryIndex = 0;
		}

		public override void SetFocus(bool state)
		{
			IsFocused = state;

			// Set focusstate to child UIElements
			textBox.IsFocused = state;

			for (int i = 0; i < textBlocks.Count(); i++)
			{
				textBlocks[i].IsFocused = state;
			}
		}

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
			guiManager.Arrange(this, availableSize);

			textBox.Width = Width;
			textBox.Height = 20;

			textBox.VerticalAlignment = VerticalAlignment.Bottom;
			textBox.HorizontalAlignment = HorizontalAlignment.Left;
			textBox.Margin = new Thickness(0, 0, 0, 0);

			var chatBoxBounds = new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);
			textBox.UpdateLayout(guiManager, chatBoxBounds);

			for (int i = 0; i < numberOfLines; i++)
			{
				textBlocks[i].Width = Width;
				textBlocks[i].Height = Height;
				textBlocks[i].VerticalAlignment = VerticalAlignment.Bottom;
				textBlocks[i].HorizontalAlignment = HorizontalAlignment.Left;
				textBlocks[i].Margin = new Thickness(5, 0, 0, 37 + (20 * i));
				textBlocks[i].UpdateLayout(guiManager, chatBoxBounds);
			}
		}

		public void UpdateText(KeyboardInputEventArgs args)
		{
			if (args.Value == (int)Keys.Enter)
			{
				for (int i = numberOfLines - 1; i > 0; i--)
				{
					textBlocks[i].Text = textBlocks[i - 1].Text;
				}

				string time = DateTime.Now.Hour.ToString() + ":" +
							  DateTime.Now.Minute.ToString() + " - ";

				// Add text to the queue
				textHistory.Enqueue(time + textBox.Text);

				// Set the latest textBlock to inputed text
				textBlocks[0].Text = time + textBox.Text;

				// Clear input-field and reset caret index
				textBox.Text = "";
				textBox.SetCaretIndex(0);
			}
			else
			{
				textBox.UpdateText(args);
			}
		}

		// Handles all MouseEvents
		public override void HandleMouseEvent(MouseState mouseState)
		{
			currentMouseState = mouseState;

			HandleMouseEventForTextBox(mouseState);
			HandleMouseEventForTextBlocks(mouseState);
			HandleMouseEventForChatBox(mouseState);

			lastMouseState = mouseState;
		}

		// Scroll through history
		private void HandleMouseEventForChatBox(MouseState mouseState)
		{
			// Is the mouse inside the bounds of the control?
			if (IsInside(mouseState.X, mouseState.Y))
			{
				float scrollDelta = lastMouseState.ScrollWheelValue - (float)currentMouseState.ScrollWheelValue;

				// Did the user scroll?
				if (scrollDelta != 0)
				{
					HandleMouseWheel(scrollDelta);
				}
			}
		}

		// Handle mouse wheel - Scroll through history.
		private void HandleMouseWheel(float scrollDelta)
		{
			if (scrollDelta > 0)
			{
				textHistoryIndex++;

				if (textHistoryIndex > textHistory.Count() - numberOfLines)
				{
					textHistoryIndex = textHistory.Count() - numberOfLines;
				}
			}
			else
			{
				textHistoryIndex--;

				if (textHistoryIndex < 0)
				{
					textHistoryIndex = 0;
				}
			}

			// Set textBlocks Text to the new values
			int index = textHistoryIndex;
			for (int i = numberOfLines - 1; i >= 0; i--)
			{
				// Fetch text from new textHistoryIndex
				string textItem = textHistory.ElementAtOrDefault(index++);

				// Check if text is null
				textBlocks[i].Text = String.IsNullOrEmpty(textItem) ? "" : textItem;
			}
		}

		// Handle left mousebutton pressed - Update caret position
		private void HandleMouseEventForTextBox(MouseState mouseState)
		{
			textBox.HandleMouseEvent(mouseState);
		}

		private void HandleMouseEventForTextBlocks(MouseState mouseState)
		{
			for (int i = numberOfLines - 1; i >= 0; i--)
			{
				textBlocks[i].HandleMouseEvent(mouseState);
			}
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			// Set the opacity of the ChatBox
			SetOpacity();

			// The ChatBox-base
			RenderChatBoxBase(device, spriteBatch);

			// The input-field
			textBox.Render(device, spriteBatch);

			// The ChatBox-text
			for (int i = numberOfLines - 1; i >= 0; i--)
			{
				textBlocks[i].Render(device, spriteBatch);
			}
		}

		private void RenderChatBoxBase(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			Texture2D rect = new Texture2D(device, (int)Width, (int)Height);

			Color[] data = new Color[(int)Width * (int)Height];
			for (int i = 0; i < data.Length; ++i)
			{
				data[i] = Color.Black;
			}

			rect.SetData(data);

			spriteBatch.Draw(rect, Position, Color.White * Opacity);
		}

		// Sets the opacity of ChatBox and its child-controls depending on the current focusstate
		private void SetOpacity()
		{
			if (IsFocused)
			{
				Opacity = 0.5f;

				textBox.Opacity = 1.0f;

				for (int i = numberOfLines - 1; i >= 0; i--)
				{
					textBlocks[i].Opacity = 1.0f;
				}
			}
			else
			{
				Opacity = 0.1f;

				textBox.Opacity = 0.2f;

				for (int i = numberOfLines - 1; i >= 0; i--)
				{
					textBlocks[i].Opacity = 0.2f;
				}
			}
		}
	}
}