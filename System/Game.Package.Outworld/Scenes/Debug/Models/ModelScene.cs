using System.Text;
using Framework.Animations;
using Framework.Core.Contexts;
using Framework.Core.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Outworld.Scenes.Debug.Models
{
	public class ModelScene : SceneBase
	{
		private Vector3 Position = Vector3.Zero;
		private float angle;
		private SkinnedModel skinnedModel;

		public override void Initialize(GameContext context)
		{
			base.Initialize(context);
		}

		public override void LoadContent()
		{
			var content = Context.Resources.Content;
			Context.Resources.Models.Add("weapon", content.Load<Model>(@"Models\Weapons\Pistol01"));
			Context.Resources.Models.Add("Chibi", content.Load<Model>(@"Models\Characters\Chibi\Chibi"));

			skinnedModel = new SkinnedModel();
			skinnedModel.Initialize(Context.Resources.Models["Chibi"]);
			skinnedModel.SetAnimationClip("Idle");
			//skinnedModel.SetAnimationClip("Run");
			//skinnedModel.SetAnimationClip("Take 001");
			//skinnedModel.SetAnimationClip("Default Take");
		}

		public override void UnloadContent()
		{
			Context.Resources.Models.Remove("weapon");
			Context.Resources.Models.Remove("Chibi");
		}

		public override void Update(GameTime gameTime)
		{
			angle = 90;
			//angle += 60f * (float)gameTime.ElapsedGameTime.TotalSeconds;

			skinnedModel.Update(gameTime);
		}

		public override void Render(GameTime gameTime)
		{
			DrawSkinnedModel(skinnedModel);

			//DrawModel(Context.Resources.Models["Chibi"]);
		}

		private void DrawSkinnedModel(SkinnedModel skinnedModel)
		{
			float aspectRatio = Context.Graphics.Device.Viewport.AspectRatio;
			Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
			Matrix view = Matrix.CreateLookAt(new Vector3(0.0f, 1.0f, 6f), Vector3.Zero, Vector3.Up);

			skinnedModel.Render(view, projection, new Vector3(0), angle);
		}

		private void DrawModel(Model m)
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
					effect.DiffuseColor = new Vector3(1, 1, 1);

					effect.View = view;
					effect.Projection = projection;
					effect.World = Matrix.Identity * transforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(Position) * Matrix.CreateRotationY(MathHelper.ToRadians(angle));
				}

				mesh.Draw();
			}
		}
	}
}