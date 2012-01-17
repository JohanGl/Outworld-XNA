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
		private const float FloatToByteDegree = 255f / 360f;
		private const float ByteToFloatDegree = 360f / 255f;

		public Vector3 ReadVector3(IMessageReader reader)
		{
			float x = reader.ReadFloat();
			float y = reader.ReadFloat();
			float z = reader.ReadFloat();
			
			return new Vector3(x, y, z);
		}

		public Vector3 ReadByteAngles(IMessageReader reader)
		{
			float angleX = AngleByteToFloat(reader.ReadByte());
			float angleY = (reader.ReadByte());

			return new Vector3(angleX, angleY, 0);
		}

		public void WriteVector3(Vector3 vector, IMessageWriter writer)
		{
			writer.Write(vector.X);
			writer.Write(vector.Y);
			writer.Write(vector.Z);
		}

		public void WriteByteAngles(Vector3 angle, IMessageWriter writer)
		{
			writer.Write(AngleFloatToByte(angle.X));
			writer.Write(AngleFloatToByte(angle.Y));
		}

		public byte AngleFloatToByte(float angle)
		{
			if (angle > 360)
			{
				angle = 360;
			}
			else if (angle < 0)
			{
				angle = 0;
			}

			return (byte)(angle * FloatToByteDegree);
		}

		public float AngleByteToFloat(byte angle)
		{
			return angle * ByteToFloatDegree;
		}
	}
}