using DNA.CastleMinerZ.UI;
using DNA.Drawing.Noise;
using System;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class OreDepositer : Biome
	{
		private IntNoise _noiseFunction = new IntNoise(new Random(1));

		public OreDepositer(WorldInfo worldInfo)
			: base(worldInfo)
		{
			_noiseFunction = new IntNoise(new Random(worldInfo.Seed));
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			int num = (int)(blender * 10f);
			for (int i = 0; i < 128; i++)
			{
				int y = i + minY;
				IntVector3 intVector = new IntVector3(worldX, y, worldZ);
				int num2 = terrain.MakeIndexFromWorldIndexVector(intVector);
				if (terrain._blocks[num2] == Biome.rockblock)
				{
					int num3 = _noiseFunction.ComputeNoise(intVector / 4);
					int num4 = _noiseFunction.ComputeNoise(intVector);
					num3 += (num4 - 128) / 8;
					if (num3 > 255 - num)
					{
						terrain._blocks[num2] = Biome.coalBlock;
					}
					else if (num3 < num - 5)
					{
						terrain._blocks[num2] = Biome.copperBlock;
					}
					GenerateLootBlock(terrain, i, intVector, num2);
					IntVector3 intVector2 = intVector + new IntVector3(1000, 1000, 1000);
					num3 = _noiseFunction.ComputeNoise(intVector2 / 3);
					num4 = _noiseFunction.ComputeNoise(intVector2);
					num3 += (num4 - 128) / 8;
					if (num3 > 264 - num)
					{
						terrain._blocks[num2] = Biome.ironBlock;
					}
					else if (num3 < -9 + num && i < 50)
					{
						terrain._blocks[num2] = Biome.goldBlock;
					}
					if (i < 50)
					{
						IntVector3 intVector3 = intVector + new IntVector3(777, 777, 777);
						num3 = _noiseFunction.ComputeNoise(intVector3 / 2);
						num4 = _noiseFunction.ComputeNoise(intVector3);
						num3 += (num4 - 128) / 8;
						if (num3 > 266 - num)
						{
							terrain._blocks[num2] = Biome.surfaceLavablock;
						}
						else if (num3 < -11 + num && i < 40)
						{
							terrain._blocks[num2] = Biome.diamondsBlock;
						}
					}
				}
				if (terrain._blocks[num2] == Biome.sandBlock || terrain._blocks[num2] == Biome.snowBlock || terrain._blocks[num2] == Biome.BloodSToneBlock)
				{
					GenerateLootBlock(terrain, i, intVector, num2);
				}
			}
		}

		private bool DoesModeAllowLootBlocks(GameModeTypes gameMode)
		{
			if (gameMode != GameModeTypes.Creative && gameMode != GameModeTypes.Scavenger)
			{
				return gameMode == GameModeTypes.Exploration;
			}
			return true;
		}

		private void GenerateLootBlock(BlockTerrain terrain, int worldLevel, IntVector3 worldPos, int index)
		{
			switch (CastleMinerZGame.Instance.GameMode)
			{
			case GameModeTypes.Endurance:
			case GameModeTypes.DragonEndurance:
				break;
			case GameModeTypes.Scavenger:
				GenerateLootBlockScavengerMode(terrain, worldLevel, worldPos, index);
				break;
			case GameModeTypes.Creative:
				GenerateLootBlockScavengerMode(terrain, worldLevel, worldPos, index);
				break;
			case GameModeTypes.Exploration:
				GenerateLootBlockSurvivalMode(terrain, worldLevel, worldPos, index);
				break;
			case GameModeTypes.Survival:
				GenerateLootBlockSurvivalMode(terrain, worldLevel, worldPos, index);
				break;
			}
		}

		private void GenerateLootBlockSurvivalMode(BlockTerrain terrain, int worldLevel, IntVector3 worldPos, int index)
		{
			IntVector3 intVector = worldPos + new IntVector3(333, 333, 333);
			if ((terrain._blocks[index] == Biome.sandBlock || terrain._blocks[index] == Biome.snowBlock) && worldLevel > 60)
			{
				return;
			}
			int num = _noiseFunction.ComputeNoise(intVector / 5);
			int num2 = _noiseFunction.ComputeNoise(intVector);
			int num3 = _noiseFunction.ComputeNoise(intVector / 2);
			num += (num2 - 128) / 8;
			if (num > 268)
			{
				if (num3 > 249 && (worldLevel < 55 || worldLevel >= 100))
				{
					terrain._blocks[index] = Biome.luckyLootBlock;
				}
				else if (num3 > 145)
				{
					terrain._blocks[index] = Biome.lootBlock;
				}
			}
		}

		private void GenerateLootBlockScavengerMode(BlockTerrain terrain, int worldLevel, IntVector3 worldPos, int index)
		{
			int num = 0;
			IntVector3 intVector = worldPos + new IntVector3(333, 333, 333);
			int num2 = _noiseFunction.ComputeNoise(intVector / 5);
			int num3 = _noiseFunction.ComputeNoise(intVector);
			int num4 = _noiseFunction.ComputeNoise(intVector / 2);
			num2 += (num3 - 128) / 8;
			if ((terrain._blocks[index] == Biome.sandBlock || terrain._blocks[index] == Biome.snowBlock) && worldLevel > 60)
			{
				num = 1;
			}
			if (num2 > 267 + num)
			{
				if (num4 > 250 + num * 3)
				{
					terrain._blocks[index] = Biome.luckyLootBlock;
				}
				else if (num4 > 165)
				{
					terrain._blocks[index] = Biome.lootBlock;
				}
			}
		}
	}
}
