using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Framework.Network.Messages;

namespace NetworkTool
{
	public class MessagePlaybackHandler
	{
		private GameClient gameClient;
		private DispatcherTimer tickrateTimer;
		private List<Message> messages;
		private int messageIndex;

		public MessagePlaybackHandler()
		{
			gameClient = new GameClient();
			gameClient.Connect();

			tickrateTimer = new DispatcherTimer();
			tickrateTimer.Interval = TimeSpan.FromMilliseconds(1000 / 20);
			tickrateTimer.Tick += TickrateTimer_Tick;
		}

		public void Play(long clientId, List<Message> messages)
		{
			this.messages = messages.Where(p => p.ClientId == clientId).ToList();

			messageIndex = 0;
			tickrateTimer.Start();

			System.Diagnostics.Debug.WriteLine("Recording started");
		}

		public void Stop()
		{
			tickrateTimer.Stop();
			messageIndex = 0;

			System.Diagnostics.Debug.WriteLine("Recording stopped");
		}

		private void TickrateTimer_Tick(object sender, EventArgs eventArgs)
		{
			if (messageIndex >= messages.Count)
			{
				Stop();
				return;
			}

			var message = messages[messageIndex];

			if (message.Type == MessageType.Data)
			{
				gameClient.SendMessage(message);
			}
			else
			{
				//if (message.Type == MessageType.Connect)
				//{
				//    gameClient.Connect();
				//}
				//else
				//{
				//    gameClient.Disconnect();
				//}
			}

			System.Diagnostics.Debug.WriteLine(string.Format("Sending message: {0} / {1}", messageIndex + 1, messages.Count));

			messageIndex++;
		}
	}
}