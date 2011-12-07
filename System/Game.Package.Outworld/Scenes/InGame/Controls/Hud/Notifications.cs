using System;
using System.Collections.Generic;
using Framework.Core.Common;
using Framework.Core.Contexts;
using Framework.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Outworld.Scenes.InGame.Controls.Hud
{
	public class NotificationItem
	{
		public bool IsEnabled;
		public string Text;
		public float Opacity;
		public GameTimer FadeTimer;
	}

	public class Notifications : UIElement
	{
		private GameContext context;
		private Vector2 positionTitle;

		private int itemDisplayCapacity;
		private List<NotificationItem> items;

		public void Initialize(GameContext context)
		{
			this.context = context;

			itemDisplayCapacity = 10;
			items = new List<NotificationItem>(itemDisplayCapacity);
		}

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
			HorizontalAlignment = HorizontalAlignment.Left;
			VerticalAlignment = VerticalAlignment.Center;
			Margin = new Thickness(16, 0, 0, 0);
			Width = 300;
			Height = 40;

			guiManager.Arrange(this, availableSize);

			positionTitle = new Vector2(Position.X, Position.Y);
		}

		public void AddNotification(string text)
		{
			// Create the item and set opacity to 8 which means that it will be visible for 8 seconds
			var item = new NotificationItem
			{
				IsEnabled = true,
				Text = text,
				Opacity = 8f,
				FadeTimer = new GameTimer(TimeSpan.FromMilliseconds(50))
			};

			items.Add(item);
		}

		public void Update(GameTime gameTime)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].IsEnabled)
				{
					if (items[i].FadeTimer.Update(gameTime))
					{
						items[i].Opacity -= 0.05f;
						items[i].IsEnabled = items[i].Opacity > 0f;
					}
				}
			}
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].IsEnabled)
				{
					spriteBatch.DrawString(context.Resources.Fonts["Hud.Small"], items[i].Text, positionTitle + new Vector2(0, i * 10), Color.White * items[i].Opacity);
				}
			}
		}
	}
}