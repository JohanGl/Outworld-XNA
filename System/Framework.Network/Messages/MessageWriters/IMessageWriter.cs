using System;

namespace Framework.Network.Messages.MessageWriters
{
	public interface IMessageWriter
	{
		void WriteNewMessage();
		void WriteTimeStamp();
		void Write(bool source);
		void Write(byte source);
		void Write(Int16 source);
		void Write(Int32 source);
		void Write(Int64 source);
		void Write(float source);
		void Write(double source);
		void Write(string source);
		object GetMessage();
	}
}