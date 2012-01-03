using System;
using System.Windows.Forms;
using Framework.Core.Contexts;
using Framework.Gui.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Timers;
using Microsoft.Xna.Framework.Input;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Timer = System.Timers.Timer;

namespace Framework.Gui
{
	public class TextBox : UIElement
	{
		public event EventHandler<TextBoxEventArgs> EnterKeyDown;

		private string text;
		public string Text
		{
			get
			{
				return text;
			}

			set
			{
				if (value.Length <= maxLength)
				{
					text = value;
					caretIndex = text.Length;
				}
			}
		}

		// The maximum lengt of textbox-text
		private int maxLength;
		public int MaxLength
		{
			get
			{
				return maxLength;
			}
			set
			{
				maxLength = value;
			}
		}

		public SpriteFont Font;

		private Timer timerCaretBlinkDuration;
		private bool isCaretVisible;
		private int caretIndex;

		// The position of text in the Textbox
		private Vector2 textPosition;

		public TextBox(string text, int maxLength, SpriteFont font = null)
		{
			MaxLength = maxLength;

			Text = text;
			Font = font;

			Initialize();
		}

		private void Initialize()
		{
			isCaretVisible = false;

			// Initialize timer for cursor
			timerCaretBlinkDuration = new Timer(500);
			timerCaretBlinkDuration.AutoReset = true;
			timerCaretBlinkDuration.Elapsed += timerCaretBlinkDuration_Elapsed;
			timerCaretBlinkDuration.Start();

			caretIndex = text.Length;

			Height = 20;
			Width = 150;

			Opacity = 1f;
		}

		public override void SetFocus(bool state)
		{
			IsFocused = state;
		}

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
			guiManager.Arrange(this, availableSize);

			textPosition = Position;
			textPosition.Y += 2;
			textPosition.X += 4;
		}

		public void UpdateText(KeyboardInputEventArgs args)
		{
			// Special events
			if (args.IsControlDown)
			{
				// Handle paste events
				if (args.Value == 22)
				{
					var clipboardText = Clipboard.GetText(TextDataFormat.Text);

					if (!String.IsNullOrEmpty(clipboardText) && clipboardText.Length < 100)
					{
						for (int i = 0; i < clipboardText.Length; i++)
						{
							AddCharacter(clipboardText[i]);
						}
					}
				}
			}
			// Handle chars
			else if (args.Value > 31)
			{
				AddCharacter((char)args.Value);
			}
			// Handle special keys
			else
			{
				Keys key = args.Key.HasValue ? args.Key.Value : (Keys)args.Value;

				switch (key)
				{
					case Keys.Enter:
						HandleInputEnter();
						break;

					case Keys.Left:
						MoveCaret(-1);
						break;

					case Keys.Right:
						MoveCaret(1);
						break;

					case Keys.Back:
						HandleInputBackspace();
						break;

					case Keys.Delete:
						HandleInputDelete();
						break;

					case Keys.Home:
						SetCaretIndex(0);
						break;

					case Keys.End:
						SetCaretIndex(text.Length);
						break;
				}
			}
		}

		private void HandleInputEnter()
		{
			if (EnterKeyDown != null)
			{
				var args = new TextBoxEventArgs(text);
				EnterKeyDown(this, args);
			}
		}

		public override void HandleMouseEvent(MouseState mouseState)
		{
			// Pressed left mousebutton
			if (mouseState.LeftButton == ButtonState.Pressed)
			{
				// Pressed inside textBox
				if (IsInside(mouseState.X, mouseState.Y))
				{
					// Calculate and set the nearest caretIndex where mouse was pressed
					int closestLetterIndex = GetClosestIndexToMouse();

					if (closestLetterIndex != -1 && closestLetterIndex != caretIndex)
					{
						SetCaretIndex(closestLetterIndex);
					}
				}
			}
		}

		/// <summary>
		/// Deletes one character from the left side of the caret
		/// </summary>
		private void HandleInputBackspace()
		{
			// text must be larger than 0
			if (text.Length != 0)
			{
				// caretIndex must be larger than 0
				if (caretIndex != 0)
				{
					// Fetch text left- and rightside of the caret
					string textLeftOfCaret = text.Substring(0, caretIndex);
					string textRightOfCaret = text.Substring(caretIndex);

					// Update text
					text = textLeftOfCaret.Substring(0, textLeftOfCaret.Length - 1) + textRightOfCaret;

					MoveCaret(-1);
				}
			}
		}

		/// <summary>
		/// Deletes one character from the right side of the caret
		/// </summary>
		private void HandleInputDelete()
		{
			// text must be larger than 0
			if (text.Length != 0)
			{
				// caretIndex must be less than text.Length
				if (caretIndex != text.Length)
				{
					// Fetch text left- and rightside of the caret
					string textLeftOfCaret = text.Substring(0, caretIndex);
					string textRightOfCaret = text.Substring(caretIndex);

					// Update text
					text = textLeftOfCaret + textRightOfCaret.Substring(1);
				}
			}
		}

		private void AddCharacter(char value)
		{
			// Skip this step if the textbox is full
			if (text.Length == MaxLength)
			{
				return;
			}

			string character = value.ToString();

			// If the user wrote a valid character then add it
			if (character != string.Empty &&
				IsCharacterWithinFontRange(value))
			{
				// Fetch text left and right side of the caret
				string textLeftOfCaret = text.Substring(0, caretIndex);
				string textRightOfCaret = text.Substring(caretIndex);

				text = textLeftOfCaret + character + textRightOfCaret;

				MoveCaret(1);
			}
		}

		private bool IsCharacterWithinFontRange(char character)
		{
			return character <= 126;
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			// Render the rectangle
			RenderTextBoxBase(device, spriteBatch);
			
			if (IsFocused)
			{
				// Render the text
				spriteBatch.DrawString(Font, text, textPosition, Color.White * Opacity);
			}
			else
			{
				// Render the text
				spriteBatch.DrawString(Font, text, textPosition, Color.White * 0.5f);
				isCaretVisible = false;
			}

			// Render the caret
			if (isCaretVisible)
			{
				RenderCaret(spriteBatch);
			}
		}

		private void RenderCaret(SpriteBatch spriteBatch)
		{
			// Fetch the text from the left side of the caret, used to calculate the position of the caret
			string textLeftOfCaret = text.Substring(0, caretIndex);

			// Calculate the position of where to draw the caret
			Vector2 caretPosition = textPosition;
			caretPosition.X += Font.MeasureString(textLeftOfCaret).X - 1;
			caretPosition.Y -= 1;

			// Draw the caret
			spriteBatch.DrawString(Font, "|", caretPosition, Color.White * Opacity);
		}

		// Render the rectangle
		private void RenderTextBoxBase(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			Texture2D rect = new Texture2D(device, (int)Width, (int)Height);

			Color[] data = new Color[(int)Width * (int)Height];
			for (int i = 0; i < data.Length; ++i)
			{
				data[i] = Color.Black;
			}

			rect.SetData(data);

			spriteBatch.Draw(rect, Position, Color.White * (Opacity / 2.0f));
		}

		/// <summary>
		/// Move caret -1 or +1	=> move caret one unit to the left or the right
		/// </summary>
		/// <param name="offset"></param>
		private void MoveCaret(int offset)
		{
			// Move caret one position-unit to the left
			if (offset < 0)
			{
				caretIndex--;

				// Caretposition can never be less than 0
				if (caretIndex < 0)
				{
					caretIndex = 0;
				}
			}
			// Move caret one position-unit to the right
			else
			{
				caretIndex++;

				// Caretposition can never be larger than text.Lengt
				if (caretIndex > text.Length)
				{
					caretIndex = text.Length;
				}
			}
		}

		// Set caretposition to index
		public void SetCaretIndex(int index)
		{
			caretIndex = index;
		}

		// Calculate and set the nearest caretIndex where mouse was pressed
		private int GetClosestIndexToMouse()
		{
			MouseState ms = Mouse.GetState();

			// Iterate through substrings of the text to see if we can find the closest point where the cursor lies
			string substring = Text;
			float closestXValue = 0f;
			int closestLetterIndex = -1;

			for (int i = 0; i <= substring.Length; i++)
			{
				Vector2 lettersSize = Font.MeasureString((substring.Substring(0, i)));
				float lettersX = textPosition.X + lettersSize.X;
				float distance = Math.Abs(ms.X - lettersX);

				if (distance < closestXValue || closestLetterIndex == -1)
				{
					closestXValue = distance;
					closestLetterIndex = i;
				}
			}
			return closestLetterIndex;
		}

		// Handles caret-blink
		private void timerCaretBlinkDuration_Elapsed(object sender, ElapsedEventArgs e)
		{
			isCaretVisible = !isCaretVisible;
		}
	}
}