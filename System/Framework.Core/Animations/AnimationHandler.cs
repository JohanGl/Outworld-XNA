using System.Collections.Generic;

namespace Framework.Core.Animations
{
	/// <summary>
	/// Handles animations and animation related tasks.
	/// T represents the Animations Dictionary key.
	/// </summary>
	public class AnimationHandler<T>
	{
		/// <summary>
		/// Gets or sets the list of animations within this handler
		/// </summary>
		public Dictionary<T, Animation> Animations;

		/// <summary>
		/// Gets the state indicating whether there are one or more animations running
		/// </summary>
		public bool HasRunningAnimations
		{
			get
			{
				// Loop through all animations
				foreach (var pair in Animations)
				{
					// Is the current animation running?
					if (pair.Value.IsRunning)
					{
						// Running animations
						return true;
					}
				}

				// No running animations
				return false;
			}
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public AnimationHandler()
		{
			Animations = new Dictionary<T, Animation>();
		}

		/// <summary>
		/// Updates all running animations
		/// </summary>
		public void Update()
		{
			// Loop through all animations
			foreach (var pair in Animations)
			{
				// If the current animation is running
				if (pair.Value.IsRunning)
				{
					// Update it
					pair.Value.Update();
				}
			}
		}
	}
}