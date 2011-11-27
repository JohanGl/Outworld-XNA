﻿using System;
using System.Text;
using Framework.Core.Contexts;
using Framework.Core.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Outworld.Scenes.Debug.Models.Animations;
using Graphics = Microsoft.Xna.Framework.Graphics;

namespace Outworld.Scenes.Debug.Models
{
	public class ModelScene : SceneBase
	{
		private StringBuilder stringBuilder;
		private Vector3 Position = Vector3.Zero;
		private float angle;

		Microsoft.Xna.Framework.Graphics.Model currentModel;
		AnimationPlayer animationPlayer;

		public override void Initialize(GameContext context)
		{
			base.Initialize(context);

			stringBuilder = new StringBuilder(100, 500);
		}

		public override void LoadContent()
		{
			var content = Context.Resources.Content;
			Context.Resources.Models.Add("weapon", content.Load<Graphics.Model>(@"Models\Weapons\MyGun02"));
		}

		public override void UnloadContent()
		{
			Context.Resources.Models.Remove("weapon");
		}

		public override void Update(GameTime gameTime)
		{
			angle += 30f * (float)gameTime.ElapsedGameTime.TotalSeconds;
		}

		public override void Render(GameTime gameTime)
		{
			DrawModel(Context.Resources.Models["weapon"]);

			//stringBuilder.Clear(); 
			//stringBuilder.Append("Models");

			//Context.Graphics.Device.Clear(Color.Purple);

			//Context.Graphics.SpriteBatch.Begin();

			//Context.Graphics.SpriteBatch.DrawString(Context.Resources.Fonts["Global.Default"],
			//                                        stringBuilder,
			//                                        new Vector2(3, 0),
			//                                        Color.White,
			//                                        0,
			//                                        new Vector2(0, 0),
			//                                        1,
			//                                        SpriteEffects.None,
			//                                        0);
			//Context.Graphics.SpriteBatch.End();
		}

		private void DrawModel(Graphics.Model m)
		{
			Matrix[] transforms = new Matrix[m.Bones.Count];
			float aspectRatio = Context.Graphics.Device.Viewport.AspectRatio;
			m.CopyAbsoluteBoneTransformsTo(transforms);
			Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
			Matrix view = Matrix.CreateLookAt(new Vector3(0.0f, 2.0f, 10f), Vector3.Zero, Vector3.Up);

			var state = new RasterizerState();
			state.CullMode = CullMode.None;
			Context.Graphics.Device.RasterizerState = state;

			foreach (ModelMesh mesh in m.Meshes)
			{
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.EnableDefaultLighting();

					effect.View = view;
					effect.Projection = projection;
					effect.World = Matrix.Identity * transforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(Position) * Matrix.CreateRotationY(MathHelper.ToRadians(angle));
				}

				mesh.Draw();
			}
		}
	}
}