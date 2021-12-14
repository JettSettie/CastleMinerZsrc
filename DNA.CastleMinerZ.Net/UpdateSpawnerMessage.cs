using DNA.CastleMinerZ.AI;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using System.IO;

namespace DNA.CastleMinerZ.Net
{
	public class UpdateSpawnerMessage : CastleMinerZMessage
	{
		public Vector3 SpawnerPosition;

		public EnemyTypeEnum EnemyTypeID;

		public int EnemyID;

		public int RandomSeed;

		public bool IsStarted;

		public EnemyType.InitPackage InitPkg;

		public override MessageTypes MessageType
		{
			get
			{
				return MessageTypes.EnemyMessage;
			}
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.Reliable;
			}
		}

		private UpdateSpawnerMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Vector3 pos, bool isStarted)
		{
			UpdateSpawnerMessage sendInstance = Message.GetSendInstance<UpdateSpawnerMessage>();
			sendInstance.SpawnerPosition = pos;
			sendInstance.IsStarted = isStarted;
			sendInstance.DoSend(from);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			SpawnerPosition = reader.ReadVector3();
			IsStarted = reader.ReadBoolean();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(SpawnerPosition);
			writer.Write(IsStarted);
		}
	}
}
