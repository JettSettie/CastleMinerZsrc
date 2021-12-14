using DNA.Net;
using DNA.Net.GamerServices;
using System.IO;

namespace DNA.CastleMinerZ.Net
{
	public class PlayerExistsMessage : CastleMinerZMessage
	{
		public byte[] AvatarDescriptionData;

		public bool RequestResponse;

		public Gamer Gamer = new SimpleGamer();

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

		private PlayerExistsMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, AvatarDescription description, bool requestResponse)
		{
			PlayerExistsMessage sendInstance = Message.GetSendInstance<PlayerExistsMessage>();
			sendInstance.AvatarDescriptionData = description.Description;
			sendInstance.RequestResponse = requestResponse;
			sendInstance.Gamer.Gamertag = from.Gamertag;
			sendInstance.Gamer.PlayerID = from.PlayerID;
			sendInstance.DoSend(from);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			RequestResponse = reader.ReadBoolean();
			int count = reader.ReadInt32();
			AvatarDescriptionData = reader.ReadBytes(count);
			Gamer.Gamertag = reader.ReadString();
			Gamer.PlayerID.Read(reader);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(RequestResponse);
			writer.Write(AvatarDescriptionData.Length);
			writer.Write(AvatarDescriptionData);
			writer.Write(Gamer.Gamertag);
			Gamer.PlayerID.Write(writer);
		}
	}
}
