using System.Collections.Generic;
using Framework.Network.Messages;
using Game.Network.Common;
using Microsoft.Xna.Framework;

namespace Game.Network.Servers.Helpers
{
	public class ServerEntityHelper
	{
		public Dictionary<ushort, ServerEntity> Entities;

		public ServerEntityHelper()
		{
			Entities = new Dictionary<ushort, ServerEntity>();
		}

		public void Clear()
		{
			Entities.Clear();
		}

		public bool IsWithinViewDistance(ServerEntity a, ServerEntity b)
		{
			// TODO: Adjust this based on entity type
			float distance = 75;

			return Vector3.Distance(a.CurrentSpatial.Position, b.CurrentSpatial.Position) < distance;
		}

		public List<ServerEntity> GetClientsWithinViewDistance(ServerEntity client, List<ServerEntity> clients)
		{
			var otherClients = new List<ServerEntity>();

			for (ushort i = 0; i < clients.Count; i++)
			{
				var otherClient = clients[i];

				if (otherClient.Id != client.Id)
				{
					if (IsWithinViewDistance(client, otherClient))
					{
						otherClients.Add(otherClient);
					}
				}
			}

			return otherClients;
		}

		public List<EntityEvent> GetRecentEvents(ServerEntity fromEntity, float currentTimeStamp, float withinSeconds = 2f)
		{
			var result = new List<EntityEvent>();

			// Subtract the clients local time to get an accurate measurement
			currentTimeStamp += fromEntity.RemoteTimeOffset;

			if (fromEntity.Events.Count > 0)
			{
				// Traverse backwards in time since the last events are the most recent
				for (int i = fromEntity.Events.Count - 1; i >= 0; i--)
				{
					// Get all events within the specified timeframe
					if (fromEntity.Events[i].TimeStamp > currentTimeStamp - withinSeconds)
					{
						result.Add(fromEntity.Events[i]);
						//System.Diagnostics.Debug.WriteLine(string.Format("Sending: TimeStamp: {0}, Type: {1}, Server: {2}, Server -2: {3}", Events[i].TimeStamp, Events[i].Type, currentTimeStamp, currentTimeStamp - seconds));
					}
					// No more events within the timespan
					else
					{
						//System.Diagnostics.Debug.WriteLine(string.Format("Failed event: TimeStamp: {0}, Type: {1}, Server: {2}", Events[i].TimeStamp, Events[i].Type, currentTimeStamp - seconds));
						break;
					}
				}
			}

			return result;
		}

		public void UpdateTimeout(GameTime gameTime, ushort entityId)
		{
			if (Entities.ContainsKey(entityId))
			{
				Entities[entityId].Timeout = gameTime.TotalGameTime.Seconds;
			}
		}

		public List<ServerEntity> GetEntitiesOfType(EntityType type)
		{
			var result = new List<ServerEntity>();

			foreach (var entity in Entities)
			{
				if (entity.Value.Type == type)
				{
					result.Add(entity.Value);
				}
			}

			return result;
		}

		public void AddSpatial(ushort id, EntitySpatial spatial)
		{
			Entities[id].SpatialData.Add(spatial);

			// Keep a maximum of 100 entries
			if (Entities[id].SpatialData.Count > 100)
			{
				Entities[id].SpatialData.RemoveAt(0);
			}
		}

		public void AddEvent(ushort id, EntityEvent entityEvent)
		{
			Entities[id].Events.Add(entityEvent);

			// Keep a maximum of 100 entries
			if (Entities[id].Events.Count > 100)
			{
				Entities[id].Events.RemoveAt(0);
			}
		}
	}
}