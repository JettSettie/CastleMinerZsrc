using DNA.CastleMinerZ.UI;
using DNA.Drawing.Noise;
using Microsoft.Xna.Framework;
using System;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class CaveBiome : Biome
	{
		private const float caveDensity = 0.0625f;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));

		private int lootBlockModifier = 5000;

		private int luckyLootBlockModifier = 10001;

		private int enemyBlockModifier = 2100;

		private int emptyBlockCount;

		public CaveBiome(WorldInfo worldInfo)
			: base(worldInfo)
		{
			_noiseFunction = new PerlinNoise(new Random(worldInfo.Seed));
		}

		private int GetEnemyBlock(IntVector3 worldPos, float noise)
		{
			int enemyBlockRareOff2 = Biome.enemyBlockRareOff;
			int enemyBlockOff2 = Biome.enemyBlockOff;
			int[] array = new int[3]
			{
				Biome.enemyBlockRareOff,
				Biome.enemyBlockOff,
				Biome.alienSpawnOff
			};
			int enemyBlockRareOff3 = Biome.enemyBlockRareOff;
			int enemyBlockOff3 = Biome.enemyBlockOff;
			int alienSpawnOff2 = Biome.alienSpawnOff;
			int hellSpawnOff2 = Biome.hellSpawnOff;
			int[] array2 = array;
			int num = MathTools.RandomInt(array2.Length);
			return array2[num];
		}

		private int GetEnemyBlockOverride(int blockID, IntVector3 worldPos, float noise)
		{
			float num = 0.5f;
			float num2 = 0f;
			float num3 = 0.35f;
			if (noise < num3 * num)
			{
				blockID = Biome.hellSpawnOff;
			}
			else if (noise < num3 * num2)
			{
				blockID = Biome.alienSpawnOff;
			}
			return blockID;
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			bool flag = true;
			for (int i = 0; i < 128; i++)
			{
				int y = i + minY;
				IntVector3 intVector = new IntVector3(worldX, y, worldZ);
				int num = terrain.MakeIndexFromWorldIndexVector(intVector);
				int num2 = terrain._blocks[num];
				if (Biome.emptyblock != num2 && Biome.uninitblock != num2 && Biome.sandBlock != num2)
				{
					Vector3 vector = intVector * 0.0625f * new Vector3(1f, 1.5f, 1f);
					float num3 = _noiseFunction.ComputeNoise(vector);
					num3 += _noiseFunction.ComputeNoise(vector * 2f) / 2f;
					if (!(num3 < -0.35f))
					{
						continue;
					}
					emptyBlockCount++;
					terrain._blocks[num] = Biome.emptyblock;
					if (flag && terrain._blocks[num] != Biome.dirtblock && terrain._blocks[num] != Biome.grassblock)
					{
						if (emptyBlockCount % lootBlockModifier == 0)
						{
							terrain._blocks[num] = Biome.lootBlock;
						}
						if (emptyBlockCount % enemyBlockModifier == 0)
						{
							terrain._blocks[num] = GetEnemyBlock(intVector, num3);
						}
						if (emptyBlockCount % luckyLootBlockModifier == 0)
						{
							terrain._blocks[num] = Biome.luckyLootBlock;
						}
						flag = false;
					}
				}
				else
				{
					flag = true;
				}
			}
		}

		public void SetLootModifiersByGameMode()
		{
			switch (CastleMinerZGame.Instance.GameMode)
			{
			case GameModeTypes.Scavenger:
				lootBlockModifier = 150;
				luckyLootBlockModifier = 1000;
				enemyBlockModifier = 200;
				break;
			case GameModeTypes.Creative:
				lootBlockModifier = 1000000;
				luckyLootBlockModifier = 1000000;
				enemyBlockModifier = 2000;
				break;
			case GameModeTypes.Exploration:
				lootBlockModifier = 2100;
				luckyLootBlockModifier = 7000;
				enemyBlockModifier = 1000;
				break;
			case GameModeTypes.Survival:
				lootBlockModifier = 20000;
				luckyLootBlockModifier = 35000;
				enemyBlockModifier = 2000;
				break;
			case GameModeTypes.DragonEndurance:
				lootBlockModifier = 1000000;
				luckyLootBlockModifier = 1000000;
				break;
			case GameModeTypes.Endurance:
				lootBlockModifier = 1000000;
				luckyLootBlockModifier = 1000000;
				break;
			}
		}
	}
}
