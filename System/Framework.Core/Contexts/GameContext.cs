using Framework.Core.Scenes;
using Microsoft.Xna.Framework;

namespace Framework.Core.Contexts
{
	public class GameContext
	{
		public Game Game;
		public GraphicsContext Graphics;
		public ViewContext View;
		public ResourceContext Resources;
		public InputContext Input;
		public SceneManager Scenes;

		public GameContext(Game game)
		{
			Game = game;
			Graphics = new GraphicsContext();
			View = new ViewContext();
			Resources = new ResourceContext();
			Input = new InputContext(this);
			Scenes = new SceneManager(this);
		}
	}
}