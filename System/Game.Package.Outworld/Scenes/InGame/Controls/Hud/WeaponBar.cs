using System;
using Framework.Core.Contexts;
using Framework.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Outworld.Scenes.InGame.Controls.Hud
{
	public class WeaponBar : UIElement
	{
		private GameContext context;
		private Vector2 positionAmount;
		private Vector2 positionProgressBar;
		private Vector2 positionTitle;

		private Rectangle destination;
		private Rectangle source;

		public int Amount;
		public double Percentage;

		public void Initialize(GameContext context)
		{
			this.context = context;

			Percentage = 100;
			Amount = 100;
		}

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
			HorizontalAlignment = HorizontalAlignment.Left;
			VerticalAlignment = VerticalAlignment.Bottom;
			Margin = new Thickness(16, 0, 0, 16);
			Width = 204;
			Height = 38;

			guiManager.Arrange(this, availableSize);

			positionAmount = new Vector2(Position.X + 7, Position.Y);
			positionTitle = new Vector2(Position.X + 83, Position.Y + 4);
			positionProgressBar = new Vector2(Position.X + 83, Position.Y + 17);

			UpdateProgressBar();
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(context.Resources.Textures["Gui.Hud.WeaponBorder"], Position, Color.White);
			spriteBatch.Draw(context.Resources.Textures["Gui.Hud.ProgressBar.Empty"], positionProgressBar, Color.White);
			spriteBatch.Draw(context.Resources.Textures["Gui.Hud.ProgressBar"], destination, source, Color.White);

			spriteBatch.DrawString(context.Resources.Fonts["Hud"], Amount.ToString("000"), positionAmount, Color.White);

			spriteBatch.DrawString(context.Resources.Fonts["Hud.Small"], "Weapon name", positionTitle, Color.White);
		}

		public void UpdateProgressBar()
		{
			int currentSlots = 0;

			if (Percentage > 0)
			{
				double clipPercentage = Percentage / 100d;

				if (clipPercentage > 1.0d)
				{
					clipPercentage = clipPercentage - (int)clipPercentage;
				}

				currentSlots = (int)(30 * clipPercentage);
			}

			int width = Math.Min(4 * currentSlots, context.Resources.Textures["Gui.Hud.ProgressBar"].Width);

			source = new Rectangle(0, 0, width, context.Resources.Textures["Gui.Hud.ProgressBar"].Height);

			destination = new Rectangle((int)positionProgressBar.X, (int)positionProgressBar.Y, width, context.Resources.Textures["Gui.Hud.ProgressBar"].Height);
		}
	}
}