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

			var radarEntity = new RadarEntity();
			radarEntity.Color = RadarEntity.RadarEntityColor.Yellow;
			radarEntity.Position = new Vector3(5, 42, 1);//playerSpatial.Position.X + 8, playerSpatial.Position.Y + 8, playerSpatial.Position.Z);
			radarEntity.Id = 1;

			Radar.RadarEntities.Add(radarEntity);
		}

		public void Update()
		{
			//for(int i = 0; i < clients.Count; i++)
			//{
			//    var radarEntity = new RadarEntity();
			//    radarEntity.Color = RadarEntity.RadarEntityColor.Yellow;
			//    radarEntity.Position = clients[i].Position;
			//    radarEntity.Id = i;

			//    Radar.RadarEntities.Add(radarEntity);
			//}


			Radar.RadarEntities.Clear();
			for (int i = 0; i < clients.Count; i++)
			{
				var radarEntity = new RadarEntity();
				radarEntity.Color = RadarEntity.RadarEntityColor.Yellow;
				radarEntity.Position = clients[i].Position;
				radarEntity.Id = i;

				Radar.RadarEntities.Add(radarEntity);
			}
			

			Radar.Center = playerSpatial.Position;
			Radar.Angle = -playerSpatial.Angle.X + 180;
		}
	}
}