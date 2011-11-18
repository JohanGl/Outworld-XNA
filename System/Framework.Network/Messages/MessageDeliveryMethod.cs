namespace Framework.Network.Messages
{
	public enum MessageDeliveryMethod
	{
		Unknown,
		Unreliable,
		UnreliableSequenced,
		ReliableUnordered,
		ReliableSequenced,
		ReliableOrdered
	}
}