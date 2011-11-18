using System.Collections.Generic;
using Framework.Core.Contexts;
using Microsoft.Xna.Framework;

namespace Framework.Core.Scenes
{
	/// <summary>
	/// Base implementation for scenes
	/// </summary>
	public abstract class SceneBase
	{
		/// <summary>
		/// Gets or sets the game context used throughout the application containing shared resources
		/// </summary>
		protected GameContext Context;

		/// <summary>
		/// Gets or sets the parent of this scene
		/// </summary>
		public SceneBase Parent;

		/// <summary>
		/// Gets or sets the children (overlays/dialogs/popups) of the scene
		/// </summary>
		public List<SceneBase> SceneChildren;

		/// <summary>
		/// Gets or sets the flag indicating whether this scene has focus or not
		/// </summary>
		public bool HasFocus;

		/// <summary>
		/// Called when the scene needs to be initialized for the first time or after a reload
		/// </summary>
		/// <param name="context">Sets the game context of the scene</param>
		public virtual void Initialize(GameContext context)
		{
			Context = context;
			SceneChildren = new List<SceneBase>();
		}

		/// <summary>
		/// Loads all scene content
		/// </summary>
		public abstract void LoadContent();

		/// <summary>
		/// Clears all scene content
		/// </summary>
		public abstract void UnloadContent();

		/// <summary>
		/// Called when the scene needs to be updated
		/// </summary>
		/// <param name="gameTime">Time elapsed since the last call</param>
		public abstract void Update(GameTime gameTime);

		/// <summary>
		/// Called when the scene needs to be rendered
		/// </summary>
		/// <param name="gameTime">Time elapsed since the last call</param>
		public abstract void Render(GameTime gameTime);
	}
}