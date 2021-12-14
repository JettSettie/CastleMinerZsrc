using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.GraphicsProfileSupport;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Text;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DNA.CastleMinerZ.Inventory
{
	public class InventoryItem
	{
		public abstract class InventoryItemClass
		{
			public InventoryItemIDs ID;

			protected string _name;

			protected string _description;

			public int MaxStackCount;

			public float EnemyDamage;

			public DamageType EnemyDamageType;

			public int ModelNameIndex;

			protected TimeSpan _coolDownTime;

			public float ItemSelfDamagePerUse;

			public IntVector3 Location;

			private string _useSoundCue;

			protected PlayerMode _playerMode;

			public string UseSound
			{
				get
				{
					return _useSoundCue;
				}
			}

			public TimeSpan CoolDownTime
			{
				get
				{
					return _coolDownTime;
				}
			}

			public string Name
			{
				get
				{
					return _name;
				}
			}

			public string Description
			{
				get
				{
					return _description;
				}
			}

			public PlayerMode PlayerAnimationMode
			{
				get
				{
					return _playerMode;
				}
			}

			public virtual bool IsMeleeWeapon
			{
				get
				{
					return true;
				}
			}

			public virtual float PickupTimeoutLength
			{
				get
				{
					return 30f;
				}
			}

			public InventoryItemClass(InventoryItemIDs id, string name, string description, int maxStack, TimeSpan coolDownTime)
			{
				_playerMode = PlayerMode.Generic;
				_useSoundCue = null;
				_coolDownTime = coolDownTime;
				_name = name;
				_description = description;
				MaxStackCount = maxStack;
				EnemyDamage = 0.1f;
				EnemyDamageType = DamageType.BLUNT;
				ID = id;
			}

			public InventoryItemClass(InventoryItemIDs id, string name, string description, int maxStack, TimeSpan coolDownTime, string useSound)
			{
				_useSoundCue = useSound;
				_coolDownTime = coolDownTime;
				_name = name;
				_description = description;
				MaxStackCount = maxStack;
				EnemyDamage = 0.1f;
				EnemyDamageType = DamageType.BLUNT;
				ID = id;
			}

			public abstract Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer);

			public virtual InventoryItem CreateItem(int stackCount)
			{
				return new InventoryItem(this, stackCount);
			}

			public virtual void OnItemEquipped()
			{
			}

			public virtual void OnItemUnequipped()
			{
			}

			public void Draw2D(SpriteBatch batch, Rectangle destRect, Color color)
			{
				if (_2DImages == null || _2DImages.IsContentLost)
				{
					FinishInitialization(batch.GraphicsDevice);
				}
				int iD = (int)ID;
				Texture2D texture;
				Rectangle value;
				if (_2DImagesLarge != null && (float)destRect.Width / 64f > 1.1f)
				{
					texture = _2DImagesLarge;
					value = new Rectangle((iD & 7) * 64 * 2, iD / 8 * 64 * 2, 128, 128);
				}
				else
				{
					texture = _2DImages;
					value = new Rectangle((iD & 7) * 64, iD / 8 * 64, 64, 64);
				}
				batch.Draw(texture, destRect, value, color);
			}

			public void Draw2D(SpriteBatch batch, Rectangle destRect)
			{
				Draw2D(batch, destRect, Color.White);
			}
		}

		public const int UIItemsPerRow = 8;

		public const int UIItemSize = 64;

		public const int UIMapWidth = 512;

		public const int UIMapHeight = 1280;

		private InventoryItemClass _class;

		private OneShotTimer _coolDownTimer;

		protected PCKeyboardInputScreen _keyboardInputScreen;

		protected string _keyboardPromptString;

		protected string _keyboardDefaultText;

		private int _stackCount;

		private bool _displayOnPickup;

		public float ItemHealthLevel = 1f;

		public TimeSpan DigTime = TimeSpan.Zero;

		public IntVector3 DigLocation;

		private StringBuilder sbuilder = new StringBuilder();

		protected static Dictionary<InventoryItemIDs, InventoryItemClass> AllItems = new Dictionary<InventoryItemIDs, InventoryItemClass>();

		public static RenderTarget2D _2DImages = null;

		public static RenderTarget2D _2DImagesLarge = null;

		protected OneShotTimer CoolDownTimer
		{
			get
			{
				return _coolDownTimer;
			}
		}

		public InventoryItemClass ItemClass
		{
			get
			{
				return _class;
			}
		}

		public bool DisplayOnPickup
		{
			get
			{
				return _displayOnPickup;
			}
			set
			{
				_displayOnPickup = value;
			}
		}

		public int StackCount
		{
			get
			{
				return _stackCount;
			}
			set
			{
				_stackCount = value;
			}
		}

		public int MaxStackCount
		{
			get
			{
				return _class.MaxStackCount;
			}
		}

		public string Name
		{
			get
			{
				return _class.Name;
			}
		}

		public string Description
		{
			get
			{
				return _class.Description;
			}
		}

		public bool IsMeleeWeapon
		{
			get
			{
				return _class.IsMeleeWeapon;
			}
		}

		public float EnemyDamage
		{
			get
			{
				return _class.EnemyDamage;
			}
		}

		public DamageType EnemyDamageType
		{
			get
			{
				return _class.EnemyDamageType;
			}
		}

		public PlayerMode PlayerMode
		{
			get
			{
				return _class.PlayerAnimationMode;
			}
		}

		public static InventoryItem Create(BinaryReader reader)
		{
			InventoryItem inventoryItem = null;
			InventoryItemIDs id = (InventoryItemIDs)reader.ReadInt16();
			inventoryItem = CreateItem(id, 0);
			inventoryItem.Read(reader);
			return inventoryItem;
		}

		private bool ItemValidWithZeroStacks(InventoryItemIDs itemID)
		{
			return itemID == InventoryItemIDs.TeleportStation;
		}

		public virtual bool IsValid()
		{
			if (_stackCount <= MaxStackCount && (_stackCount > 0 || ItemValidWithZeroStacks(ItemClass.ID)) && ItemClass.ID != InventoryItemIDs.BloodStoneBullets)
			{
				return ItemClass.ID != InventoryItemIDs.SpaceRock;
			}
			return false;
		}

		public virtual void GetDisplayText(StringBuilder builder)
		{
			builder.Append(_class.Name);
		}

		public bool CanStack(InventoryItem item)
		{
			if (item != this && _class == item._class)
			{
				return StackCount < MaxStackCount;
			}
			return false;
		}

		public void Stack(InventoryItem item)
		{
			if (_class == item._class && item != this && StackCount < MaxStackCount)
			{
				StackCount += item.StackCount;
				item.StackCount = 0;
				if (StackCount > MaxStackCount)
				{
					item.StackCount += StackCount - MaxStackCount;
					StackCount = MaxStackCount;
				}
			}
		}

		public InventoryItem Split()
		{
			InventoryItem inventoryItem = CreateItem(ItemClass.ID, StackCount / 2);
			StackCount -= inventoryItem.StackCount;
			return inventoryItem;
		}

		public InventoryItem PopOneItem()
		{
			InventoryItem result = CreateItem(ItemClass.ID, 1);
			StackCount--;
			return result;
		}

		protected InventoryItem(InventoryItemClass cls, int stackCount)
		{
			_class = cls;
			_coolDownTimer = new OneShotTimer(_class.CoolDownTime);
			StackCount = stackCount;
			SetDefaultValues();
		}

		public bool CanConsume(InventoryItemClass itemType, int amount)
		{
			if (_class != itemType || StackCount < amount)
			{
				return false;
			}
			return true;
		}

		public virtual InventoryItem CreatesWhenDug(BlockTypeEnum block, IntVector3 location)
		{
			switch (block)
			{
			case BlockTypeEnum.Grass:
				return CreateItem(InventoryItemIDs.DirtBlock, 1);
			case BlockTypeEnum.SurfaceLava:
				return CreateItem(InventoryItemIDs.RockBlock, 1);
			case BlockTypeEnum.SpaceRock:
				return CreateItem(InventoryItemIDs.SpaceRockInventory, 1);
			default:
				return BlockInventoryItemClass.CreateBlockItem(block, 1, location);
			}
		}

		public virtual bool InflictDamage()
		{
			ItemHealthLevel -= ItemClass.ItemSelfDamagePerUse;
			if (CastleMinerZGame.Instance.InfiniteResourceMode)
			{
				ItemHealthLevel -= ItemClass.ItemSelfDamagePerUse;
			}
			if (ItemHealthLevel <= 0f)
			{
				return true;
			}
			return false;
		}

		public virtual TimeSpan TimeToDig(BlockTypeEnum blockType)
		{
			if (BlockType.IsContainer(blockType))
			{
				return TimeSpan.FromSeconds(2.0);
			}
			if (BlockType.IsStructure(blockType))
			{
				return TimeSpan.FromSeconds(2.0);
			}
			switch (blockType)
			{
			case BlockTypeEnum.SurfaceLava:
				return TimeSpan.FromSeconds(0.0);
			case BlockTypeEnum.Rock:
				return TimeSpan.FromSeconds(10.0);
			case BlockTypeEnum.Ice:
				return TimeSpan.FromSeconds(5.0);
			case BlockTypeEnum.Log:
				return TimeSpan.FromSeconds(4.0);
			case BlockTypeEnum.Wood:
				return TimeSpan.FromSeconds(3.0);
			case BlockTypeEnum.Leaves:
				return TimeSpan.FromSeconds(1.0);
			case BlockTypeEnum.Sand:
				return TimeSpan.FromSeconds(1.0);
			case BlockTypeEnum.Snow:
				return TimeSpan.FromSeconds(1.0);
			case BlockTypeEnum.Dirt:
				return TimeSpan.FromSeconds(1.5);
			case BlockTypeEnum.Grass:
				return TimeSpan.FromSeconds(1.5);
			case BlockTypeEnum.Torch:
				return TimeSpan.FromSeconds(0.0);
			case BlockTypeEnum.NormalLowerDoor:
			case BlockTypeEnum.StrongLowerDoor:
				return TimeSpan.FromSeconds(2.0);
			case BlockTypeEnum.Lantern:
			case BlockTypeEnum.LanternFancy:
				return TimeSpan.FromSeconds(2.0);
			default:
				return TimeSpan.MaxValue;
			}
		}

		public virtual void ProcessInput(InGameHUD hud, CastleMinerZControllerMapping controller)
		{
			if (hud.ConstructionProbe._worldIndex != DigLocation)
			{
				DigLocation = hud.ConstructionProbe._worldIndex;
				DigTime = TimeSpan.Zero;
			}
			if (controller.Use.Held || controller.Shoulder.Held)
			{
				BlockTypeEnum blockWithChanges = BlockTerrain.Instance.GetBlockWithChanges(hud.ConstructionProbe._worldIndex);
				BlockType type = BlockType.GetType(blockWithChanges);
				TimeSpan t = TimeToDig(type.ParentBlockType);
				float crackAmount = (float)(DigTime.TotalSeconds / t.TotalSeconds);
				CastleMinerZGame.Instance.GameScreen.CrackBox.CrackAmount = crackAmount;
				if ((type._type == BlockTypeEnum.TNT || type._type == BlockTypeEnum.C4) && !(hud.ActiveInventoryItem.ItemClass is SpadeInventoryClass))
				{
					if (controller.Use.Pressed || controller.Shoulder.Pressed)
					{
						hud.SetFuseForExplosive(hud.ConstructionProbe._worldIndex, (type._type != BlockTypeEnum.TNT) ? ExplosiveTypes.C4 : ExplosiveTypes.TNT);
					}
				}
				else if (type.IsItemEntity)
				{
					CastleMinerZGame.Instance.GameScreen.CrackBox.CrackAmount = 0f;
				}
				if (CoolDownTimer.Expired)
				{
					CoolDownTimer.Reset();
					hud.LocalPlayer.UsingTool = true;
					CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(ItemClass.ID);
					itemStats.Used++;
					if (hud.ConstructionProbe.HitPlayer)
					{
						hud.MeleePlayer(this, hud.ConstructionProbe.PlayerHit);
					}
					else if (hud.ConstructionProbe.AbleToBuild)
					{
						if (DigTime >= t)
						{
							hud.Dig(this, true);
							DigTime = TimeSpan.Zero;
						}
						else
						{
							hud.Dig(this, false);
						}
					}
					else if (hud.ConstructionProbe.HitZombie)
					{
						hud.Melee(this);
					}
					return;
				}
			}
			else
			{
				DigTime = TimeSpan.Zero;
			}
			hud.LocalPlayer.UsingTool = false;
		}

		public void Update(GameTime gameTime)
		{
			if (InGameHUD.Instance != null && InGameHUD.Instance.ConstructionProbe.AbleToBuild)
			{
				DigTime += gameTime.ElapsedGameTime;
			}
			else
			{
				DigTime = TimeSpan.Zero;
			}
			_coolDownTimer.Update(gameTime.ElapsedGameTime);
		}

		public void Draw2D(SpriteBatch spriteBatch, Rectangle dest, Color color, bool drawAmt)
		{
			_class.Draw2D(spriteBatch, dest, color);
			if (StackCount > 1 && drawAmt)
			{
				sbuilder.Length = 0;
				sbuilder.Concat(StackCount);
				SpriteFont smallFont = CastleMinerZGame.Instance._smallFont;
				spriteBatch.DrawOutlinedText(smallFont, sbuilder, new Vector2(dest.X + 8, dest.Y + dest.Height - smallFont.LineSpacing), Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			}
		}

		public void Draw2D(SpriteBatch spriteBatch, Rectangle dest)
		{
			if (ItemClass.ItemSelfDamagePerUse > 0f)
			{
				spriteBatch.Draw(CastleMinerZGame.Instance.DummyTexture, new Rectangle(dest.X + 9, dest.Bottom - 16, dest.Width - 18, 7), Color.Black);
				spriteBatch.Draw(CastleMinerZGame.Instance.DummyTexture, new Rectangle(dest.X + 10, dest.Bottom - 15, (int)((float)(dest.Width - 20) * ItemHealthLevel), 5), new Color(67, 188, 0));
			}
			Draw2D(spriteBatch, dest, Color.White, true);
		}

		public virtual void SetLocation(IntVector3 location)
		{
			_class.Location = location;
		}

		public virtual void SetModelNameIndex(int modelNameIndex)
		{
			_class.ModelNameIndex = modelNameIndex;
		}

		public Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			return _class.CreateEntity(use, attachedToLocalPlayer);
		}

		public virtual void Write(BinaryWriter writer)
		{
			writer.Write((short)_class.ID);
			writer.Write((short)StackCount);
			writer.Write(ItemHealthLevel);
		}

		protected virtual void Read(BinaryReader reader)
		{
			StackCount = reader.ReadInt16();
			ItemHealthLevel = reader.ReadSingle();
		}

		protected void InitKeyboard()
		{
			_keyboardInputScreen = new PCKeyboardInputScreen(CastleMinerZGame.Instance, Strings.Name, _keyboardPromptString, CastleMinerZGame.Instance.DialogScreenImage, CastleMinerZGame.Instance._myriadMed, true, CastleMinerZGame.Instance.ButtonFrame);
			_keyboardInputScreen.ClickSound = "Click";
			_keyboardInputScreen.OpenSound = "Popup";
		}

		protected virtual void SetDefaultValues()
		{
			_keyboardPromptString = Strings.Enter_A_Name_For_This_Locator;
			_keyboardDefaultText = "Beta";
		}

		protected virtual void OnKeyboardSubmit()
		{
		}

		protected virtual void OnKeyboardCancel()
		{
		}

		public virtual void ShowKeyboard(string defaultTextOverride = null)
		{
			if (_keyboardInputScreen == null)
			{
				InitKeyboard();
			}
			_keyboardInputScreen.DefaultText = (string.IsNullOrEmpty(defaultTextOverride) ? _keyboardDefaultText : defaultTextOverride);
			CastleMinerZGame.Instance.GameScreen._uiGroup.ShowPCDialogScreen(_keyboardInputScreen, delegate
			{
				if (_keyboardInputScreen.OptionSelected != -1)
				{
					OnKeyboardSubmit();
				}
				else
				{
					OnKeyboardCancel();
				}
			});
		}

		public static InventoryItem CreateItem(InventoryItemIDs id, int stackCount)
		{
			return AllItems[id].CreateItem(stackCount);
		}

		public static InventoryItemClass GetClass(InventoryItemIDs id)
		{
			return AllItems[id];
		}

		public static Entity CreateEntity(InventoryItemIDs id, ItemUse use, bool attachedToLocalPlayer)
		{
			InventoryItemClass @class = GetClass(id);
			return @class.CreateEntity(use, attachedToLocalPlayer);
		}

		public static void Initalize(ContentManager content)
		{
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.WoodBlock, BlockTypeEnum.Wood, Strings.Made_from_logs + ". " + Strings.This_is_a_raw_material_that_must_be_found, 0.075f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.RockBlock, BlockTypeEnum.Rock, Strings.Commonly_found_underground + ". " + Strings.This_is_a_raw_material_that_must_be_found, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.SandBlock, BlockTypeEnum.Sand, Strings.Found_on_the_surface + ". " + Strings.This_is_a_raw_material_that_must_be_found, 0.01f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.DirtBlock, BlockTypeEnum.Dirt, Strings.Found_on_the_surface + ". " + Strings.This_is_a_raw_material_that_must_be_found, 0.01f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.LogBlock, BlockTypeEnum.Log, Strings.Comes_from_trees + ". " + Strings.This_is_a_raw_material_that_must_be_found, 0.075f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.LanternBlock, BlockTypeEnum.Lantern, Strings.Lights_the_world + ". " + Strings.More_durable_than_a_torch, 0.075f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.BloodStoneBlock, BlockTypeEnum.BloodStone, Strings.Found_in_hell + ". " + Strings.Bloodstone_is_very_hard, 0.15f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.SpaceRock, BlockTypeEnum.SpaceRock, Strings.Comes_from_space + ". " + Strings.Junk, 0.15f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.SpaceRockInventory, BlockTypeEnum.SpaceRockInventory, Strings.Comes_from_space + ". " + Strings.Used_to_make_alien_tools_and_weapons, 0.15f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.IronWall, BlockTypeEnum.IronWall, Strings.Strong_walls_for_building + ". " + Strings.Prevents_some_monsters_from_digging, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.CopperWall, BlockTypeEnum.CopperWall, Strings.Strong_walls_for_building + ". " + Strings.Prevents_some_monsters_from_digging, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.GoldenWall, BlockTypeEnum.GoldenWall, Strings.Strong_walls_for_building + ". " + Strings.Prevents_some_monsters_from_digging, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.DiamondWall, BlockTypeEnum.DiamondWall, Strings.Strong_walls_for_building + ". " + Strings.Prevents_some_monsters_from_digging, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.Crate, BlockTypeEnum.Crate, Strings.Used_for_storing_items, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.Snow, BlockTypeEnum.Snow, Strings.Found_on_the_surface + ". " + Strings.This_is_a_raw_material_that_must_be_found, 0.01f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.TNT, BlockTypeEnum.TNT, Strings.Used_to_blow_up_large_areas + ". " + Strings.This_is_a_raw_material_that_must_be_found, 0.01f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.Ice, BlockTypeEnum.Ice, Strings.Found_on_the_surface + ". " + Strings.Only_destroys_certain_materials, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.C4, BlockTypeEnum.C4, Strings.Used_to_blow_up_large_areas + ". " + Strings.Destroys_everything, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.Slime, BlockTypeEnum.Slime, Strings.Space_Goo + ". " + Strings.Used_to_make_alien_weapons, 0.075f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.GlassWindowWood, BlockTypeEnum.GlassBasic, Strings.Window_Sticks_flavor1 + ". " + Strings.Window_Sticks_flavor2, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.GlassWindowIron, BlockTypeEnum.GlassIron, Strings.Window_Iron_flavor1 + ". " + Strings.Window_Iron_flavor2, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.GlassWindowGold, BlockTypeEnum.GlassStrong, Strings.Window_Bulletproof_flavor1 + ". " + Strings.Window_Bulletproof_flavor2, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.GlassWindowDiamond, BlockTypeEnum.GlassMystery, Strings.Window_Clear_flavor1 + ". " + Strings.Window_Clear_flavor2, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.StoneContainer, BlockTypeEnum.CrateStone, Strings.Used_for_storing_items, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.CopperContainer, BlockTypeEnum.CrateCopper, Strings.Used_for_storing_items, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.IronContainer, BlockTypeEnum.CrateIron, Strings.Used_for_storing_items, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.GoldContainer, BlockTypeEnum.CrateGold, Strings.Used_for_storing_items, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.DiamondContainer, BlockTypeEnum.CrateDiamond, Strings.Used_for_storing_items, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.BloodstoneContainer, BlockTypeEnum.CrateBloodstone, Strings.Used_for_storing_items, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.LanternFancyBlock, BlockTypeEnum.LanternFancy, Strings.Lights_the_world + ". " + Strings.More_durable_than_a_torch, 0.075f));
			Model model = content.Load<Model>("Props\\Tools\\PickAxe\\Model");
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.StonePickAxe, ToolMaterialTypes.Stone, model, Strings.Stone_PickAxe, Strings.Used_for_breaking_certain_stones_and_ores, 0.1f));
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.CopperPickAxe, ToolMaterialTypes.Copper, model, Strings.Copper_PickAxe, Strings.Used_for_breaking_certain_stones_and_ores, 0.2f));
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.IronPickAxe, ToolMaterialTypes.Iron, model, Strings.Iron_PickAxe, Strings.Used_for_breaking_certain_stones_and_ores, 0.4f));
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.GoldPickAxe, ToolMaterialTypes.Gold, model, Strings.Gold_PickAxe, Strings.Used_for_breaking_certain_stones_and_ores, 0.8f));
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.DiamondPickAxe, ToolMaterialTypes.Diamond, model, Strings.Diamond_PickAxe, Strings.Used_for_breaking_certain_stones_and_ores, 1.6f));
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.BloodstonePickAxe, ToolMaterialTypes.BloodStone, model, Strings.BloodStone_PickAxe, Strings.Used_for_breaking_certain_stones_and_ores, 3f));
			Model model2 = content.Load<Model>("Props\\Weapons\\Space\\Saber\\Model");
			RegisterItemClass(new SaberInventoryItemClass(InventoryItemIDs.CopperLaserSword, ToolMaterialTypes.Copper, model2, Strings.Copper_Laser_Sword, Strings.Advanced_melee_and_mining_tool, 8f));
			RegisterItemClass(new SaberInventoryItemClass(InventoryItemIDs.IronLaserSword, ToolMaterialTypes.Iron, model2, Strings.Iron_Laser_Sword, Strings.Advanced_melee_and_mining_tool, 8f));
			RegisterItemClass(new SaberInventoryItemClass(InventoryItemIDs.GoldLaserSword, ToolMaterialTypes.Gold, model2, Strings.Gold_Laser_Sword, Strings.Advanced_melee_and_mining_tool, 8f));
			RegisterItemClass(new SaberInventoryItemClass(InventoryItemIDs.DiamondLaserSword, ToolMaterialTypes.Diamond, model2, Strings.Diamond_Laser_Sword, Strings.Advanced_melee_and_mining_tool, 8f));
			RegisterItemClass(new SaberInventoryItemClass(InventoryItemIDs.BloodStoneLaserSword, ToolMaterialTypes.BloodStone, model2, Strings.BloodStone_Laser_Sword, Strings.Advanced_melee_and_mining_tool, 8f));
			Model model3 = content.Load<Model>("Props\\Tools\\Spade\\Model");
			RegisterItemClass(new SpadeInventoryClass(InventoryItemIDs.StoneSpade, ToolMaterialTypes.Stone, model3, Strings.Stone_Spade, Strings.Used_for_digging_dirt_and_sand + ". " + Strings.Also_removes_C4_and_TNT, 0.1f));
			RegisterItemClass(new SpadeInventoryClass(InventoryItemIDs.CopperSpade, ToolMaterialTypes.Copper, model3, Strings.Copper_Spade, Strings.Used_for_digging_dirt_and_sand + ". " + Strings.Also_removes_C4_and_TNT, 0.2f));
			RegisterItemClass(new SpadeInventoryClass(InventoryItemIDs.IronSpade, ToolMaterialTypes.Iron, model3, Strings.Iron_Spade, Strings.Used_for_digging_dirt_and_sand + ". " + Strings.Also_removes_C4_and_TNT, 0.4f));
			RegisterItemClass(new SpadeInventoryClass(InventoryItemIDs.GoldSpade, ToolMaterialTypes.Gold, model3, Strings.Gold_Spade, Strings.Used_for_digging_dirt_and_sand + ". " + Strings.Also_removes_C4_and_TNT, 0.8f));
			RegisterItemClass(new SpadeInventoryClass(InventoryItemIDs.DiamondSpade, ToolMaterialTypes.Diamond, model3, Strings.Diamond_Spade, Strings.Used_for_digging_dirt_and_sand + ". " + Strings.Also_removes_C4_and_TNT, 1.6f));
			Model model4 = content.Load<Model>("Props\\Tools\\Axe\\Model");
			RegisterItemClass(new AxeInventoryClass(InventoryItemIDs.StoneAxe, ToolMaterialTypes.Stone, model4, Strings.Stone_Axe, Strings.Used_for_chopping_wood + ". " + Strings.Can_also_be_used_for_basic_melee_defense, 0.15f));
			RegisterItemClass(new AxeInventoryClass(InventoryItemIDs.CopperAxe, ToolMaterialTypes.Copper, model4, Strings.Copper_Axe, Strings.Used_for_chopping_wood + ". " + Strings.Can_also_be_used_for_basic_melee_defense, 0.3f));
			RegisterItemClass(new AxeInventoryClass(InventoryItemIDs.IronAxe, ToolMaterialTypes.Iron, model4, Strings.Iron_Axe, Strings.Used_for_chopping_wood + ". " + Strings.Can_also_be_used_for_basic_melee_defense, 0.5f));
			RegisterItemClass(new AxeInventoryClass(InventoryItemIDs.GoldAxe, ToolMaterialTypes.Gold, model4, Strings.Gold_Axe, Strings.Used_for_chopping_wood + ". " + Strings.Can_also_be_used_for_basic_melee_defense, 1f));
			RegisterItemClass(new AxeInventoryClass(InventoryItemIDs.DiamondAxe, ToolMaterialTypes.Diamond, model4, Strings.Diamond_Axe, Strings.Used_for_chopping_wood + ". " + Strings.Can_also_be_used_for_basic_melee_defense, 2f));
			Model model5 = content.Load<Model>("Props\\Tools\\Chainsaw\\Model");
			RegisterItemClass(new ChainsawInventoryItemClass(InventoryItemIDs.Chainsaw1, ToolMaterialTypes.BloodStone, model5, Strings.Chainsaw_1, Strings.Used_for_chopping_wood + ". " + Strings.Can_also_be_used_for_basic_melee_defense, 5f));
			Model model6 = content.Load<Model>("Props\\Items\\Ammo\\Model");
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.BrassCasing, model6, Strings.Brass_Casing, Strings.Used_for_making_ammunition, 5000, TimeSpan.FromSeconds(0.3), Color.Transparent, CMZColors.Brass));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.IronCasing, model6, Strings.Iron_Casing, Strings.Used_for_making_ammunition, 5000, TimeSpan.FromSeconds(0.3), Color.Transparent, CMZColors.Iron));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.GoldCasing, model6, Strings.Gold_Casing, Strings.Used_for_making_ammunition, 5000, TimeSpan.FromSeconds(0.3), Color.Transparent, CMZColors.Gold));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.DiamondCasing, model6, Strings.Diamond_Casing, Strings.Used_for_making_ammunition, 5000, TimeSpan.FromSeconds(0.3), Color.Transparent, CMZColors.Diamond));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.Bullets, model6, Strings.Bullets, Strings.Ammo_for_conventional_weapons, 5000, TimeSpan.FromSeconds(0.3), Color.DarkGray, CMZColors.Brass));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.IronBullets, model6, Strings.Iron_Bullets, Strings.Ammo_for_gold_weapons, 5000, TimeSpan.FromSeconds(0.3), Color.LightGray, CMZColors.Brass));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.GoldBullets, model6, Strings.Gold_Bullets, Strings.Ammo_for_diamond_weapons, 5000, TimeSpan.FromSeconds(0.3), new Color(255, 215, 0), CMZColors.Iron));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.DiamondBullets, model6, Strings.Diamond_Bullets, Strings.Ammo_for_bloodstone, 5000, TimeSpan.FromSeconds(0.3), Color.Cyan, CMZColors.Gold));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.BloodStoneBullets, model6, Strings.BloodStone_Bullets, "", 5000, TimeSpan.FromSeconds(0.3), Color.DarkRed, CMZColors.Diamond));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.LaserBullets, model6, Strings.Laser_Bullets, Strings.Ammo_for_laser_weapons, 5000, TimeSpan.FromSeconds(0.3), Color.LimeGreen, CMZColors.Stone));
			Model model7 = content.Load<Model>("Props\\Weapons\\Conventional\\RPG\\RPGGrenade");
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.RocketAmmo, model7, Strings.Rockets, Strings.Ammo_for_rocket_launchers, 5000, TimeSpan.FromSeconds(0.3), Color.DarkGray, CMZColors.Brass));
			RegisterItemClass(new RocketLauncherInventoryItemClass(InventoryItemIDs.RocketLauncher, Strings.Rocket_Launcher, Strings.Dumb_fired_projectile_grenade + ". " + Strings.Uses_Rockets, 100f, 1f, GetClass(InventoryItemIDs.RocketAmmo)));
			RegisterItemClass(new RocketLauncherGuidedInventoryItemClass(InventoryItemIDs.RocketLauncherGuided, Strings.Anti_Dragon_Guided_Missile, Strings.Guided_missile_used_for_killing_dragons + ". " + Strings.Uses_Rockets, 100f, 1f, GetClass(InventoryItemIDs.RocketAmmo)));
			RegisterItemClass(new RocketLauncherInventoryItemClass(InventoryItemIDs.RocketLauncherShotFired, "", "", 100f, 1f, GetClass(InventoryItemIDs.RocketAmmo)));
			RegisterItemClass(new RocketLauncherInventoryItemClass(InventoryItemIDs.RocketLauncherGuidedShotFired, "", "", 100f, 1f, GetClass(InventoryItemIDs.RocketAmmo)));
			RegisterItemClass(new GrenadeLauncherInventoryItemClass(InventoryItemIDs.BasicGrenadeLauncher, Strings.Grenade_Launcher, Strings.Dumb_fired_projectile_grenade + ". " + Strings.Uses_grenades_or_ball_projectiles_from_top_left_inventory_first, 100f, 1f, GetClass(InventoryItemIDs.RocketAmmo)));
			Model model8 = content.Load<Model>("Props\\Weapons\\Conventional\\Grenade\\Model");
			RegisterItemClass(new GrenadeInventoryItemClass(InventoryItemIDs.Grenade, model8, Strings.Grenade, Strings.Blow_up_Zombies, GrenadeTypeEnum.HE));
			RegisterItemClass(new GrenadeInventoryItemClass(InventoryItemIDs.StickyGrenade, model8, Strings.Sticky_Grenade, Strings.Sticks_to_terrain_and_zombies, GrenadeTypeEnum.Sticky));
			RegisterItemClass(new StickInventoryItemClass(InventoryItemIDs.Stick, Color.Gray, model, Strings.Wood_Stick, Strings.Use_this_to_make_various_items + ". " + Strings.Such_as_a_pickaxe_or_a_torch, 0.05f));
			RegisterItemClass(new TorchInventoryItemClass());
			RegisterItemClass(new DoorInventoryItemClass(InventoryItemIDs.Door, BlockTypeEnum.NormalLowerDoor, DoorEntity.ModelNameEnum.Wood, Strings.Open_or_close_to_keep_monsters_out));
			RegisterItemClass(new DoorInventoryItemClass(InventoryItemIDs.IronDoor, BlockTypeEnum.StrongLowerDoor, DoorEntity.ModelNameEnum.Iron, Strings.Strong_Door_Description));
			RegisterItemClass(new DoorInventoryItemClass(InventoryItemIDs.DiamondDoor, BlockTypeEnum.StrongLowerDoor, DoorEntity.ModelNameEnum.Diamond, Strings.Strong_Door_Description));
			RegisterItemClass(new DoorInventoryItemClass(InventoryItemIDs.TechDoor, BlockTypeEnum.StrongLowerDoor, DoorEntity.ModelNameEnum.Tech, Strings.Strong_Door_Description));
			Model model9 = content.Load<Model>("Props\\Items\\GunPowder\\Model");
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.GunPowder, model9, Strings.Gun_Powder, Strings.Used_to_craft_ammunition + ". " + Strings.This_is_a_raw_material_that_must_be_found, 64, TimeSpan.FromSeconds(0.30000001192092896), Color.Gray));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.ExplosivePowder, model9, Strings.Explosive_Powder, Strings.Used_to_craft_explosives + ". " + Strings.Dropped_by_dragons_and_demons, 64, TimeSpan.FromSeconds(0.30000001192092896), Color.Red));
			int maxStack = 255;
			Model model10 = content.Load<Model>("Props\\Items\\Ore\\Model");
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.Coal, model10, Strings.Coal, Strings.Used_to_craft_items + ". " + Strings.This_is_a_raw_material_that_must_be_found, maxStack, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.Coal, CMZColors.Coal));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.IronOre, model10, Strings.Iron_Ore, Strings.Can_be_made_into_iron + ". " + Strings.This_is_a_raw_material_that_must_be_found, maxStack, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.IronOre, Color.Gray));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.CopperOre, model10, Strings.Copper_Ore, Strings.Can_be_made_into_copper + ". " + Strings.This_is_a_raw_material_that_must_be_found, maxStack, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.CopperOre, Color.Gray));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.GoldOre, model10, Strings.Gold_Ore, Strings.Can_be_made_into_gold + ". " + Strings.This_is_a_raw_material_that_must_be_found, maxStack, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.Gold, Color.Gray));
			Model model11 = content.Load<Model>("Props\\Items\\Gems\\Model");
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.Diamond, model11, Strings.Diamond, Strings.Very_hard_substance + ". " + Strings.Used_to_make_diamond_tools, maxStack, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.Diamond, Color.Gray));
			Model model12 = content.Load<Model>("Props\\Items\\Bars\\Model");
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.Iron, model12, Strings.Iron, Strings.Used_to_craft_items + ". " + Strings.Made_from_Iron_ore, maxStack, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.Iron, Color.Gray));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.Copper, model12, Strings.Copper, Strings.Used_to_craft_items + ". " + Strings.Made_from_Copper_ore, maxStack, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.Copper, Color.Gray));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.Gold, model12, Strings.Gold, Strings.Used_to_craft_items + ". " + Strings.Made_from_Gold_ore, maxStack, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.Gold, Color.Gray));
			Model model13 = content.Load<Model>("Props\\Tools\\Compass\\Model");
			RegisterItemClass(new CompassInventoryItemClass(InventoryItemIDs.Compass, model13));
			Model model14 = content.Load<Model>("Props\\Tools\\Locator\\Model");
			Model model15 = content.Load<Model>("Props\\Tools\\Teleporter\\Model");
			RegisterItemClass(new GPSItemClass(InventoryItemIDs.GPS, model14, Strings.Locator, Strings.Show_the_direction_to_a_chosen_location_and_GPS_coordinates));
			RegisterItemClass(new GPSItemClass(InventoryItemIDs.TeleportGPS, model15, Strings.Teleporter, Strings.Show_the_direction_to_a_chosen_location_and_GPS_coordinates + ". " + Strings.Use_the_item_by_pressing_the_left_trigger_to_teleport_to_the_chosen_location));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.SpawnBasic, BlockTypeEnum.SpawnPointBasic, Strings.Spawn_Point_Flavor, 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.TeleportStation, BlockTypeEnum.TeleportStation, Strings.Teleport_Station_Description, 0.1f, 1));
			Model model16 = content.Load<Model>("Props\\Tools\\Clock\\Model");
			RegisterItemClass(new ClockInventoryItemClass(InventoryItemIDs.Clock, model16));
			RegisterItemClass(new LaserDrillInventoryItemClass(InventoryItemIDs.LaserDrill, ToolMaterialTypes.BloodStone, Strings.Laser_Drill, Strings.A_modified_Laser_Rifle_that_is_able_to_harvest_ore + "! " + Strings.Uses_Gold_Bullets, 0.2f, 0.0001f, GetClass(InventoryItemIDs.GoldBullets)));
			RegisterItemClass(new BareHandInventoryItemClass());
			Model model17 = content.Load<Model>("Props\\Weapons\\Conventional\\Knife\\Model");
			RegisterItemClass(new KnifeInventoryItemClass(InventoryItemIDs.Knife, model17, ToolMaterialTypes.Iron, Strings.Knife, Strings.Basic_Melee_Defense, 0.5f, 0.02f, TimeSpan.FromSeconds(0.5)));
			RegisterItemClass(new KnifeInventoryItemClass(InventoryItemIDs.GoldKnife, model17, ToolMaterialTypes.Gold, Strings.Gold_Knife, Strings.Basic_Melee_Defense, 1f, 0.01f, TimeSpan.FromSeconds(0.4)));
			RegisterItemClass(new KnifeInventoryItemClass(InventoryItemIDs.DiamondKnife, model17, ToolMaterialTypes.Diamond, Strings.Diamond_Knife, Strings.Basic_Melee_Defense, 2f, 0.005f, TimeSpan.FromSeconds(0.3)));
			RegisterItemClass(new KnifeInventoryItemClass(InventoryItemIDs.BloodStoneKnife, model17, ToolMaterialTypes.BloodStone, Strings.BloodStone_Knife, Strings.Basic_Melee_Defense, 4f, 0.00333333341f, TimeSpan.FromSeconds(0.25)));
			RegisterItemClass(new LaserARInventoryItemClass(InventoryItemIDs.IronSpaceAssultRifle, ToolMaterialTypes.Iron, Strings.Laser_Assault_Rifle, Strings.High_power_full_auto + ". " + Strings.Uses_Laser_Bullets, 15f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserSMGClass(InventoryItemIDs.IronSpaceSMGGun, ToolMaterialTypes.Iron, Strings.Laser_Sub_Machine_Gun, Strings.High_rate_of_fire + ". " + Strings.Uses_Laser_Bullets, 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserPistolClass(InventoryItemIDs.IronSpacePistol, ToolMaterialTypes.Iron, Strings.Laser_Pistol, Strings.Basic_semi_automatic_gun + ". " + Strings.Uses_Laser_Bullets, 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserRifleClass(InventoryItemIDs.IronSpaceBoltActionRifle, ToolMaterialTypes.Iron, Strings.Laser_Rifle, Strings.High_power_very_accurate + ". " + Strings.Uses_Laser_Bullets, 15f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserShotgunClass(InventoryItemIDs.IronSpacePumpShotgun, ToolMaterialTypes.Iron, Strings.Laser_Shotgun, Strings.Short_range_burst_fire + ". " + Strings.Uses_Laser_Bullets, 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserARInventoryItemClass(InventoryItemIDs.CopperSpaceAssultRifle, ToolMaterialTypes.Copper, Strings.Laser_Assault_Rifle, Strings.High_power_full_auto + ". " + Strings.Uses_Laser_Bullets, 15f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserSMGClass(InventoryItemIDs.CopperSpaceSMGGun, ToolMaterialTypes.Copper, Strings.Laser_Sub_Machine_Gun, Strings.High_rate_of_fire + ". " + Strings.Uses_Laser_Bullets, 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserPistolClass(InventoryItemIDs.CopperSpacePistol, ToolMaterialTypes.Copper, Strings.Laser_Pistol, Strings.Basic_semi_automatic_gun + ". " + Strings.Uses_Laser_Bullets, 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserRifleClass(InventoryItemIDs.CopperSpaceBoltActionRifle, ToolMaterialTypes.Copper, Strings.Laser_Rifle, Strings.High_power_very_accurate + ". " + Strings.Uses_Laser_Bullets, 15f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserShotgunClass(InventoryItemIDs.CopperSpacePumpShotgun, ToolMaterialTypes.Copper, Strings.Laser_Shotgun, Strings.Short_range_burst_fire + ". " + Strings.Uses_Laser_Bullets, 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserARInventoryItemClass(InventoryItemIDs.GoldSpaceAssultRifle, ToolMaterialTypes.Gold, Strings.Laser_Assault_Rifle, Strings.High_power_full_auto + ". " + Strings.Uses_Laser_Bullets, 15f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserSMGClass(InventoryItemIDs.GoldSpaceSMGGun, ToolMaterialTypes.Gold, Strings.Laser_Sub_Machine_Gun, Strings.High_rate_of_fire + ". " + Strings.Uses_Laser_Bullets, 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserPistolClass(InventoryItemIDs.GoldSpacePistol, ToolMaterialTypes.Gold, Strings.Laser_Pistol, Strings.Basic_semi_automatic_gun + ". " + Strings.Uses_Laser_Bullets, 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserRifleClass(InventoryItemIDs.GoldSpaceBoltActionRifle, ToolMaterialTypes.Gold, Strings.Laser_Rifle, Strings.High_power_very_accurate + ". " + Strings.Uses_Laser_Bullets, 15f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserShotgunClass(InventoryItemIDs.GoldSpacePumpShotgun, ToolMaterialTypes.Gold, Strings.Laser_Shotgun, Strings.Short_range_burst_fire + ". " + Strings.Uses_Laser_Bullets, 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserARInventoryItemClass(InventoryItemIDs.DiamondSpaceAssultRifle, ToolMaterialTypes.Diamond, Strings.Laser_Assault_Rifle, Strings.High_power_full_auto + ". " + Strings.Uses_Laser_Bullets, 15f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserSMGClass(InventoryItemIDs.DiamondSpaceSMGGun, ToolMaterialTypes.Diamond, Strings.Laser_Sub_Machine_Gun, Strings.High_rate_of_fire + ". " + Strings.Uses_Laser_Bullets, 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserPistolClass(InventoryItemIDs.DiamondSpacePistol, ToolMaterialTypes.Diamond, Strings.Laser_Pistol, Strings.Basic_semi_automatic_gun + ". " + Strings.Uses_Laser_Bullets, 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserRifleClass(InventoryItemIDs.DiamondSpaceBoltActionRifle, ToolMaterialTypes.Diamond, Strings.Laser_Rifle, Strings.High_power_very_accurate + ". " + Strings.Uses_Laser_Bullets, 15f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserShotgunClass(InventoryItemIDs.DiamondSpacePumpShotgun, ToolMaterialTypes.Diamond, Strings.Laser_Shotgun, Strings.Short_range_burst_fire + ". " + Strings.Uses_Laser_Bullets, 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new AssultRifleInventoryItemClass(InventoryItemIDs.AssultRifle, ToolMaterialTypes.Iron, Strings.Assault_Rifle, Strings.High_power_full_auto + ". " + Strings.Uses_Bullets, 0.5f, 0.001f, GetClass(InventoryItemIDs.Bullets)));
			RegisterItemClass(new PumpShotgunInventoryItemClass(InventoryItemIDs.PumpShotgun, ToolMaterialTypes.Iron, Strings.Shotgun, Strings.Short_range_burst_fire + ". " + Strings.Uses_Bullets, 0.3f, 0.001f, GetClass(InventoryItemIDs.Bullets)));
			RegisterItemClass(new SMGInventoryItemClass(InventoryItemIDs.SMGGun, ToolMaterialTypes.Iron, Strings.Sub_Machine_Gun, Strings.High_rate_of_fire + ". " + Strings.Uses_Bullets, 0.3f, 0.001f, GetClass(InventoryItemIDs.Bullets)));
			RegisterItemClass(new LMGInventoryItemClass(InventoryItemIDs.LMGGun, ToolMaterialTypes.Iron, Strings.Light_Machine_Gun, Strings.Powerful_with_a_large_clip_capacity + ". " + Strings.Uses_Bullets, 0.5f, 0.001f, GetClass(InventoryItemIDs.Bullets)));
			RegisterItemClass(new BoltRifleInventoryItemClass(InventoryItemIDs.BoltActionRifle, ToolMaterialTypes.Iron, Strings.Rifle, Strings.High_power_very_accurate + ". " + Strings.Uses_Bullets, 0.5f, 0.001f, GetClass(InventoryItemIDs.Bullets)));
			RegisterItemClass(new PistolInventoryItemClass(InventoryItemIDs.Pistol, ToolMaterialTypes.Iron, Strings.Pistol, Strings.Basic_semi_automatic_gun + ". " + Strings.Uses_Bullets, 0.3f, 0.001f, GetClass(InventoryItemIDs.Bullets)));
			RegisterItemClass(new AssultRifleInventoryItemClass(InventoryItemIDs.GoldAssultRifle, ToolMaterialTypes.Gold, Strings.Gold_Assault_Rifle, Strings.High_power_full_auto + ". " + Strings.Uses_Iron_Bullets, 2.5f, 0.000454545458f, GetClass(InventoryItemIDs.IronBullets)));
			RegisterItemClass(new PumpShotgunInventoryItemClass(InventoryItemIDs.GoldPumpShotgun, ToolMaterialTypes.Gold, Strings.Gold_Shotgun, Strings.Short_range_burst_fire + ". " + Strings.Uses_Iron_Bullets, 1f, 0.000454545458f, GetClass(InventoryItemIDs.IronBullets)));
			RegisterItemClass(new SMGInventoryItemClass(InventoryItemIDs.GoldSMGGun, ToolMaterialTypes.Gold, Strings.Gold_Sub_Machine_Gun, Strings.High_rate_of_fire + ". " + Strings.Uses_Iron_Bullets, 1f, 0.000454545458f, GetClass(InventoryItemIDs.IronBullets)));
			RegisterItemClass(new LMGInventoryItemClass(InventoryItemIDs.GoldLMGGun, ToolMaterialTypes.Gold, Strings.Gold_Light_Machine_Gun, Strings.Powerful_with_a_large_clip_capacity + ". " + Strings.Uses_Iron_Bullets, 2.5f, 0.000454545458f, GetClass(InventoryItemIDs.IronBullets)));
			RegisterItemClass(new BoltRifleInventoryItemClass(InventoryItemIDs.GoldBoltActionRifle, ToolMaterialTypes.Gold, Strings.Gold_Rifle, Strings.High_power_very_accurate + ". " + Strings.Uses_Iron_Bullets, 2.5f, 0.000454545458f, GetClass(InventoryItemIDs.IronBullets)));
			RegisterItemClass(new PistolInventoryItemClass(InventoryItemIDs.GoldPistol, ToolMaterialTypes.Gold, Strings.Gold_Pistol, Strings.Basic_semi_automatic_gun + ". " + Strings.Uses_Iron_Bullets, 1f, 0.000454545458f, GetClass(InventoryItemIDs.IronBullets)));
			RegisterItemClass(new AssultRifleInventoryItemClass(InventoryItemIDs.DiamondAssultRifle, ToolMaterialTypes.Diamond, Strings.Diamond_Assault_Rifle, Strings.High_power_full_auto + ". " + Strings.Uses_Gold_Bullets, 6f, 0.000239234447f, GetClass(InventoryItemIDs.GoldBullets)));
			RegisterItemClass(new PumpShotgunInventoryItemClass(InventoryItemIDs.DiamondPumpShotgun, ToolMaterialTypes.Diamond, Strings.Diamond_Shotgun, Strings.Short_range_burst_fire + ". " + Strings.Uses_Gold_Bullets, 4f, 0.000239234447f, GetClass(InventoryItemIDs.GoldBullets)));
			RegisterItemClass(new SMGInventoryItemClass(InventoryItemIDs.DiamondSMGGun, ToolMaterialTypes.Diamond, Strings.Diamond_Sub_Machine_Gun, Strings.High_rate_of_fire + ". " + Strings.Uses_Gold_Bullets, 4f, 0.000239234447f, GetClass(InventoryItemIDs.GoldBullets)));
			RegisterItemClass(new LMGInventoryItemClass(InventoryItemIDs.DiamondLMGGun, ToolMaterialTypes.Diamond, Strings.Diamond_Light_Machine_Gun, Strings.Powerful_with_a_large_clip_capacity + ". " + Strings.Uses_Gold_Bullets, 6f, 0.000239234447f, GetClass(InventoryItemIDs.GoldBullets)));
			RegisterItemClass(new BoltRifleInventoryItemClass(InventoryItemIDs.DiamondBoltActionRifle, ToolMaterialTypes.Diamond, Strings.Diamond_Rifle, Strings.High_power_very_accurate + ". " + Strings.Uses_Gold_Bullets, 6f, 0.000239234447f, GetClass(InventoryItemIDs.GoldBullets)));
			RegisterItemClass(new PistolInventoryItemClass(InventoryItemIDs.DiamondPistol, ToolMaterialTypes.Diamond, Strings.Diamond_Pistol, Strings.Basic_semi_automatic_gun + ". " + Strings.Uses_Gold_Bullets, 4f, 0.000239234447f, GetClass(InventoryItemIDs.GoldBullets)));
			RegisterItemClass(new AssultRifleInventoryItemClass(InventoryItemIDs.BloodStoneAssultRifle, ToolMaterialTypes.BloodStone, Strings.BloodStone_Assault_Rifle, Strings.High_power_full_auto + ". " + Strings.Uses_Diamond_Bullets, 12f, 0.0001f, GetClass(InventoryItemIDs.DiamondBullets)));
			RegisterItemClass(new PumpShotgunInventoryItemClass(InventoryItemIDs.BloodStonePumpShotgun, ToolMaterialTypes.BloodStone, Strings.BloodStone_Shotgun, Strings.Short_range_burst_fire + ". " + Strings.Uses_Diamond_Bullets, 8f, 0.0001f, GetClass(InventoryItemIDs.DiamondBullets)));
			RegisterItemClass(new SMGInventoryItemClass(InventoryItemIDs.BloodStoneSMGGun, ToolMaterialTypes.BloodStone, Strings.BloodStone_Sub_Machine_Gun, Strings.High_rate_of_fire + ". " + Strings.Uses_Diamond_Bullets, 8f, 0.0001f, GetClass(InventoryItemIDs.DiamondBullets)));
			RegisterItemClass(new LMGInventoryItemClass(InventoryItemIDs.BloodStoneLMGGun, ToolMaterialTypes.BloodStone, Strings.BloodStone_Light_Machine_Gun, Strings.Powerful_with_a_large_clip_capacity + ". " + Strings.Uses_Diamond_Bullets, 12f, 0.0001f, GetClass(InventoryItemIDs.DiamondBullets)));
			RegisterItemClass(new BoltRifleInventoryItemClass(InventoryItemIDs.BloodStoneBoltActionRifle, ToolMaterialTypes.BloodStone, Strings.BloodStone_Rifle, Strings.High_power_very_accurate + ". " + Strings.Uses_Diamond_Bullets, 12f, 0.0001f, GetClass(InventoryItemIDs.DiamondBullets)));
			RegisterItemClass(new PistolInventoryItemClass(InventoryItemIDs.BloodStonePistol, ToolMaterialTypes.BloodStone, Strings.BloodStone_Pistol, Strings.Basic_semi_automatic_gun + ". " + Strings.Uses_Diamond_Bullets, 8f, 0.0001f, GetClass(InventoryItemIDs.DiamondBullets)));
			RegisterItemClass(new LaserPrecisionInventoryItemClass(InventoryItemIDs.PrecisionLaser, ToolMaterialTypes.BloodStone, Strings.Laser_Drill, Strings.A_modified_Laser_Rifle_that_is_able_to_harvest_ore + "! " + Strings.Uses_Gold_Bullets, 25f, 0.0001f, GetClass(InventoryItemIDs.GoldBullets)));
		}

		private static void RegisterItemClass(InventoryItemClass itemClass)
		{
			AllItems[itemClass.ID] = itemClass;
		}

		public static void FinishInitialization(GraphicsDevice device)
		{
			if (_2DImages != null && !_2DImages.IsContentLost)
			{
				return;
			}
			if (_2DImages == null)
			{
				_2DImages = new RenderTarget2D(CastleMinerZGame.Instance.GraphicsDevice, 512, 1280, false, SurfaceFormat.Color, DepthFormat.Depth16);
				if (GraphicsProfileManager.Instance.IsHiDef)
				{
					_2DImagesLarge = new RenderTarget2D(CastleMinerZGame.Instance.GraphicsDevice, 1024, 2560, false, SurfaceFormat.Color, DepthFormat.Depth16);
				}
				else
				{
					_2DImagesLarge = null;
				}
			}
			RasterizerState rasterizerState = device.RasterizerState;
			DepthStencilState depthStencilState = device.DepthStencilState;
			for (int i = 0; i < ((!GraphicsProfileManager.Instance.IsHiDef) ? 1 : 2); i++)
			{
				if (i == 0)
				{
					device.SetRenderTarget(_2DImages);
					device.Viewport = new Viewport(0, 0, 512, 1280);
				}
				else
				{
					device.SetRenderTarget(_2DImagesLarge);
					device.Viewport = new Viewport(0, 0, 1024, 2560);
				}
				Color color = new Color(0f, 0f, 0f, 0f);
				device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, color, 1f, 0);
				device.RasterizerState = RasterizerState.CullCounterClockwise;
				device.DepthStencilState = DepthStencilState.Default;
				Matrix projection = Matrix.CreateOrthographic(512f, 1280f, 0.1f, 500f);
				GameTime gameTime = new GameTime();
				BlockEntity.InitUIRendering(projection);
				foreach (InventoryItemClass value in AllItems.Values)
				{
					int iD = (int)value.ID;
					Entity entity = value.CreateEntity(ItemUse.UI, false);
					Vector3 vector = new Vector3(-256 + (iD & 7) * 64 + 32, -640 + iD / 8 * 64 + 32, -200f);
					vector.Y = 0f - vector.Y;
					entity.LocalPosition += vector;
					entity.Update(CastleMinerZGame.Instance, gameTime);
					entity.Draw(device, gameTime, Matrix.Identity, projection);
				}
			}
			device.SetRenderTarget(CastleMinerZGame.Instance.OffScreenBuffer);
			device.RasterizerState = rasterizerState;
			device.DepthStencilState = depthStencilState;
		}
	}
}
