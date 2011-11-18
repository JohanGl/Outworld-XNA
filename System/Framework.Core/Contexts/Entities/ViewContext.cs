using System.Collections.Generic;
using Framework.Core.Scenes;
using Framework.Core.Scenes.Cameras;
using Microsoft.Xna.Framework;

namespace Framework.Core.Contexts
{
	public class ViewContext
	{
		/// <summary>
		/// The total boundaries of the application window
		/// </summary>
		public Rectangle Area;

		/// <summary>
		/// The safe area of the application window used when rendering to tv-sets which may not cover the whole area of a monitor
		/// </summary>
		public Rectangle SafeArea;

		/// <summary>
		/// Gets or sets the current scene being displayed in the application
		/// </summary>
		public SceneBase Scene;

		/// <summary>
		/// Gets or sets the available cameras
		/// </summary>
		public Dictionary<string, CameraBase> Cameras;

		public ViewContext()
		{
			Area = new Rectangle(0, 0, 1280, 720);
			Cameras = new Dictionary<string, CameraBase>();
		}
	}
}