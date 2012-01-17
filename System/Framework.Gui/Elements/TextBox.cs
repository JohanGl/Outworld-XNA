using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Core.Common;
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
		private struct BackgroundPart
		{
			public Rectangle[] Sections;
		}

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
				if (value.Length <= info.MaxLength)
				{
					text = value;
					caretIndex = text.Length;
				}
			}
		}

		private TextBoxInfo info;
		private Dictionary<byte, BackgroundPart> backgroundParts;
		private Rectangle[] backgroundPartDestinations;

		private Timer timerCaretBlinkDuration;
		private bool isCaretVisible;
		private int caretIndex;

		// The position of text in the Textbox
		private Vector2 textPosition;

		public TextBox(string text, TextBoxInfo info)
		{
			Text = text;
			this.info = info;

			Initialize();
		}

		private void Initialize()
		{
			isCaretVisible = false;
			caretIndex = text.Length;

			// Initialize the caret blink timer
			timerCaretBlinkDuration = new Timer(500);
			timerCaretBlinkDuration.AutoReset = true;
			timerCaretBlinkDuration.Elapsed += timerCaretBlinkDuration_Elapsed;
			timerCaretBlinkDuration.Start();

			Height = 20;
			Width = 150;

			Opacity = 1f;

			InitializeBackground();
		}

		private void InitializeBackground()
		{
			if (info.Background == null)
			{
				return;
			}

			backgroundParts = new Dictionary<byte, BackgroundPart>();
			backgroundPartDestinations = new Rectangle[9];

			backgroundParts.Add(0, GetBackgroundPart(0));
			backgroundParts.Add(1, GetBackgroundPart(1));
			backgroundParts.Add(2, GetBackgroundPart(2));
		}

		private BackgroundPart GetBackgroundPart(byte partIndex)
		{
			var size = new Vector2i(info.Background.Height, (info.Background.Width - 2) / 3);
			var unitSize = new Vector2i(size.X / 3, size.Y / 3);

			var partOffsetX = (size.X * partIndex) + partIndex;
			var offsetX = new int[] { partOffsetX, partOffsetX + unitSize.X, partOffsetX + (unitSize.X * 2) };
			var offsetY = new int[] { 0, unitSize.Y, (unitSize.Y * 2) };

			var part = new BackgroundPart();
			part.Sections = new Rectangle[9];

			part.Sections[0] = new Rectangle(offsetX[0], offsetY[0], unitSize.X, unitSize.Y);
			part.Sections[1] = new Rectangle(offsetX[1], offsetY[0], unitSize.X, unitSize.Y);
			part.Sections[2] = new Rectangle(offsetX[2], offsetY[0], unitSize.X, unitSize.Y);

			part.Sections[3] = new Rectangle(offsetX[0], offsetY[1], unitSize.X, unitSize.Y);
			part.Sections[4] = new Rectangle(offsetX[1], offsetY[1], unitSize.X, unitSize.Y);
			part.Sections[5] = new Rectangle(offsetX[2], offsetY[1], unitSize.X, unitSize.Y);

			part.Sections[6] = new Rectangle(offsetX[0], offsetY[2], unitSize.X, unitSize.Y);
			part.Sections[7] = new Rectangle(offsetX[1], offsetY[2], unitSize.X, unitSize.Y);
			part.Sections[8] = new Rectangle(offsetX[2], offsetY[2], unitSize.X, unitSize.Y);

			return part;
		}

		private void UpdateBackgroundPartLayout()
		{
			if (info.Background == null)
			{
				return;
			}

			var part = backgroundParts[0];

			int x = (int)Position.X;
			int y = (int)Position.Y;
			int w = part.Sections[0].Width;
			int h = part.Sections[0].Height;
			int controlWidth = (int)Width;
			int centerWidth = controlWidth - (w * 2);
			int centerHeight = (int)Height - (h * 2) + 4;
			int r = x + (controlWidth - w);

			backgroundPartDestinations[0] = new Rectangle(x, y, w, h);
			backgroundPartDestinations[1] = new Rectangle(x + w, y, centerWidth, h);
			backgroundPartDestinations[2] = new Rectangle(r, y, w, h);

			y += h;
			backgroundPartDestinations[3] = new Rectangle(x, y, w, centerHeight);
			backgroundPartDestinations[4] = new Rectangle(x + w, y, centerWidth, centerHeight);
			backgroundPartDestinations[5] = new Rectangle(r, y, w, centerHeight);

			y += centerHeight;
			backgroundPartDestinations[6] = new Rectangle(x, y, w, h);
			backgroundPartDestinations[7] = new Rectangle(x + w, y, centerWidth, h);
			backgroundPartDestinations[8] = new Rectangle(r, y, w, h);
		}

		public override void SetFocus(bool state)
		{
			IsFocused = state;
			SetCaretIndex(text.Length);
		}

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
			guiManager.Arrange(this, availableSize);

			textPosition = Position;
			textPosition.Y += 2;
			textPosition.X += 4;

			UpdateBackgroundPartLayout();
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
			if (text.Length == info.MaxLength)
			{
				return;
			}
			
			// Skip this step if there is a character mask and it doesnt contain the character
			if (info.CharacterMask != null)
			{
				if (!info.CharacterMask.Contains(value))
				{
					return;
				}
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
			return character >= 32 && character <= 126;
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			// Render the background
			RenderBackground(spriteBatch);
			
			if (IsFocused)
			{
				spriteBatch.DrawString(info.SpriteFont, text, textPosition, Color.White * Opacity);
			}
			else
			{
				spriteBatch.DrawString(info.SpriteFont, text, textPosition, Color.White * 0.5f);
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
			caretPosition.X += info.SpriteFont.MeasureString(textLeftOfCaret).X - 1;
			caretPosition.Y -= 1;

			// Draw the caret
			spriteBatch.DrawString(info.SpriteFont, "|", caretPosition, Color.White * Opacity);
		}

		private void RenderBackground(SpriteBatch spriteBatch)
		{
			if (info.Background == null)
			{
				return;
			}

			BackgroundPart part;

			if (IsFocused)
			{
				part = backgroundParts[2];
			}
			else if (IsHighlighted)
			{
				part = backgroundParts[1];
			}
			else
			{
				part = backgroundParts[0];
			}

			for (int i = 0; i < 9; i++)
			{
				spriteBatch.Draw(info.Background, backgroundPartDestinations[i], part.Sections[i], Color.White);
			}
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
				Vector2 lettersSize = info.SpriteFont.MeasureString((substring.Substring(0, i)));
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