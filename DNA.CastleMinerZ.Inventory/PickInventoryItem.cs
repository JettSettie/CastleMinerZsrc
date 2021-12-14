using DNA.Audio;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class PickInventoryItem : InventoryItem
	{
		private const InventoryItemIDs CANNOT_HARVEST_ID = InventoryItemIDs.BareHands;

		private string _useSound;

		public PickInventoryItem(InventoryItemClass cls, int stackCount)
			: base(cls, stackCount)
		{
			PickInventoryItemClass pickInventoryItemClass = (PickInventoryItemClass)base.ItemClass;
			if (pickInventoryItemClass.ID == InventoryItemIDs.IronLaserSword || pickInventoryItemClass.ID == InventoryItemIDs.GoldLaserSword || pickInventoryItemClass.ID == InventoryItemIDs.DiamondLaserSword || pickInventoryItemClass.ID == InventoryItemIDs.BloodStoneLaserSword || pickInventoryItemClass.ID == InventoryItemIDs.CopperLaserSword)
			{
				_useSound = "LightSaberSwing";
			}
		}

		public override void ProcessInput(InGameHUD hud, CastleMinerZControllerMapping controller)
		{
			if (_useSound != null && (controller.Use.Held || controller.Shoulder.Held) && base.CoolDownTimer.Expired)
			{
				SoundManager.Instance.PlayInstance(_useSound);
			}
			base.ProcessInput(hud, controller);
		}

		public override InventoryItem CreatesWhenDug(BlockTypeEnum block, IntVector3 location)
		{
			PickInventoryItemClass pcls = (PickInventoryItemClass)base.ItemClass;
			InventoryItemIDs outputFromBlock = GetOutputFromBlock(block);
			int pickaxeBlockTier = GetPickaxeBlockTier(block);
			int toolTier = GetToolTier(pcls);
			int stackCount = 1;
			if (toolTier >= pickaxeBlockTier && outputFromBlock != 0)
			{
				return InventoryItem.CreateItem(outputFromBlock, stackCount);
			}
			return base.CreatesWhenDug(block, location);
		}

		private InventoryItemIDs MatchOutputFromList(BlockTypeEnum block, BlockTypeEnum[] harvestList)
		{
			InventoryItemIDs result = InventoryItemIDs.RockBlock;
			for (int i = 0; i < harvestList.Length; i++)
			{
				if (harvestList[i] == block)
				{
					result = GetOutputFromBlock(block);
				}
			}
			return result;
		}

		private InventoryItemIDs GetOutputFromBlock(BlockTypeEnum block)
		{
			InventoryItemIDs result = InventoryItemIDs.BareHands;
			switch (block)
			{
			case BlockTypeEnum.CoalOre:
				result = InventoryItemIDs.Coal;
				break;
			case BlockTypeEnum.CopperOre:
				result = InventoryItemIDs.CopperOre;
				break;
			case BlockTypeEnum.IronOre:
				result = InventoryItemIDs.IronOre;
				break;
			case BlockTypeEnum.GoldOre:
				result = InventoryItemIDs.GoldOre;
				break;
			case BlockTypeEnum.DiamondOre:
				result = InventoryItemIDs.Diamond;
				break;
			case BlockTypeEnum.CopperWall:
				result = InventoryItemIDs.CopperWall;
				break;
			case BlockTypeEnum.IronWall:
				result = InventoryItemIDs.IronWall;
				break;
			case BlockTypeEnum.GoldenWall:
				result = InventoryItemIDs.GoldenWall;
				break;
			case BlockTypeEnum.DiamondWall:
				result = InventoryItemIDs.DiamondWall;
				break;
			}
			return result;
		}

		private int GetPickaxeBlockTier(BlockTypeEnum blockType)
		{
			int result = int.MaxValue;
			switch (blockType)
			{
			case BlockTypeEnum.Dirt:
			case BlockTypeEnum.Grass:
			case BlockTypeEnum.Sand:
			case BlockTypeEnum.Rock:
			case BlockTypeEnum.Snow:
			case BlockTypeEnum.Leaves:
			case BlockTypeEnum.Wood:
				result = 0;
				break;
			case BlockTypeEnum.Ice:
				result = 0;
				break;
			case BlockTypeEnum.CoalOre:
				result = 1;
				break;
			case BlockTypeEnum.CopperOre:
				result = 2;
				break;
			case BlockTypeEnum.IronOre:
				result = 3;
				break;
			case BlockTypeEnum.GoldOre:
				result = 5;
				break;
			case BlockTypeEnum.DiamondOre:
				result = 6;
				break;
			case BlockTypeEnum.BloodStone:
				result = 8;
				break;
			case BlockTypeEnum.SpaceRock:
				result = 9;
				break;
			case BlockTypeEnum.SpaceRockInventory:
				result = 9;
				break;
			case BlockTypeEnum.Slime:
				result = 10;
				break;
			case BlockTypeEnum.CopperWall:
				result = 2;
				break;
			case BlockTypeEnum.IronWall:
				result = 3;
				break;
			case BlockTypeEnum.GoldenWall:
				result = 4;
				break;
			case BlockTypeEnum.DiamondWall:
				result = 5;
				break;
			case BlockTypeEnum.LootBlock:
				result = 3;
				break;
			case BlockTypeEnum.LuckyLootBlock:
				result = 6;
				break;
			}
			return result;
		}

		private int GetToolTier(PickInventoryItemClass pcls)
		{
			int result = 0;
			if (pcls.ID == InventoryItemIDs.IronLaserSword || pcls.ID == InventoryItemIDs.GoldLaserSword || pcls.ID == InventoryItemIDs.DiamondLaserSword || pcls.ID == InventoryItemIDs.BloodStoneLaserSword || pcls.ID == InventoryItemIDs.CopperLaserSword)
			{
				result = 12;
			}
			else
			{
				switch (pcls.Material)
				{
				case ToolMaterialTypes.Wood:
					result = 1;
					break;
				case ToolMaterialTypes.Stone:
					result = 3;
					break;
				case ToolMaterialTypes.Copper:
					result = 4;
					break;
				case ToolMaterialTypes.Iron:
					result = 5;
					break;
				case ToolMaterialTypes.Gold:
					result = 6;
					break;
				case ToolMaterialTypes.Diamond:
					result = 8;
					break;
				case ToolMaterialTypes.BloodStone:
					result = 10;
					break;
				}
			}
			return result;
		}

		private float GetBlockDifficultyModifier(int blockTier)
		{
			float result = 5f;
			switch (blockTier)
			{
			case 0:
				result = 0.1f;
				break;
			case 1:
				result = 0.25f;
				break;
			case 2:
				result = 0.5f;
				break;
			case 3:
				result = 1f;
				break;
			case 4:
				result = 1.5f;
				break;
			case 5:
				result = 2f;
				break;
			case 6:
				result = 3f;
				break;
			case 7:
				result = 4.5f;
				break;
			case 8:
				result = 6f;
				break;
			case 9:
				result = 8f;
				break;
			case 10:
				result = 10f;
				break;
			}
			return result;
		}

		private float GetDifficultyRatio(int diff)
		{
			float result = 1f;
			switch (diff)
			{
			case 0:
				result = 1f;
				break;
			case 1:
				result = 0.75f;
				break;
			case 2:
				result = 0.5f;
				break;
			case 3:
				result = 0.35f;
				break;
			case 4:
				result = 0.25f;
				break;
			case 5:
				result = 0.2f;
				break;
			case 6:
				result = 0.17f;
				break;
			case 7:
				result = 0.15f;
				break;
			case 8:
				result = 0.14f;
				break;
			case 9:
				result = 0.13f;
				break;
			case 10:
				result = 0.12f;
				break;
			}
			return result;
		}

		private float GetAverageDigTimeForToolTier(int toolTier)
		{
			float result = 20f;
			switch (toolTier)
			{
			case 0:
				result = 16f;
				break;
			case 1:
				result = 14f;
				break;
			case 2:
				result = 12f;
				break;
			case 3:
				result = 10f;
				break;
			case 4:
				result = 9f;
				break;
			case 5:
				result = 6f;
				break;
			case 6:
				result = 4f;
				break;
			case 7:
				result = 3f;
				break;
			case 8:
				result = 2f;
				break;
			case 9:
				result = 1.5f;
				break;
			case 10:
				result = 1f;
				break;
			case 12:
				result = 0.5f;
				break;
			}
			return result;
		}

		public override TimeSpan TimeToDig(BlockTypeEnum blockType)
		{
			PickInventoryItemClass pcls = (PickInventoryItemClass)base.ItemClass;
			int pickaxeBlockTier = GetPickaxeBlockTier(blockType);
			int toolTier = GetToolTier(pcls);
			if (pickaxeBlockTier > toolTier)
			{
				return base.TimeToDig(blockType);
			}
			float num = ComputeDigTime(toolTier, pickaxeBlockTier);
			return TimeSpan.FromSeconds(num);
		}

		private float ComputeDigTime(int toolTier, int blockTier)
		{
			int diff = toolTier - blockTier;
			float averageDigTimeForToolTier = GetAverageDigTimeForToolTier(toolTier);
			float blockDifficultyModifier = GetBlockDifficultyModifier(blockTier);
			averageDigTimeForToolTier *= blockDifficultyModifier;
			averageDigTimeForToolTier *= GetDifficultyRatio(diff);
			if (averageDigTimeForToolTier < 0.01f)
			{
				averageDigTimeForToolTier = 0.01f;
			}
			return averageDigTimeForToolTier;
		}

		public TimeSpan TimeToDigOld(BlockTypeEnum blockType)
		{
			PickInventoryItemClass pickInventoryItemClass = (PickInventoryItemClass)base.ItemClass;
			if (pickInventoryItemClass.ID == InventoryItemIDs.IronLaserSword || pickInventoryItemClass.ID == InventoryItemIDs.GoldLaserSword || pickInventoryItemClass.ID == InventoryItemIDs.DiamondLaserSword || pickInventoryItemClass.ID == InventoryItemIDs.BloodStoneLaserSword || pickInventoryItemClass.ID == InventoryItemIDs.CopperLaserSword)
			{
				switch (blockType)
				{
				case BlockTypeEnum.Dirt:
				case BlockTypeEnum.Grass:
				case BlockTypeEnum.Sand:
				case BlockTypeEnum.Rock:
				case BlockTypeEnum.Snow:
				case BlockTypeEnum.Leaves:
				case BlockTypeEnum.Wood:
					return TimeSpan.FromSeconds(0.01);
				case BlockTypeEnum.Ice:
					return TimeSpan.FromSeconds(0.01);
				case BlockTypeEnum.CoalOre:
					return TimeSpan.FromSeconds(0.1);
				case BlockTypeEnum.CopperOre:
					return TimeSpan.FromSeconds(0.2);
				case BlockTypeEnum.IronOre:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.GoldOre:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.DiamondOre:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.BloodStone:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.SpaceRock:
					return TimeSpan.FromSeconds(2.0);
				case BlockTypeEnum.SpaceRockInventory:
					return TimeSpan.FromSeconds(2.0);
				case BlockTypeEnum.Slime:
					return TimeSpan.FromSeconds(4.0);
				case BlockTypeEnum.CopperWall:
					return TimeSpan.FromSeconds(0.2);
				case BlockTypeEnum.IronWall:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.GoldenWall:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.DiamondWall:
					return TimeSpan.FromSeconds(0.5);
				}
			}
			switch (pickInventoryItemClass.Material)
			{
			case ToolMaterialTypes.Wood:
				return base.TimeToDig(blockType);
			case ToolMaterialTypes.Stone:
				switch (blockType)
				{
				case BlockTypeEnum.Rock:
					return TimeSpan.FromSeconds(2.0);
				case BlockTypeEnum.Ice:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.CoalOre:
					return TimeSpan.FromSeconds(3.0);
				case BlockTypeEnum.CopperOre:
					return TimeSpan.FromSeconds(6.0);
				case BlockTypeEnum.IronOre:
					return TimeSpan.FromSeconds(7.0);
				}
				break;
			case ToolMaterialTypes.Copper:
				switch (blockType)
				{
				case BlockTypeEnum.Rock:
					return TimeSpan.FromSeconds(1.5);
				case BlockTypeEnum.Ice:
					return TimeSpan.FromSeconds(0.75);
				case BlockTypeEnum.CoalOre:
					return TimeSpan.FromSeconds(1.5);
				case BlockTypeEnum.CopperOre:
					return TimeSpan.FromSeconds(3.0);
				case BlockTypeEnum.IronOre:
					return TimeSpan.FromSeconds(6.0);
				case BlockTypeEnum.CopperWall:
					return TimeSpan.FromSeconds(3.0);
				}
				break;
			case ToolMaterialTypes.Iron:
				switch (blockType)
				{
				case BlockTypeEnum.Rock:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.Ice:
					return TimeSpan.FromSeconds(0.25);
				case BlockTypeEnum.CoalOre:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.CopperOre:
					return TimeSpan.FromSeconds(1.5);
				case BlockTypeEnum.IronOre:
					return TimeSpan.FromSeconds(3.0);
				case BlockTypeEnum.GoldOre:
					return TimeSpan.FromSeconds(6.0);
				case BlockTypeEnum.CopperWall:
					return TimeSpan.FromSeconds(1.5);
				case BlockTypeEnum.IronWall:
					return TimeSpan.FromSeconds(3.0);
				}
				break;
			case ToolMaterialTypes.Gold:
				switch (blockType)
				{
				case BlockTypeEnum.Rock:
					return TimeSpan.FromSeconds(0.25);
				case BlockTypeEnum.Ice:
					return TimeSpan.FromSeconds(0.1);
				case BlockTypeEnum.CoalOre:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.CopperOre:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.IronOre:
					return TimeSpan.FromSeconds(2.0);
				case BlockTypeEnum.GoldOre:
					return TimeSpan.FromSeconds(3.0);
				case BlockTypeEnum.DiamondOre:
					return TimeSpan.FromSeconds(5.0);
				case BlockTypeEnum.CopperWall:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.IronWall:
					return TimeSpan.FromSeconds(2.0);
				case BlockTypeEnum.GoldenWall:
					return TimeSpan.FromSeconds(3.0);
				}
				break;
			case ToolMaterialTypes.Diamond:
				switch (blockType)
				{
				case BlockTypeEnum.Rock:
					return TimeSpan.FromSeconds(0.1);
				case BlockTypeEnum.Ice:
					return TimeSpan.FromSeconds(0.05);
				case BlockTypeEnum.CoalOre:
					return TimeSpan.FromSeconds(0.25);
				case BlockTypeEnum.CopperOre:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.IronOre:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.GoldOre:
					return TimeSpan.FromSeconds(2.0);
				case BlockTypeEnum.DiamondOre:
					return TimeSpan.FromSeconds(3.0);
				case BlockTypeEnum.BloodStone:
					return TimeSpan.FromSeconds(8.0);
				case BlockTypeEnum.CopperWall:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.IronWall:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.GoldenWall:
					return TimeSpan.FromSeconds(2.0);
				case BlockTypeEnum.DiamondWall:
					return TimeSpan.FromSeconds(3.0);
				}
				break;
			case ToolMaterialTypes.BloodStone:
				switch (blockType)
				{
				case BlockTypeEnum.Rock:
					return TimeSpan.FromSeconds(0.01);
				case BlockTypeEnum.Ice:
					return TimeSpan.FromSeconds(0.01);
				case BlockTypeEnum.CoalOre:
					return TimeSpan.FromSeconds(0.1);
				case BlockTypeEnum.CopperOre:
					return TimeSpan.FromSeconds(0.2);
				case BlockTypeEnum.IronOre:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.GoldOre:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.DiamondOre:
					return TimeSpan.FromSeconds(1.5);
				case BlockTypeEnum.BloodStone:
					return TimeSpan.FromSeconds(3.0);
				case BlockTypeEnum.SpaceRock:
					return TimeSpan.FromSeconds(3.0);
				case BlockTypeEnum.SpaceRockInventory:
					return TimeSpan.FromSeconds(3.0);
				case BlockTypeEnum.Slime:
					return TimeSpan.FromSeconds(6.0);
				case BlockTypeEnum.CopperWall:
					return TimeSpan.FromSeconds(0.2);
				case BlockTypeEnum.IronWall:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.GoldenWall:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.DiamondWall:
					return TimeSpan.FromSeconds(1.5);
				}
				break;
			}
			return base.TimeToDig(blockType);
		}
	}
}
