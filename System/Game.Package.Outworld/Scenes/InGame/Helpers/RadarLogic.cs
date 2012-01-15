using System;
using System.Collections.Generic;
using Framework.Core.Contexts;
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

		public void Initialize(GameContext context, Entity player, List<ServerEntity> clients)
		{
			this.player = player;
			this.clients = clients;

			playerSpatial = player.Components.Get<SpatialComponent>();
			
			Radar = new Radar();
			Radar.Initialize(context);
			Radar.Center = playerSpatial.Position;

			//for(int i = 0; i < clients.Count; i++)
			//{
			//    var radarEntity = new RadarEntity();
			//    radarEntity.Color = RadarEntity.RadarEntityColor.Yellow;
			//    radarEntity.Position = clients[i].Position;
			//    radarEntity.Id = i;

			//    Radar.RadarEntities.Add(radarEntity);
			//}

			var radarEntityA = new RadarEntity();
			radarEntityA.Opacity = 1.0f;
			radarEntityA.Color = RadarEntity.RadarEntityColor.Yellow;
			radarEntityA.Position = new Vector3(-14, 42, -23);
			radarEntityA.Id = 99;

			var radarEntityB = new RadarEntity();
			radarEntityB.Opacity = 1.0f;
			radarEntityB.Color = RadarEntity.RadarEntityColor.Red;
			radarEntityB.Position = new Vector3(51, 45, -22);
			radarEntityB.Id = 98;

			var radarEntityC = new RadarEntity();
			radarEntityC.Opacity = 1.0f;
			radarEntityC.Color = RadarEntity.RadarEntityColor.Green;
			radarEntityC.Position = new Vector3(94, 38, 41);
			radarEntityC.Id = 97;

			Radar.RadarEntities.Add(radarEntityA);
			Radar.RadarEntities.Add(radarEntityB);
			Radar.RadarEntities.Add(radarEntityC);
		}

		public void Update(GameTime gameTime)
		{
			//Radar.RadarEntities.Clear();
			//for (int i = 0; i < clients.Count; i++)
			//{
			//    var radarEntity = new RadarEntity();
			//    radarEntity.Opacity = 1.0f;
			//    radarEntity.Color = RadarEntity.RadarEntityColor.Yellow;
			//    radarEntity.Position = clients[i].Position;
			//    radarEntity.Id = i;

			//    Radar.RadarEntities.Add(radarEntity);
			//}
			
			Radar.Center = playerSpatial.Position;
			Radar.Angle = -playerSpatial.Angle.X + 180;

			Radar.Update(gameTime);
		}
	}
}