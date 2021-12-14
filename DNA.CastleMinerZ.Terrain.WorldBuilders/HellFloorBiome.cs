using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.UI;
using DNA.Drawing.Noise;
using DNA.Net.GamerServices;
using System;
using System.Collections.Generic;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class HellFloorBiome : Biome
	{
		private const int HellHeight = 32;

		private const int LavaLevel = 4;

		private const int MaxHillHeight = 32;

		private const float worldScale = 0.03125f;

		private const int cMaxBossesToSpawn = 50;

		private List<IntVector3> bossSpawnerLocs;

		private Random Rnd;

		private int bossSpawnBlockCountdown;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));

		private bool IsBossSpawnerGameMode()
		{
			if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Scavenger || CastleMinerZGame.Instance.GameMode == GameModeTypes.Survival || CastleMinerZGame.Instance.GameMode == GameModeTypes.Exploration || CastleMinerZGame.Instance.GameMode == GameModeTypes.Creative)
			{
				return true;
			}
			return false;
		}

		private int GetNextBossBlockCountdown(int spawnCount)
		{
			float num = 0.2f;
			float num2 = 1.1f;
			if (Rnd != null)
			{
				Rnd.RandomDouble(0f - num, num);
				double num3 = 1.0;
				int num4 = 4000 * (int)Math.Pow(spawnCount + 1, num2);
				return (int)(num3 * (double)num4);
			}
			return 0;
		}

		public HellFloorBiome(WorldInfo worldInfo)
			: base(worldInfo)
		{
			_noiseFunction = new PerlinNoise(new Random(worldInfo.Seed));
			InitializeBossSpawnParameters(worldInfo);
		}

		private void InitializeBossSpawnParameters(WorldInfo worldInfo)
		{
			Rnd = new Random(worldInfo.Seed);
			bossSpawnBlockCountdown = GetNextBossBlockCountdown(CastleMinerZGame.Instance.CurrentWorld.HellBossesSpawned);
			bossSpawnerLocs = new List<IntVector3>();
			if (CastleMinerZGame.Instance.CurrentWorld.MaxHellBossSpawns == 0)
			{
				CastleMinerZGame.Instance.CurrentWorld.MaxHellBossSpawns = 50;
			}
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			int num = 1;
			float num2 = 0f;
			int num3 = 4;
			for (int i = 0; i < num3; i++)
			{
				num2 += _noiseFunction.ComputeNoise(0.03125f * (float)worldX * (float)num + 200f, 0.03125f * (float)worldZ * (float)num + 200f) / (float)num;
				num *= 2;
			}
			int num4 = 4 + (int)(num2 * 10f) + 3;
			for (int j = 0; j < 32; j++)
			{
				int y = j + minY;
				IntVector3 intVector = new IntVector3(worldX, y, worldZ);
				int num5 = terrain.MakeIndexFromWorldIndexVector(intVector);
				if (j < num4)
				{
					terrain._blocks[num5] = Biome.BloodSToneBlock;
				}
				else if (j <= 4)
				{
					terrain._blocks[num5] = Biome.deepLavablock;
				}
				CheckForBossSpawns(terrain, intVector, num5, j, num4);
			}
		}

		private void CheckForBossSpawns(BlockTerrain terrain, IntVector3 worldPos, int index, int y, int groundlevel)
		{
			if (CastleMinerZGame.Instance.CurrentWorld.HellBossesSpawned >= CastleMinerZGame.Instance.CurrentWorld.MaxHellBossSpawns)
			{
				bossSpawnBlockCountdown = 0;
			}
			else if (bossSpawnBlockCountdown != 0 && y == groundlevel && y > 4)
			{
				bossSpawnBlockCountdown--;
				if (bossSpawnBlockCountdown <= 0)
				{
					CastleMinerZGame.Instance.CurrentWorld.HellBossesSpawned++;
					bossSpawnBlockCountdown = GetNextBossBlockCountdown(CastleMinerZGame.Instance.CurrentWorld.HellBossesSpawned);
					terrain._blocks[index] = Biome.bossSpawnOff;
					bossSpawnerLocs.Add(worldPos);
					long num = (long)worldPos.Z * (long)worldPos.Z;
					Math.Sqrt((long)worldPos.X * (long)worldPos.X + num);
				}
			}
		}

		private void ProcessBossSpawns()
		{
			if (!IsBossSpawnerGameMode() || CastleMinerZGame.Instance == null || CastleMinerZGame.Instance.CurrentNetworkSession == null || CastleMinerZGame.Instance.LocalPlayer == null || CastleMinerZGame.Instance.LocalPlayer.Gamer == null || CastleMinerZGame.Instance.LocalPlayer.Gamer.Session == null)
			{
				return;
			}
			for (int i = 0; i < bossSpawnerLocs.Count; i++)
			{
				if (i < bossSpawnerLocs.Count)
				{
					AlterBlockMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, bossSpawnerLocs[i], BlockTypeEnum.BossSpawnOff);
				}
			}
			if (bossSpawnerLocs.Count > 0)
			{
				bossSpawnerLocs.Clear();
			}
		}

		public void PostChunkProcess()
		{
			ProcessBossSpawns();
		}
	}
}
