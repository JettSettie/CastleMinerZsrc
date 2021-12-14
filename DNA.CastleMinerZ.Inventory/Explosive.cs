using DNA.Audio;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.Collections;
using DNA.Drawing;
using DNA.Drawing.Particles;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace DNA.CastleMinerZ.Inventory
{
	public class Explosive : IEquatable<Explosive>
	{
		public enum EnemyBreakBlocksResult
		{
			BlocksWillBreak,
			BlocksWillNotBreak,
			BlocksBroken,
			RegionIsEmpty
		}

		private const float BLOCK_DAMAGE_RADIUS = 5.5f;

		private static readonly int[] cDestructionRanges;

		private static readonly float[] cEnemyDamageRanges;

		private static readonly float[] cDamageRanges;

		private static readonly float[] cKillRanges;

		public IntVector3 Position;

		public OneShotTimer Timer = new OneShotTimer(TimeSpan.FromSeconds(4.0));

		public ExplosiveTypes ExplosiveType;

		private static ParticleEffect _flashEffect;

		private static ParticleEffect _firePuffEffect;

		private static ParticleEffect _smokePuffEffect;

		private static ParticleEffect _rockBlastEffect;

		private static ParticleEffect _digSmokeEffect;

		private static ParticleEffect _digRocksEffect;

		private static bool[,] BreakLookup;

		private static bool[] Level1Hardness;

		private static bool[] Level2Hardness;

		private static readonly IntVector3 _sMaxBufferBounds;

		private static Queue<Explosive> _sEnemyDiggingTNTToExplode;

		private static Set<IntVector3> _sEnemyDiggingBlocksToRemove;

		private static Dictionary<IntVector3, BlockTypeEnum> _sEnemyDiggingDependentsToRemove;

		private static readonly Vector3 _sHalf;

		static Explosive()
		{
			cDestructionRanges = new int[6]
			{
				2,
				3,
				3,
				0,
				1,
				0
			};
			cEnemyDamageRanges = new float[6]
			{
				12f,
				24f,
				15f,
				1f,
				8f,
				1f
			};
			cDamageRanges = new float[6]
			{
				6f,
				12f,
				7f,
				1f,
				5f,
				1f
			};
			cKillRanges = new float[6]
			{
				3f,
				6f,
				4f,
				1f,
				2.5f,
				1f
			};
			_sMaxBufferBounds = new IntVector3(383, 127, 383);
			_sEnemyDiggingTNTToExplode = new Queue<Explosive>();
			_sEnemyDiggingBlocksToRemove = new Set<IntVector3>();
			_sEnemyDiggingDependentsToRemove = new Dictionary<IntVector3, BlockTypeEnum>();
			_sHalf = new Vector3(0.5f);
			_flashEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\FlashEffect");
			_firePuffEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\FirePuff");
			_smokePuffEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\BigSmokePuff");
			_rockBlastEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\BigRockBlast");
			_digSmokeEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\SmokeEffect");
			_digRocksEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\RocksEffect");
			BreakLookup = new bool[6, 95];
			Level1Hardness = new bool[95];
			Level2Hardness = new bool[95];
			for (int i = 0; i < 6; i++)
			{
				BreakLookup[i, 0] = true;
				BreakLookup[i, 5] = true;
				BreakLookup[i, 14] = true;
				BreakLookup[i, 94] = true;
				BreakLookup[i, 43] = true;
				BreakLookup[i, 21] = true;
				BreakLookup[i, 44] = true;
			}
			BreakLookup[0, 4] = true;
			BreakLookup[0, 67] = true;
			BreakLookup[0, 7] = true;
			BreakLookup[0, 8] = true;
			BreakLookup[0, 9] = true;
			BreakLookup[0, 10] = true;
			BreakLookup[0, 11] = true;
			BreakLookup[0, 22] = true;
			BreakLookup[0, 23] = true;
			BreakLookup[0, 24] = true;
			BreakLookup[0, 25] = true;
			BreakLookup[0, 20] = true;
			BreakLookup[0, 45] = true;
			BreakLookup[0, 46] = true;
			BreakLookup[0, 48] = true;
			BreakLookup[0, 47] = true;
			BreakLookup[0, 56] = true;
			BreakLookup[0, 57] = true;
			BreakLookup[0, 58] = true;
			BreakLookup[0, 59] = true;
			BreakLookup[3, 4] = true;
			BreakLookup[3, 67] = true;
			BreakLookup[3, 7] = true;
			BreakLookup[3, 8] = true;
			BreakLookup[3, 9] = true;
			BreakLookup[3, 10] = true;
			BreakLookup[3, 11] = true;
			BreakLookup[3, 22] = true;
			BreakLookup[3, 23] = true;
			BreakLookup[3, 24] = true;
			BreakLookup[3, 25] = true;
			BreakLookup[3, 20] = true;
			BreakLookup[3, 45] = true;
			BreakLookup[3, 46] = true;
			BreakLookup[3, 48] = true;
			BreakLookup[3, 47] = true;
			BreakLookup[3, 56] = true;
			BreakLookup[3, 57] = true;
			BreakLookup[3, 58] = true;
			BreakLookup[3, 59] = true;
			BreakLookup[4, 4] = true;
			BreakLookup[4, 67] = true;
			BreakLookup[4, 7] = true;
			BreakLookup[4, 8] = true;
			BreakLookup[4, 9] = true;
			BreakLookup[4, 10] = true;
			BreakLookup[4, 11] = true;
			BreakLookup[4, 22] = true;
			BreakLookup[4, 23] = true;
			BreakLookup[4, 24] = true;
			BreakLookup[4, 25] = true;
			BreakLookup[4, 20] = true;
			BreakLookup[4, 45] = true;
			BreakLookup[4, 46] = true;
			BreakLookup[4, 48] = true;
			BreakLookup[4, 47] = true;
			BreakLookup[4, 56] = true;
			BreakLookup[4, 57] = true;
			BreakLookup[4, 58] = true;
			BreakLookup[4, 59] = true;
			BreakLookup[5, 4] = true;
			BreakLookup[5, 67] = true;
			BreakLookup[5, 22] = true;
			BreakLookup[5, 23] = true;
			BreakLookup[5, 24] = true;
			BreakLookup[5, 25] = true;
			BreakLookup[5, 20] = true;
			BreakLookup[5, 45] = true;
			BreakLookup[5, 46] = true;
			BreakLookup[5, 48] = true;
			BreakLookup[5, 47] = true;
			BreakLookup[5, 56] = true;
			BreakLookup[5, 57] = true;
			BreakLookup[5, 58] = true;
			BreakLookup[5, 59] = true;
			for (int j = 0; j < 95; j++)
			{
				switch (BlockType.GetType((BlockTypeEnum)j).Hardness)
				{
				case 1:
					Level1Hardness[j] = true;
					break;
				case 2:
					Level1Hardness[j] = true;
					break;
				case 3:
					Level2Hardness[j] = true;
					break;
				case 4:
					Level2Hardness[j] = true;
					break;
				}
			}
		}

		public Explosive(IntVector3 position, ExplosiveTypes explosiveType)
		{
			Position = position;
			ExplosiveType = explosiveType;
		}

		public bool Equals(Explosive other)
		{
			if (other.Position == Position && other.ExplosiveType == ExplosiveType)
			{
				return true;
			}
			return false;
		}

		public void Update(TimeSpan gameTime)
		{
			if (!Timer.Expired)
			{
				Timer.Update(gameTime);
				BlockTypeEnum blockWithChanges = BlockTerrain.Instance.GetBlockWithChanges(Position);
				BlockTypeEnum blockTypeEnum = (ExplosiveType == ExplosiveTypes.TNT) ? BlockTypeEnum.TNT : BlockTypeEnum.C4;
				if (Timer.Expired && blockWithChanges == blockTypeEnum)
				{
					DetonateExplosiveMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, Position, true, ExplosiveType);
				}
			}
		}

		public static void HandleDetonateRocketMessage(DetonateRocketMessage msg)
		{
			AddEffects(msg.Location, !msg.HitDragon);
			if (msg.HitDragon)
			{
				if (EnemyManager.Instance != null && EnemyManager.Instance.DragonIsActive)
				{
					EnemyManager.Instance.ApplyExplosiveDamageToDragon(msg.Location, 200f, msg.Sender.Id, msg.ItemType);
				}
			}
			else
			{
				ApplySplashDamageToLocalPlayerAndZombies(msg.Location, msg.ExplosiveType, msg.ItemType, msg.Sender.Id);
			}
		}

		public static void DetonateGrenade(Vector3 position, ExplosiveTypes grenadeType, byte shooterID, bool wantRockChunks)
		{
			AddEffects(position, wantRockChunks);
			ApplySplashDamageToLocalPlayerAndZombies(position, grenadeType, InventoryItemIDs.Grenade, shooterID);
		}

		private static void ApplySplashDamageToLocalPlayerAndZombies(Vector3 location, ExplosiveTypes explosiveType, InventoryItemIDs itemType, byte shooterID)
		{
			if (CastleMinerZGame.Instance.LocalPlayer != null && CastleMinerZGame.Instance.LocalPlayer.ValidLivingGamer)
			{
				Vector3 worldPosition = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
				worldPosition.Y += 1f;
				float num = Vector3.Distance(worldPosition, location);
				float num2 = cKillRanges[(int)explosiveType];
				float num3 = cDamageRanges[(int)explosiveType];
				if (num < num3)
				{
					DamageLOSProbe damageLOSProbe = new DamageLOSProbe();
					damageLOSProbe.Init(location, worldPosition);
					damageLOSProbe.DragonTypeIndex = 0;
					BlockTerrain.Instance.Trace(damageLOSProbe);
					float num4 = 0f;
					num4 = ((!(num < num2)) ? (damageLOSProbe.TotalDamageMultiplier * (1f - (num - num2) / (num3 - num2))) : 1f);
					InGameHUD.Instance.ApplyDamage(num4, location);
				}
				if (EnemyManager.Instance != null)
				{
					EnemyManager.Instance.ApplyExplosiveDamageToZombies(location, cEnemyDamageRanges[(int)explosiveType], shooterID, itemType);
				}
			}
		}

		public static void HandleDetonateExplosiveMessage(DetonateExplosiveMessage msg)
		{
			BlockTerrain.Instance.SetBlock(msg.Location, BlockTypeEnum.Empty);
			if (CastleMinerZGame.Instance.GameScreen != null)
			{
				CastleMinerZGame.Instance.GameScreen.RemoveExplosiveFlashModel(msg.Location);
			}
			if (msg.OriginalExplosion)
			{
				AddEffects(msg.Location, true);
			}
			if (CastleMinerZGame.Instance.LocalPlayer != null && CastleMinerZGame.Instance.LocalPlayer.ValidLivingGamer)
			{
				ApplySplashDamageToLocalPlayerAndZombies(msg.Location, msg.ExplosiveType, (msg.ExplosiveType == ExplosiveTypes.TNT) ? InventoryItemIDs.TNT : InventoryItemIDs.C4, msg.Sender.Id);
			}
			if (msg.Sender.IsLocal && msg.OriginalExplosion)
			{
				FindBlocksToRemove(msg.Location, msg.ExplosiveType, false);
			}
		}

		private static void RememberDependentObjects(IntVector3 worldIndex, Dictionary<IntVector3, BlockTypeEnum> dependentsToRemove)
		{
			for (BlockFace blockFace = BlockFace.POSX; blockFace < BlockFace.NUM_FACES; blockFace++)
			{
				IntVector3 neighborIndex = BlockTerrain.Instance.GetNeighborIndex(worldIndex, blockFace);
				if (!dependentsToRemove.ContainsKey(neighborIndex))
				{
					BlockTypeEnum blockWithChanges = BlockTerrain.Instance.GetBlockWithChanges(neighborIndex);
					if (BlockType.GetType(blockWithChanges).Facing == blockFace)
					{
						dependentsToRemove.Add(neighborIndex, blockWithChanges);
					}
				}
			}
		}

		private static void ProcessOneExplosion(Queue<Explosive> tntToExplode, Set<IntVector3> blocksToRemove, Dictionary<IntVector3, BlockTypeEnum> dependentsToRemove, ref bool explosionFlashNotYetShown)
		{
			Explosive explosive = tntToExplode.Dequeue();
			if (explosionFlashNotYetShown && (explosive.ExplosiveType == ExplosiveTypes.C4 || explosive.ExplosiveType == ExplosiveTypes.TNT))
			{
				AddExplosionEffectsMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, explosive.Position);
				explosionFlashNotYetShown = false;
			}
			int num = cDestructionRanges[(int)explosive.ExplosiveType];
			IntVector3 zero = IntVector3.Zero;
			zero.X = -num;
			IntVector3 intVector = default(IntVector3);
			IntVector3 a = default(IntVector3);
			while (zero.X <= num)
			{
				intVector.X = explosive.Position.X + zero.X;
				a.X = intVector.X - BlockTerrain.Instance._worldMin.X;
				if (a.X >= 0 && a.X < 384)
				{
					zero.Z = -num;
					while (zero.Z <= num)
					{
						intVector.Z = explosive.Position.Z + zero.Z;
						a.Z = intVector.Z - BlockTerrain.Instance._worldMin.Z;
						if (a.Z >= 0 && a.Z < 384)
						{
							zero.Y = -num;
							while (zero.Y <= num)
							{
								intVector.Y = explosive.Position.Y + zero.Y;
								a.Y = intVector.Y - BlockTerrain.Instance._worldMin.Y;
								if (a.Y >= 0 && a.Y < 128 && !blocksToRemove.Contains(intVector))
								{
									BlockTypeEnum typeIndex = Block.GetTypeIndex(BlockTerrain.Instance.GetBlockAt(a));
									if (typeIndex == BlockTypeEnum.TNT || typeIndex == BlockTypeEnum.C4)
									{
										ExplosiveTypes explosiveType = (typeIndex != BlockTypeEnum.TNT) ? ExplosiveTypes.C4 : ExplosiveTypes.TNT;
										tntToExplode.Enqueue(new Explosive(intVector, explosiveType));
										DetonateExplosiveMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, intVector, false, explosiveType);
										blocksToRemove.Add(intVector);
									}
									else if (!BreakLookup[(int)explosive.ExplosiveType, (int)typeIndex] && BlockWithinLevelBlastRange(zero, typeIndex, explosive.ExplosiveType) && !BlockType.IsUpperDoor(typeIndex))
									{
										blocksToRemove.Add(intVector);
										if (BlockType.IsContainer(typeIndex))
										{
											DestroyCrateMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, intVector);
											Crate value;
											if (CastleMinerZGame.Instance.CurrentWorld.Crates.TryGetValue(intVector, out value))
											{
												value.EjectContents();
											}
										}
										if (BlockType.IsDoor(typeIndex))
										{
											DestroyCustomBlockMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, intVector, typeIndex);
										}
										RememberDependentObjects(intVector, dependentsToRemove);
										if (BlockType.IsLowerDoor(typeIndex))
										{
											IntVector3 intVector2 = intVector;
											intVector2.Y++;
											if (!blocksToRemove.Contains(intVector2))
											{
												blocksToRemove.Add(intVector2);
												RememberDependentObjects(intVector2, dependentsToRemove);
											}
										}
										if (explosive.ExplosiveType == ExplosiveTypes.Harvest)
										{
											ProcessHarvestExplosion(typeIndex, intVector);
										}
										if (BlockType.ShouldDropLoot(typeIndex))
										{
											PossibleLootType.ProcessLootBlockOutput(typeIndex, intVector);
										}
									}
								}
								zero.Y++;
							}
						}
						zero.Z++;
					}
				}
				zero.X++;
			}
		}

		private static InventoryItem GetHarvestedItem(BlockTypeEnum blockType)
		{
			switch (blockType)
			{
			case BlockTypeEnum.CoalOre:
				return InventoryItem.CreateItem(InventoryItemIDs.Coal, 1);
			case BlockTypeEnum.IronOre:
				return InventoryItem.CreateItem(InventoryItemIDs.IronOre, 1);
			case BlockTypeEnum.CopperOre:
				return InventoryItem.CreateItem(InventoryItemIDs.CopperOre, 1);
			case BlockTypeEnum.GoldOre:
				return InventoryItem.CreateItem(InventoryItemIDs.GoldOre, 1);
			case BlockTypeEnum.DiamondOre:
				return InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1);
			case BlockTypeEnum.CopperWall:
				return InventoryItem.CreateItem(InventoryItemIDs.CopperWall, 1);
			case BlockTypeEnum.IronWall:
				return InventoryItem.CreateItem(InventoryItemIDs.IronWall, 1);
			case BlockTypeEnum.GoldenWall:
				return InventoryItem.CreateItem(InventoryItemIDs.GoldenWall, 1);
			case BlockTypeEnum.DiamondWall:
				return InventoryItem.CreateItem(InventoryItemIDs.DiamondWall, 1);
			default:
				return null;
			}
		}

		private static void ProcessHarvestExplosion(BlockTypeEnum blockType, IntVector3 intPos)
		{
			PossibleLootType.PlaceWorldItem(GetHarvestedItem(blockType), intPos);
		}

		private static void ProcessExplosionDependents(Set<IntVector3> blocksToRemove, Dictionary<IntVector3, BlockTypeEnum> dependentsToRemove)
		{
			foreach (IntVector3 key in dependentsToRemove.Keys)
			{
				if (!blocksToRemove.Contains(key))
				{
					BlockTypeEnum blockTypeEnum = dependentsToRemove[key];
					InventoryItem.InventoryItemClass inventoryItemClass = BlockInventoryItemClass.BlockClasses[BlockType.GetType(blockTypeEnum).ParentBlockType];
					PickupManager.Instance.CreatePickup(inventoryItemClass.CreateItem(1), IntVector3.ToVector3(key) + new Vector3(0.5f, 0.5f, 0.5f), false);
					blocksToRemove.Add(key);
					if (BlockType.IsLowerDoor(blockTypeEnum))
					{
						blocksToRemove.Add(key + new IntVector3(0, 1, 0));
					}
				}
			}
		}

		public static void FindBlocksToRemove(IntVector3 pos, ExplosiveTypes extype, bool showExplosionFlash)
		{
			Queue<Explosive> queue = new Queue<Explosive>();
			Set<IntVector3> set = new Set<IntVector3>();
			Dictionary<IntVector3, BlockTypeEnum> dependentsToRemove = new Dictionary<IntVector3, BlockTypeEnum>();
			queue.Enqueue(new Explosive(pos, extype));
			if (extype == ExplosiveTypes.C4 || extype == ExplosiveTypes.TNT)
			{
				set.Add(pos);
			}
			bool explosionFlashNotYetShown = showExplosionFlash;
			while (queue.Count > 0)
			{
				ProcessOneExplosion(queue, set, dependentsToRemove, ref explosionFlashNotYetShown);
			}
			ProcessExplosionDependents(set, dependentsToRemove);
			IntVector3[] array = new IntVector3[set.Count];
			set.CopyTo(array);
			RemoveBlocksMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, array.Length, array, false);
		}

		public static EnemyBreakBlocksResult EnemyBreakBlocks(IntVector3 minCorner, IntVector3 maxCorner, int hits, int maxHardness, bool enemyIsLocallyOwned)
		{
			IntVector3 value = IntVector3.Subtract(minCorner, BlockTerrain.Instance._worldMin);
			IntVector3 value2 = IntVector3.Subtract(maxCorner, BlockTerrain.Instance._worldMin);
			value = IntVector3.Clamp(value, IntVector3.Zero, _sMaxBufferBounds);
			value2 = IntVector3.Clamp(value2, IntVector3.Zero, _sMaxBufferBounds);
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			IntVector3 intVector = default(IntVector3);
			intVector.Z = value.Z;
			while (intVector.Z <= value2.Z)
			{
				intVector.X = value.X;
				while (intVector.X <= value2.X)
				{
					intVector.Y = value.Y;
					while (intVector.Y <= value2.Y)
					{
						BlockTypeEnum typeIndex = Block.GetTypeIndex(BlockTerrain.Instance.GetBlockAt(intVector));
						if (typeIndex != 0 && typeIndex != BlockTypeEnum.NumberOfBlocks)
						{
							BlockType type = BlockType.GetType(typeIndex);
							if (type.BlockPlayer)
							{
								flag3 = true;
							}
							if (type.Hardness <= maxHardness)
							{
								flag = true;
								if (hits > type.Hardness && MathTools.RandomBool())
								{
									flag2 = true;
									IntVector3 intVector2 = intVector + BlockTerrain.Instance._worldMin;
									if (typeIndex == BlockTypeEnum.TNT || typeIndex == BlockTypeEnum.C4)
									{
										ExplosiveTypes explosiveType = (typeIndex != BlockTypeEnum.TNT) ? ExplosiveTypes.C4 : ExplosiveTypes.TNT;
										_sEnemyDiggingTNTToExplode.Enqueue(new Explosive(intVector2, explosiveType));
										DetonateExplosiveMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, intVector2, false, explosiveType);
										_sEnemyDiggingBlocksToRemove.Add(intVector2);
									}
									else if (!BlockType.IsUpperDoor(typeIndex))
									{
										_sEnemyDiggingBlocksToRemove.Add(intVector2);
										if (BlockType.IsContainer(typeIndex) && enemyIsLocallyOwned)
										{
											DestroyCrateMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, intVector2);
											Crate value3;
											if (CastleMinerZGame.Instance.CurrentWorld.Crates.TryGetValue(intVector2, out value3))
											{
												value3.EjectContents();
											}
										}
										if (BlockType.IsDoor(typeIndex))
										{
											DestroyCustomBlockMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, intVector2, typeIndex);
										}
										RememberDependentObjects(intVector2, _sEnemyDiggingDependentsToRemove);
										if (BlockType.IsLowerDoor(typeIndex))
										{
											IntVector3 intVector3 = intVector2;
											intVector3.Y++;
											if (!_sEnemyDiggingBlocksToRemove.Contains(intVector3))
											{
												_sEnemyDiggingBlocksToRemove.Add(intVector3);
												RememberDependentObjects(intVector3, _sEnemyDiggingDependentsToRemove);
											}
										}
									}
								}
								else if (enemyIsLocallyOwned && (typeIndex == BlockTypeEnum.TNT || typeIndex == BlockTypeEnum.C4))
								{
									InGameHUD.Instance.SetFuseForExplosive(intVector + BlockTerrain.Instance._worldMin, (typeIndex != BlockTypeEnum.TNT) ? ExplosiveTypes.C4 : ExplosiveTypes.TNT);
								}
							}
						}
						intVector.Y++;
					}
					intVector.X++;
				}
				intVector.Z++;
			}
			if (enemyIsLocallyOwned && _sEnemyDiggingBlocksToRemove.Count != 0)
			{
				ProcessExplosionDependents(_sEnemyDiggingBlocksToRemove, _sEnemyDiggingDependentsToRemove);
				IntVector3[] array = new IntVector3[_sEnemyDiggingBlocksToRemove.Count];
				_sEnemyDiggingBlocksToRemove.CopyTo(array);
				RemoveBlocksMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, array.Length, array, true);
				if (_sEnemyDiggingTNTToExplode.Count != 0)
				{
					bool explosionFlashNotYetShown = true;
					while (_sEnemyDiggingTNTToExplode.Count > 0)
					{
						ProcessOneExplosion(_sEnemyDiggingTNTToExplode, _sEnemyDiggingBlocksToRemove, _sEnemyDiggingDependentsToRemove, ref explosionFlashNotYetShown);
					}
					ProcessExplosionDependents(_sEnemyDiggingBlocksToRemove, _sEnemyDiggingDependentsToRemove);
					array = new IntVector3[_sEnemyDiggingBlocksToRemove.Count];
					_sEnemyDiggingBlocksToRemove.CopyTo(array);
					RemoveBlocksMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, array.Length, array, false);
				}
			}
			_sEnemyDiggingTNTToExplode.Clear();
			_sEnemyDiggingDependentsToRemove.Clear();
			_sEnemyDiggingBlocksToRemove.Clear();
			if (flag2)
			{
				return EnemyBreakBlocksResult.BlocksBroken;
			}
			if (!flag3)
			{
				return EnemyBreakBlocksResult.RegionIsEmpty;
			}
			if (flag)
			{
				return EnemyBreakBlocksResult.BlocksWillBreak;
			}
			return EnemyBreakBlocksResult.BlocksWillNotBreak;
		}

		private static bool BlockWithinLevelBlastRange(IntVector3 offset, BlockTypeEnum block, ExplosiveTypes explosiveType)
		{
			int num = (explosiveType == ExplosiveTypes.TNT || explosiveType == ExplosiveTypes.Rocket) ? 1 : 2;
			int num2 = (explosiveType == ExplosiveTypes.TNT) ? 1 : 1;
			int num3;
			if (Level2Hardness[(int)block])
			{
				num3 = num2;
			}
			else
			{
				if (!Level1Hardness[(int)block])
				{
					return false;
				}
				num3 = num;
			}
			if (offset.X >= -num3 && offset.X <= num3 && offset.Y >= -num3 && offset.Y <= num3 && offset.Z >= -num3 && offset.Z <= num3)
			{
				return true;
			}
			return false;
		}

		public static void HandleRemoveBlocksMessage(RemoveBlocksMessage msg)
		{
			if (msg.DoDigEffects)
			{
				for (int i = 0; i < msg.NumBlocks; i++)
				{
					BlockTerrain.Instance.SetBlock(msg.BlocksToRemove[i], BlockTypeEnum.Empty);
					AddDigEffects(IntVector3.ToVector3(msg.BlocksToRemove[i]) + _sHalf);
				}
			}
			else
			{
				for (int j = 0; j < msg.NumBlocks; j++)
				{
					BlockTerrain.Instance.SetBlock(msg.BlocksToRemove[j], BlockTypeEnum.Empty);
				}
			}
		}

		public static void AddDigEffects(Vector3 position)
		{
			if (TracerManager.Instance == null)
			{
				return;
			}
			Scene scene = TracerManager.Instance.Scene;
			if (scene != null && scene.Children != null)
			{
				AudioEmitter audioEmitter = new AudioEmitter();
				audioEmitter.Position = position;
				SoundManager.Instance.PlayInstance("GroundCrash", audioEmitter);
				ParticleEmitter particleEmitter = _digSmokeEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter.Reset();
				particleEmitter.Emitting = true;
				particleEmitter.DrawPriority = 900;
				scene.Children.Add(particleEmitter);
				ParticleEmitter particleEmitter2 = _digRocksEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter2.Reset();
				particleEmitter2.Emitting = true;
				particleEmitter2.DrawPriority = 900;
				scene.Children.Add(particleEmitter2);
				Vector3 vector = position - CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
				float num = vector.LengthSquared();
				if ((double)num > 1E-06)
				{
					vector.Normalize();
				}
				else
				{
					vector = Vector3.Forward;
				}
				Vector3 axis = Vector3.Cross(Vector3.Forward, vector);
				Quaternion quaternion = Quaternion.CreateFromAxisAngle(axis, Vector3.Forward.AngleBetween(vector).Radians);
				Vector3 vector4 = particleEmitter2.LocalPosition = (particleEmitter.LocalPosition = position);
				Quaternion quaternion4 = particleEmitter2.LocalRotation = (particleEmitter.LocalRotation = quaternion);
			}
		}

		public static void AddEffects(Vector3 Position, bool wantRockChunks)
		{
			AudioEmitter audioEmitter = new AudioEmitter();
			audioEmitter.Position = Position;
			SoundManager.Instance.PlayInstance("Explosion", audioEmitter);
			if (TracerManager.Instance == null)
			{
				return;
			}
			Scene scene = TracerManager.Instance.Scene;
			if (scene != null && scene.Children != null)
			{
				ParticleEmitter particleEmitter = _flashEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter.Reset();
				particleEmitter.Emitting = true;
				particleEmitter.LocalPosition = Position;
				particleEmitter.DrawPriority = 900;
				scene.Children.Add(particleEmitter);
				particleEmitter = _firePuffEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter.Reset();
				particleEmitter.Emitting = true;
				particleEmitter.LocalPosition = Position;
				particleEmitter.DrawPriority = 900;
				scene.Children.Add(particleEmitter);
				particleEmitter = _smokePuffEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter.Reset();
				particleEmitter.Emitting = true;
				particleEmitter.LocalPosition = Position;
				particleEmitter.DrawPriority = 900;
				scene.Children.Add(particleEmitter);
				if (wantRockChunks)
				{
					particleEmitter = _rockBlastEffect.CreateEmitter(CastleMinerZGame.Instance);
					particleEmitter.Reset();
					particleEmitter.Emitting = true;
					particleEmitter.LocalPosition = Position;
					particleEmitter.DrawPriority = 900;
					scene.Children.Add(particleEmitter);
				}
			}
		}
	}
}
