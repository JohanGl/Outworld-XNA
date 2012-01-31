using System;
using Framework.Core.Common;
using Framework.Gui;
using Game.Entities;
using Game.Entities.Outworld;
using Game.Entities.Outworld.World.SpatialSensor;
using Game.Network.Clients;
using Game.Network.Common;
using Game.World.Terrains.Parts.Areas.Helpers;
using Outworld.Scenes.InGame.Controls.Hud;
using Game.World.Terrains.Rendering.MeshPools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Outworld.Scenes.InGame
{
	public partial class InGameScene
	{
		private GuiManager gui;
		private WeaponBar weaponBar;
		private HealthBar healthBar;
		private RadarLogic radarLogic;
		private Notifications notifications;

		private void InitializeGui()
		{
			gui = new GuiManager(Context.Input, Context.Graphics.Device, Context.Graphics.SpriteBatch);

			// Radar
			radarLogic.Initialize(Context, player, gameClient.ServerEntities);
			gui.Elements.Add(radarLogic.Radar);

			// Weapon
			weaponBar = new WeaponBar();
			weaponBar.Initialize(Context);
			gui.Elements.Add(weaponBar);

			// Health
			healthBar = new HealthBar();
			healthBar.Initialize(Context);
			gui.Elements.Add(healthBar);

			// Notifications
			notifications = new Notifications();
			notifications.Initialize(Context, 10);
			gui.Elements.Add(notifications);

			gui.UpdateLayout();
		}

		private void UpdateGui(GameTime gameTime)
		{
			// Update the healthbar
			if (healthBar.Amount != playerHealth.Health)
			{
				healthBar.Amount = (int)playerHealth.Health;
				healthBar.Percentage = playerHealth.Percentage;
				healthBar.UpdateProgressBar();
			}

			radarLogic.Update(gameTime);

			UpdateNetworkNotifications(gameTime);
		}

		private void UpdateNetworkNotifications(GameTime gameTime)
		{
			if (messageHandler.MessageGroups.ContainsKey(MessageHandlerType.GameClient))
			{
				bool connectedPlayers = false;
				bool disconnectedPlayers = false;

				var messages = messageHandler.GetMessages<NetworkMessage>(MessageHandlerType.GameClient);

				for (int i = 0; i < messages.Count; i++)
				{
					var message = messages[i];

					notifications.AddNotification(message.Text);

					if (message.Type == NetworkMessageType.Connected)
					{
						connectedPlayers = true;
					}
					else if (message.Type == NetworkMessageType.Disconnected)
					{
						disconnectedPlayers = true;
					}
					else if (message.Type == NetworkMessageType.EntityEvent)
					{
						if (message.EntityEventType == EntityEventType.Dead)
						{
							audioHandler.PlaySound("Notification2");
						}
					}
				}

				if (connectedPlayers)
				{
					audioHandler.PlaySound("Notification1");
				}

				if (disconnectedPlayers)
				{
					audioHandler.PlaySound("Notification2");
				}
			}

			notifications.Update(gameTime);
		}

		private void RenderGui()
		{
			gui.Render();
			RenderText();
		}

		private void RenderText()
		{
			// Construct the string
			stringBuilder.Clear();

			stringBuilder.Append("Fps: ");
			stringBuilder.Append((int)Context.Graphics.Fps);
			stringBuilder.Append(Environment.NewLine);

			//stringBuilder.Append("Total Tiles: ");
			//stringBuilder.Append(gameClient.World.TerrainContext.Visibility.Statistics.TotalVisibleTiles);
			//stringBuilder.Append(" (");
			//stringBuilder.Append(gameClient.World.TerrainContext.Visibility.Statistics.TotalTiles);
			//stringBuilder.Append(")");
			//stringBuilder.Append(Environment.NewLine);

			//stringBuilder.Append("Total Faces: ");
			//stringBuilder.Append(gameClient.World.TerrainContext.Visibility.Statistics.TotalVisibleFaces);
			//stringBuilder.Append(" (");
			//stringBuilder.Append(gameClient.World.TerrainContext.Visibility.Statistics.TotalFaces);
			//stringBuilder.Append(")");
			//stringBuilder.Append(Environment.NewLine);

			stringBuilder.Append("Direction: ");
			stringBuilder.Append((int)playerInput.MovementDirection);
			stringBuilder.Append(Environment.NewLine);

			stringBuilder.Append("Position (");
			stringBuilder.Append((int)playerSpatial.RigidBody.Position.X);
			stringBuilder.Append(", ");
			stringBuilder.Append((int)playerSpatial.RigidBody.Position.Y);
			stringBuilder.Append(", ");
			stringBuilder.Append((int)playerSpatial.RigidBody.Position.Z);
			stringBuilder.Append(")");
			stringBuilder.Append(Environment.NewLine);

			stringBuilder.Append("Velocity (");
			stringBuilder.Append((int)playerSpatial.RigidBody.Velocity.X);
			stringBuilder.Append(", ");
			stringBuilder.Append((int)playerSpatial.RigidBody.Velocity.Y);
			stringBuilder.Append(", ");
			stringBuilder.Append((int)playerSpatial.RigidBody.Velocity.Z);
			stringBuilder.Append(")");
			stringBuilder.Append(Environment.NewLine);

			stringBuilder.Append("Area (");
			stringBuilder.Append(playerSpatial.Area.X);
			stringBuilder.Append(", ");
			stringBuilder.Append(playerSpatial.Area.Y);
			stringBuilder.Append(", ");
			stringBuilder.Append(playerSpatial.Area.Z);
			stringBuilder.Append(")");
			stringBuilder.Append(Environment.NewLine);

			//stringBuilder.Append("Total Areas: ");
			//stringBuilder.Append(gameClient.World.TerrainContext.Visibility.Statistics.TotalAreas);
			//stringBuilder.Append(Environment.NewLine);

			//stringBuilder.Append("Total Cached Areas: ");
			//stringBuilder.Append(gameClient.World.TerrainContext.Visibility.Statistics.TotalCachedAreas);
			//stringBuilder.Append(Environment.NewLine);

			//stringBuilder.Append("MeshPool: ");
			//stringBuilder.Append(TerrainMeshPool.Statistics);
			//stringBuilder.Append(Environment.NewLine);

			stringBuilder.Append("Allocated RAM: ");
			stringBuilder.Append((currentProcess.PeakWorkingSet64 / 1024) / 1024);
			stringBuilder.Append(" MB");
			stringBuilder.Append(Environment.NewLine);

			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("Ascending ");
			stringBuilder.Append(playerSpatialSensor.State[SpatialSensorStateType.Ascending].IsActive);
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("Descending ");
			stringBuilder.Append(playerSpatialSensor.State[SpatialSensorStateType.Descending].IsActive);
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("Impact ");
			stringBuilder.Append(playerSpatialSensor.State[SpatialSensorStateType.Impact].IsActive);
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("HorizontalMovement ");
			stringBuilder.Append(playerSpatialSensor.State[SpatialSensorStateType.HorizontalMovement].IsActive);
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("VerticalMovement ");
			stringBuilder.Append(playerSpatialSensor.State[SpatialSensorStateType.VerticalMovement].IsActive);

			Context.Graphics.SpriteBatch.Begin();

			Context.Graphics.SpriteBatch.DrawString(Context.Resources.Fonts["Global.Default"],
													stringBuilder,
													new Vector2(3, 0),
													Color.White,
													0,
													new Vector2(0, 0),
													1,
													SpriteEffects.None,
													0);
			Context.Graphics.SpriteBatch.End();
		}
	}
}