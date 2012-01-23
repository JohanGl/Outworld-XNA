using System;

namespace Framework.Network.Messages.MessageReaders
{
	public interface IMessageReader
	{
		void ReadNewMessage(Message message);
		float ReadTimeStamp();
		bool ReadBool();
		byte ReadByte();
		Int16 ReadInt16();
		Int32 ReadInt32();
		Int64 ReadInt64();
		float ReadFloat();
		double ReadDouble();
		string ReadString();
	}
}