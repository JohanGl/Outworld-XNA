using System;

namespace Framework.Network.Messages.MessageReaders
{
	public class DefaultMessageReader : IMessageReader
	{
		private Message readMessage;
		private int readMessagePosition;

		public void ReadNewMessage(Message message)
		{
			readMessage = message;
			readMessagePosition = 0;
		}

		public bool ReadBool()
		{
			return BitConverter.ToBoolean(ReadMessageBytes(1), 0);
		}

		public byte ReadByte()
		{
			return ReadMessageBytes(1)[0];
		}

		public Int16 ReadInt16()
		{
			return BitConverter.ToInt16(ReadMessageBytes(2), 0);
		}

		public Int32 ReadInt32()
		{
			return BitConverter.ToInt32(ReadMessageBytes(4), 0);
		}

		public Int64 ReadInt64()
		{
			return BitConverter.ToInt64(ReadMessageBytes(8), 0);
		}

		public float ReadFloat()
		{
			return BitConverter.ToSingle(ReadMessageBytes(4), 0);
		}

		public double ReadDouble()
		{
			return BitConverter.ToDouble(ReadMessageBytes(8), 0);
		}

		public string ReadString()
		{
			// Mess from the Lidgren source in 'NetIncomingMessage.Read.cs' kept for compatibility reasons
			int length;
			int num1 = 0;
			int num2 = 0;
			while (true)
			{
				byte num3 = ReadByte();
				num1 |= (num3 & 0x7f) << num2;
				num2 += 7;
				if ((num3 & 0x80) == 0)
				{
					length = (int)num1;
					break;
				}
			}

			if (length == 0)
			{
				return String.Empty;
			}

			var stringBytes = ReadMessageBytes((int)length);

			return System.Text.Encoding.UTF8.GetString(stringBytes, 0, stringBytes.Length);
		}

		private byte[] ReadMessageBytes(int size)
		{
			var data = new byte[size];

#if BIGENDIAN
			int j = 0;
			for (int i = readMessagePosition; i < readMessagePosition + size; i++)
			{
				data[(size - 1) - j] = readMessage.Data[i];
				j++;
			}
#else
			int j = 0;
			for (int i = readMessagePosition; i < readMessagePosition + size; i++)
			{
				data[j] = readMessage.Data[i];
				j++;
			}
#endif

			readMessagePosition += size;

			return data;
		}
	}
}