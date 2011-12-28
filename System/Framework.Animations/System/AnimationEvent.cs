using System;

namespace Framework.Animations.System
{
	/// <summary>
	/// Information about an event in our animation
	/// </summary>
	public class AnimationEvent
	{
		/// <summary>
		/// The name of the event
		/// </summary>
		public String EventName { get; set; }

		/// <summary>
		/// The time of the event
		/// </summary>
		public TimeSpan EventTime { get; set; }
	}
}