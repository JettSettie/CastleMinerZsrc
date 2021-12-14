using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.UI;
using DNA.CastleMinerZ.Utils;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace DNA.CastleMinerZ.Terrain
{
	public class Spawner
	{
		private struct DifficultyData
		{
			public int depth;

			public int distance;

			public int days;

			public int players;

			public int spawnersDefeated;

			public GameDifficultyTypes difficultyMode;
		}

		public enum SpawnLightState
		{
			None,
			On,
			Off
		}

		public enum SpawnState
		{
			None,
			Chargeable,
			Listening,
			StartTriggered,
			Activated,
			Spawning,
			WaveIntermission,
			Depleted,
			Completed,
			RewardCollected,
			Reset
		}

		private const float c_LightOnTime = 0.8f;

		private const float c_LightOffTime = 0.3f;

		private const int c_MinimumRequiredSpawnValue = 100;

		private static readonly SpawnData[] _SpawnData = new SpawnData[6]
		{
			new SpawnData(BlockTypeEnum.EnemySpawnOff, 1000, 2f, 4, 5f, 1, 2, 20, 7, PackageBitFlags.None),
			new SpawnData(BlockTypeEnum.EnemySpawnRareOff, 1200, 0.5f, 3, 4f, 1, 3, 50, 10, PackageBitFlags.None),
			new SpawnData(BlockTypeEnum.AlienSpawnOff, 500, 4f, 3, 7f, 3, 3, 50, 10, PackageBitFlags.Alien),
			new SpawnData(BlockTypeEnum.AlienHordeOff, 1200, 2f, 3, 8f, 3, 3, 50, 10, PackageBitFlags.Alien),
			new SpawnData(BlockTypeEnum.HellSpawnOff, 1500, 4f, 3, 7f, 4, 7, 50, 10, PackageBitFlags.Hell),
			new SpawnData(BlockTypeEnum.BossSpawnOff, 1000, 4f, 1, 3f, 4, 7, 50, 10, PackageBitFlags.Epic | PackageBitFlags.Hell | PackageBitFlags.Boss)
		};

		public static int SpawnerDefeatedCount = 0;

		public static int TotalSpawnerCount = 0;

		public bool Destroyed;

		public LootGrade Tier;

		private IntVector3 _location;

		private SpawnState _currentState = SpawnState.Listening;

		private SpawnData _currentData;

		private string _currentSpawnID;

		private float _spawnTimer;

		private int _remainingSpawnValue;

		private int _currentlySpawnedEnemyCount;

		private int _remainingWaveCount;

		private BlockTypeEnum _currentBlockType;

		private SpawnLightState _spawnLightState;

		private List<int> _enemyIDs = new List<int>();

		private byte _lastPlayerID;

		private BlockTypeEnum _originalBlockType;

		private List<TargetSearchResult> _nearbyPlayers = new List<TargetSearchResult>();

		private SpawnBlockView spawnerView;

		public IntVector3 Location
		{
			get
			{
				return _location;
			}
		}

		public Spawner(IntVector3 location)
		{
			_location = location;
			_currentSpawnID = "spawner" + TotalSpawnerCount;
			TotalSpawnerCount++;
		}

		public Spawner(BinaryReader reader)
		{
			Read(reader);
		}

		public void SetSourceData(BlockTypeEnum blockSource)
		{
			_currentData = GetSpawnData(blockSource);
		}

		public void SetSourceFromID(string spawnID)
		{
			_currentData = GetSpawnData(spawnID);
		}

		public void StartSpawner(BlockTypeEnum blockSource)
		{
			spawnerView = new SpawnBlockView(Location, blockSource);
			SetState(SpawnState.StartTriggered);
			Player localPlayer = CastleMinerZGame.Instance.LocalPlayer;
			_originalBlockType = blockSource;
			_currentBlockType = GetActiveSpawnBlockType();
			GameUtils.ClearSurroundingBlocks(Location);
			TriggerNearbySpawnBlocks();
			spawnerView.SetBlockLight(true);
			SetSourceData(blockSource);
			_currentlySpawnedEnemyCount = 0;
			_remainingWaveCount = _currentData.waveCount;
			StartWave();
		}

		private Player GetRandomPlayer(List<TargetSearchResult> targetList)
		{
			if (targetList.Count == 0)
			{
				return CastleMinerZGame.Instance.LocalPlayer;
			}
			string text = "";
			foreach (TargetSearchResult target in targetList)
			{
				text = text + target.player.Gamer.Gamertag + ", ";
			}
			int num = MathTools.RandomInt(0, targetList.Count);
			DebugUtils.Log("TargetList: " + text + " Roll " + num + " = " + targetList[num].player);
			num = Math.Min(num, targetList.Count - 1);
			return targetList[num].player;
		}

		private void UpdateTargetList()
		{
			List<TargetSearchResult> list = TargetUtils.FindTargetsInRange(Location, 24f);
			foreach (TargetSearchResult item in list)
			{
				bool flag = true;
				foreach (TargetSearchResult nearbyPlayer in _nearbyPlayers)
				{
					if (nearbyPlayer.player == item.player)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					BroadcastTextMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, item.player.Gamer.Gamertag + " is a valid spawn target!");
				}
			}
			_nearbyPlayers.Clear();
			_nearbyPlayers = list;
		}

		public bool CanStart()
		{
			return _currentState == SpawnState.Listening;
		}

		private void StartWave()
		{
			_remainingSpawnValue += _currentData.spawnPoints / _currentData.waveCount;
			SetState(SpawnState.Activated);
		}

		public void HandleStopSpawningMessage()
		{
			StopSpawning();
			SetState(SpawnState.Depleted);
		}

		private void HandleWaveComplete()
		{
			_remainingWaveCount--;
			if (_remainingWaveCount <= 0)
			{
				HandleStopSpawningMessage();
				return;
			}
			SetState(SpawnState.WaveIntermission);
			_spawnTimer = _currentData.waveRate;
		}

		private DifficultyData GetCurrentDifficulty()
		{
			DifficultyData result = default(DifficultyData);
			result.spawnersDefeated = SpawnerDefeatedCount;
			result.difficultyMode = CastleMinerZGame.Instance.Difficulty;
			return result;
		}

		private float GetSoftCappedValue(float value, float softCap, float hardCap, float factor = 3f)
		{
			if (value > softCap)
			{
				value += (value - softCap) / factor;
			}
			if (value > hardCap)
			{
				value += (value - hardCap) / (factor * 10f);
			}
			return value;
		}

		private float GetSpawnPoints(float basePoints, DifficultyData diffData)
		{
			basePoints += 300f;
			basePoints += GetSoftCappedValue((float)diffData.spawnersDefeated * 150f, 1000f, 2000f);
			switch (diffData.difficultyMode)
			{
			default:
				basePoints *= 1f;
				break;
			case GameDifficultyTypes.HARD:
				basePoints *= 1.5f;
				break;
			}
			return basePoints;
		}

		private SpawnData GetMockSpawnData()
		{
			if ((MathTools.RandomInt(0, 1) == 1) ? true : false)
			{
				return _SpawnData[1];
			}
			return _SpawnData[0];
		}

		private void ApplyDifficultyModifiers(ref SpawnData spawnData, DifficultyData difficultyData)
		{
			float spawnPoints = GetSpawnPoints(spawnData.spawnPoints, difficultyData);
			spawnPoints = MathTools.RandomFloat(spawnPoints * 0.8f, spawnPoints * 1.2f);
			spawnData.spawnPoints = (int)spawnPoints;
			spawnData.maxTier = Math.Min(spawnData.maxTier + difficultyData.spawnersDefeated, 10);
		}

		private void GetNearbyPlayerList()
		{
		}

		private SpawnData GetSpawnData(string spawnID)
		{
			SpawnData spawnDataById = GetSpawnDataById(spawnID);
			return GetLocalSpawnData(spawnDataById);
		}

		private SpawnData GetSpawnData(BlockTypeEnum blockSource)
		{
			SpawnData spawnDataByBlockSource = GetSpawnDataByBlockSource(blockSource);
			return GetLocalSpawnData(spawnDataByBlockSource);
		}

		private SpawnData GetLocalSpawnData(SpawnData sourceSpawnData)
		{
			SpawnData spawnData = sourceSpawnData;
			if (spawnData == null)
			{
				spawnData = GetMockSpawnData();
			}
			SpawnData spawnData2 = spawnData.Copy();
			ApplyDifficultyModifiers(ref spawnData2, GetCurrentDifficulty());
			return spawnData2;
		}

		public void StopSpawning()
		{
			_remainingSpawnValue = 0;
			_spawnTimer = 0f;
		}

		private SpawnData GetSpawnDataById(string id)
		{
			SpawnData[] spawnData = _SpawnData;
			foreach (SpawnData spawnData2 in spawnData)
			{
				if (spawnData2.id.Equals(id))
				{
					return spawnData2;
				}
			}
			return null;
		}

		private SpawnData GetSpawnDataByBlockSource(BlockTypeEnum blockSource)
		{
			SpawnData[] spawnData = _SpawnData;
			foreach (SpawnData spawnData2 in spawnData)
			{
				if (blockSource == spawnData2.blockTypeSource)
				{
					return spawnData2;
				}
			}
			return null;
		}

		private void HandlePlayerDiedOrFled()
		{
			SetState(SpawnState.Listening);
		}

		private void HandleSpawnEvent()
		{
			if (_currentState == SpawnState.WaveIntermission)
			{
				StartWave();
				return;
			}
			UpdateTargetList();
			SetState(SpawnState.Spawning);
			LootResult randomSpawn = PossibleLootType.GetRandomSpawn(Location.Y, 1, 5, false, _remainingSpawnValue, _currentData.packageFlags);
			IntVector3 location = Location;
			location.Y++;
			int num = randomSpawn.value / randomSpawn.count;
			int value = num * randomSpawn.count;
			Vector3 newpos = location + Vector3.Zero;
			for (int i = 0; i < randomSpawn.count; i++)
			{
				string gamertag = GetRandomPlayer(_nearbyPlayers).Gamer.Gamertag;
				EnemyManager.Instance.SpawnEnemy(newpos, randomSpawn.spawnID, Location, num, gamertag);
				_currentlySpawnedEnemyCount++;
				DebugUtils.Log("Spawning Enemy #" + _currentlySpawnedEnemyCount + " Value " + num + " RemainingSpawnValue " + _remainingSpawnValue);
			}
			DeductSpawnValue(value);
		}

		private void ResetSpawnTimer()
		{
			_spawnTimer = _currentData.spawnRate;
		}

		public void SetState(SpawnState newState)
		{
			_currentState = newState;
			switch (newState)
			{
			case SpawnState.Spawning:
			case SpawnState.WaveIntermission:
			case SpawnState.RewardCollected:
				break;
			case SpawnState.Activated:
				ResetSpawnTimer();
				break;
			case SpawnState.Depleted:
				_spawnTimer = 0f;
				spawnerView.SetBlockLight(false);
				break;
			case SpawnState.Reset:
				ResetSpawner();
				break;
			case SpawnState.Completed:
			{
				Player localPlayer = CastleMinerZGame.Instance.LocalPlayer;
				if (_currentData.blockTypeSource == BlockTypeEnum.AlienSpawnOff)
				{
					PlaceRewardBlock(new IntVector3(Location.X, Location.Y, Location.Z + 1), GetRewardBlockType(Tier));
				}
				if (_currentData.blockTypeSource == BlockTypeEnum.BossSpawnOff)
				{
					PlaceRewardBlock(new IntVector3(Location.X + 1, Location.Y, Location.Z), GetRewardBlockType(Tier));
					PlaceRewardBlock(new IntVector3(Location.X - 1, Location.Y, Location.Z), GetRewardBlockType(Tier));
					if (MathTools.RandomInt(100) < 50)
					{
						PlaceRewardBlock(new IntVector3(Location.X, Location.Y - 1, Location.Z), GetRewardBlockType(Tier));
					}
					if (MathTools.RandomInt(100) < 50)
					{
						PlaceRewardBlock(new IntVector3(Location.X, Location.Y + 1, Location.Z), GetRewardBlockType(Tier));
					}
				}
				PlaceRewardBlock(Location, GetRewardBlockType(Tier));
				NetworkGamer gamerFromID = CastleMinerZGame.Instance.GetGamerFromID(_lastPlayerID);
				string gamertag = localPlayer.Gamer.Gamertag;
				if (gamerFromID != null)
				{
					gamertag = gamerFromID.Gamertag;
				}
				BroadcastTextMessage.Send((LocalNetworkGamer)localPlayer.Gamer, gamertag + " " + Strings.Has_extinguished_a_spawn_block);
				SpawnerDefeatedCount++;
				break;
			}
			}
		}

		private void PlaceRewardBlock(IntVector3 rewardBlockLocation, BlockTypeEnum rewardBlockType)
		{
			Player localPlayer = CastleMinerZGame.Instance.LocalPlayer;
			AlterBlockMessage.Send((LocalNetworkGamer)localPlayer.Gamer, rewardBlockLocation, rewardBlockType);
		}

		private BlockTypeEnum SetCurrentBlock(BlockTypeEnum blockType)
		{
			_currentBlockType = blockType;
			return _currentBlockType;
		}

		private void DeductSpawnValue(int value)
		{
			_remainingSpawnValue -= value;
			if (_remainingSpawnValue < 100)
			{
				HandleWaveComplete();
			}
			else
			{
				SetState(SpawnState.Activated);
			}
		}

		private void DeductEnemyCount()
		{
			_currentlySpawnedEnemyCount--;
			if (_currentState == SpawnState.Depleted && _currentlySpawnedEnemyCount <= 0)
			{
				SetState(SpawnState.Completed);
			}
		}

		public void HandleEnemyDefeated(int spawnValue, byte killerID)
		{
			_lastPlayerID = killerID;
			DeductEnemyCount();
		}

		public void HandleEnemyRemoved(int spawnValue)
		{
			if (_currentState != SpawnState.Listening)
			{
				DebugUtils.Log("HandleEnemyRemoved: " + _remainingSpawnValue + "(+" + spawnValue + ")EnemyCount " + _currentlySpawnedEnemyCount + " State " + _currentState);
				SetState(SpawnState.Reset);
			}
		}

		public bool IsHellBlock()
		{
			return _originalBlockType == BlockTypeEnum.HellSpawnOff;
		}

		public void HandleRewardCollected()
		{
			if (_currentState == SpawnState.Completed)
			{
				SetState(SpawnState.RewardCollected);
			}
		}

		public void UpdateSpawner(GameTime gameTime)
		{
			if (_currentState != 0 && _currentState != SpawnState.Spawning && _spawnTimer != 0f)
			{
				_spawnTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
				spawnerView.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
				if (_spawnTimer <= 0f)
				{
					HandleSpawnEvent();
				}
			}
		}

		private BlockTypeEnum GetRewardBlockType(LootGrade tier)
		{
			int num = _currentData.baseLuckyLootChance + (int)tier * _currentData.luckyLootChancePerTier;
			if (num < MathTools.RandomInt(1, 100))
			{
				return BlockTypeEnum.LuckyLootBlock;
			}
			return BlockTypeEnum.LootBlock;
		}

		public void ResetSpawner()
		{
			SetState(SpawnState.Listening);
			_spawnTimer = 0f;
			_currentBlockType = _originalBlockType;
			spawnerView.Reset();
			AlterBlockMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, Location, _currentBlockType);
			BroadcastTextMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, "No available targets. Spawner reset. Stay closer to avoid this!");
			EnemyManager.Instance.ResetSpawnerEnemies(this);
		}

		public void Write(BinaryWriter writer)
		{
			_location.Write(writer);
			writer.Write(_currentSpawnID);
		}

		public void Read(BinaryReader reader)
		{
			_location = IntVector3.Read(reader);
			_currentSpawnID = reader.ReadString();
		}

		private BlockTypeEnum GetActiveSpawnBlockType()
		{
			return spawnerView.GetActiveSpawnBlockType();
		}

		private void TriggerNearbySpawnBlocks()
		{
			int num = 10;
			for (int i = Location.Y; i < num + Location.Y; i++)
			{
				for (int j = Location.X - num; j < num + Location.X; j++)
				{
					for (int k = Location.Z - num; k < num + Location.Z; k++)
					{
						IntVector3 intVector = new IntVector3(j, i, k);
						if (intVector == Location)
						{
							continue;
						}
						BlockTypeEnum block = InGameHUD.GetBlock(intVector);
						if (!BlockType.IsSpawnerClickable(block))
						{
							continue;
						}
						Spawner spawner = CastleMinerZGame.Instance.CurrentWorld.GetSpawner(intVector, true, block);
						if (spawner.CanStart())
						{
							if (CastleMinerZGame.Instance.IsOnlineGame)
							{
								BroadcastTextMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, Strings.A_linked_spawner_was_also_triggered);
							}
							spawner.StartSpawner(block);
						}
					}
				}
			}
		}
	}
}
