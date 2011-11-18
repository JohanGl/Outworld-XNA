using Microsoft.Xna.Framework.Graphics;

namespace Game.World.Terrains.Rendering.Contexts
{
	public class RenderStateContext
	{
		public RasterizerState RasterizerState = new RasterizerState()
		{
			CullMode = CullMode.CullCounterClockwiseFace
		};

		public DepthStencilState DepthStencilState = new DepthStencilState()
		{
			DepthBufferEnable = true
		};

		public SamplerState SamplerState = new SamplerState()
		{
			AddressU = TextureAddressMode.Wrap,
			AddressV = TextureAddressMode.Wrap,
			AddressW = TextureAddressMode.Wrap,
			Filter = TextureFilter.Anisotropic
			//Filter = TextureFilter.LinearMipPoint
		};

		public void SetRenderStates(GraphicsDevice device)
		{
			// Set render states
			device.RasterizerState = RasterizerState;
			device.DepthStencilState = DepthStencilState;
			device.SamplerStates[0] = SamplerState;
			device.BlendState = BlendState.Opaque;
		}

		/// <summary>
		/// Clears all content within the context
		/// </summary>
		public void Clear()
		{
			RasterizerState = null;
			DepthStencilState = null;
			SamplerState = null;
		}
	}
}