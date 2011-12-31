using Framework.Core.Scenes;
using Microsoft.Xna.Framework;

namespace Outworld.Scenes.MainMenu.ChildScenes
{
	public class NewGameScene : SceneBase
	{
		public override void LoadContent()
		{
		}

		public override void UnloadContent()
		{
		}

		public override void Update(GameTime gameTime)
		{
		}

		public override void Render(GameTime gameTime)
		{
			Context.Graphics.SpriteBatch.Begin();
			Context.Graphics.SpriteBatch.DrawString(Context.Resources.Fonts["Hud"], "Test", new Vector2(10, 10), Color.White);
			Context.Graphics.SpriteBatch.End();
		}
	}
}