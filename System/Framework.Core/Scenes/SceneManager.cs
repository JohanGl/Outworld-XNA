using System.Collections.Generic;
using Framework.Core.Contexts;
using Microsoft.Xna.Framework;

namespace Framework.Core.Scenes
{
	public class SceneManager
	{
		private GameContext context;
		private List<SceneBase> Scenes { get; set; }

		public SceneManager(GameContext context)
		{
			this.context = context;
			Scenes = new List<SceneBase>();
		}

		#region Adding and removal of scenes

		public void Add(SceneBase scene, bool initializeAndLoadContent = false)
		{
			if (initializeAndLoadContent)
			{
				scene.Initialize(context);
				scene.LoadContent();
			}

			Scenes.Add(scene);
		}

		public void Remove(SceneBase scene)
		{
			// Remove a root scenes
			if (scene.Parent == null)
			{
				for (int i = 0; i < Scenes.Count; i++)
				{
					var currentScene = Scenes[i];

					// Remove the scene
					if (scene == currentScene)
					{
						scene.UnloadContent();
						Scenes.Remove(scene);
						break;
					}
				}
			}
			// Remove a child scene
			else
			{
				for (int i = 0; i < Scenes.Count; i++)
				{
					var currentScene = Scenes[i];

					// Remove the scene if found as a child-scene to the current scene
					for (int j = 0; j < currentScene.SceneChildren.Count; j++)
					{
						var child = currentScene.SceneChildren[j];

						if (scene == child)
						{
							scene.UnloadContent();
							currentScene.SceneChildren.Remove(scene);
							break;
						}
					}
				}
			}
		}

		public void RemoveAll()
		{
			UnloadContent();
			Scenes.Clear();
		}

		#endregion

		public void AddChild(SceneBase parent, SceneBase child, bool initializeAndLoadContent = false)
		{
			child.Parent = parent;

			if (initializeAndLoadContent)
			{
				child.Initialize(context);
				child.LoadContent();
			}

			parent.SceneChildren.Add(child);
		}

		#region Scene broadcast methods

		public void Initialize(GameContext context)
		{
			for (int i = 0; i < Scenes.Count; i++)
			{
				Scenes[i].Initialize(context);
			}
		}

		public void LoadContent()
		{
			for (int i = 0; i < Scenes.Count; i++)
			{
				Scenes[i].LoadContent();
			}
		}

		public void UnloadContent()
		{
			for (int i = 0; i < Scenes.Count; i++)
			{
				Scenes[i].UnloadContent();
			}
		}

		public void Update(GameTime gameTime)
		{
			for (int i = 0; i < Scenes.Count; i++)
			{
				var scene = Scenes[i];

				// If the current scene has children then set focus to the last child scene
				if (scene.SceneChildren.Count > 0)
				{
					for (int j = 0; j < scene.SceneChildren.Count; j++)
					{
						scene.SceneChildren[j].HasFocus = (j == scene.SceneChildren.Count - 1);
						scene.SceneChildren[j].Update(gameTime);
					}

					scene.HasFocus = false;
				}
				// The current scene does not have any children
				else
				{
					scene.HasFocus = (i == Scenes.Count - 1);
				}

				scene.Update(gameTime);
			}
		}

		public void Render(GameTime gameTime)
		{
			bool hasRendered = false;

			if (Scenes.Count > 0)
			{
				var scene = Scenes[Scenes.Count - 1];

				// Render the scene
				scene.Render(gameTime);

				// Render all children of the current scene from the last to the first
				for (int i = scene.SceneChildren.Count - 1; i >= 0; i--)
				{
					scene.SceneChildren[i].Render(gameTime);
				}

				hasRendered = true;
			}

			// If no scenes were rendered, then clear the screen with the default clear color
			if (!hasRendered)
			{
				context.Graphics.Device.Clear(Color.Black);
			}
		}

		#endregion
	}
}