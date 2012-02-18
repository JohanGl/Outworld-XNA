using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Controls;
using Game.Network.Common;
using Microsoft.Xna.Framework;

namespace NetworkTool
{
	public partial class MainWindow
	{
		private MessageHandler messageHandler;
		private List<Message> messages;

		private MessagePlaybackHandler playbackHandler;

		public MainWindow()
		{
			InitializeComponent();

			messageHandler = new MessageHandler();
			messages = new List<Message>();

			playbackHandler = new MessagePlaybackHandler();

			InitializeAvailableRecordings();
		}

		private void InitializeAvailableRecordings()
		{
			var result = new List<FileRecording>();

			var path = Path.Combine(Environment.CurrentDirectory, "Recordings");
			var files = Directory.GetFiles(path).Where(p => p.EndsWith(".xml"));

			foreach (var file in files)
			{
				var recording = new FileRecording
				{
					Path = file,
					Name = file.Substring(file.LastIndexOf('\\') + 1)
				};

				result.Add(recording);
			}

			comboSelectedFile.DisplayMemberPath = "Name";
			comboSelectedFile.ItemsSource = result;
		}

		private void InitializeAvailableClients()
		{
			var clients = messages.Select(p => p.ClientId).Distinct();

			comboSelectedClient.ItemsSource = clients;
		}

		private void ButtonPlayback_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			playbackHandler.Play((long)comboSelectedClient.SelectedItem, messages);
		}

		private void updateTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			//System.Diagnostics.Debug.WriteLine("Index " + index + " / " + recordedMessages.Count);

			//if (index >= recordedMessages.Count)
			//{
			//    StopPlayback();
			//    return;
			//}

			//var message = recordedMessages[index];

			//bool hasSpatial = HasSpatialData(message.Spatial);
			//bool hasEvents = message.Events.Count > 0;
			//bool isCombined = hasSpatial && hasEvents;

			//if (isCombined)
			//{
			//    gameClient.BeginCombinedMessage();
			//}

			//if (hasSpatial)
			//{
			//    gameClient.SendSpatial(message.Spatial.Position, message.Spatial.Velocity, message.Spatial.Angle);
			//}

			//if (hasEvents)
			//{
			//    gameClient.SendEvents(message.Events);
			//}

			//if (isCombined)
			//{
			//    gameClient.EndCombinedMessage();
			//}

			//index++;
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

		private void comboSelectedFile_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var file = (FileRecording)comboSelectedFile.SelectedItem;

			messages = messageHandler.GetRecordings(file.Path);

			InitializeAvailableClients();
		}

		private void comboSelectedClient_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ButtonPlayback.IsEnabled = true;
		}
	}
}