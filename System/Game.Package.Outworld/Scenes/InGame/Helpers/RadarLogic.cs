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

		private float radarDetectionRange = 50.0f;
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
			radarEntity.Position = new Vector3(playerSpatial.Position.X + 8, playerSpatial.Position.Y + 8, playerSpatial.Position.Z);
			radarEntity.Id = 1;

			Radar.RadarEntities.Add(radarEntity);
		}

		public void Update()
		{
			//if(Radar.RadarEntities.Count == 0)
			//{
			//    for (int i = 0; i < clients.Count; i++)
			//    {
			//        var radarEntity = new RadarEntity();
			//        radarEntity.Color = RadarEntity.RadarEntityColor.Yellow;
			//        radarEntity.Position = clients[i].Position;
			//        radarEntity.Id = i;

			//        Radar.RadarEntities.Add(radarEntity);
			//    }
			//}

			//// Update network-players positions
			//for(int c = 0; c < clients.Count; c++)
			//{
			//    if(Radar.RadarEntities[c].Id == c)
			//    {
			//        Radar.RadarEntities[c].Position = clients[c].Position;
			//    }
			//}

			Radar.Center = playerSpatial.Position;
			Radar.Angle = playerSpatial.Angle.X;

			for (int i = 0; i < Radar.RadarEntities.Count; i++)
			{
				var radarEntity = Radar.RadarEntities[i];

				//Vector2 diffVector = new Vector2(radarEntity.Position.X - position.X, radarEntity.Position.Y - position.Y);
				//Vector3 diffVector = position - radarEntity.Position;

				//float distance = diffVector.Length();

				//if (distance <= radarDetectionRange)
				//{
				//    Matrix rotateMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(playerSpatial.Angle.X));
				//    diffVector = Vector3.Transform(diffVector, rotateMatrix);
					
				//    diffVector *= ((Radar.Width * 0.5f) / radarDetectionRange);
					
				//    diffVector += Radar.Center;
					
				//    radarEntity.Position = diffVector;

				//    System.Diagnostics.Debug.WriteLine(string.Format("RE: {0}, {1}, {2}", radarEntity.Position.X, radarEntity.Position.Y, radarEntity.Position.Z));
				//}
			}
		}
	}
}