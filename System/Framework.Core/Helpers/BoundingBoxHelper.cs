using Microsoft.Xna.Framework;

namespace Framework.Core.Helpers
{
	public class BoundingBoxHelper
	{
		public Vector3 GetCenter(BoundingBox boundingBox)
		{
			var result = new Vector3
			{
				X = boundingBox.Min.X + ((boundingBox.Max.X - boundingBox.Min.X) * 0.5f),
				Y = boundingBox.Min.Y + ((boundingBox.Max.Y - boundingBox.Min.Y) * 0.5f),
				Z = boundingBox.Min.Z + ((boundingBox.Max.Z - boundingBox.Min.Z) * 0.5f)
			};

			return result;
		}
	}
}