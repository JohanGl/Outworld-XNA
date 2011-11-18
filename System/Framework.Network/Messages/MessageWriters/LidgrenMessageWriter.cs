using System;
using Lidgren.Network;

namespace Framework.Network.Messages.MessageWriters
{
	public class LidgrenMessageWriter : IMessageWriter
	{
		private NetPeer peer;
		private NetOutgoingMessage messageOut;
		private NetOutgoingMessage messageOutPrevious;

		public LidgrenMessageWriter(NetPeer peer)
		{
			this.peer = peer;
		}

		public void WriteNewMessage()
		{
			messageOutPrevious = messageOut;
			messageOut = peer.CreateMessage();
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

		public object GetMessage()
		{
			return messageOut;
		}
	}
}