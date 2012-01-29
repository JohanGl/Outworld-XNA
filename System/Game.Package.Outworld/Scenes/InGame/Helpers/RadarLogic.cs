using System;
using System.Collections.Generic;
using Framework.Core.Contexts;
using Framework.Core.Messaging;
using Framework.Core.Services;
using Game.Entities;
using Game.Entities.Outworld;
using Game.Entities.Outworld.World;
using Game.Network.Clients;
using Microsoft.Xna.Framework;

namespace Outworld.Scenes.InGame.Controls.Hud
{
	public class RadarLogic
	{
		public Radar Radar;

		private Entity player;
		private List<ServerEntity> clients;

		private SpatialComponent playerSpatial;

		private IMessageHandler messageHandler;

		public void Initialize(GameContext context, Entity player, List<ServerEntity> clients)
		{
			this.player = player;
			this.clients = clients;

			playerSpatial = player.Components.Get<SpatialComponent>();
			
			Radar = new Radar(84.0f, 12.0f);
			Radar.Initialize(context);
			Radar.Center = playerSpatial.Position;

			messageHandler = ServiceLocator.Get<IMessageHandler>();

			for(int i = 0; i < clients.Count; i++)
			{
				var client = clients[i];

				var radarEntityA = new RadarEntity();
				radarEntityA.Opacity = 1.0f;
				radarEntityA.Color = Color.LightGreen;
				radarEntityA.Position = client.Position;
				radarEntityA.Id = client.Id;

				Radar.RadarEntities.Add(radarEntityA);
			}

			//var radarEntityA = new RadarEntity();
			//radarEntityA.Opacity = 1.0f;
			//radarEntityA.Color = Color.Yellow;
			//radarEntityA.Position = new Vector3(-14, 42, -23);
			//radarEntityA.Id = 99;

			//var radarEntityB = new RadarEntity();
			//radarEntityB.Opacity = 1.0f;
			//radarEntityB.Color = Color.Red;
			//radarEntityB.Position = new Vector3(51, 45, -22);
			//radarEntityB.Id = 98;

			//var radarEntityC = new RadarEntity();
			//radarEntityC.Opacity = 1.0f;
			//radarEntityC.Color = Color.LightGreen;
			//radarEntityC.Position = new Vector3(120, 38, 65);
			//radarEntityC.Id = 97;

			//Radar.RadarEntities.Add(radarEntityA);
			//Radar.RadarEntities.Add(radarEntityB);
			//Radar.RadarEntities.Add(radarEntityC);
		}

		public void Update(GameTime gameTime)
		{
			Radar.Center = playerSpatial.Position;
			Radar.Angle = -playerSpatial.Angle.X + 180;

			var messages = messageHandler.GetMessages<NetworkMessage>(MessageHandlerType.GameClient);

			for (int i = 0; i < messages.Count; i++ )
			{
				var message = messages[i];

				if (message.Type == NetworkMessage.MessageType.Disconnected)
				{
					for(int j = 0; j < Radar.RadarEntities.Count; j++)
					{
						var radarEntity = Radar.RadarEntities[j];

						if (radarEntity.Id == message.ClientId)
						{
							Radar.RadarEntities.Remove(radarEntity);
							break;
						}
					}
				}
				else if(message.Type == NetworkMessage.MessageType.Connected)
				{
					for (int j = 0; j < clients.Count; j++)
					{
						var client = clients[j];

						if (client.Id == message.ClientId)
						{
							if (!containsRadarEntityId(client.Id))
							{
								var radarEntity = new RadarEntity();
								radarEntity.Id = message.ClientId;
								radarEntity.Opacity = 1.0f;
								radarEntity.Color = Color.LightGreen;
								radarEntity.Position = client.Position;

								Radar.RadarEntities.Add(radarEntity);
							}
						}
					}
				}
			}

			for (int i = 0; i < Radar.RadarEntities.Count; i++)
			{
				var radarEntity = Radar.RadarEntities[i];
				
				for (int j = 0; j < clients.Count; j++)
				{
					if (radarEntity.Id == clients[j].Id)
					{
						radarEntity.Position = clients[j].Position;
					}
				}
			}

			Radar.Update(gameTime);
		}

		private bool containsRadarEntityId(int id)
		{
			for (int i = 0; i < Radar.RadarEntities.Count; i++)
			{
				var radarEntity = Radar.RadarEntities[i];

				if (id == radarEntity.Id)
				{
					return true;
				}
			}

			return false;
		}
	}
}