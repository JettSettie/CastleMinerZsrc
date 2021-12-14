using DNA.CastleMinerZ.Terrain;
using DNA.Net;
using DNA.Net.GamerServices;
using System.IO;

namespace DNA.CastleMinerZ.Net
{
	public class DestroyCustomBlockMessage : CastleMinerZMessage
	{
		public IntVector3 Location;

		public BlockTypeEnum BlockType;

		public override MessageTypes MessageType
		{
			get
			{
				return MessageTypes.System;
			}
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.ReliableInOrder;
			}
		}

		private DestroyCustomBlockMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, IntVector3 location, BlockTypeEnum blockType)
		{
			DestroyCustomBlockMessage sendInstance = Message.GetSendInstance<DestroyCustomBlockMessage>();
			sendInstance.Location = location;
			sendInstance.BlockType = blockType;
			sendInstance.DoSend(from);
		}

		protected override void SendData(BinaryWriter writer)
		{
			Location.Write(writer);
			writer.Write((byte)BlockType);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			Location = IntVector3.Read(reader);
			BlockType = (BlockTypeEnum)reader.ReadByte();
		}
	}
}
