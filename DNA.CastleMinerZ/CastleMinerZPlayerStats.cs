using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.Distribution;
using DNA.Distribution.Steam;
using DNA.Input;
using System;
using System.Collections.Generic;
using System.IO;

namespace DNA.CastleMinerZ
{
	public class CastleMinerZPlayerStats : PlayerStats
	{
		private class CMZStatBase
		{
			private static int _sIndexCount;

			private int _index;

			private string _apiName;

			protected bool _valueChanged;

			protected bool _initialized;

			private StatType _statType;

			public CMZStatBase(string name, StatType type)
			{
				_apiName = name;
				_statType = type;
				_index = _sIndexCount++;
			}

			public int GetIndex()
			{
				return _index;
			}

			public string GetName()
			{
				return _apiName;
			}

			public bool DidValueChange()
			{
				return _valueChanged;
			}

			public void ClearValueChanged()
			{
				_valueChanged = false;
			}

			public StatType ValueType()
			{
				return _statType;
			}
		}

		private class CMZIntStat : CMZStatBase, IStatInterface
		{
			private int _value;

			public CMZIntStat(string name)
				: base(name, StatType.INT)
			{
			}

			public CMZIntStat(string name, int def)
				: base(name, StatType.INT)
			{
				_value = def;
			}

			public float GetFloatValue()
			{
				return _value;
			}

			public int GetIntValue()
			{
				return _value;
			}

			public void InitValue(float value)
			{
				InitValue((int)value);
			}

			public void SetValue(float value)
			{
				SetValue((int)value);
			}

			public void SetValue(int value)
			{
				if (_value != value)
				{
					_value = value;
					_valueChanged = true;
				}
			}

			public void InitValue(int value)
			{
				_value = value;
				_initialized = true;
				_valueChanged = false;
			}
		}

		private class CMZFloatStat : CMZStatBase, IStatInterface
		{
			private float _value;

			public CMZFloatStat(string name)
				: base(name, StatType.FLOAT)
			{
			}

			public CMZFloatStat(string name, float def)
				: base(name, StatType.FLOAT)
			{
				_value = def;
			}

			public float GetFloatValue()
			{
				return _value;
			}

			public int GetIntValue()
			{
				return (int)_value;
			}

			public void SetValue(float value)
			{
				if (_value != value)
				{
					_value = value;
					_valueChanged = true;
				}
			}

			public void InitValue(float value)
			{
				_value = value;
				_valueChanged = false;
				_initialized = true;
			}

			public void SetValue(int value)
			{
				SetValue((float)value);
			}

			public void InitValue(int value)
			{
				SetValue((float)value);
			}
		}

		public enum CMZStat
		{
			TotalKills,
			MaxDaysSurvived,
			Version,
			TimeInGame,
			GamesPlayed,
			MaxDistanceTraveled,
			MaxDepth,
			TotalItemsCrafted,
			UndeadDragonKills,
			ForestDragonKills,
			IceDragonKills,
			FireDragonKills,
			SandDragonKills,
			AlienEncounters,
			DragonsKilledWithGuidedMissile,
			EnemiesKilledWithTNT,
			EnemiesKilledWithGrenade,
			EnemiesKilledWithLaserWeapon
		}

		public class ItemStats
		{
			private InventoryItemIDs _itemID;

			public TimeSpan TimeHeld;

			public int Crafted;

			public int Used;

			public int Hits;

			public int KillsZombies;

			public int KillsSkeleton;

			public int KillsHell;

			public InventoryItemIDs ItemID
			{
				get
				{
					return _itemID;
				}
			}

			public ItemStats(InventoryItemIDs itemID)
			{
				_itemID = itemID;
			}

			public void Write(BinaryWriter writer)
			{
				writer.Write(TimeHeld.Ticks);
				writer.Write(Crafted);
				writer.Write(Used);
				writer.Write(Hits);
				writer.Write(KillsZombies);
				writer.Write(KillsSkeleton);
				writer.Write(KillsHell);
			}

			public void Read(BinaryReader reader)
			{
				TimeHeld = TimeSpan.FromTicks(reader.ReadInt64());
				Crafted = reader.ReadInt32();
				Used = reader.ReadInt32();
				Hits = reader.ReadInt32();
				KillsZombies = reader.ReadInt32();
				KillsSkeleton = reader.ReadInt32();
				KillsHell = reader.ReadInt32();
			}

			internal void AddStat(SessionStats.StatType category)
			{
				switch (category)
				{
				case SessionStats.StatType.FelguardDefeated:
				case SessionStats.StatType.AlienDefeated:
				case SessionStats.StatType.DragonDefeated:
					break;
				case SessionStats.StatType.HellMinionDefeated:
					KillsHell++;
					break;
				case SessionStats.StatType.ZombieDefeated:
					KillsZombies++;
					break;
				case SessionStats.StatType.SkeletonDefeated:
				case SessionStats.StatType.SKELETONARCHER:
				case SessionStats.StatType.SKELETONAXES:
				case SessionStats.StatType.SKELETONSWORD:
					KillsSkeleton++;
					break;
				}
			}
		}

		private enum StatVersion
		{
			PreControlBinding = 2,
			PreGraphicsUpdate,
			PreFlySprintUpdate,
			PreExtraTrayUpdate,
			PreFadeTrayOption,
			CurrentVersion
		}

		private IStatInterface[] _stats = new IStatInterface[18]
		{
			new CMZIntStat("TotalKills"),
			new CMZIntStat("MaxDaysSurvived"),
			new CMZIntStat("Version"),
			new CMZFloatStat("TimeInGame"),
			new CMZIntStat("GamesPlayed"),
			new CMZFloatStat("MaxDistanceTraveled"),
			new CMZFloatStat("MaxDepth"),
			new CMZIntStat("TotalItemsCrafted"),
			new CMZIntStat("UndeadDragonKills"),
			new CMZIntStat("ForestDragonKills"),
			new CMZIntStat("IceDragonKills"),
			new CMZIntStat("FireDragonKills"),
			new CMZIntStat("SandDragonKills"),
			new CMZIntStat("AlienEncounters"),
			new CMZIntStat("DragonsKilledWithGuidedMissile"),
			new CMZIntStat("EnemiesKilledWithTNT"),
			new CMZIntStat("EnemiesKilledWithGrenade"),
			new CMZIntStat("EnemiesKilledWithLaserWeapon")
		};

		private WeakReference<SteamWorks> _steamAPI;

		public TimeSpan TimeOnline;

		public DateTime TimeOfPurchase;

		public DateTime FirstPlayTime = DateTime.UtcNow;

		public TimeSpan TimeInTrial;

		public TimeSpan TimeInFull;

		public TimeSpan TimeInMenu;

		public bool SecondTrayFaded;

		public bool InvertYAxis;

		public float brightness;

		public float musicVolume = 1f;

		public bool musicMute;

		public float controllerSensitivity = 1f;

		public bool AutoClimb = true;

		public int DrawDistance = 1;

		public bool PostOnAchievement = true;

		public bool PostOnHost = true;

		private Dictionary<BlockTypeEnum, int> BlocksDug = new Dictionary<BlockTypeEnum, int>();

		private Dictionary<InventoryItemIDs, ItemStats> AllItemStats = new Dictionary<InventoryItemIDs, ItemStats>();

		public Dictionary<ulong, DateTime> BanList = new Dictionary<ulong, DateTime>();

		private SessionStats sessionStats = new SessionStats();

		public List<ServerInfo> ServerList = new List<ServerInfo>();

		public SteamWorks SteamAPI
		{
			get
			{
				if (_steamAPI != null)
				{
					return _steamAPI.Target;
				}
				return null;
			}
		}

		public override int Version
		{
			get
			{
				return 7;
			}
		}

		public int TotalKills
		{
			get
			{
				return _stats[0].GetIntValue();
			}
			set
			{
				_stats[0].SetValue(value);
			}
		}

		public int GamesPlayed
		{
			get
			{
				return _stats[4].GetIntValue();
			}
			set
			{
				_stats[4].SetValue(value);
			}
		}

		public int MaxDaysSurvived
		{
			get
			{
				return _stats[1].GetIntValue();
			}
			set
			{
				_stats[1].SetValue(value);
			}
		}

		public float MaxDistanceTraveled
		{
			get
			{
				return _stats[5].GetFloatValue();
			}
			set
			{
				_stats[5].SetValue(value);
			}
		}

		public float MaxDepth
		{
			get
			{
				return _stats[6].GetFloatValue();
			}
			set
			{
				_stats[6].SetValue(value);
			}
		}

		public int TotalItemsCrafted
		{
			get
			{
				return _stats[7].GetIntValue();
			}
			set
			{
				_stats[7].SetValue(value);
			}
		}

		public int UndeadDragonKills
		{
			get
			{
				return _stats[8].GetIntValue();
			}
			set
			{
				_stats[8].SetValue(value);
			}
		}

		public int ForestDragonKills
		{
			get
			{
				return _stats[9].GetIntValue();
			}
			set
			{
				_stats[9].SetValue(value);
			}
		}

		public int IceDragonKills
		{
			get
			{
				return _stats[10].GetIntValue();
			}
			set
			{
				_stats[10].SetValue(value);
			}
		}

		public int FireDragonKills
		{
			get
			{
				return _stats[11].GetIntValue();
			}
			set
			{
				_stats[11].SetValue(value);
			}
		}

		public int SandDragonKills
		{
			get
			{
				return _stats[12].GetIntValue();
			}
			set
			{
				_stats[12].SetValue(value);
			}
		}

		public int AlienEncounters
		{
			get
			{
				return _stats[13].GetIntValue();
			}
			set
			{
				_stats[13].SetValue(value);
			}
		}

		public int DragonsKilledWithGuidedMissile
		{
			get
			{
				return _stats[14].GetIntValue();
			}
			set
			{
				_stats[14].SetValue(value);
			}
		}

		public int EnemiesKilledWithTNT
		{
			get
			{
				return _stats[15].GetIntValue();
			}
			set
			{
				_stats[15].SetValue(value);
			}
		}

		public int EnemiesKilledWithGrenade
		{
			get
			{
				return _stats[16].GetIntValue();
			}
			set
			{
				_stats[16].SetValue(value);
			}
		}

		public int EnemiesKilledWithLaserWeapon
		{
			get
			{
				return _stats[17].GetIntValue();
			}
			set
			{
				_stats[17].SetValue(value);
			}
		}

		public CastleMinerZPlayerStats()
		{
			if (CastleMinerZGame.Instance != null)
			{
				_steamAPI = new WeakReference<SteamWorks>(((SteamOnlineServices)CastleMinerZGame.Instance.LicenseServices).SteamAPI);
				SteamAPI.SetupStats(_stats);
				SetDefaultStats();
			}
		}

		public void AddStat(SessionStats.StatType statType)
		{
			sessionStats.AddStat(statType);
		}

		public int BlocksDugCount(BlockTypeEnum type)
		{
			int value;
			if (!BlocksDug.TryGetValue(type, out value))
			{
				return 0;
			}
			return value;
		}

		public void DugBlock(BlockTypeEnum type)
		{
			int value = 0;
			BlocksDug.TryGetValue(type, out value);
			value++;
			BlocksDug[type] = value;
		}

		public ItemStats GetItemStats(InventoryItemIDs ItemID)
		{
			ItemStats value;
			if (!AllItemStats.TryGetValue(ItemID, out value))
			{
				value = new ItemStats(ItemID);
				AllItemStats[ItemID] = value;
			}
			return value;
		}

		private void SetupStatsForWriting()
		{
			_stats[2].SetValue(Version);
			_stats[3].SetValue((float)TimeOnline.TotalHours);
			SteamAPI.MinimalUpdate();
		}

		protected override void SaveData(BinaryWriter writer)
		{
			if (SteamAPI != null)
			{
				SetupStatsForWriting();
				SteamAPI.StoreStats();
			}
			writer.Write(TimeOfPurchase.Ticks);
			writer.Write(FirstPlayTime.Ticks);
			writer.Write((float)TimeInTrial.TotalMinutes);
			writer.Write((float)TimeInFull.TotalMinutes);
			writer.Write((float)TimeInMenu.TotalMinutes);
			writer.Write(BlocksDug.Count);
			foreach (KeyValuePair<BlockTypeEnum, int> item in BlocksDug)
			{
				writer.Write((int)item.Key);
				writer.Write(item.Value);
			}
			writer.Write(AllItemStats.Count);
			foreach (KeyValuePair<InventoryItemIDs, ItemStats> allItemStat in AllItemStats)
			{
				writer.Write((int)allItemStat.Key);
				allItemStat.Value.Write(writer);
			}
			writer.Write(BanList.Count);
			foreach (KeyValuePair<ulong, DateTime> ban in BanList)
			{
				writer.Write(ban.Key);
				writer.Write(ban.Value.Ticks);
			}
			writer.Write(SecondTrayFaded);
			writer.Write(InvertYAxis);
			writer.Write(brightness);
			writer.Write(musicVolume);
			writer.Write(controllerSensitivity);
			writer.Write(AutoClimb);
			writer.Write((byte)DrawDistance);
			writer.Write(PostOnAchievement);
			writer.Write(PostOnHost);
			writer.Write(AutoClimb);
			InputBinding binding = CastleMinerZGame.Instance._controllerMapping.Binding;
			binding.SaveData(writer);
			writer.Write(musicMute);
		}

		protected void SetDefaultStats()
		{
			TimeInFull = TimeSpan.Zero;
			TimeInMenu = TimeSpan.Zero;
			TimeInTrial = TimeSpan.Zero;
			TimeOnline = TimeSpan.Zero;
		}

		protected override void LoadData(BinaryReader reader, int version)
		{
			if (SteamAPI != null)
			{
				SteamAPI.RetrieveStats();
			}
			TimeOfPurchase = new DateTime(reader.ReadInt64());
			if (version >= 0 && version <= 7)
			{
				FirstPlayTime = new DateTime(reader.ReadInt64());
				TimeInTrial = TimeSpan.FromMinutes(reader.ReadSingle());
				TimeInFull = TimeSpan.FromMinutes(reader.ReadSingle());
				TimeInMenu = TimeSpan.FromMinutes(reader.ReadSingle());
				int num = reader.ReadInt32();
				BlocksDug.Clear();
				for (int i = 0; i < num; i++)
				{
					BlocksDug[(BlockTypeEnum)reader.ReadInt32()] = reader.ReadInt32();
				}
				num = reader.ReadInt32();
				AllItemStats.Clear();
				for (int j = 0; j < num; j++)
				{
					InventoryItemIDs inventoryItemIDs = (InventoryItemIDs)reader.ReadInt32();
					ItemStats itemStats = new ItemStats(inventoryItemIDs);
					itemStats.Read(reader);
					AllItemStats[inventoryItemIDs] = itemStats;
				}
				num = reader.ReadInt32();
				BanList.Clear();
				for (int k = 0; k < num; k++)
				{
					ulong key = (ulong)reader.ReadInt64();
					BanList[key] = new DateTime(reader.ReadInt64());
				}
				if (version > 6)
				{
					SecondTrayFaded = reader.ReadBoolean();
				}
				InvertYAxis = reader.ReadBoolean();
				brightness = reader.ReadSingle();
				musicVolume = reader.ReadSingle();
				controllerSensitivity = reader.ReadSingle();
				AutoClimb = reader.ReadBoolean();
				DrawDistance = reader.ReadByte();
				PostOnAchievement = reader.ReadBoolean();
				PostOnHost = reader.ReadBoolean();
				AutoClimb = reader.ReadBoolean();
				if (version > 2)
				{
					InputBinding binding = CastleMinerZGame.Instance._controllerMapping.Binding;
					binding.LoadData(reader);
				}
				else
				{
					CastleMinerZGame.Instance._controllerMapping.SetToDefault();
				}
				if (version > 3)
				{
					musicMute = reader.ReadBoolean();
				}
				if (version == 4)
				{
					CastleMinerZGame.Instance._controllerMapping.SetToDefault();
				}
				if (version == 5)
				{
					CastleMinerZGame.Instance._controllerMapping.SetTrayDefaultKeys();
				}
			}
		}
	}
}
