using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.Terrain.WorldBuilders;
using DNA.Drawing.UI;
using DNA.IO;
using DNA.IO.Storage;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace DNA.CastleMinerZ
{
	public class WorldInfo
	{
		private enum WorldInfoVersion
		{
			Initial = 1,
			Doors,
			Spawners,
			HellBosses,
			CurrentVersion
		}

		public static Vector3 DefaultStartLocation = new Vector3(8f, 128f, -8f);

		public Dictionary<IntVector3, Crate> Crates = new Dictionary<IntVector3, Crate>();

		public Dictionary<IntVector3, Door> Doors = new Dictionary<IntVector3, Door>();

		public Dictionary<IntVector3, Spawner> Spawners = new Dictionary<IntVector3, Spawner>();

		public static List<string> CorruptWorlds = new List<string>();

		private static readonly string BasePath = "Worlds";

		private static readonly string FileName = "world.info";

		private string _savePath;

		public WorldTypeIDs _terrainVersion = WorldTypeIDs.CastleMinerZ;

		private string _name = Strings.World;

		private string _ownerGamerTag;

		private string _creatorGamerTag;

		private DateTime _createdDate;

		private DateTime _lastPlayedDate;

		private int _seed;

		private Guid _worldID;

		private Vector3 _lastPosition = DefaultStartLocation;

		public bool InfiniteResourceMode;

		public int HellBossesSpawned;

		public int MaxHellBossSpawns;

		public string ServerMessage = Strings.Server;

		public string ServerPassword = "";

		public int Version
		{
			get
			{
				return 5;
			}
		}

		public string SavePath
		{
			get
			{
				if (OwnerGamerTag == null)
				{
					return null;
				}
				return _savePath;
			}
			set
			{
				_savePath = value;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public string OwnerGamerTag
		{
			get
			{
				return _ownerGamerTag;
			}
		}

		public string CreatorGamerTag
		{
			get
			{
				return _creatorGamerTag;
			}
		}

		public DateTime CreatedDate
		{
			get
			{
				return _createdDate;
			}
		}

		public DateTime LastPlayedDate
		{
			get
			{
				return _lastPlayedDate;
			}
			set
			{
				_lastPlayedDate = value;
			}
		}

		public int Seed
		{
			get
			{
				return _seed;
			}
		}

		public Guid WorldID
		{
			get
			{
				return _worldID;
			}
		}

		public Vector3 LastPosition
		{
			get
			{
				return _lastPosition;
			}
			set
			{
				_lastPosition = value;
			}
		}

		private WorldInfo()
		{
			ServerMessage = string.Concat(Screen.CurrentGamer, "'s ", Strings.Server);
			CreateSavePath();
		}

		public WorldInfo(BinaryReader reader)
		{
			CreateSavePath();
			Load(reader);
		}

		public WorldInfo(WorldInfo info)
		{
			_savePath = info._savePath;
			_terrainVersion = info._terrainVersion;
			_name = info._name;
			_ownerGamerTag = info._ownerGamerTag;
			_creatorGamerTag = info._creatorGamerTag;
			_createdDate = info._createdDate;
			_lastPlayedDate = info._lastPlayedDate;
			_seed = info._seed;
			_worldID = info._worldID;
			_lastPosition = info._lastPosition;
			InfiniteResourceMode = info.InfiniteResourceMode;
		}

		public void SetDoor(IntVector3 location, DoorEntity.ModelNameEnum modelName)
		{
			Door value;
			if (!Doors.TryGetValue(location, out value))
			{
				value = new Door(location, modelName);
				Doors[location] = value;
			}
		}

		public Spawner GetSpawner(IntVector3 location, bool createIfMissing, BlockTypeEnum blockType)
		{
			Spawner value;
			if (!Spawners.TryGetValue(location, out value))
			{
				if (!createIfMissing)
				{
					return null;
				}
				value = new Spawner(location);
				Spawners[location] = value;
			}
			return value;
		}

		public Door GetDoor(IntVector3 location)
		{
			Door value;
			if (Doors.TryGetValue(location, out value))
			{
				return value;
			}
			return null;
		}

		public Crate GetCrate(IntVector3 crateLocation, bool createIfMissing)
		{
			Crate value;
			if (!Crates.TryGetValue(crateLocation, out value))
			{
				if (!createIfMissing)
				{
					return null;
				}
				value = new Crate(crateLocation);
				Crates[crateLocation] = value;
			}
			return value;
		}

		public static WorldInfo CreateNewWorld(SignedInGamer gamer)
		{
			Random random = new Random();
			WorldInfo worldInfo = new WorldInfo();
			int seed = 839880689;
			if (!CastleMinerZGame.TrialMode)
			{
				seed = random.Next();
			}
			worldInfo.MakeNew(gamer, seed);
			return worldInfo;
		}

		public static WorldInfo CreateNewWorld(SignedInGamer gamer, int seed)
		{
			WorldInfo worldInfo = new WorldInfo();
			worldInfo.MakeNew(gamer, seed);
			return worldInfo;
		}

		public static WorldInfo CreateNewWorld(int seed)
		{
			return CreateNewWorld(null, seed);
		}

		public static WorldInfo[] LoadWorldInfo(SaveDevice device)
		{
			try
			{
				CorruptWorlds.Clear();
				if (!device.DirectoryExists(BasePath))
				{
					return new WorldInfo[0];
				}
				List<WorldInfo> list = new List<WorldInfo>();
				string[] directories = device.GetDirectories(BasePath);
				string[] array = directories;
				foreach (string text in array)
				{
					WorldInfo worldInfo = null;
					try
					{
						worldInfo = LoadFromStroage(text, device);
					}
					catch
					{
						worldInfo = null;
						CorruptWorlds.Add(text);
					}
					if (worldInfo != null)
					{
						list.Add(worldInfo);
					}
				}
				return list.ToArray();
			}
			catch
			{
				return new WorldInfo[0];
			}
		}

		public void CreateSavePath()
		{
			_savePath = Path.Combine(path2: Guid.NewGuid().ToString(), path1: BasePath);
		}

		private void MakeNew(SignedInGamer creator, int seed)
		{
			if (creator == null)
			{
				_name = Strings.New_World + " " + DateTime.Now.ToString("g");
			}
			else
			{
				_name = string.Concat(creator, "'s ", Strings.World, " ", DateTime.Now.ToString("g"));
			}
			CreateSavePath();
			if (creator == null)
			{
				_ownerGamerTag = (_creatorGamerTag = null);
			}
			else
			{
				_ownerGamerTag = (_creatorGamerTag = creator.Gamertag);
			}
			_createdDate = (_lastPlayedDate = DateTime.Now);
			_worldID = Guid.NewGuid();
			_seed = seed;
		}

		public void TakeOwnership(SignedInGamer gamer, SaveDevice device)
		{
			if (_creatorGamerTag == null)
			{
				_creatorGamerTag = gamer.Gamertag;
			}
			_ownerGamerTag = gamer.Gamertag;
			_worldID = Guid.NewGuid();
			SaveToStorage(gamer, device);
		}

		public void SaveToStorage(SignedInGamer gamer, SaveDevice saveDevice)
		{
			try
			{
				if (!saveDevice.DirectoryExists(SavePath))
				{
					saveDevice.CreateDirectory(SavePath);
				}
				string fileName = Path.Combine(SavePath, FileName);
				saveDevice.Save(fileName, true, true, delegate(Stream stream)
				{
					BinaryWriter binaryWriter = new BinaryWriter(stream);
					Save(binaryWriter);
					binaryWriter.Flush();
				});
			}
			catch
			{
			}
		}

		private static WorldInfo LoadFromStroage(string folder, SaveDevice saveDevice)
		{
			WorldInfo info = new WorldInfo();
			saveDevice.Load(Path.Combine(folder, FileName), delegate(Stream stream)
			{
				BinaryReader reader = new BinaryReader(stream);
				info.Load(reader);
				info._savePath = folder;
			});
			return info;
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write(Version);
			writer.Write((int)_terrainVersion);
			writer.Write(_name);
			writer.Write(_ownerGamerTag);
			writer.Write(_creatorGamerTag);
			writer.Write(_createdDate.Ticks);
			writer.Write(_lastPlayedDate.Ticks);
			writer.Write(_seed);
			writer.Write(_worldID.ToByteArray());
			writer.Write(_lastPosition);
			writer.Write(Crates.Count);
			foreach (KeyValuePair<IntVector3, Crate> crate in Crates)
			{
				crate.Value.Write(writer);
			}
			writer.Write(Doors.Count);
			foreach (KeyValuePair<IntVector3, Door> door in Doors)
			{
				door.Value.Write(writer);
			}
			writer.Write(Spawners.Count);
			foreach (KeyValuePair<IntVector3, Spawner> spawner in Spawners)
			{
				spawner.Value.Write(writer);
			}
			writer.Write(InfiniteResourceMode);
			writer.Write(ServerMessage);
			writer.Write(ServerPassword);
			writer.Write(HellBossesSpawned);
			writer.Write(MaxHellBossSpawns);
		}

		private void Load(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			WorldInfoVersion worldInfoVersion = (WorldInfoVersion)num;
			if (num < 1 || worldInfoVersion > WorldInfoVersion.CurrentVersion)
			{
				throw new Exception("Bad Info Version");
			}
			_terrainVersion = (WorldTypeIDs)reader.ReadInt32();
			_name = reader.ReadString();
			_ownerGamerTag = reader.ReadString();
			_creatorGamerTag = reader.ReadString();
			_createdDate = new DateTime(reader.ReadInt64());
			_lastPlayedDate = new DateTime(reader.ReadInt64());
			_seed = reader.ReadInt32();
			_worldID = new Guid(reader.ReadBytes(16));
			_lastPosition = reader.ReadVector3();
			int num2 = reader.ReadInt32();
			Crates.Clear();
			for (int i = 0; i < num2; i++)
			{
				Crate crate = new Crate(reader);
				Crates[crate.Location] = crate;
			}
			if (worldInfoVersion > WorldInfoVersion.Doors)
			{
				int num3 = reader.ReadInt32();
				Doors.Clear();
				for (int j = 0; j < num3; j++)
				{
					Door door = new Door(reader);
					Doors[door.Location] = door;
				}
			}
			if (worldInfoVersion > WorldInfoVersion.Spawners)
			{
				int num4 = reader.ReadInt32();
				Spawners.Clear();
				for (int k = 0; k < num4; k++)
				{
					Spawner spawner = new Spawner(reader);
					Spawners[spawner.Location] = spawner;
				}
			}
			InfiniteResourceMode = reader.ReadBoolean();
			ServerMessage = reader.ReadString();
			ServerPassword = reader.ReadString();
			if (worldInfoVersion > WorldInfoVersion.HellBosses)
			{
				HellBossesSpawned = reader.ReadInt32();
				MaxHellBossSpawns = reader.ReadInt32();
			}
		}

		public WorldBuilder GetBuilder()
		{
			return new CastleMinerZBuilder(this);
		}

		public void Update(GameTime gameTime)
		{
			foreach (KeyValuePair<IntVector3, Spawner> spawner in Spawners)
			{
				spawner.Value.UpdateSpawner(gameTime);
			}
		}
	}
}
