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
		private const float floatToByteDegree = 255f / 360f;
		private const float byteToFloatDegree = 360f / 255f;

		public Vector3 ReadVector3(IMessageReader reader)
		{
			float x = reader.ReadFloat();
			float y = reader.ReadFloat();
			float z = reader.ReadFloat();
			return new Vector3(x * byteToFloatDegree, y * byteToFloatDegree, z * byteToFloatDegree);
		}

		public Vector3 ReadVector3FromVector3b(IMessageReader reader)
		{
			float x = reader.ReadByte();
			float y = reader.ReadByte();
			float z = reader.ReadByte();

			return new Vector3(x, y, z);
		}

		public void WriteVector3(Vector3 vector, IMessageWriter writer)
		{
			writer.Write(vector.X);
			writer.Write(vector.Y);
			writer.Write(vector.Z);
		}

		public void WriteVector3AsVector3b(Vector3 angle, IMessageWriter writer)
		{
			writer.Write((byte)(angle.X * floatToByteDegree));
			writer.Write((byte)(angle.Y * floatToByteDegree));
			writer.Write((byte)(angle.Z * floatToByteDegree));
		}
	}
}