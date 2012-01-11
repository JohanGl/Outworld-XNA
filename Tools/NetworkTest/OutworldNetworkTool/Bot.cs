using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Network.Clients;
using Game.Network.Clients.Settings;
using Microsoft.Xna.Framework;

namespace OutworldNetworkTool
{
	class Bot
	{
		public IGameClient Client;

		public List<ClientSendItem> ClientSendItems;
//		private List<ServerSendItem> serverSendItems;

		// Bots current values
		public string Time;
		public Vector3 Position;
		public Vector3 Velocity;
		public Vector3 Angle;

		public Bot(string serverAddress)
		{
			ClientSendItems = new List<ClientSendItem>();

			Client = CreateGameClient(serverAddress);

			Time = DateTime.Now.ToString();
			Position = new Vector3(23, 23, 3);
			Velocity = new Vector3(0, 0, 0);
			Angle = new Vector3(0, 0, 0);
		}

		public void StoreClientSentData()
		{
			ClientSendItem clientSendItem = new ClientSendItem();
			clientSendItem.Time = DateTime.Now.ToString();
			clientSendItem.Position = Position;
			clientSendItem.Velocity = Velocity;
			clientSendItem.Angle = Angle;

			ClientSendItems.Add(clientSendItem);
		}

		private IGameClient CreateGameClient(string serverAddress)
		{
			// Initialize the settings for the client
			var clientSettings = new GameClientSettings
			{
				ServerAddress = serverAddress,
				ServerPort = 14242
			};

			// Create, initialize and return the client
			var client = new GameClient();
			client.Initialize(clientSettings);

			return client;
		}
	}
}
