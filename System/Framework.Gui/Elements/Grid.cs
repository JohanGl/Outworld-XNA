using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Framework.Gui
{
	public class Grid : UIElement
	{
		public List<ColumnDefinition> ColumnDefinitions;
		public List<RowDefinition> RowDefinitions;

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
		}
	}

	public class ColumnDefinition
	{
		public GridLength Width;
	}

	public class RowDefinition
	{
		public GridLength Height;
	}

	public class GridLength
	{
		public GridUnitType GridUnitType;
		public double Value;
	}

	public enum GridUnitType
	{
		// The size is determined by the size properties of the content object
		Auto,

		// The value is expressed as a pixel
		Pixel,
	
		// The value is expressed as a weighted proportion of available space
		Star
	}
}