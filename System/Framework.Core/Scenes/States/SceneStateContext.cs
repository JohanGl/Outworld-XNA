using System;
using System.Collections.Generic;

namespace Framework.Core.Scenes
{
	public class SceneStateContext
	{
		/// <summary>
		/// Gets or sets the current scene state
		/// </summary>
		public SceneStateType State;

		/// <summary>
		/// Gets or sets the flag indicating whether the scene has focus or not
		/// </summary>
		public bool HasFocus;

		/// <summary>
		/// Gets or sets the flag indicating whether the scene has transparent elements to it and if so,
		/// allow for the scene behind it to be rendered as well during the application render cycle
		/// </summary>
		public bool IsOverlay;

		/// <summary>
		/// Gets or sets the available transition durations
		/// </summary>
		public Dictionary<SceneStateType, TimeSpan> TransitionDurations;

		/// <summary>
		/// Default constructor
		/// </summary>
		public SceneStateContext()
		{
			// Set the default state
			State = SceneStateType.TransitionIn;

			// Initialize transition durations
			TransitionDurations = new Dictionary<SceneStateType, TimeSpan>();
			TransitionDurations[SceneStateType.TransitionIn] = TimeSpan.FromSeconds(1);
			TransitionDurations[SceneStateType.TransitionOut] = TimeSpan.FromSeconds(1);
		}
	}

	/// <summary>
	/// Available scene state types
	/// </summary>
	public enum SceneStateType
	{
		TransitionIn,
		TransitionOut,
		Active,
		Hidden
	}
}