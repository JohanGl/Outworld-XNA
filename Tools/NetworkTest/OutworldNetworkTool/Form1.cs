using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows.Forms;
using Game.Network.Clients;
using Game.Network.Clients.Events;
using Game.Network.Clients.Settings;
using Microsoft.Xna.Framework;
using Timer = System.Timers.Timer;
using System.Linq;

namespace OutworldNetworkTool
{
	public partial class Form1 : Form
	{
		private Timer timerReportSpatialData;
		private Timer timerUpdateClients;

		private List<Bot> bots;
		private Bot selectedBot;

		private int spatialUpdateCounter;

		public Form1()
		{
			InitializeComponent();

			bots = new List<Bot>();
		}

		private void TimerClientsUpdateElapsed(object sender, ElapsedEventArgs e)
		{
			lock (bots)
			{
				Console.WriteLine(bots.Count.ToString());

				for (int i = 0; i < bots.Count; i++)
				{
					var bot = bots[i];
					bot.Client.Update(null);
				}
			}
		}

		// Send data to server
		private void TimerReportSpatialDataElapsed(object sender, ElapsedEventArgs e)
		{
			lock (bots)
			{
				Console.WriteLine(bots.Count.ToString());

				// Sends our current position to the server
				for (int i = 0; i < bots.Count; i++)
				{
					var bot = bots[i];

					//float radian = (float)Math.PI / 180f;
					//float radius = 20;
					//bot.Position.X = radius * (float)Math.Sin(radian * bot.Angle.X);
					//bot.Position.Z = radius * (float)Math.Cos(radian * bot.Angle.X);
					//bot.Angle.X += 5.0f;

					bot.Client.SendClientSpatial(bot.Position, bot.Velocity, bot.Angle);

					bot.StoreClientSentData();

					if (selectedBot == bot)
					{
						ListViewItem lvi = new ListViewItem();
						lvi.Text = DateTime.Now.ToString();

						ListViewItem.ListViewSubItem lviSub1 = new ListViewItem.ListViewSubItem();
						lviSub1.Text = String.Format("{0}, {1}, {2}",
													 bot.Position.ToString(),
													 bot.Velocity.ToString(),
													 bot.Angle.ToString());

						lvi.SubItems.Add(lviSub1);

						if (listView3.InvokeRequired)
						{
							listView3.BeginInvoke(new MethodInvoker(
													() => listView3.Items.Add(lvi)));
						}
						else
						{
							listView3.Items.Add(lvi);
						}

						break;
					}
				}
			}
		}

		// Connect a bot
		private void button1_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < numericUpDown1.Value; i++)
			{
				Bot bot = AddBot();

				bot.Client.GetClientSpatialCompleted += gameClient_GetClientSpatialCompleted;
				bot.Client.Connect();
				bot.Client.GetGameSettingsCompleted += client_GetGameSettingsCompleted;
				bot.Client.GetGameSettings();
			}

			// Initialize the client update interval
			timerReportSpatialData = new Timer((double)numericUpDown2.Value);

			if (bots.Count == 1)
			{
				timerReportSpatialData.Start();
				timerReportSpatialData.Elapsed += TimerReportSpatialDataElapsed;
			}

			timerUpdateClients = new Timer(50);
			timerUpdateClients.Start();
			timerUpdateClients.Elapsed += TimerClientsUpdateElapsed;
		}

		private Bot AddBot()
		{
			Bot bot = new Bot(textBox2.Text);
			bots.Add(bot);

			ListViewItem lvItem = new ListViewItem(textBox1.Text + " " + listView2.Items.Count.ToString());
			lvItem.Tag = bot;
			listView2.Items.Add(lvItem);
			listView2.Update();

			return bot;
		}

		private void client_GetGameSettingsCompleted(object sender, GameSettingsEventArgs e)
		{
		}

		// Get data from server
		private void gameClient_GetClientSpatialCompleted(object sender, ClientSpatialEventArgs e)
		{
			if (spatialUpdateCounter++ < 100)
			{
				return;
			}

			for (int i = 0; i < e.ClientData.Length; i++)
			{
				var clientData = e.ClientData[i];

				var client = sender as GameClient;

				for (int j = 0; j < client.ServerEntities.Count; j++)
				{
					if (client.ServerEntities[j].Id == clientData.ClientId)
					{
						ListViewItem lvi = new ListViewItem();
						lvi.Text = DateTime.Now.ToString();

						ListViewItem.ListViewSubItem lviSub1 = new ListViewItem.ListViewSubItem();
						lviSub1.Text = String.Format("{0}, {1}, {2}",
													 clientData.Position.ToString(),
													 clientData.Velocity.ToString(),
													 clientData.Angle.ToString());

						lvi.SubItems.Add(lviSub1);

						if (listView1.InvokeRequired)
						{
							listView1.BeginInvoke(new MethodInvoker(
													() => listView1.Items.Add(lvi)));
						}
						else
						{
							listView1.Items.Add(lvi);
						}


						break;
					}
				}
			}

			spatialUpdateCounter = 0;
		}

		private void listView2_SelectedIndexChanged(object sender, EventArgs e)
		{
			listView3.Items.Clear();

			if (listView2.SelectedItems.Count == 1)
			{
				selectedBot = (listView2.SelectedItems[0].Tag as Bot);

				lock (selectedBot)
				{
					for (int i = 0; i < selectedBot.ClientSendItems.Count; i++)
					{
						ClientSendItem history = selectedBot.ClientSendItems[i];

						ListViewItem lvItem = new ListViewItem();

						lvItem.Text = history.Time;

						ListViewItem.ListViewSubItem lvItemSub = new ListViewItem.ListViewSubItem();
						lvItemSub.Text = String.Format("{0}, {1}, {2}",
						                               history.Position.ToString(),
						                               history.Velocity.ToString(),
						                               history.Angle.ToString());

						lvItem.SubItems.Add(lvItemSub);

						listView3.Items.Add(lvItem);
					}
				}
			}
			else
			{
				selectedBot = null;
				
			}

			listView3.Update();
		}

		// Disconnect selected bot
		private void button2_Click(object sender, EventArgs e)
		{
			lock (bots)
			{
				Console.WriteLine(bots.Count.ToString());

				Bot bot;

				// Sends our current position to the server
				for (int i = 0; i < bots.Count; i++)
				{
					bot = bots[i];

//					bot.Client.SendClientSpatial(bot.Position, bot.Velocity, bot.Angle);
//					bot.StoreClientSentData();

					if (selectedBot == bot)
					{
						bot.Client.Disconnect(bot.Client.ClientId.ToString());

						bots.Remove(bot);

						ListViewItem lvi = new ListViewItem();
						lvi.Text = DateTime.Now.ToString();

						ListViewItem.ListViewSubItem lviSub1 = new ListViewItem.ListViewSubItem();
						lviSub1.Text = String.Format("Bot disconnected");

						lvi.SubItems.Add(lviSub1);

						if (listView3.InvokeRequired)
						{
							listView3.BeginInvoke(new MethodInvoker(
													() => listView3.Items.Add(lvi)));
						}
						else
						{
							listView3.Items.Add(lvi);
						}

						break;
					}
				}
			}
		}
	}
}
