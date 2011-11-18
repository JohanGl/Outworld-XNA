using Framework.Network.Messages.MessageReaders;
using Framework.Network.Messages.MessageWriters;
using Microsoft.Xna.Framework;

namespace Game.Network.Common
{
	/// <summary>
	/// Contains helper functions related to read/write operations of messages with XNA
	/// </summary>
	public class MessageHelper
	{
		public Vector3 ReadVector3(IMessageReader reader)
		{
			float x = reader.ReadFloat();
			float y = reader.ReadFloat();
			float z = reader.ReadFloat();
			return new Vector3(x, y, z);
		}

		public void WriteVector3(Vector3 vector, IMessageWriter writer)
		{
			writer.Write(vector.X);
			writer.Write(vector.Y);
			writer.Write(vector.Z);
		}
	}
}