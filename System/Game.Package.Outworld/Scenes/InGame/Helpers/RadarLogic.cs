using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Framework.Core.Contexts;
using Framework.Gui;
using Game.Entities.Outworld;
using Game.Entities.Outworld.World;
using Game.Network.Clients;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Outworld.Scenes.InGame.Controls.Hud
{
	public class RadarLogic
	{
		private Entity player;
		private List<ServerEntity> clients;

		private float radarDetectionRange = 50.0f;
		public Radar Radar;

		public void Initialize(GameContext context, Entity player, List<ServerEntity> clients)
		{
			this.player = player;
			this.clients = clients;
			
			Radar = new Radar();
			Radar.Initialize(context);
			
			for(int i = 0; i < clients.Count; i++)
			{
				RadarEntity radarEntity = new RadarEntity();
				radarEntity.Color = RadarEntity.RadarEntityColor.Yellow;
				radarEntity.Position = clients[i].Position;
				radarEntity.Id = i;

				Radar.RadarEnteties.Add(radarEntity);
			}
		}

		public void Update()
		{
			if(Radar.RadarEnteties.Count == 0)
			{
				for (int i = 0; i < clients.Count; i++)
				{
					RadarEntity radarEntity = new RadarEntity();
					radarEntity.Color = RadarEntity.RadarEntityColor.Yellow;
					radarEntity.Position = clients[i].Position;
					radarEntity.Id = i;

					Radar.RadarEnteties.Add(radarEntity);
				}
			}

			// Update network-players positions
			for(int c = 0; c < clients.Count; c++)
			{
				if(Radar.RadarEnteties[c].Id == c)
				{
					Radar.RadarEnteties[c].Position = clients[c].Position;
				}
			}
			
			
			Vector3 position = player.Components.Get<SpatialComponent>().Position;

			for (int i = 0; i < Radar.RadarEnteties.Count; i++)
			{
				var radarEntity = Radar.RadarEnteties[i];

				//Vector2 diffVector = new Vector2(radarEntity.Position.X - position.X, radarEntity.Position.Y - position.Y);
				Vector3 diffVector = position - radarEntity.Position;

				float distance = diffVector.Length();

				if (distance <= radarDetectionRange)
				{
					Matrix rotateMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(player.Components.Get<SpatialComponent>().Angle.X));
					diffVector = Vector3.Transform(diffVector, rotateMatrix);
					
					diffVector *= ((Radar.Width * 0.5f) / radarDetectionRange);
					
					diffVector += Radar.Center;
					
					radarEntity.Position = diffVector;

					//diffVector *= ((Radar.Width * 0.5f) / radarDetectionRange);
					//diffVector += Radar.Center;
					//radarEntity.Position = diffVector;
				}
			}
		}
	}
}