using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Terrain;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DNA.CastleMinerZ.Inventory
{
	public class PossibleLootType
	{
		private const int ANY_MIN_WORLD_LEVEL = -1000;

		private const int ANY_MAX_WORLD_LEVEL = 1000;

		private const int SURFACE_LEVEL = 100;

		private const int HELL_LEVEL = -40;

		private const int TIER_1_LEVEL = 70;

		private const int TIER_2_LEVEL = 50;

		private const int TIER_3_LEVEL = 40;

		private const int RARE_LOOT_SEED = 20;

		private static readonly PossibleLootType[] _PossibleLootTypes = new PossibleLootType[49]
		{
			new PossibleLootType(InventoryItemIDs.Bullets, 100, 1, -1000, 1000, 50, 400),
			new PossibleLootType(InventoryItemIDs.IronBullets, 100, 2, -1000, 1000, 50, 300),
			new PossibleLootType(InventoryItemIDs.GoldBullets, 80, 3, -1000, 1000, 50, 250),
			new PossibleLootType(InventoryItemIDs.DiamondBullets, 60, 4, -1000, 1000, 50, 200),
			new PossibleLootType(InventoryItemIDs.LaserBullets, 35, 6, -1000, 1000, 50, 100),
			new PossibleLootType(InventoryItemIDs.LaserBullets, 15, 6, -1000, 1000, 300, 400),
			new PossibleLootType(InventoryItemIDs.Copper, 150, 1, -1000, 1000, 3, 6),
			new PossibleLootType(InventoryItemIDs.Iron, 130, 1, -1000, 1000, 3, 6),
			new PossibleLootType(InventoryItemIDs.Gold, 120, 3, -1000, 1000, 2, 4),
			new PossibleLootType(InventoryItemIDs.Diamond, 50, 6, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.Diamond, 10, 8, -1000, 1000, 2, 3),
			new PossibleLootType(InventoryItemIDs.BloodStoneBlock, 75, 5, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.BloodStoneBlock, 10, 7, -1000, 1000, 8, 10),
			new PossibleLootType(InventoryItemIDs.SpaceRockInventory, 45, 7, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.SpaceRockInventory, 15, 7, -1000, 1000, 3, 5),
			new PossibleLootType(InventoryItemIDs.Slime, 30, 7, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.Slime, 6, 8, -1000, 1000, 3, 5),
			new PossibleLootType(InventoryItemIDs.IronContainer, 50, 6, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.Torch, 75, 2, -1000, 1000, 2, 8),
			new PossibleLootType(InventoryItemIDs.WoodBlock, 70, 1, -1000, 1000, 2, 4),
			new PossibleLootType(InventoryItemIDs.ExplosivePowder, 50, 6, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.ExplosivePowder, 10, 8, -1000, 1000, 4, 6),
			new PossibleLootType(InventoryItemIDs.GoldLaserSword, 5, 6, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.DiamondKnife, 30, 5, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.Grenade, 50, 6, -1000, 1000, 1, 2),
			new PossibleLootType(InventoryItemIDs.StickyGrenade, 50, 7, -1000, 1000, 1, 2),
			new PossibleLootType(InventoryItemIDs.C4, 30, 7, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.TNT, 30, 7, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.RocketLauncher, 20, 5, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.IronSpacePistol, 6, 8, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.IronSpaceBoltActionRifle, 5, 8, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.IronSpaceAssultRifle, 4, 8, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.IronSpacePumpShotgun, 5, 8, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.IronSpaceSMGGun, 4, 8, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.BloodStonePistol, 15, 8, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.DiamondLMGGun, 20, 7, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.GoldAssultRifle, 25, 6, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.GoldBoltActionRifle, 35, 5, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.GoldLMGGun, 25, 6, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.GoldPistol, 35, 5, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.GoldPumpShotgun, 25, 6, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.GoldSMGGun, 25, 6, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.LaserDrill, 5, 8, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.BloodstonePickAxe, 7, 7, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.DiamondPickAxe, 20, 6, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.GoldPickAxe, 30, 3, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.IronPickAxe, 50, 2, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.DiamondSpade, 25, 5, -1000, 1000),
			new PossibleLootType(InventoryItemIDs.DiamondAxe, 25, 5, -1000, 1000)
		};

		public static readonly PossibleLootType[] _PossibleSpawnTypes = new PossibleLootType[19]
		{
			new PossibleLootType(EnemyTypeEnum.ARCHER_0_0, 100, 1, -1000, 1000),
			new PossibleLootType(EnemyTypeEnum.ARCHER_0_0, 200, 1, -1000, 1000, 4, 4, 250),
			new PossibleLootType(EnemyTypeEnum.ARCHER_1_0, 100, 3, -1000, 1000),
			new PossibleLootType(EnemyTypeEnum.SKEL_0_0, 100, 2, -1000, 1000),
			new PossibleLootType(EnemyTypeEnum.SKEL_0_1, 100, 2, -1000, 1000),
			new PossibleLootType(EnemyTypeEnum.SKEL_0_1, 120, 2, -1000, 1000, 2, 3, 200),
			new PossibleLootType(EnemyTypeEnum.SKEL_0_2, 100, 2, -1000, 1000),
			new PossibleLootType(EnemyTypeEnum.SKEL_0_3, 100, 3, -1000, 1000),
			new PossibleLootType(EnemyTypeEnum.ZOMBIE_0_0, 100, 1, -1000, 1000),
			new PossibleLootType(EnemyTypeEnum.ZOMBIE_0_0, 150, 1, -1000, 1000, 3, 3, 200),
			new PossibleLootType(EnemyTypeEnum.ZOMBIE_0_1, 60, 1, -1000, 1000),
			new PossibleLootType(EnemyTypeEnum.ZOMBIE_0_3, 35, 2, -1000, 1000),
			new PossibleLootType(EnemyTypeEnum.ZOMBIE_0_3, 35, 2, -1000, 1000, 250, PackageBitFlags.Hell),
			new PossibleLootType(EnemyTypeEnum.ZOMBIE_0_4, 15, 3, -1000, 1000),
			new PossibleLootType(EnemyTypeEnum.ALIEN, 100, 3, -1000, 1000, 100, PackageBitFlags.Alien),
			new PossibleLootType(EnemyTypeEnum.ALIEN, 20, 3, -1000, 1000, 2, 2, 200, PackageBitFlags.Alien),
			new PossibleLootType(EnemyTypeEnum.ALIEN, 15, 4, -1000, 1000, 250, PackageBitFlags.Epic | PackageBitFlags.Alien),
			new PossibleLootType(EnemyTypeEnum.FELGUARD, 15, 4, -1000, 1000, 500, PackageBitFlags.Hell),
			new PossibleLootType(EnemyTypeEnum.HELL_LORD, 10000, 5, -1000, 1000, 1000, PackageBitFlags.Boss)
		};

		public string Name;

		public InventoryItemIDs itemID;

		public int weight;

		public int lootTier = 1;

		public int minWorldLevel;

		public int maxWorldLevel;

		public int minQuantity = 1;

		public int maxQuantity = 1;

		public LootGrade lootGrade;

		public int worth = 100;

		public SpawnType spawnType = SpawnType.Loot;

		public EnemyTypeEnum enemyType;

		public PackageBitFlags packageFlags = PackageBitFlags.Common;

		public override string ToString()
		{
			return Name;
		}

		private PossibleLootType(EnemyTypeEnum pItemID, int pSeed, int pLootTier, int pMinDepth, int pMaxDepth)
		{
			enemyType = pItemID;
			weight = pSeed;
			lootTier = pLootTier;
			minWorldLevel = pMinDepth;
			maxWorldLevel = pMaxDepth;
			worth = 100;
		}

		private PossibleLootType(EnemyTypeEnum pItemID, int pSeed, int pLootTier, int pMinDepth, int pMaxDepth, int pWorth, PackageBitFlags pPackageFlags)
		{
			enemyType = pItemID;
			weight = pSeed;
			lootTier = pLootTier;
			minWorldLevel = pMinDepth;
			maxWorldLevel = pMaxDepth;
			packageFlags = pPackageFlags;
			worth = pWorth;
		}

		private PossibleLootType(EnemyTypeEnum pItemID, int pSeed, int pLootTier, int pMinDepth, int pMaxDepth, int pMinCount, int pMaxCount, int pWorth, PackageBitFlags pPackageFlags)
		{
			enemyType = pItemID;
			weight = pSeed;
			lootTier = pLootTier;
			minWorldLevel = pMinDepth;
			maxWorldLevel = pMaxDepth;
			packageFlags = pPackageFlags;
			minQuantity = pMinCount;
			maxQuantity = pMaxCount;
			worth = pWorth;
		}

		private PossibleLootType(EnemyTypeEnum pItemID, int pSeed, int pLootTier, int pMinDepth, int pMaxDepth, int pMinCount, int pMaxCount, int pWorth)
		{
			enemyType = pItemID;
			weight = pSeed;
			lootTier = pLootTier;
			minWorldLevel = pMinDepth;
			maxWorldLevel = pMaxDepth;
			minQuantity = pMinCount;
			maxQuantity = pMaxCount;
			worth = pWorth;
		}

		private PossibleLootType(InventoryItemIDs pItemID, int pSeed, int pLootTier, int pMinDepth, int pMaxDepth)
		{
			itemID = pItemID;
			weight = pSeed;
			lootTier = pLootTier;
			minWorldLevel = pMinDepth;
			maxWorldLevel = pMaxDepth;
			worth = 100;
		}

		private PossibleLootType(InventoryItemIDs pItemID, int pSeed, int pLootTier, int pMinDepth, int pMaxDepth, int pMinQuantity, int pMaxQuantity)
		{
			itemID = pItemID;
			weight = pSeed;
			lootTier = pLootTier;
			minWorldLevel = pMinDepth;
			maxWorldLevel = pMaxDepth;
			minQuantity = pMinQuantity;
			maxQuantity = pMaxQuantity;
			worth = 100;
		}

		public static PossibleLootType[] GetLootSource()
		{
			return _PossibleLootTypes;
		}

		public static PossibleLootType[] GetEnemySource()
		{
			return _PossibleSpawnTypes;
		}

		public static LootResult GetRandomSpawn(int worldLevel, int minTier, int maxTier, bool rareLoot, int maxValue, PackageBitFlags permittedFlags)
		{
			PossibleLootType[] enemySource = GetEnemySource();
			return GetRandomResult(enemySource, worldLevel, minTier, maxTier, rareLoot, maxValue, permittedFlags);
		}

		public static LootResult GetRandomLoot(int worldLevel, int minTier, int maxTier, bool rareLoot)
		{
			PossibleLootType[] lootSource = GetLootSource();
			return GetRandomResult(lootSource, worldLevel, minTier, maxTier, rareLoot, int.MaxValue);
		}

		public static LootResult GetRandomResult(PossibleLootType[] lootSource, int worldLevel, int minTier, int maxTier, bool rareLoot, int maxValue, PackageBitFlags permittedFlags = PackageBitFlags.None)
		{
			int num = 0;
			List<PossibleLootType> list = new List<PossibleLootType>();
			for (int i = 0; i < lootSource.Length; i++)
			{
				PossibleLootType possibleLootType = lootSource[i];
				if ((!rareLoot || possibleLootType.IsRareLoot()) && ((permittedFlags == PackageBitFlags.None && possibleLootType.packageFlags == PackageBitFlags.Common) || HasPackageFlag(permittedFlags, possibleLootType.packageFlags)) && (lootSource[i].minWorldLevel == -1000 || worldLevel >= lootSource[i].minWorldLevel) && (lootSource[i].maxWorldLevel == 1000 || worldLevel <= lootSource[i].maxWorldLevel) && possibleLootType.lootTier >= minTier && possibleLootType.lootTier <= maxTier && possibleLootType.worth <= maxValue)
				{
					list.Add(possibleLootType);
					num += possibleLootType.weight;
				}
			}
			int num2 = MathTools.RandomInt(num);
			LootResult result = default(LootResult);
			result.lootItemID = InventoryItemIDs.Snow;
			result.count = 1;
			result.value = 500;
			for (int j = 0; j < list.Count; j++)
			{
				num2 -= list[j].weight;
				if (num2 <= 0)
				{
					result.lootItemID = list[j].itemID;
					result.spawnID = list[j].enemyType;
					result.count = MathTools.RandomInt(list[j].minQuantity, list[j].maxQuantity);
					result.value = list[j].worth;
					break;
				}
			}
			list.Clear();
			return result;
		}

		public static bool HasPackageFlag(PackageBitFlags availableFlags, PackageBitFlags flagToCheck)
		{
			if (flagToCheck == PackageBitFlags.None || availableFlags.HasFlag(flagToCheck))
			{
				return true;
			}
			return false;
		}

		private bool IsRareLoot()
		{
			return weight <= 20;
		}

		public static void PlaceWorldItem(InventoryItem item, IntVector3 intPos)
		{
			if (item != null)
			{
				PickupManager.Instance.CreatePickup(item, IntVector3.ToVector3(intPos) + new Vector3(0.5f, 0.5f, 0.5f), false, true);
			}
		}

		public static void ProcessLootBlockOutput(BlockTypeEnum blockType, IntVector3 intPos)
		{
			SessionStats.StatType statType = SessionStats.StatType.LootBlockOpened;
			int num = 1;
			int minTier = 1;
			int maxTier = 5;
			if (MathTools.RandomInt(100) < 8)
			{
				num = 1;
			}
			if (blockType == BlockTypeEnum.LuckyLootBlock)
			{
				minTier = 4;
				maxTier = 10;
				num++;
				PlaceWorldItem(GetLootItem(blockType, intPos, minTier, maxTier, true), intPos);
				statType = SessionStats.StatType.LuckyLootBlockOpened;
			}
			CastleMinerZGame.Instance.PlayerStats.AddStat(statType);
			for (int i = 0; i < num; i++)
			{
				PlaceWorldItem(GetLootItem(blockType, intPos, minTier, maxTier), intPos);
			}
		}

		private static InventoryItem GetLootItem(BlockTypeEnum blockType, IntVector3 intPos, int minTier, int maxTier, bool rareLoot = false)
		{
			LootResult randomLoot = GetRandomLoot(intPos.Y, minTier, maxTier, rareLoot);
			return InventoryItem.CreateItem(randomLoot.lootItemID, randomLoot.count);
		}
	}
}
