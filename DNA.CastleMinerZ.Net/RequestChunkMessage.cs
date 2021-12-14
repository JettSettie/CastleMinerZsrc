using DNA.Net;
using DNA.Net.GamerServices;
using System.IO;

namespace DNA.CastleMinerZ.Net
{
	internal class RequestChunkMessage : CastleMinerZMessage
	{
		public IntVector3 BlockLocation;

		public int Priority;

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.Reliable;
			}
		}

		public override MessageTypes MessageType
		{
			get
			{
				return MessageTypes.System;
			}
		}

		private RequestChunkMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, IntVector3 blockLocation, int priority)
		{
			RequestChunkMessage sendInstance = Message.GetSendInstance<RequestChunkMessage>();
			sendInstance.BlockLocation = blockLocation;
			sendInstance.Priority = priority;
			sendInstance.SendToHost(from);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			BlockLocation = IntVector3.Read(reader);
			Priority = reader.ReadByte();
		}

		protected override void SendData(BinaryWriter writer)
		{
			BlockLocation.Write(writer);
			writer.Write((byte)Priority);
		}
	}
}
