using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Framework.Network.Messages;
using Game.Network.Common;
using Microsoft.Xna.Framework;

namespace NetworkTool
{
	public class ClientPlaybackHandler
	{
		private GameClient gameClient;
		private DispatcherTimer tickrateTimer;
		private List<RecordedMessage> messages;
		private int messageIndex;

		public ClientPlaybackHandler()
		{
			gameClient = new GameClient();
			gameClient.Connect();

			tickrateTimer = new DispatcherTimer();
			tickrateTimer.Interval = TimeSpan.FromMilliseconds(1000 / 20);
			tickrateTimer.Tick += TickrateTimer_Tick;
		}

		public void Play(List<RecordedMessage> messages)
		{
			this.messages = messages;
			messageIndex = 0;
			tickrateTimer.Start();

			System.Diagnostics.Debug.WriteLine("Playback started");
		}

		public void Stop()
		{
			tickrateTimer.Stop();
			messageIndex = 0;

			System.Diagnostics.Debug.WriteLine("Playback stopped");
		}

		private void TickrateTimer_Tick(object sender, EventArgs eventArgs)
		{
			if (messageIndex >= messages.Count)
			{
				Stop();
				return;
			}

			var message = messages[messageIndex++];

			if (message.Spatial.Position == Vector3.Zero)
			{
				return;
			}

			if (message.Events.Count > 0)
			{
				gameClient.BeginCombinedMessage();
				gameClient.SendSpatial(message.Spatial.Position, message.Spatial.Velocity, message.Spatial.Angle);
				gameClient.SendEvents(message.Events);
				gameClient.EndCombinedMessage();
			}
			else
			{
				gameClient.SendSpatial(message.Spatial.Position, message.Spatial.Velocity, message.Spatial.Angle);
			}

			System.Diagnostics.Debug.WriteLine(string.Format("Sending message: {0} / {1}", messageIndex + 1, messages.Count));

			messageIndex++;
		}
	}
}