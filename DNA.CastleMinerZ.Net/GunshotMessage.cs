using DNA.CastleMinerZ.Inventory;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using System.IO;

namespace DNA.CastleMinerZ.Net
{
	public class GunshotMessage : CastleMinerZMessage
	{
		public Vector3 Direction;

		public InventoryItemIDs ItemID;

		public override MessageTypes MessageType
		{
			get
			{
				return MessageTypes.PlayerUpdate;
			}
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.Reliable;
			}
		}

		private GunshotMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Matrix m, Angle innacuracy, InventoryItemIDs item, bool addDropCompensation)
		{
			GunshotMessage sendInstance = Message.GetSendInstance<GunshotMessage>();
			Vector3 forward = m.Forward;
			if (addDropCompensation)
			{
				forward += m.Up * 0.015f;
			}
			Matrix matrix = Matrix.CreateRotationX(MathTools.RandomFloat(0f - innacuracy.Radians, innacuracy.Radians));
			matrix *= Matrix.CreateRotationY(MathTools.RandomFloat(0f - innacuracy.Radians, innacuracy.Radians));
			forward = Vector3.TransformNormal(forward, matrix);
			sendInstance.Direction = Vector3.Normalize(forward);
			sendInstance.ItemID = item;
			sendInstance.DoSend(from);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			Direction = reader.ReadVector3();
			ItemID = (InventoryItemIDs)reader.ReadInt16();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(Direction);
			writer.Write((short)ItemID);
		}
	}
}
