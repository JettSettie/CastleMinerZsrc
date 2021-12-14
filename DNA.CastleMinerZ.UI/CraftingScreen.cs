using DNA.Audio;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Inventory;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using DNA.Input;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace DNA.CastleMinerZ.UI
{
	public class CraftingScreen : UIControlScreen
	{
		private const int Columns = 4;

		private const int Rows = 8;

		private const int ItemSize = 64;

		private Sprite _background;

		public Sprite _gridSelector;

		public Sprite _craftSelector;

		public Sprite _gridSquare;

		private Sprite _gridSprite;

		public Sprite _tier2Back;

		private CastleMinerZGame _game;

		private SpriteFont _bigFont;

		private SpriteFont _smallFont;

		private InGameHUD _hud;

		private Recipe _selectedRecipe;

		private int _selectedIngredientIndex;

		private PCDialogScreen _buyToCraftDialog;

		private List<Tier1Item> _tier1Items = new List<Tier1Item>();

		public Tier1Item SelectedTier1Item;

		private Rectangle _backgroundRectangle;

		private TextRegionElement _selectedItemNameText;

		private TextRegionElement _selectedItemDescriptionText;

		private TextRegionElement _selectedItemIngredientsText;

		private Rectangle[] _inventoryItemLocations = new Rectangle[48];

		private InventoryItem _holdingItem;

		private Point _selectedLocation = new Point(0, 0);

		private bool _hitTestTrue;

		private Point _mousePointerLocation = default(Point);

		private OneShotTimer _popUpTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private OneShotTimer _popUpFadeInTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private OneShotTimer _popUpFadeOutTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private Point _popUpLocation;

		private InventoryItem _popUpItem;

		private InventoryItem _nextPopUpItem;

		private string CRAFTING = Strings.Crafting.ToUpper();

		private string INVENTORY = Strings.Inventory.ToUpper();

		private Rectangle _prevScreenRect;

		private StringBuilder sbuilder = new StringBuilder();

		public InventoryItem SelectedItem
		{
			get
			{
				if (_selectedLocation.Y < 8)
				{
					return _hud.PlayerInventory.Inventory[_selectedLocation.X + _selectedLocation.Y * 4];
				}
				if (_selectedLocation.Y >= 8)
				{
					return _hud.PlayerInventory.TrayManager.GetTrayItem(GetTrayIndexFromRow(_selectedLocation.Y), _selectedLocation.X);
				}
				return null;
			}
			set
			{
				if (_selectedLocation.Y < 8)
				{
					_hud.PlayerInventory.Inventory[_selectedLocation.X + _selectedLocation.Y * 4] = value;
				}
				if (_selectedLocation.Y >= 8)
				{
					_hud.PlayerInventory.TrayManager.SetTrayItem(GetTrayIndexFromRow(_selectedLocation.Y), _selectedLocation.X, value);
				}
			}
		}

		public int SelectedRecipeIndex
		{
			get
			{
				return _hud.PlayerInventory.DiscoveredRecipies.IndexOf(_selectedRecipe);
			}
			set
			{
				if (_hud.PlayerInventory.DiscoveredRecipies.Count > 0 && value >= 0 && value < _hud.PlayerInventory.DiscoveredRecipies.Count)
				{
					_selectedRecipe = _hud.PlayerInventory.DiscoveredRecipies[value];
				}
				else
				{
					_selectedRecipe = null;
				}
			}
		}

		private PlayerInventory Inventory
		{
			get
			{
				return _hud.PlayerInventory;
			}
		}

		private List<Recipe> DiscoveredReceipes
		{
			get
			{
				return _hud.PlayerInventory.DiscoveredRecipies;
			}
		}

		private int GetTrayIndexFromRow(int row)
		{
			return (row - 8) / 2;
		}

		public CraftingScreen(CastleMinerZGame game, InGameHUD hud)
			: base(false)
		{
			_game = game;
			_hud = hud;
			_bigFont = _game._medFont;
			_smallFont = _game._smallFont;
			_background = _game._uiSprites["BlockUIBack"];
			_gridSelector = _game._uiSprites["Selector"];
			_gridSquare = _game._uiSprites["SingleGrid"];
			_tier2Back = _game._uiSprites["Tier2Back"];
			_gridSprite = _game._uiSprites["HudGrid"];
			_craftSelector = CastleMinerZGame.Instance._uiSprites["CraftSelector"];
			_buyToCraftDialog = new PCDialogScreen(Strings.Purchase_Game, Strings.You_must_purchase_the_game_to_craft_this_item, null, true, _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_buyToCraftDialog.UseDefaultValues();
			_selectedItemNameText = new TextRegionElement(_game._medFont);
			_selectedItemNameText.Color = CMZColors.MenuAqua;
			_selectedItemDescriptionText = new TextRegionElement(_game._smallFont);
			_selectedItemDescriptionText.Color = CMZColors.MenuBlue;
			_selectedItemIngredientsText = new TextRegionElement(_game._smallFont);
			_selectedItemIngredientsText.Color = CMZColors.MenuBlue;
			_selectedItemNameText.Size = new Vector2(215f, 50f);
			_selectedItemDescriptionText.Size = new Vector2(215f, 60f);
			_selectedItemIngredientsText.Size = new Vector2(72f, 130f);
			_selectedItemNameText.OutlineWidth = (_selectedItemDescriptionText.OutlineWidth = 0);
			_selectedItemIngredientsText.OutlineWidth = 0;
			Tier1Item tier1Item = new Tier1Item(Strings.Materials, new Vector2(14f, 29f), _game._uiSprites["MaterialsIcon"], this);
			tier1Item.SetItems(new List<Tier2Item>
			{
				new Tier2Item(Strings.Ores, Recipe.RecipeTypes.Ores, tier1Item, this),
				new Tier2Item(Strings.Components, Recipe.RecipeTypes.Components, tier1Item, this)
			});
			_tier1Items.Add(tier1Item);
			SelectedTier1Item = tier1Item;
			Tier1Item tier1Item2 = new Tier1Item(Strings.Tools, new Vector2(14f, 77f), _game._uiSprites["ToolsIcon"], this);
			tier1Item2.SetItems(new List<Tier2Item>
			{
				new Tier2Item(Strings.Pickaxes, Recipe.RecipeTypes.Pickaxes, tier1Item2, this),
				new Tier2Item(Strings.Spades, Recipe.RecipeTypes.Spades, tier1Item2, this),
				new Tier2Item(Strings.Axes, Recipe.RecipeTypes.Axes, tier1Item2, this),
				new Tier2Item(Strings.Special, Recipe.RecipeTypes.SpecialTools, tier1Item2, this)
			});
			_tier1Items.Add(tier1Item2);
			Tier1Item tier1Item3 = new Tier1Item(Strings.Weapons, new Vector2(14f, 126f), _game._uiSprites["WeaponsIcon"], this);
			tier1Item3.SetItems(new List<Tier2Item>
			{
				new Tier2Item(Strings.Ammo, Recipe.RecipeTypes.Ammo, tier1Item3, this),
				new Tier2Item(Strings.Knives, Recipe.RecipeTypes.Knives, tier1Item3, this),
				new Tier2Item(Strings.Pistols, Recipe.RecipeTypes.Pistols, tier1Item3, this),
				new Tier2Item(Strings.Shotguns, Recipe.RecipeTypes.Shotguns, tier1Item3, this),
				new Tier2Item(Strings.Rifles, Recipe.RecipeTypes.Rifles, tier1Item3, this),
				new Tier2Item(Strings.Assault_Rifles, Recipe.RecipeTypes.AssaultRifles, tier1Item3, this),
				new Tier2Item(Strings.SMG_s, Recipe.RecipeTypes.SMGs, tier1Item3, this),
				new Tier2Item(Strings.LMG_s, Recipe.RecipeTypes.LMGs, tier1Item3, this),
				new Tier2Item(Strings.RPG, Recipe.RecipeTypes.RPG, tier1Item3, this),
				new Tier2Item(Strings.Explosives, Recipe.RecipeTypes.Explosives, tier1Item3, this),
				new Tier2Item(Strings.Laser_Swords, Recipe.RecipeTypes.LaserSwords, tier1Item3, this)
			});
			_tier1Items.Add(tier1Item3);
			Tier1Item tier1Item4 = new Tier1Item(Strings.Structures, new Vector2(14f, 172f), _game._uiSprites["StructuresIcon"], this);
			tier1Item4.SetItems(new List<Tier2Item>
			{
				new Tier2Item(Strings.Walls, Recipe.RecipeTypes.Walls, tier1Item4, this),
				new Tier2Item(Strings.Containers, Recipe.RecipeTypes.Containers, tier1Item4, this),
				new Tier2Item(Strings.Spawn_Points, Recipe.RecipeTypes.SpawnPoints, tier1Item4, this),
				new Tier2Item(Strings.Other, Recipe.RecipeTypes.OtherStructure, tier1Item4, this),
				new Tier2Item(Strings.Doors, Recipe.RecipeTypes.Doors, tier1Item4, this)
			});
			_tier1Items.Add(tier1Item4);
			_popUpFadeOutTimer.Update(TimeSpan.FromSeconds(2.0));
		}

		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			spriteBatch.Begin();
			spriteBatch.Draw(_background, _backgroundRectangle, Color.White);
			_game.GameScreen.HUD.DrawPlayerStats(spriteBatch);
			_game.GameScreen.HUD.DrawDistanceStr(spriteBatch);
			for (int i = 0; i < _tier1Items.Count; i++)
			{
				_tier1Items[i].Draw(spriteBatch);
			}
			_selectedItemNameText.Draw(device, spriteBatch, gameTime, false);
			_selectedItemDescriptionText.Draw(device, spriteBatch, gameTime, false);
			_selectedItemIngredientsText.Draw(device, spriteBatch, gameTime, false);
			spriteBatch.DrawOutlinedText(_smallFont, CRAFTING, new Vector2((float)_backgroundRectangle.X + (84f - _smallFont.MeasureString(CRAFTING).X / 2f) * Screen.Adjuster.ScaleFactor.Y, (float)_backgroundRectangle.Y + 5f * Screen.Adjuster.ScaleFactor.Y), CMZColors.MenuOrange, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			spriteBatch.DrawOutlinedText(_smallFont, INVENTORY, new Vector2((float)_backgroundRectangle.X + (717f - _smallFont.MeasureString(INVENTORY).X / 2f) * Screen.Adjuster.ScaleFactor.Y, (float)_backgroundRectangle.Y + 70f * Screen.Adjuster.ScaleFactor.Y), CMZColors.MenuOrange, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			PlayerInventory playerInventory = _hud.PlayerInventory;
			Vector2 vector = new Vector2((float)_backgroundRectangle.X + 646f * Screen.Adjuster.ScaleFactor.Y, (float)_backgroundRectangle.Y + 96f * Screen.Adjuster.ScaleFactor.Y);
			float num = 59f * Screen.Adjuster.ScaleFactor.Y;
			int num2 = (int)(64f * Screen.Adjuster.ScaleFactor.Y);
			for (int j = 0; j < 8; j++)
			{
				for (int k = 0; k < 4; k++)
				{
					Vector2 vector2 = vector + num * new Vector2(k, j);
					_inventoryItemLocations[k + j * 4] = new Rectangle((int)vector2.X, (int)vector2.Y, (int)num, (int)num);
					InventoryItem inventoryItem = playerInventory.Inventory[j * 4 + k];
					if (inventoryItem != null && inventoryItem != _holdingItem)
					{
						inventoryItem.Draw2D(spriteBatch, new Rectangle((int)vector2.X, (int)vector2.Y, num2, num2));
					}
				}
			}
			int num3 = 0;
			int num4 = 2;
			Rectangle[] array = new Rectangle[num4];
			Size size = new Size((int)((float)_gridSprite.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)_gridSprite.Height * Screen.Adjuster.ScaleFactor.Y));
			array[num3] = new Rectangle(Screen.Adjuster.ScreenRect.Center.X - size.Width, Screen.Adjuster.ScreenRect.Bottom - size.Height - (int)(5f * Screen.Adjuster.ScaleFactor.Y), size.Width, size.Height);
			Rectangle rectangle = array[num3];
			_gridSprite.Draw(spriteBatch, rectangle, Color.White);
			DrawItemTray(num3, 32, rectangle, num2, num, playerInventory, spriteBatch);
			num3 = 1;
			array[num3] = new Rectangle(Screen.Adjuster.ScreenRect.Center.X, Screen.Adjuster.ScreenRect.Bottom - size.Height - (int)(5f * Screen.Adjuster.ScaleFactor.Y), size.Width, size.Height);
			rectangle = array[num3];
			_gridSprite.Draw(spriteBatch, rectangle, Color.White);
			DrawItemTray(num3, 40, rectangle, num2, num, playerInventory, spriteBatch);
			Vector2 value = vector;
			Rectangle destinationRectangle;
			if (_selectedLocation.Y < 8)
			{
				Vector2 vector3 = value + num * new Vector2(_selectedLocation.X, _selectedLocation.Y);
				destinationRectangle = new Rectangle((int)vector3.X, (int)vector3.Y, num2, num2);
			}
			else
			{
				Rectangle rectangle2 = array[GetTrayIndexFromRow(_selectedLocation.Y)];
				destinationRectangle = new Rectangle(rectangle2.Left + (int)(7f * Screen.Adjuster.ScaleFactor.Y + num * (float)_selectedLocation.X), (int)((float)rectangle2.Top + 7f * Screen.Adjuster.ScaleFactor.Y), num2, num2);
			}
			if (_hitTestTrue)
			{
				_gridSelector.Draw(spriteBatch, destinationRectangle, (_holdingItem == null) ? Color.White : Color.Red);
			}
			if (_holdingItem != null)
			{
				_holdingItem.Draw2D(spriteBatch, new Rectangle((int)((float)_mousePointerLocation.X - num / 2f), (int)((float)_mousePointerLocation.Y - num / 2f), (int)num, (int)num));
			}
			InventoryItem selectedItem = SelectedItem;
			InventoryItem holdingItem = _holdingItem;
			if (SelectedItem != null && _holdingItem == null && _hitTestTrue && _popUpTimer.Expired)
			{
				DrawingTools.DrawOutlinedText(textColor: new Color(_popUpFadeInTimer.PercentComplete, _popUpFadeInTimer.PercentComplete, _popUpFadeInTimer.PercentComplete, _popUpFadeInTimer.PercentComplete), outlineColor: new Color(0f, 0f, 0f, _popUpFadeInTimer.PercentComplete), spriteBatch: spriteBatch, font: _smallFont, text: _popUpItem.Name, location: _popUpLocation, outlineWidth: 1, scale: Screen.Adjuster.ScaleFactor.Y, rotation: 0f, orgin: Vector2.Zero);
			}
			else if (!_popUpFadeOutTimer.Expired)
			{
				DrawingTools.DrawOutlinedText(textColor: new Color(1f - _popUpFadeOutTimer.PercentComplete, 1f - _popUpFadeOutTimer.PercentComplete, 1f - _popUpFadeOutTimer.PercentComplete, 1f - _popUpFadeOutTimer.PercentComplete), outlineColor: new Color(0f, 0f, 0f, 1f - _popUpFadeOutTimer.PercentComplete), spriteBatch: spriteBatch, font: _smallFont, text: _popUpItem.Name, location: _popUpLocation, outlineWidth: 1, scale: Screen.Adjuster.ScaleFactor.Y, rotation: 0f, orgin: Vector2.Zero);
			}
			spriteBatch.End();
			base.Draw(device, spriteBatch, gameTime);
		}

		private void DrawItemTray(int trayIndex, int columnOffset, Rectangle gridRect, int size, float itemSpacing, PlayerInventory inventory, SpriteBatch spriteBatch)
		{
			for (int i = 0; i < 8; i++)
			{
				Vector2 vector = new Vector2(itemSpacing * (float)i + (float)gridRect.Left + 2f * Screen.Adjuster.ScaleFactor.Y, (float)gridRect.Top + 2f * Screen.Adjuster.ScaleFactor.Y);
				_inventoryItemLocations[i + columnOffset] = new Rectangle((int)vector.X, (int)vector.Y, size, size);
				InventoryItem trayItem = inventory.TrayManager.GetTrayItem(trayIndex, i);
				if (trayItem != null && trayItem != _holdingItem)
				{
					trayItem.Draw2D(spriteBatch, new Rectangle((int)vector.X, (int)vector.Y, size, size));
				}
			}
		}

		public override void OnPushed()
		{
			_nextPopUpItem = null;
			_popUpFadeInTimer.Reset();
			_popUpFadeOutTimer.Update(TimeSpan.FromSeconds(0.2));
			_hitTestTrue = false;
			base.OnPushed();
		}

		public int HitTest(Point p)
		{
			for (int i = 0; i < _inventoryItemLocations.Length; i++)
			{
				if (_inventoryItemLocations[i].Contains(p))
				{
					return i;
				}
			}
			return -1;
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			if (inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				if (_holdingItem != null)
				{
					_hud.PlayerInventory.AddInventoryItem(_holdingItem);
					_holdingItem = null;
				}
				else
				{
					PopMe();
				}
				SoundManager.Instance.PlayInstance("Click");
			}
			else if (inputManager.Keyboard.WasKeyPressed(Keys.E))
			{
				if (_holdingItem != null)
				{
					_hud.PlayerInventory.AddInventoryItem(_holdingItem);
					_holdingItem = null;
				}
				SoundManager.Instance.PlayInstance("Click");
				PopMe();
			}
			if (_holdingItem == null && SelectedItem != null && _hitTestTrue)
			{
				if (SelectedItem == _nextPopUpItem)
				{
					if (_popUpTimer.Expired)
					{
						if (!_popUpFadeInTimer.Expired)
						{
							_popUpFadeInTimer.Update(gameTime.ElapsedGameTime);
							_popUpFadeOutTimer = new OneShotTimer(_popUpFadeInTimer.ElaspedTime);
						}
					}
					else
					{
						_popUpFadeOutTimer.Update(gameTime.ElapsedGameTime);
						_popUpTimer.Update(gameTime.ElapsedGameTime);
						if (_popUpTimer.Expired)
						{
							_popUpItem = SelectedItem;
							Vector2 vector = _smallFont.MeasureString(_popUpItem.Name) * Screen.Adjuster.ScaleFactor.Y;
							_popUpLocation = new Point(inputManager.Mouse.Position.X, inputManager.Mouse.Position.Y + (int)vector.Y);
							if ((float)_popUpLocation.Y + vector.Y > (float)Screen.Adjuster.ScreenRect.Bottom)
							{
								_popUpLocation.Y -= (int)(vector.Y * 2f);
							}
							if ((float)_popUpLocation.X + vector.X > (float)Screen.Adjuster.ScreenRect.Right)
							{
								_popUpLocation.X = (int)((float)Screen.Adjuster.ScreenRect.Right - vector.X);
							}
						}
					}
				}
				else
				{
					_nextPopUpItem = SelectedItem;
					_popUpTimer.Reset();
					_popUpFadeInTimer.Reset();
					_popUpFadeOutTimer.Update(gameTime.ElapsedGameTime);
				}
			}
			else
			{
				_popUpFadeOutTimer.Update(gameTime.ElapsedGameTime);
				_nextPopUpItem = null;
				_popUpFadeInTimer.Reset();
			}
			if (_holdingItem == null)
			{
				for (int i = 0; i < _tier1Items.Count; i++)
				{
					if (_tier1Items[i].CheckInput(inputManager))
					{
						SelectedTier1Item = _tier1Items[i];
					}
				}
			}
			if ((inputManager.Keyboard.IsKeyDown(Keys.LeftShift) || inputManager.Keyboard.IsKeyDown(Keys.RightShift)) && inputManager.Mouse.LeftButtonPressed && HitTest(inputManager.Mouse.Position) >= 0)
			{
				if (_holdingItem != null)
				{
					if (_selectedLocation.Y < 8)
					{
						int num = _hud.PlayerInventory.AddItemToTray(_holdingItem);
						if (num == 0)
						{
							_holdingItem = null;
							SoundManager.Instance.PlayInstance("Click");
						}
						else if (_holdingItem.StackCount != num)
						{
							_holdingItem.StackCount = num;
							SoundManager.Instance.PlayInstance("Click");
						}
					}
					else
					{
						int num2 = _hud.PlayerInventory.AddItemToInventory(_holdingItem);
						if (num2 == 0)
						{
							_holdingItem = null;
							SoundManager.Instance.PlayInstance("Click");
						}
						else if (_holdingItem.StackCount != num2)
						{
							_holdingItem.StackCount = num2;
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
				else if (SelectedItem != null)
				{
					if (_selectedLocation.Y < 8)
					{
						int num3 = _hud.PlayerInventory.AddItemToTray(SelectedItem);
						if (num3 == 0)
						{
							SelectedItem = null;
							SoundManager.Instance.PlayInstance("Click");
						}
						else if (SelectedItem.StackCount != num3)
						{
							SelectedItem.StackCount = num3;
							SoundManager.Instance.PlayInstance("Click");
						}
					}
					else
					{
						int num4 = _hud.PlayerInventory.AddItemToInventory(SelectedItem);
						if (num4 == 0)
						{
							SelectedItem = null;
							SoundManager.Instance.PlayInstance("Click");
						}
						else if (SelectedItem.StackCount != num4)
						{
							SelectedItem.StackCount = num4;
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
			}
			else if (inputManager.Mouse.LeftButtonPressed && HitTest(inputManager.Mouse.Position) >= 0)
			{
				if (_holdingItem == null)
				{
					if (SelectedItem != null)
					{
						_holdingItem = SelectedItem;
						_hud.PlayerInventory.Remove(_holdingItem);
					}
					SoundManager.Instance.PlayInstance("Click");
				}
				else
				{
					InventoryItem selectedItem = SelectedItem;
					if (selectedItem != null && selectedItem.CanStack(_holdingItem))
					{
						selectedItem.Stack(_holdingItem);
						SelectedItem = selectedItem;
						if (_holdingItem.StackCount == 0)
						{
							_holdingItem = null;
						}
					}
					else
					{
						SelectedItem = _holdingItem;
						_holdingItem = selectedItem;
					}
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			else if (inputManager.Mouse.RightButtonPressed && HitTest(inputManager.Mouse.Position) >= 0)
			{
				if (_holdingItem == null)
				{
					if (SelectedItem != null)
					{
						SoundManager.Instance.PlayInstance("Click");
						if (SelectedItem.StackCount == 1)
						{
							_holdingItem = SelectedItem;
							_hud.PlayerInventory.Remove(_holdingItem);
						}
						else
						{
							_holdingItem = SelectedItem.Split();
						}
					}
				}
				else if (_holdingItem != null)
				{
					SoundManager.Instance.PlayInstance("Click");
					if (SelectedItem != null)
					{
						if (_holdingItem.ItemClass == SelectedItem.ItemClass)
						{
							if (inputManager.Mouse.RightButtonPressed)
							{
								if (SelectedItem.StackCount < SelectedItem.MaxStackCount)
								{
									if (_holdingItem.StackCount > 1)
									{
										SelectedItem.Stack(_holdingItem.PopOneItem());
									}
									else
									{
										SelectedItem.Stack(_holdingItem);
										_holdingItem = null;
									}
								}
							}
							else if (SelectedItem.StackCount > 1)
							{
								InventoryItem item = SelectedItem.Split();
								_holdingItem.Stack(item);
								SelectedItem.Stack(item);
							}
							else if (_holdingItem.StackCount < _holdingItem.MaxStackCount)
							{
								_holdingItem.Stack(SelectedItem);
								SelectedItem = null;
							}
						}
						else
						{
							InventoryItem selectedItem2 = SelectedItem;
							SelectedItem = _holdingItem;
							_holdingItem = selectedItem2;
						}
					}
					else if (inputManager.Mouse.RightButtonPressed)
					{
						if (_holdingItem.StackCount > 1)
						{
							SelectedItem = _holdingItem.PopOneItem();
						}
						else
						{
							SelectedItem = _holdingItem;
							_holdingItem = null;
						}
					}
					else if (_holdingItem.StackCount > 1)
					{
						SelectedItem = _holdingItem.Split();
					}
					else
					{
						SelectedItem = _holdingItem;
						_holdingItem = null;
					}
				}
			}
			else if (inputManager.Keyboard.WasKeyPressed(Keys.Q))
			{
				SoundManager.Instance.PlayInstance("Click");
				if (_holdingItem != null)
				{
					Vector3 localPosition = _game.LocalPlayer.LocalPosition;
					localPosition.Y += 1f;
					PickupManager.Instance.CreatePickup(_holdingItem, localPosition, true);
					SoundManager.Instance.PlayInstance("dropitem");
					_holdingItem = null;
				}
				else if (SelectedItem != null)
				{
					_hud.PlayerInventory.DropItem(SelectedItem);
				}
			}
			else if (inputManager.Mouse.LeftButtonPressed && !_backgroundRectangle.Contains(inputManager.Mouse.Position) && _holdingItem != null)
			{
				SoundManager.Instance.PlayInstance("Click");
				Vector3 localPosition2 = _game.LocalPlayer.LocalPosition;
				localPosition2.Y += 1f;
				PickupManager.Instance.CreatePickup(_holdingItem, localPosition2, true);
				SoundManager.Instance.PlayInstance("dropitem");
				_holdingItem = null;
			}
			else if (inputManager.Mouse.RightButtonPressed && !_backgroundRectangle.Contains(inputManager.Mouse.Position) && _holdingItem != null)
			{
				InventoryItem item2;
				if (_holdingItem.StackCount > 1)
				{
					item2 = _holdingItem.PopOneItem();
				}
				else
				{
					item2 = _holdingItem;
					_holdingItem = null;
				}
				SoundManager.Instance.PlayInstance("Click");
				Vector3 localPosition3 = _game.LocalPlayer.LocalPosition;
				localPosition3.Y += 1f;
				PickupManager.Instance.CreatePickup(item2, localPosition3, true);
				SoundManager.Instance.PlayInstance("dropitem");
			}
			else if (inputManager.Mouse.DeltaWheel < 0 && HitTest(inputManager.Mouse.Position) >= 0)
			{
				if (ItemCountDown())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			else if (inputManager.Mouse.DeltaWheel > 0 && HitTest(inputManager.Mouse.Position) >= 0 && ItemCountUp())
			{
				SoundManager.Instance.PlayInstance("Click");
			}
			if (inputManager.Mouse.Position != inputManager.Mouse.LastPosition)
			{
				_mousePointerLocation = inputManager.Mouse.Position;
				int num5 = HitTest(inputManager.Mouse.Position);
				if (num5 >= 0)
				{
					_hitTestTrue = true;
					_selectedLocation.Y = num5 / 4;
					if (_selectedLocation.Y < 8)
					{
						_selectedLocation.X = num5 % 4;
					}
					else
					{
						_selectedLocation.X = num5 % 8;
					}
				}
				else
				{
					_hitTestTrue = false;
				}
			}
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		public bool ItemCountDown()
		{
			if (_holdingItem == null)
			{
				return false;
			}
			if (SelectedItem == null)
			{
				if (_holdingItem.StackCount == 1)
				{
					SelectedItem = _holdingItem;
					_holdingItem = null;
				}
				else
				{
					SelectedItem = _holdingItem.PopOneItem();
				}
			}
			else
			{
				if (_holdingItem.ItemClass != SelectedItem.ItemClass || SelectedItem.StackCount >= SelectedItem.MaxStackCount)
				{
					return false;
				}
				if (_holdingItem.StackCount == 1)
				{
					SelectedItem.Stack(_holdingItem);
					_holdingItem = null;
				}
				else
				{
					SelectedItem.Stack(_holdingItem.PopOneItem());
				}
			}
			return true;
		}

		public bool ItemCountUp()
		{
			if (_holdingItem == null)
			{
				if (SelectedItem == null)
				{
					return false;
				}
				if (SelectedItem.StackCount == 1)
				{
					_holdingItem = SelectedItem;
					_hud.PlayerInventory.Remove(_holdingItem);
				}
				else
				{
					_holdingItem = SelectedItem.PopOneItem();
				}
			}
			else
			{
				if (SelectedItem == null)
				{
					return false;
				}
				if (_holdingItem.ItemClass != SelectedItem.ItemClass || _holdingItem.StackCount >= _holdingItem.MaxStackCount)
				{
					return false;
				}
				if (SelectedItem.StackCount == 1)
				{
					_holdingItem.Stack(SelectedItem);
					SelectedItem = null;
				}
				else
				{
					_holdingItem.Stack(SelectedItem.PopOneItem());
				}
			}
			return true;
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (_hud.LocalPlayer.Dead)
			{
				if (_holdingItem != null)
				{
					_hud.PlayerInventory.AddInventoryItem(_holdingItem);
					_holdingItem = null;
				}
				PopMe();
			}
			Size size = new Size((int)((float)_background.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)_background.Height * Screen.Adjuster.ScaleFactor.Y));
			_backgroundRectangle = new Rectangle(Screen.Adjuster.ScreenRect.Center.X - size.Width / 2, (int)(55f * Screen.Adjuster.ScaleFactor.Y), size.Width, size.Height);
			_selectedItemNameText.Text = SelectedTier1Item.SelectedTier2Item.SelectedItem.Name;
			_selectedItemDescriptionText.Location = new Vector2((float)_backgroundRectangle.X + 32f * Screen.Adjuster.ScaleFactor.Y, (float)_backgroundRectangle.Y + (float)(367 + _game._medFont.LineSpacing * _selectedItemNameText.NumberOfLines) * Screen.Adjuster.ScaleFactor.Y);
			_selectedItemDescriptionText.Text = SelectedTier1Item.SelectedTier2Item.SelectedItem.Description;
			sbuilder.Clear();
			sbuilder.Append(Strings.Components);
			sbuilder.Append(":\n");
			sbuilder.Append(SelectedTier1Item.SelectedTier2Item.SelectedItemIngredients[0].Name);
			for (int i = 1; i < SelectedTier1Item.SelectedTier2Item.SelectedItemIngredients.Count; i++)
			{
				sbuilder.Append(", ");
				sbuilder.Append(SelectedTier1Item.SelectedTier2Item.SelectedItemIngredients[i].Name);
			}
			_selectedItemIngredientsText.Location = new Vector2((float)_backgroundRectangle.X + 170f * Screen.Adjuster.ScaleFactor.Y, (float)_backgroundRectangle.Y + 235f * Screen.Adjuster.ScaleFactor.Y);
			_selectedItemIngredientsText.Text = sbuilder.ToString();
			if (_prevScreenRect != Screen.Adjuster.ScreenRect)
			{
				for (int j = 0; j < _tier1Items.Count; j++)
				{
					_tier1Items[j].UpdateScaledLocation(_backgroundRectangle);
				}
				_selectedItemNameText.Location = new Vector2((float)_backgroundRectangle.X + 32f * Screen.Adjuster.ScaleFactor.Y, (float)_backgroundRectangle.Y + 367f * Screen.Adjuster.ScaleFactor.Y);
			}
			_prevScreenRect = Screen.Adjuster.ScreenRect;
			base.OnUpdate(game, gameTime);
		}
	}
}
