using DNA.CastleMinerZ.Inventory;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using System.IO;

namespace DNA.CastleMinerZ.Net
{
	public class CreatePickupMessage : CastleMinerZMessage
	{
		public Vector3 SpawnPosition;

		public Vector3 SpawnVector;

		public InventoryItem Item;

		public int PickupID;

		public bool Dropped;

		public bool DisplayOnPickup;

		public override MessageTypes MessageType
		{
			get
			{
				return MessageTypes.PickupMessage;
			}
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.Reliable;
			}
		}

		private CreatePickupMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Vector3 pos, Vector3 vec, int pickupID, InventoryItem item, bool dropped, bool displayOnPickup)
		{
			CreatePickupMessage sendInstance = Message.GetSendInstance<CreatePickupMessage>();
			sendInstance.SpawnPosition = pos;
			sendInstance.SpawnVector = vec;
			sendInstance.Item = item;
			sendInstance.Dropped = dropped;
			sendInstance.DisplayOnPickup = displayOnPickup;
			sendInstance.PickupID = pickupID;
			sendInstance.DoSend(from);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			SpawnPosition = reader.ReadVector3();
			SpawnVector = reader.ReadVector3();
			PickupID = reader.ReadInt32();
			Item = InventoryItem.Create(reader);
			Dropped = reader.ReadBoolean();
			DisplayOnPickup = reader.ReadBoolean();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(SpawnPosition);
			writer.Write(SpawnVector);
			writer.Write(PickupID);
			Item.Write(writer);
			writer.Write(Dropped);
			writer.Write(DisplayOnPickup);
		}
	}
}
