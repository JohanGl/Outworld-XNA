using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Game.Network.Common;

namespace NetworkTool
{
	public partial class ClientMainWindow
	{
		private MessageHandler messageHandler;
		private List<RecordedMessage> messages;

		private ClientPlaybackHandler playbackHandler;

		public ClientMainWindow()
		{
			InitializeComponent();

			messageHandler = new MessageHandler();
			messages = new List<RecordedMessage>();

			playbackHandler = new ClientPlaybackHandler();

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

		private void ButtonPlayback_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			playbackHandler.Play(messages);
		}

		private void comboSelectedFile_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var file = (FileRecording)comboSelectedFile.SelectedItem;
			messages = messageHandler.GetClientRecordings(file.Path);
			ButtonPlayback.IsEnabled = true;
		}
	}
}