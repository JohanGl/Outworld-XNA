using System;
using System.Collections.Generic;
using Framework.Core.Contexts;
using Framework.Core.Messaging;
using Framework.Core.Services;
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
			
			Radar = new Radar();
			Radar.Initialize(context);
			Radar.Center = playerSpatial.Position;

			messageHandler = ServiceLocator.Get<IMessageHandler>();

			var radarEntityA = new RadarEntity();
			radarEntityA.Opacity = 1.0f;
			radarEntityA.Color = Color.Yellow;
			radarEntityA.Position = new Vector3(-14, 42, -23);
			radarEntityA.Id = 99;

			var radarEntityB = new RadarEntity();
			radarEntityB.Opacity = 1.0f;
			radarEntityB.Color = Color.Red;
			radarEntityB.Position = new Vector3(51, 45, -22);
			radarEntityB.Id = 98;

			var radarEntityC = new RadarEntity();
			radarEntityC.Opacity = 1.0f;
			radarEntityC.Color = Color.LightGreen;
			radarEntityC.Position = new Vector3(94, 38, 41);
			radarEntityC.Id = 97;

			Radar.RadarEntities.Add(radarEntityA);
			Radar.RadarEntities.Add(radarEntityB);
			Radar.RadarEntities.Add(radarEntityC);
		}

		public void Update(GameTime gameTime)
		{
			Radar.Center = playerSpatial.Position;
			Radar.Angle = -playerSpatial.Angle.X + 180;

			var messages = messageHandler.GetMessages<NetworkMessage>("GameClient");
			
			Radar.Update(gameTime);
		}
	}
}