using System;
using Lidgren.Network;

namespace Framework.Network.Messages.MessageWriters
{
	public class LidgrenMessageWriter : IMessageWriter
	{
		private NetPeer peer;
		private NetOutgoingMessage messageOut;

		public LidgrenMessageWriter(NetPeer peer)
		{
			this.peer = peer;
		}

		public void WriteNewMessage()
		{
			messageOut = peer.CreateMessage();
		}

		public void WriteTimeStamp()
		{
			messageOut.WriteTime(NetTime.Now, false);
		}

		public void Write(bool source)
		{
			messageOut.Write(source);
		}

		public void Write(byte source)
		{
			messageOut.Write(source);
		}

		public void Write(Int16 source)
		{
			messageOut.Write(source);
		}

		public void Write(Int32 source)
		{
			messageOut.Write(source);
		}

		public void Write(Int64 source)
		{
			messageOut.Write(source);
		}

		public void Write(float source)
		{
			messageOut.Write(source);
		}

		public void Write(double source)
		{
			messageOut.Write(source);
		}

		public void Write(string source)
		{
			messageOut.Write(source);
		}

		public byte[] GetBytes()
		{
			return messageOut.PeekDataBuffer();
		}

		public object GetMessage()
		{
			return messageOut;
		}
	}
}