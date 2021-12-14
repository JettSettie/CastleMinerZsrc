using System.Collections.Generic;

namespace DNA.CastleMinerZ.Inventory
{
	public class Recipe
	{
		public enum RecipeTypes
		{
			Ores,
			Components,
			Pickaxes,
			Spades,
			Axes,
			SpecialTools,
			Ammo,
			Knives,
			Pistols,
			Shotguns,
			Rifles,
			AssaultRifles,
			SMGs,
			LMGs,
			RPG,
			LaserSwords,
			Walls,
			Doors,
			OtherStructure,
			Explosives,
			Containers,
			SpawnPoints,
			Advanced
		}

		public static List<Recipe> CookBook;

		private List<InventoryItem> _ingredients;

		private InventoryItem _result;

		private RecipeTypes _type;

		public List<InventoryItem> Ingredients
		{
			get
			{
				return _ingredients;
			}
		}

		public InventoryItem Result
		{
			get
			{
				return _result;
			}
		}

		public RecipeTypes Type
		{
			get
			{
				return _type;
			}
		}

		public static List<Recipe> GetRecipes(RecipeTypes type)
		{
			List<Recipe> list = new List<Recipe>();
			foreach (Recipe item in CookBook)
			{
				if (item.Type == type)
				{
					list.Add(item);
				}
			}
			return list;
		}

		public Recipe(RecipeTypes type, InventoryItem result, params InventoryItem[] ingredients)
		{
			_type = type;
			_result = result;
			_ingredients = new List<InventoryItem>(ingredients);
		}

		static Recipe()
		{
			CookBook = new List<Recipe>();
			CookBook.Add(new Recipe(RecipeTypes.Components, InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 4), InventoryItem.CreateItem(InventoryItemIDs.LogBlock, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Components, InventoryItem.CreateItem(InventoryItemIDs.Stick, 4), InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 1)));
			CookBook.Add(new Recipe(RecipeTypes.SpecialTools, InventoryItem.CreateItem(InventoryItemIDs.Torch, 4), InventoryItem.CreateItem(InventoryItemIDs.Coal, 1), InventoryItem.CreateItem(InventoryItemIDs.Stick, 1)));
			CookBook.Add(new Recipe(RecipeTypes.OtherStructure, InventoryItem.CreateItem(InventoryItemIDs.LanternBlock, 4), InventoryItem.CreateItem(InventoryItemIDs.Torch, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1), InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 4), InventoryItem.CreateItem(InventoryItemIDs.Coal, 2)));
			CookBook.Add(new Recipe(RecipeTypes.OtherStructure, InventoryItem.CreateItem(InventoryItemIDs.LanternFancyBlock, 4), InventoryItem.CreateItem(InventoryItemIDs.Torch, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1), InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 4), InventoryItem.CreateItem(InventoryItemIDs.Coal, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Components, InventoryItem.CreateItem(InventoryItemIDs.BrassCasing, 200), InventoryItem.CreateItem(InventoryItemIDs.Copper, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Components, InventoryItem.CreateItem(InventoryItemIDs.IronCasing, 200), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Components, InventoryItem.CreateItem(InventoryItemIDs.GoldCasing, 200), InventoryItem.CreateItem(InventoryItemIDs.Gold, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Components, InventoryItem.CreateItem(InventoryItemIDs.DiamondCasing, 100), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Ammo, InventoryItem.CreateItem(InventoryItemIDs.Bullets, 100), InventoryItem.CreateItem(InventoryItemIDs.RockBlock, 1), InventoryItem.CreateItem(InventoryItemIDs.BrassCasing, 100), InventoryItem.CreateItem(InventoryItemIDs.Coal, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Ammo, InventoryItem.CreateItem(InventoryItemIDs.IronBullets, 100), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1), InventoryItem.CreateItem(InventoryItemIDs.BrassCasing, 100), InventoryItem.CreateItem(InventoryItemIDs.Coal, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Ammo, InventoryItem.CreateItem(InventoryItemIDs.GoldBullets, 100), InventoryItem.CreateItem(InventoryItemIDs.Gold, 1), InventoryItem.CreateItem(InventoryItemIDs.IronCasing, 100), InventoryItem.CreateItem(InventoryItemIDs.Coal, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Ammo, InventoryItem.CreateItem(InventoryItemIDs.DiamondBullets, 100), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1), InventoryItem.CreateItem(InventoryItemIDs.GoldCasing, 100), InventoryItem.CreateItem(InventoryItemIDs.Coal, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Ammo, InventoryItem.CreateItem(InventoryItemIDs.LaserBullets, 100), InventoryItem.CreateItem(InventoryItemIDs.SpaceRockInventory, 1), InventoryItem.CreateItem(InventoryItemIDs.DiamondCasing, 100)));
			CookBook.Add(new Recipe(RecipeTypes.Components, InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 1), InventoryItem.CreateItem(InventoryItemIDs.Coal, 2), InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 2)));
			CookBook.Add(new Recipe(RecipeTypes.SpecialTools, InventoryItem.CreateItem(InventoryItemIDs.Compass, 1), InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 1), InventoryItem.CreateItem(InventoryItemIDs.Coal, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)));
			CookBook.Add(new Recipe(RecipeTypes.SpecialTools, InventoryItem.CreateItem(InventoryItemIDs.Clock, 1), InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 1), InventoryItem.CreateItem(InventoryItemIDs.Coal, 1), InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 1), InventoryItem.CreateItem(InventoryItemIDs.Copper, 1)));
			CookBook.Add(new Recipe(RecipeTypes.SpecialTools, InventoryItem.CreateItem(InventoryItemIDs.GPS, 1), InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 1), InventoryItem.CreateItem(InventoryItemIDs.Coal, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1), InventoryItem.CreateItem(InventoryItemIDs.Gold, 1)));
			CookBook.Add(new Recipe(RecipeTypes.SpecialTools, InventoryItem.CreateItem(InventoryItemIDs.TeleportGPS, 1), InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 1), InventoryItem.CreateItem(InventoryItemIDs.Coal, 1), InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 1), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Containers, InventoryItem.CreateItem(InventoryItemIDs.Crate, 1), InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 10), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Containers, InventoryItem.CreateItem(InventoryItemIDs.StoneContainer, 1), InventoryItem.CreateItem(InventoryItemIDs.RockBlock, 4), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Containers, InventoryItem.CreateItem(InventoryItemIDs.CopperContainer, 1), InventoryItem.CreateItem(InventoryItemIDs.Copper, 2), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Containers, InventoryItem.CreateItem(InventoryItemIDs.IronContainer, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 4)));
			CookBook.Add(new Recipe(RecipeTypes.Containers, InventoryItem.CreateItem(InventoryItemIDs.GoldContainer, 1), InventoryItem.CreateItem(InventoryItemIDs.Gold, 2), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Containers, InventoryItem.CreateItem(InventoryItemIDs.DiamondContainer, 1), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Containers, InventoryItem.CreateItem(InventoryItemIDs.BloodstoneContainer, 1), InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 10), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)));
			CookBook.Add(new Recipe(RecipeTypes.SpawnPoints, InventoryItem.CreateItem(InventoryItemIDs.SpawnBasic, 1), InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 2), InventoryItem.CreateItem(InventoryItemIDs.Coal, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2), InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 10)));
			CookBook.Add(new Recipe(RecipeTypes.SpawnPoints, InventoryItem.CreateItem(InventoryItemIDs.TeleportStation, 1), InventoryItem.CreateItem(InventoryItemIDs.Copper, 40), InventoryItem.CreateItem(InventoryItemIDs.Iron, 20), InventoryItem.CreateItem(InventoryItemIDs.Gold, 20), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1)));
			CookBook.Add(new Recipe(RecipeTypes.SpawnPoints, InventoryItem.CreateItem(InventoryItemIDs.TeleportStation, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 15), InventoryItem.CreateItem(InventoryItemIDs.RockBlock, 999), InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 2), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1)));
			CookBook.Add(new Recipe(RecipeTypes.SpawnPoints, InventoryItem.CreateItem(InventoryItemIDs.TeleportStation, 1), InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 100), InventoryItem.CreateItem(InventoryItemIDs.Snow, 100), InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 40)));
			CookBook.Add(new Recipe(RecipeTypes.Pickaxes, InventoryItem.CreateItem(InventoryItemIDs.StonePickAxe, 1), InventoryItem.CreateItem(InventoryItemIDs.RockBlock, 2), InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Pickaxes, InventoryItem.CreateItem(InventoryItemIDs.CopperPickAxe, 1), InventoryItem.CreateItem(InventoryItemIDs.Copper, 2), InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Pickaxes, InventoryItem.CreateItem(InventoryItemIDs.IronPickAxe, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2), InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Pickaxes, InventoryItem.CreateItem(InventoryItemIDs.GoldPickAxe, 1), InventoryItem.CreateItem(InventoryItemIDs.Gold, 2), InventoryItem.CreateItem(InventoryItemIDs.Iron, 3)));
			CookBook.Add(new Recipe(RecipeTypes.Pickaxes, InventoryItem.CreateItem(InventoryItemIDs.DiamondPickAxe, 1), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2), InventoryItem.CreateItem(InventoryItemIDs.Gold, 3)));
			CookBook.Add(new Recipe(RecipeTypes.Pickaxes, InventoryItem.CreateItem(InventoryItemIDs.BloodstonePickAxe, 1), InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 10), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3)));
			CookBook.Add(new Recipe(RecipeTypes.Pickaxes, InventoryItem.CreateItem(InventoryItemIDs.LaserDrill, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 5), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 6), InventoryItem.CreateItem(InventoryItemIDs.Coal, 3)));
			CookBook.Add(new Recipe(RecipeTypes.Spades, InventoryItem.CreateItem(InventoryItemIDs.StoneSpade, 1), InventoryItem.CreateItem(InventoryItemIDs.RockBlock, 2), InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Spades, InventoryItem.CreateItem(InventoryItemIDs.CopperSpade, 1), InventoryItem.CreateItem(InventoryItemIDs.Copper, 1), InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Spades, InventoryItem.CreateItem(InventoryItemIDs.IronSpade, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1), InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Spades, InventoryItem.CreateItem(InventoryItemIDs.GoldSpade, 1), InventoryItem.CreateItem(InventoryItemIDs.Gold, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Spades, InventoryItem.CreateItem(InventoryItemIDs.DiamondSpade, 1), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1), InventoryItem.CreateItem(InventoryItemIDs.Gold, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Axes, InventoryItem.CreateItem(InventoryItemIDs.StoneAxe, 1), InventoryItem.CreateItem(InventoryItemIDs.RockBlock, 4), InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Axes, InventoryItem.CreateItem(InventoryItemIDs.CopperAxe, 1), InventoryItem.CreateItem(InventoryItemIDs.Copper, 2), InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Axes, InventoryItem.CreateItem(InventoryItemIDs.IronAxe, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2), InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Axes, InventoryItem.CreateItem(InventoryItemIDs.GoldAxe, 1), InventoryItem.CreateItem(InventoryItemIDs.Gold, 2), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Axes, InventoryItem.CreateItem(InventoryItemIDs.DiamondAxe, 1), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2), InventoryItem.CreateItem(InventoryItemIDs.Gold, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Copper, 1), InventoryItem.CreateItem(InventoryItemIDs.CopperOre, 2), InventoryItem.CreateItem(InventoryItemIDs.Coal, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Iron, 1), InventoryItem.CreateItem(InventoryItemIDs.IronOre, 2), InventoryItem.CreateItem(InventoryItemIDs.Coal, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Gold, 1), InventoryItem.CreateItem(InventoryItemIDs.GoldOre, 2), InventoryItem.CreateItem(InventoryItemIDs.Coal, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Iron, 1), InventoryItem.CreateItem(InventoryItemIDs.IronOre, 2), InventoryItem.CreateItem(InventoryItemIDs.LogBlock, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Gold, 1), InventoryItem.CreateItem(InventoryItemIDs.GoldOre, 2), InventoryItem.CreateItem(InventoryItemIDs.LogBlock, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Copper, 1), InventoryItem.CreateItem(InventoryItemIDs.CopperOre, 2), InventoryItem.CreateItem(InventoryItemIDs.LogBlock, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Knives, InventoryItem.CreateItem(InventoryItemIDs.Knife, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1), InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Knives, InventoryItem.CreateItem(InventoryItemIDs.GoldKnife, 1), InventoryItem.CreateItem(InventoryItemIDs.Gold, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Knives, InventoryItem.CreateItem(InventoryItemIDs.DiamondKnife, 1), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1), InventoryItem.CreateItem(InventoryItemIDs.Gold, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Knives, InventoryItem.CreateItem(InventoryItemIDs.BloodStoneKnife, 1), InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 10), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1)));
			CookBook.Add(new Recipe(RecipeTypes.LaserSwords, InventoryItem.CreateItem(InventoryItemIDs.IronLaserSword, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2), InventoryItem.CreateItem(InventoryItemIDs.SpaceRockInventory, 4)));
			CookBook.Add(new Recipe(RecipeTypes.LaserSwords, InventoryItem.CreateItem(InventoryItemIDs.CopperLaserSword, 1), InventoryItem.CreateItem(InventoryItemIDs.Copper, 2), InventoryItem.CreateItem(InventoryItemIDs.SpaceRockInventory, 4)));
			CookBook.Add(new Recipe(RecipeTypes.LaserSwords, InventoryItem.CreateItem(InventoryItemIDs.GoldLaserSword, 1), InventoryItem.CreateItem(InventoryItemIDs.Gold, 2), InventoryItem.CreateItem(InventoryItemIDs.SpaceRockInventory, 4)));
			CookBook.Add(new Recipe(RecipeTypes.LaserSwords, InventoryItem.CreateItem(InventoryItemIDs.DiamondLaserSword, 1), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2), InventoryItem.CreateItem(InventoryItemIDs.SpaceRockInventory, 4)));
			CookBook.Add(new Recipe(RecipeTypes.LaserSwords, InventoryItem.CreateItem(InventoryItemIDs.BloodStoneLaserSword, 1), InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 2), InventoryItem.CreateItem(InventoryItemIDs.SpaceRockInventory, 4)));
			CookBook.Add(new Recipe(RecipeTypes.Pistols, InventoryItem.CreateItem(InventoryItemIDs.Pistol, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2), InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Pistols, InventoryItem.CreateItem(InventoryItemIDs.GoldPistol, 1), InventoryItem.CreateItem(InventoryItemIDs.Gold, 2), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Pistols, InventoryItem.CreateItem(InventoryItemIDs.DiamondPistol, 1), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2), InventoryItem.CreateItem(InventoryItemIDs.Gold, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Pistols, InventoryItem.CreateItem(InventoryItemIDs.BloodStonePistol, 1), InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 30), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Pistols, InventoryItem.CreateItem(InventoryItemIDs.IronSpacePistol, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 2), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Pistols, InventoryItem.CreateItem(InventoryItemIDs.CopperSpacePistol, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 2), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2), InventoryItem.CreateItem(InventoryItemIDs.Copper, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Pistols, InventoryItem.CreateItem(InventoryItemIDs.GoldSpacePistol, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 2), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2), InventoryItem.CreateItem(InventoryItemIDs.Gold, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Pistols, InventoryItem.CreateItem(InventoryItemIDs.DiamondSpacePistol, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 2), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3)));
			CookBook.Add(new Recipe(RecipeTypes.LMGs, InventoryItem.CreateItem(InventoryItemIDs.LMGGun, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 6), InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 2)));
			CookBook.Add(new Recipe(RecipeTypes.LMGs, InventoryItem.CreateItem(InventoryItemIDs.GoldLMGGun, 1), InventoryItem.CreateItem(InventoryItemIDs.Gold, 6), InventoryItem.CreateItem(InventoryItemIDs.Iron, 3)));
			CookBook.Add(new Recipe(RecipeTypes.LMGs, InventoryItem.CreateItem(InventoryItemIDs.DiamondLMGGun, 1), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 6), InventoryItem.CreateItem(InventoryItemIDs.Gold, 3)));
			CookBook.Add(new Recipe(RecipeTypes.LMGs, InventoryItem.CreateItem(InventoryItemIDs.BloodStoneLMGGun, 1), InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 60), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 6)));
			CookBook.Add(new Recipe(RecipeTypes.SMGs, InventoryItem.CreateItem(InventoryItemIDs.SMGGun, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 3), InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 2)));
			CookBook.Add(new Recipe(RecipeTypes.SMGs, InventoryItem.CreateItem(InventoryItemIDs.GoldSMGGun, 1), InventoryItem.CreateItem(InventoryItemIDs.Gold, 3), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)));
			CookBook.Add(new Recipe(RecipeTypes.SMGs, InventoryItem.CreateItem(InventoryItemIDs.DiamondSMGGun, 1), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3), InventoryItem.CreateItem(InventoryItemIDs.Gold, 2)));
			CookBook.Add(new Recipe(RecipeTypes.SMGs, InventoryItem.CreateItem(InventoryItemIDs.BloodStoneSMGGun, 1), InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 20), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3)));
			CookBook.Add(new Recipe(RecipeTypes.SMGs, InventoryItem.CreateItem(InventoryItemIDs.IronSpaceSMGGun, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 3), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)));
			CookBook.Add(new Recipe(RecipeTypes.SMGs, InventoryItem.CreateItem(InventoryItemIDs.CopperSpaceSMGGun, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 3), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3), InventoryItem.CreateItem(InventoryItemIDs.Copper, 1)));
			CookBook.Add(new Recipe(RecipeTypes.SMGs, InventoryItem.CreateItem(InventoryItemIDs.GoldSpaceSMGGun, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 3), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3), InventoryItem.CreateItem(InventoryItemIDs.Gold, 1)));
			CookBook.Add(new Recipe(RecipeTypes.SMGs, InventoryItem.CreateItem(InventoryItemIDs.DiamondSpaceSMGGun, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 3), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 4)));
			CookBook.Add(new Recipe(RecipeTypes.Rifles, InventoryItem.CreateItem(InventoryItemIDs.BoltActionRifle, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 3), InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Rifles, InventoryItem.CreateItem(InventoryItemIDs.GoldBoltActionRifle, 1), InventoryItem.CreateItem(InventoryItemIDs.Gold, 3), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Rifles, InventoryItem.CreateItem(InventoryItemIDs.DiamondBoltActionRifle, 1), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3), InventoryItem.CreateItem(InventoryItemIDs.Gold, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Rifles, InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBoltActionRifle, 1), InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 20), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3)));
			CookBook.Add(new Recipe(RecipeTypes.Rifles, InventoryItem.CreateItem(InventoryItemIDs.IronSpaceBoltActionRifle, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 3), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Rifles, InventoryItem.CreateItem(InventoryItemIDs.CopperSpaceBoltActionRifle, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 3), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3), InventoryItem.CreateItem(InventoryItemIDs.Copper, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Rifles, InventoryItem.CreateItem(InventoryItemIDs.GoldSpaceBoltActionRifle, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 3), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3), InventoryItem.CreateItem(InventoryItemIDs.Gold, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Rifles, InventoryItem.CreateItem(InventoryItemIDs.DiamondSpaceBoltActionRifle, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 3), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 4)));
			CookBook.Add(new Recipe(RecipeTypes.Shotguns, InventoryItem.CreateItem(InventoryItemIDs.PumpShotgun, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 3), InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Shotguns, InventoryItem.CreateItem(InventoryItemIDs.GoldPumpShotgun, 1), InventoryItem.CreateItem(InventoryItemIDs.Gold, 3), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Shotguns, InventoryItem.CreateItem(InventoryItemIDs.DiamondPumpShotgun, 1), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3), InventoryItem.CreateItem(InventoryItemIDs.Gold, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Shotguns, InventoryItem.CreateItem(InventoryItemIDs.BloodStonePumpShotgun, 1), InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 20), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3)));
			CookBook.Add(new Recipe(RecipeTypes.Shotguns, InventoryItem.CreateItem(InventoryItemIDs.IronSpacePumpShotgun, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 3), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Shotguns, InventoryItem.CreateItem(InventoryItemIDs.CopperSpacePumpShotgun, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 3), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3), InventoryItem.CreateItem(InventoryItemIDs.Copper, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Shotguns, InventoryItem.CreateItem(InventoryItemIDs.GoldSpacePumpShotgun, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 3), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3), InventoryItem.CreateItem(InventoryItemIDs.Gold, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Shotguns, InventoryItem.CreateItem(InventoryItemIDs.DiamondSpacePumpShotgun, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 3), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 4)));
			CookBook.Add(new Recipe(RecipeTypes.AssaultRifles, InventoryItem.CreateItem(InventoryItemIDs.AssultRifle, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 5), InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 3)));
			CookBook.Add(new Recipe(RecipeTypes.AssaultRifles, InventoryItem.CreateItem(InventoryItemIDs.GoldAssultRifle, 1), InventoryItem.CreateItem(InventoryItemIDs.Gold, 5), InventoryItem.CreateItem(InventoryItemIDs.Iron, 3)));
			CookBook.Add(new Recipe(RecipeTypes.AssaultRifles, InventoryItem.CreateItem(InventoryItemIDs.DiamondAssultRifle, 1), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 5), InventoryItem.CreateItem(InventoryItemIDs.Gold, 3)));
			CookBook.Add(new Recipe(RecipeTypes.AssaultRifles, InventoryItem.CreateItem(InventoryItemIDs.BloodStoneAssultRifle, 1), InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 50), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 6)));
			CookBook.Add(new Recipe(RecipeTypes.AssaultRifles, InventoryItem.CreateItem(InventoryItemIDs.IronSpaceAssultRifle, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 5), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 6), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)));
			CookBook.Add(new Recipe(RecipeTypes.AssaultRifles, InventoryItem.CreateItem(InventoryItemIDs.CopperSpaceAssultRifle, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 5), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 6), InventoryItem.CreateItem(InventoryItemIDs.Copper, 1)));
			CookBook.Add(new Recipe(RecipeTypes.AssaultRifles, InventoryItem.CreateItem(InventoryItemIDs.GoldSpaceAssultRifle, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 5), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 6), InventoryItem.CreateItem(InventoryItemIDs.Gold, 1)));
			CookBook.Add(new Recipe(RecipeTypes.AssaultRifles, InventoryItem.CreateItem(InventoryItemIDs.DiamondSpaceAssultRifle, 1), InventoryItem.CreateItem(InventoryItemIDs.Slime, 5), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 7)));
			CookBook.Add(new Recipe(RecipeTypes.Advanced, InventoryItem.CreateItem(InventoryItemIDs.PrecisionLaser, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 5), InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 3)));
			CookBook.Add(new Recipe(RecipeTypes.Walls, InventoryItem.CreateItem(InventoryItemIDs.CopperWall, 1), InventoryItem.CreateItem(InventoryItemIDs.Copper, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Copper, 1), InventoryItem.CreateItem(InventoryItemIDs.CopperWall, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Walls, InventoryItem.CreateItem(InventoryItemIDs.IronWall, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Iron, 1), InventoryItem.CreateItem(InventoryItemIDs.IronWall, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Walls, InventoryItem.CreateItem(InventoryItemIDs.GoldenWall, 1), InventoryItem.CreateItem(InventoryItemIDs.Gold, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Gold, 1), InventoryItem.CreateItem(InventoryItemIDs.GoldenWall, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Walls, InventoryItem.CreateItem(InventoryItemIDs.DiamondWall, 1), InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2)));
			CookBook.Add(new Recipe(RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1), InventoryItem.CreateItem(InventoryItemIDs.DiamondWall, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Explosives, InventoryItem.CreateItem(InventoryItemIDs.TNT, 1), InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 1), InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 3), InventoryItem.CreateItem(InventoryItemIDs.Coal, 3)));
			CookBook.Add(new Recipe(RecipeTypes.Explosives, InventoryItem.CreateItem(InventoryItemIDs.C4, 1), InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 3), InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 3), InventoryItem.CreateItem(InventoryItemIDs.Coal, 3)));
			CookBook.Add(new Recipe(RecipeTypes.Explosives, InventoryItem.CreateItem(InventoryItemIDs.Grenade, 1), InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Explosives, InventoryItem.CreateItem(InventoryItemIDs.StickyGrenade, 1), InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)));
			CookBook.Add(new Recipe(RecipeTypes.RPG, InventoryItem.CreateItem(InventoryItemIDs.RocketLauncher, 1), InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 2), InventoryItem.CreateItem(InventoryItemIDs.Iron, 3), InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 2)));
			CookBook.Add(new Recipe(RecipeTypes.RPG, InventoryItem.CreateItem(InventoryItemIDs.RocketLauncherGuided, 1), InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 2), InventoryItem.CreateItem(InventoryItemIDs.Iron, 3), InventoryItem.CreateItem(InventoryItemIDs.Copper, 2), InventoryItem.CreateItem(InventoryItemIDs.SpaceRockInventory, 1)));
			CookBook.Add(new Recipe(RecipeTypes.Doors, InventoryItem.CreateItem(InventoryItemIDs.Door, 1), InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 5), InventoryItem.CreateItem(InventoryItemIDs.Copper, 3)));
			CookBook.Add(new Recipe(RecipeTypes.Doors, InventoryItem.CreateItem(InventoryItemIDs.IronDoor, 1), InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 5), InventoryItem.CreateItem(InventoryItemIDs.Iron, 5)));
			CookBook.Add(new Recipe(RecipeTypes.OtherStructure, InventoryItem.CreateItem(InventoryItemIDs.GlassWindowWood, 1), InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 2), InventoryItem.CreateItem(InventoryItemIDs.Stick, 1)));
			CookBook.Add(new Recipe(RecipeTypes.OtherStructure, InventoryItem.CreateItem(InventoryItemIDs.GlassWindowIron, 1), InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 5), InventoryItem.CreateItem(InventoryItemIDs.Coal, 1), InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)));
			CookBook.Add(new Recipe(RecipeTypes.OtherStructure, InventoryItem.CreateItem(InventoryItemIDs.GlassWindowGold, 1), InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 10), InventoryItem.CreateItem(InventoryItemIDs.Coal, 2), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)));
			CookBook.Add(new Recipe(RecipeTypes.OtherStructure, InventoryItem.CreateItem(InventoryItemIDs.GlassWindowDiamond, 1), InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 10), InventoryItem.CreateItem(InventoryItemIDs.Coal, 2), InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)));
		}
	}
}
