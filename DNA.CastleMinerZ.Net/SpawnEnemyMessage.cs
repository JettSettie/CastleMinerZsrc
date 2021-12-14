using DNA.CastleMinerZ.AI;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using System.IO;

namespace DNA.CastleMinerZ.Net
{
	public class SpawnEnemyMessage : CastleMinerZMessage
	{
		public Vector3 SpawnPosition;

		public Vector3 SpawnerPosition;

		public EnemyTypeEnum EnemyTypeID;

		public string PlayerName;

		public int EnemyID;

		public int RandomSeed;

		public EnemyType.InitPackage InitPkg;

		public int SpawnValue;

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

		private SpawnEnemyMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Vector3 pos, EnemyTypeEnum enemyType, float midnight, int id, int seed, Vector3 spawnerPos, int spawnValue = 0, string playerName = null)
		{
			SpawnEnemyMessage sendInstance = Message.GetSendInstance<SpawnEnemyMessage>();
			sendInstance.SpawnPosition = pos;
			sendInstance.SpawnerPosition = spawnerPos;
			sendInstance.EnemyTypeID = enemyType;
			sendInstance.EnemyID = id;
			sendInstance.SpawnValue = spawnValue;
			sendInstance.RandomSeed = seed;
			if (playerName == null)
			{
				playerName = "";
			}
			sendInstance.PlayerName = playerName;
			sendInstance.InitPkg = EnemyType.Types[(int)enemyType].CreateInitPackage(midnight);
			sendInstance.DoSend(from);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			SpawnPosition = reader.ReadVector3();
			SpawnerPosition = reader.ReadVector3();
			EnemyTypeID = (EnemyTypeEnum)reader.ReadByte();
			EnemyID = reader.ReadInt32();
			RandomSeed = reader.ReadInt32();
			SpawnValue = reader.ReadInt32();
			PlayerName = reader.ReadString();
			InitPkg = EnemyType.InitPackage.Read(reader);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(SpawnPosition);
			writer.Write(SpawnerPosition);
			writer.Write((byte)EnemyTypeID);
			writer.Write(EnemyID);
			writer.Write(RandomSeed);
			writer.Write(SpawnValue);
			writer.Write(PlayerName);
			InitPkg.Write(writer);
		}
	}
}
