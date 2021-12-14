using DNA.CastleMinerZ.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Terrain
{
	public class SpawnBlockView
	{
		private class SpawnBlockProperties
		{
			public BlockTypeEnum DimBlockTypeEnum;

			public BlockTypeEnum OffBlockTypeEnum;

			public BlockTypeEnum OnBlockTypeEnum;

			public SpawnBlockProperties(BlockTypeEnum pOffBlockType, BlockTypeEnum pOnBlockType, BlockTypeEnum pDimBlockType)
			{
				OffBlockTypeEnum = pOffBlockType;
				OnBlockTypeEnum = pOnBlockType;
				DimBlockTypeEnum = pDimBlockType;
			}
		}

		public enum SpawnLightState
		{
			None,
			On,
			Off
		}

		private const float c_LightOnTime = 0.8f;

		private const float c_LightOffTime = 0.3f;

		private static readonly SpawnBlockProperties[] _spawnBlockProperties = new SpawnBlockProperties[6]
		{
			new SpawnBlockProperties(BlockTypeEnum.EnemySpawnOff, BlockTypeEnum.EnemySpawnOn, BlockTypeEnum.EnemySpawnDim),
			new SpawnBlockProperties(BlockTypeEnum.EnemySpawnRareOff, BlockTypeEnum.EnemySpawnRareOn, BlockTypeEnum.EnemySpawnRareDim),
			new SpawnBlockProperties(BlockTypeEnum.AlienSpawnOff, BlockTypeEnum.AlienSpawnOn, BlockTypeEnum.AlienSpawnDim),
			new SpawnBlockProperties(BlockTypeEnum.AlienHordeOff, BlockTypeEnum.AlienHordeOn, BlockTypeEnum.AlienHordeDim),
			new SpawnBlockProperties(BlockTypeEnum.HellSpawnOff, BlockTypeEnum.HellSpawnOn, BlockTypeEnum.HellSpawnDim),
			new SpawnBlockProperties(BlockTypeEnum.BossSpawnOff, BlockTypeEnum.BossSpawnOn, BlockTypeEnum.BossSpawnDim)
		};

		public bool Destroyed;

		private IntVector3 _location;

		private float _lightTimer;

		private BlockTypeEnum _currentBlockType;

		private BlockTypeEnum _originalBlockType;

		private SpawnLightState _spawnLightState;

		private BlockTypeEnum _dimBlockTypeEnum;

		private BlockTypeEnum _offBlockTypeEnum;

		private BlockTypeEnum _onBlockTypeEnum;

		private SpawnBlockProperties _blockProperties;

		public IntVector3 Location
		{
			get
			{
				return _location;
			}
		}

		public SpawnBlockView(IntVector3 location, BlockTypeEnum blockSource)
		{
			SetBlockProperty(blockSource);
			_originalBlockType = blockSource;
			_location = location;
		}

		private void SetBlockProperty(BlockTypeEnum blockSource)
		{
			SpawnBlockProperties[] spawnBlockProperties = _spawnBlockProperties;
			int num = 0;
			SpawnBlockProperties spawnBlockProperties2;
			while (true)
			{
				if (num < spawnBlockProperties.Length)
				{
					spawnBlockProperties2 = spawnBlockProperties[num];
					if (spawnBlockProperties2.OffBlockTypeEnum == blockSource)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			_blockProperties = spawnBlockProperties2;
		}

		private BlockTypeEnum SetCurrentBlock(BlockTypeEnum blockType)
		{
			_currentBlockType = blockType;
			return _currentBlockType;
		}

		public void Reset()
		{
			AlterBlockMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, Location, _blockProperties.OffBlockTypeEnum);
			_spawnLightState = SpawnLightState.Off;
		}

		public void ToggleLightState()
		{
			if (_spawnLightState == SpawnLightState.On)
			{
				SetBlockLight(false);
			}
			else if (_spawnLightState == SpawnLightState.Off)
			{
				SetBlockLight(true);
			}
		}

		public void SetBlockLight(bool enable)
		{
			Player localPlayer = CastleMinerZGame.Instance.LocalPlayer;
			if (enable)
			{
				AlterBlockMessage.Send((LocalNetworkGamer)localPlayer.Gamer, Location, GetLightedBlock());
				_spawnLightState = SpawnLightState.On;
				_lightTimer = 0.8f;
			}
			else
			{
				AlterBlockMessage.Send((LocalNetworkGamer)localPlayer.Gamer, Location, GetDimBlock());
				_spawnLightState = SpawnLightState.Off;
				_lightTimer = 0.3f;
			}
		}

		public static BlockTypeEnum GetInActiveSpawnBlockType(BlockTypeEnum blockType)
		{
			BlockTypeEnum result = blockType;
			switch (blockType)
			{
			case BlockTypeEnum.EnemySpawnOn:
				result = BlockTypeEnum.EnemySpawnOff;
				break;
			case BlockTypeEnum.EnemySpawnRareOn:
				result = BlockTypeEnum.EnemySpawnRareOff;
				break;
			case BlockTypeEnum.AlienSpawnOn:
				result = BlockTypeEnum.AlienSpawnOff;
				break;
			case BlockTypeEnum.HellSpawnOn:
				result = BlockTypeEnum.HellSpawnOff;
				break;
			case BlockTypeEnum.BossSpawnOn:
				result = BlockTypeEnum.BossSpawnOff;
				break;
			}
			return result;
		}

		public BlockTypeEnum GetActiveSpawnBlockType()
		{
			return _blockProperties.OnBlockTypeEnum;
		}

		private BlockTypeEnum GetAlternateBlock(BlockTypeEnum blockType)
		{
			if (blockType == _blockProperties.OnBlockTypeEnum)
			{
				return _blockProperties.OffBlockTypeEnum;
			}
			if (blockType == _blockProperties.OffBlockTypeEnum)
			{
				return _blockProperties.OnBlockTypeEnum;
			}
			if (blockType == _blockProperties.DimBlockTypeEnum)
			{
				return _blockProperties.OnBlockTypeEnum;
			}
			return BlockTypeEnum.Empty;
		}

		public BlockTypeEnum GetLightedBlock()
		{
			return _blockProperties.OnBlockTypeEnum;
		}

		public BlockTypeEnum GetDimBlock()
		{
			return _blockProperties.DimBlockTypeEnum;
		}

		public void Update(float delta)
		{
			_lightTimer -= delta;
			if (_lightTimer <= 0f)
			{
				ToggleLightState();
			}
		}
	}
}
