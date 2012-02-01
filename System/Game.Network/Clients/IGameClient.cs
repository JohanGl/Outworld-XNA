using System;
using System.Collections.Generic;
using Game.Network.Clients.Events;
using Game.Network.Clients.Settings;
using Game.Network.Common;
using Game.World;
using Microsoft.Xna.Framework;

namespace Game.Network.Clients
{
	public interface IGameClient
	{
		event EventHandler<GameSettingsEventArgs> GetGameSettingsCompleted;
		event EventHandler<ClientSpatialEventArgs> GetEntitySpatialCompleted;
		event EventHandler<ClientEventsEventArgs> GetEntityEventsCompleted;

		ushort ClientId { get; }
		bool IsConnected { get; }
		float TimeStamp { get; }
		WorldContext World { get; set; }
		List<ServerEntity> ServerEntities { get; set; }

		void Initialize(GameClientSettings settings);
		void Connect();
		void Disconnect(string message = null);
		void Update(GameTime gameTime);

		void GetGameSettings();
		void SendClientSpatial(Vector3 position, Vector3 velocity, Vector3 angle);
		void SendClientEvents(List<EntityEvent> events);

		void BeginCombinedMessage();
		void EndCombinedMessage();
	}
}