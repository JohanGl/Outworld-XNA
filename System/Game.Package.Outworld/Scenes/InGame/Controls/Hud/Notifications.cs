using System;
using System.Collections.Generic;
using Framework.Core.Common;
using Framework.Core.Contexts;
using Framework.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Outworld.Scenes.InGame.Controls.Hud
{
	public class Notifications : UIElement
	{
		protected class NotificationItem
		{
			public float Opacity;
			public string Text;
		}

		private GameContext context;
		private Vector2 positionTitle;
		private GameTimer fadeTimer;

		private int capacity;
		private List<NotificationItem> items;

		public void Initialize(GameContext context, int capacity)
		{
			this.context = context;
			this.capacity = capacity;

			items = new List<NotificationItem>(capacity);

			fadeTimer = new GameTimer(TimeSpan.FromMilliseconds(50));
		}

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
			HorizontalAlignment = HorizontalAlignment.Left;
			VerticalAlignment = VerticalAlignment.Center;
			Margin = new Thickness(4, 0, 0, 0);
			Width = 300;
			Height = 40;

			guiManager.Arrange(this, availableSize);

			positionTitle = new Vector2(Position.X, Position.Y);
		}

		public void AddNotification(string text)
		{
			if (items.Count == capacity)
			{
				items.RemoveAt(0);
			}

			items.Add(new NotificationItem() { Text = text, Opacity = 5f });
		}

		public void Update(GameTime gameTime)
		{
			// Update all items fade state
			if (fadeTimer.Update(gameTime))
			{
				for (int i = 0; i < items.Count; i++)
				{
					var item = items[i];

					if (item.Opacity > 0f)
					{
						item.Opacity -= 0.05f;
					}
					else
					{
						items.Remove(item);
					}
				}
			}
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			for (int i = 0; i < items.Count; i++)
			{
				var item = items[i];

				spriteBatch.DrawString(context.Resources.Fonts["Hud.Small"], item.Text,
										positionTitle + new Vector2(0, i * 12),
										Color.White * item.Opacity);
			}
		}
	}
}