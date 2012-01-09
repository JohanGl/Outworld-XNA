using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Framework.Gui
{
	public struct TextBoxInfo
	{
		public int MaxLength;
		public SpriteFont SpriteFont;
		public Texture2D Background;
		public List<int> CharacterMask;
	}
}