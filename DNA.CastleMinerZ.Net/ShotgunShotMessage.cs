using DNA.CastleMinerZ.Inventory;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using System.IO;

namespace DNA.CastleMinerZ.Net
{
	public class ShotgunShotMessage : CastleMinerZMessage
	{
		public Vector3[] Directions = new Vector3[5];

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

		private ShotgunShotMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Matrix m, Angle innacuracy, InventoryItemIDs item, bool addDropCompensation)
		{
			ShotgunShotMessage sendInstance = Message.GetSendInstance<ShotgunShotMessage>();
			for (int i = 0; i < 5; i++)
			{
				Vector3 forward = m.Forward;
				if (addDropCompensation)
				{
					forward += m.Up * 0.015f;
				}
				Matrix matrix = Matrix.CreateRotationX(MathTools.RandomFloat(0f - innacuracy.Radians, innacuracy.Radians));
				matrix *= Matrix.CreateRotationY(MathTools.RandomFloat(0f - innacuracy.Radians, innacuracy.Radians));
				forward = Vector3.TransformNormal(forward, matrix);
				sendInstance.Directions[i] = Vector3.Normalize(forward);
			}
			sendInstance.ItemID = item;
			sendInstance.DoSend(from);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			for (int i = 0; i < 5; i++)
			{
				Directions[i] = reader.ReadVector3();
			}
			ItemID = (InventoryItemIDs)reader.ReadInt16();
		}

		protected override void SendData(BinaryWriter writer)
		{
			for (int i = 0; i < 5; i++)
			{
				writer.Write(Directions[i]);
			}
			writer.Write((short)ItemID);
		}
	}
}
