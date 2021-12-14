using DNA.CastleMinerZ.Inventory;

namespace DNA.CastleMinerZ.Terrain
{
	public class SpawnData
	{
		public string id;

		public BlockTypeEnum blockTypeSource;

		public int minTier;

		public int maxTier;

		public int spawnPoints;

		public float spawnRate;

		public int waveCount;

		public float waveRate;

		public float surgeChance;

		public bool bossEnding;

		public int baseLuckyLootChance;

		public int luckyLootChancePerTier;

		public PackageBitFlags packageFlags;

		public SpawnData(BlockTypeEnum pBlockTypeSource, int pSpawnPoints, float pSpawnRate, int pWaveCount, float pWaveRate, int pMinTier, int pMaxTier, int pLuckyLootChance, int pLuckyLootChancePerTier, PackageBitFlags pPackageFlags)
		{
			id = pBlockTypeSource.ToString();
			blockTypeSource = pBlockTypeSource;
			spawnPoints = pSpawnPoints;
			spawnRate = pSpawnRate;
			waveCount = pWaveCount;
			waveRate = pWaveRate;
			minTier = pMinTier;
			maxTier = pMaxTier;
			baseLuckyLootChance = pLuckyLootChance;
			luckyLootChancePerTier = pLuckyLootChancePerTier;
			packageFlags = pPackageFlags;
		}

		public SpawnData Copy()
		{
			SpawnData spawnData = new SpawnData(blockTypeSource, spawnPoints, spawnRate, waveCount, waveRate, minTier, maxTier, baseLuckyLootChance, luckyLootChancePerTier, packageFlags);
			spawnData.surgeChance = surgeChance;
			spawnData.bossEnding = bossEnding;
			return spawnData;
		}
	}
}
