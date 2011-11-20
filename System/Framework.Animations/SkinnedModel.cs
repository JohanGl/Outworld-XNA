using System;
using Framework.Animations.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Framework.Animations
{
	public class SkinnedModel
	{
		private Model model;
		private SkinningData skinningData;
		private AnimationPlayer animationPlayer;
		private AnimationClip animationClip;

		public float Scale { get; set; }

		public SkinnedModel()
		{
			Scale = 1f;
		}

		public void Initialize(Model model)
		{
			this.model = model;

			// Look up our custom skinning information.
			skinningData = model.Tag as SkinningData;

			if (skinningData == null)
			{
				throw new InvalidOperationException("This model does not contain a SkinningData tag.");
			}

			// Create an animation player, and start decoding an animation clip.
			animationPlayer = new AnimationPlayer(skinningData);
		}

		public void SetAnimationClip(string name)
		{
			animationClip = skinningData.AnimationClips[name];
			animationPlayer.StartClip(animationClip);
		}

		public void Update(GameTime gameTime)
		{
			//TimeSpan slowmotion = TimeSpan.FromMilliseconds(gameTime.ElapsedGameTime.TotalMilliseconds * 0.5f);
			//animationPlayer.Update(slowmotion, true, Matrix.Identity);

			animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
		}

		public void Render(Matrix view, Matrix projection, Vector3 position, float angle)
		{
			Matrix[] bones = animationPlayer.GetSkinTransforms();

			// Render the skinned mesh.
			foreach (ModelMesh mesh in model.Meshes)
			{
				foreach (SkinnedEffect effect in mesh.Effects)
				{
					effect.SetBoneTransforms(bones);

					effect.World = Matrix.Identity * Matrix.CreateScale(Scale) * Matrix.CreateRotationY(MathHelper.ToRadians(-angle)) * Matrix.CreateTranslation(position);
					effect.View = view;
					effect.Projection = projection;

					effect.EnableDefaultLighting();

					effect.SpecularColor = new Vector3(0.25f);
					effect.SpecularPower = 16;
				}

				mesh.Draw();
			}
		}
	}
}