using System.Collections.Generic;
using System.Timers;
using Framework.Network.Clients;
using Microsoft.Xna.Framework;

namespace NetworkTool
{
	public partial class MainWindow
	{
		private Timer updateTimer;
		private List<RecordedMessage> recordedMessages;
		private int index;
		private GameClient gameClient;

		public MainWindow()
		{
			InitializeComponent();

			updateTimer = new Timer(1000 / 20);
			updateTimer.Elapsed += updateTimer_Elapsed;

			var reader = new RecordedFileReader(@"Recordings/recording01.xml");
			recordedMessages = reader.GetRecording();

			gameClient = new GameClient();
			gameClient.Connect();
		}

		private void ButtonPlayback_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			StartPlayback();
		}

		private void StartPlayback()
		{
			index = 0;
			updateTimer.Start();
		}

		private void StopPlayback()
		{
			updateTimer.Stop();
		}

		private void updateTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("Index " + index + " / " + recordedMessages.Count);

			if (index >= recordedMessages.Count)
			{
				StopPlayback();
				return;
			}

			var message = recordedMessages[index];

			bool hasSpatial = HasSpatialData(message.Spatial);
			bool hasEvents = message.Events.Count > 0;
			bool isCombined = hasSpatial && hasEvents;

			if (isCombined)
			{
				gameClient.BeginCombinedMessage();
			}

			if (hasSpatial)
			{
				gameClient.SendSpatial(message.Spatial.Position, message.Spatial.Velocity, message.Spatial.Angle);
			}

			if (hasEvents)
			{
				gameClient.SendEvents(message.Events);
			}

			if (isCombined)
			{
				gameClient.EndCombinedMessage();
			}

			index++;
		}

		private bool HasSpatialData(EntitySpatial spatial)
		{
			if (spatial.Position == Vector3.Zero &&
				spatial.Velocity == Vector3.Zero &&
				spatial.Angle == Vector3.Zero)
			{
				return false;
			}

			return true;
		}
	}
}