using DNA.Net;
using DNA.Net.GamerServices;
using System.IO;

namespace DNA.CastleMinerZ.Net
{
	internal class ProvideChunkMessage : CastleMinerZMessage
	{
		public IntVector3 BlockLocation;

		public byte Priority;

		public int[] Delta;

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

		private ProvideChunkMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, NetworkGamer recipient, IntVector3 blockLocaion, int[] delta, int priority)
		{
			ProvideChunkMessage sendInstance = Message.GetSendInstance<ProvideChunkMessage>();
			sendInstance.BlockLocation = blockLocaion;
			sendInstance.Priority = (byte)priority;
			sendInstance.Delta = delta;
			if (recipient == null)
			{
				sendInstance.DoSend(from);
			}
			else
			{
				sendInstance.DoSend(from, recipient);
			}
		}

		protected override void RecieveData(BinaryReader reader)
		{
			BlockLocation = IntVector3.Read(reader);
			Priority = reader.ReadByte();
			int num = reader.ReadInt32();
			if (num > 0)
			{
				Delta = new int[num];
				for (int i = 0; i < num; i++)
				{
					Delta[i] = reader.ReadInt32();
				}
			}
			else
			{
				Delta = null;
			}
		}

		protected override void SendData(BinaryWriter writer)
		{
			BlockLocation.Write(writer);
			writer.Write(Priority);
			int num = (Delta != null) ? Delta.Length : 0;
			writer.Write(num);
			if (num != 0)
			{
				for (int i = 0; i < num; i++)
				{
					writer.Write(Delta[i]);
				}
			}
		}
	}
}
