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
		public string Text;
		public float Alpha;
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
			var item = new NotificationItem();
			item.Text = text;
			item.Alpha = 1f;
			item.FadeTimer = new GameTimer(TimeSpan.FromMilliseconds(100));

			items.Add(item);
		}

		public void Update(GameTime gameTime)
		{
			for (int i = 0; i < itemDisplayCapacity; i++)
			{
				if (items[i].FadeTimer.Update(gameTime))
				{
				}
			}
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			spriteBatch.DrawString(context.Resources.Fonts["Hud.Small"], "Notifications!", positionTitle, new Color(255, 255, 255, 0.5f));
		}
	}
}