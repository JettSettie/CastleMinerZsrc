using DNA.Audio;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Input;
using DNA.Net.GamerServices;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Text;

namespace DNA.CastleMinerZ.UI
{
	public class CrateScreen : Screen
	{
		private const int Columns = 4;

		private const int Rows = 8;

		private const int ItemSize = 64;

		public Crate CurrentCrate;

		private int hoverItem;

		private bool _setSelectorToCrate;

		private Sprite _grid;

		private Sprite _gridSelector;

		private Sprite _gridSprite;

		private CastleMinerZGame _game;

		private SpriteFont _bigFont;

		private SpriteFont _smallFont;

		private InGameHUD _hud;

		private Point _selectedLocation = new Point(0, 0);

		private InventoryItem _holdingItem;

		private Rectangle[] _crateItemLocations = new Rectangle[32];

		private Rectangle[] _inventoryItemLocations = new Rectangle[48];

		private Rectangle _backgroundRect;

		private bool _mousePointerActive;

		private Point _mousePointerLocation = default(Point);

		private bool _hitTestTrue;

		private OneShotTimer _popUpTimer = new OneShotTimer(TimeSpan.FromSeconds(1.0));

		private OneShotTimer _popUpFadeInTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private OneShotTimer _popUpFadeOutTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private Point _popUpLocation;

		private InventoryItem _popUpItem;

		private InventoryItem _nextPopUpItem;

		private StringBuilder stringBuilder = new StringBuilder();

		private string CRATE = Strings.Crate.ToUpper();

		private string INVENTORY = Strings.Inventory.ToUpper();

		private OneShotTimer waitScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		private OneShotTimer autoScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.10000000149011612));

		private OneShotTimer itemCountWaitScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		private OneShotTimer itemCountAutoScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.10000000149011612));

		private bool _selectorInCrateGrid
		{
			get
			{
				if (_setSelectorToCrate)
				{
					return _selectedLocation.Y < 8;
				}
				return false;
			}
			set
			{
				_setSelectorToCrate = value;
			}
		}

		private int SelectorIndex
		{
			get
			{
				return _selectedLocation.X + _selectedLocation.Y * 4;
			}
		}

		public InventoryItem SelectedItem
		{
			get
			{
				if (_selectedLocation.Y < 8)
				{
					if (_selectorInCrateGrid)
					{
						return CurrentCrate.Inventory[_selectedLocation.X + _selectedLocation.Y * 4];
					}
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
					if (_selectorInCrateGrid)
					{
						ItemCrateMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, value, CurrentCrate, _selectedLocation.X + _selectedLocation.Y * 4);
					}
					else
					{
						_hud.PlayerInventory.Inventory[_selectedLocation.X + _selectedLocation.Y * 4] = value;
					}
				}
				if (_selectedLocation.Y >= 8)
				{
					_hud.PlayerInventory.TrayManager.SetTrayItem(GetTrayIndexFromRow(_selectedLocation.Y), _selectedLocation.X, value);
				}
			}
		}

		private int GetTrayIndexFromRow(int row)
		{
			return (row - 8) / 2;
		}

		public bool IsSelectedSlotLocked()
		{
			if (_selectorInCrateGrid && _game.CurrentNetworkSession != null)
			{
				foreach (NetworkGamer remoteGamer in _game.CurrentNetworkSession.RemoteGamers)
				{
					if (remoteGamer.Tag != null)
					{
						Player player = (Player)remoteGamer.Tag;
						if (player.FocusCrate == CurrentCrate.Location && player.FocusCrateItem == _selectedLocation)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public bool IsSlotLocked(int index)
		{
			foreach (NetworkGamer remoteGamer in _game.CurrentNetworkSession.RemoteGamers)
			{
				if (remoteGamer.Tag != null)
				{
					Player player = (Player)remoteGamer.Tag;
					int num = player.FocusCrateItem.X + player.FocusCrateItem.Y * 4;
					if (player.FocusCrate == CurrentCrate.Location && num == index)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override void OnPushed()
		{
			_selectedLocation = new Point(_game.GameScreen.HUD.PlayerInventory.SelectedInventoryIndex, 8);
			_selectorInCrateGrid = false;
			CrateFocusMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, _selectorInCrateGrid ? CurrentCrate.Location : IntVector3.Zero, _selectedLocation);
			_nextPopUpItem = null;
			_popUpFadeInTimer.Reset();
			_popUpFadeOutTimer.Update(TimeSpan.FromSeconds(2.0));
			_hitTestTrue = false;
			base.OnPushed();
		}

		public int InventoryHitTest(Point p)
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

		public int CrateHitTest(Point p)
		{
			for (int i = 0; i < _crateItemLocations.Length; i++)
			{
				if (_crateItemLocations[i].Contains(p))
				{
					return i;
				}
			}
			return -1;
		}

		public void ForceClose()
		{
			if (_holdingItem != null)
			{
				_hud.PlayerInventory.AddInventoryItem(_holdingItem);
				_holdingItem = null;
			}
			if (SelectedItem != null && _selectorInCrateGrid && !IsSelectedSlotLocked())
			{
				Vector3 localPosition = _game.LocalPlayer.LocalPosition;
				localPosition.Y += 1f;
				PickupManager.Instance.CreatePickup(SelectedItem, localPosition, true);
				SoundManager.Instance.PlayInstance("dropitem");
			}
			PopMe();
			CrateFocusMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, IntVector3.Zero, Point.Zero);
			_game.GameScreen.ShowBlockPicker();
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			if (_hud.LocalPlayer.Dead)
			{
				PopMe();
			}
			else if (CurrentCrate == null)
			{
				PopMe();
				_game.GameScreen.ShowBlockPicker();
			}
			else if (SelectedItem != null && SelectedItem.StackCount < 1)
			{
				SelectedItem = null;
			}
			base.Update(game, gameTime);
		}

		public CrateScreen(CastleMinerZGame game, InGameHUD hud)
			: base(true, false)
		{
			_hud = hud;
			_game = game;
			_bigFont = _game._medFont;
			_smallFont = _game._smallFont;
			_gridSelector = _game._uiSprites["Selector"];
			_grid = _game._uiSprites["InventoryGrid"];
			_gridSprite = _game._uiSprites["HudGrid"];
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			Vector2 vector = new Vector2(_grid.Width * 2 + 16, _grid.Height) * Screen.Adjuster.ScaleFactor.Y;
			_backgroundRect = new Rectangle((int)((float)screenRect.Center.X - vector.X / 2f), (int)((float)screenRect.Center.Y - vector.Y / 2f), (int)vector.X, (int)vector.Y);
			_popUpFadeOutTimer.Update(TimeSpan.FromSeconds(2.0));
		}

		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			_game.GameScreen.HUD.console.Draw(device, spriteBatch, gameTime, false);
			spriteBatch.Begin();
			_game.GameScreen.HUD.DrawPlayerStats(spriteBatch);
			_game.GameScreen.HUD.DrawDistanceStr(spriteBatch);
			SpriteFont smallFont = CastleMinerZGame.Instance._smallFont;
			Vector2 vector = new Vector2(_grid.Width * 2 + 16, _grid.Height) * Screen.Adjuster.ScaleFactor.Y;
			_backgroundRect = new Rectangle((int)((float)Screen.Adjuster.ScreenRect.Center.X - vector.X / 2f), (int)((float)Screen.Adjuster.ScreenRect.Center.Y - vector.Y / 2f), (int)vector.X, (int)vector.Y);
			_grid.Draw(spriteBatch, new Rectangle(_backgroundRect.X, _backgroundRect.Y, (int)((float)_grid.Width * Screen.Adjuster.ScaleFactor.Y), (int)vector.Y), Color.White);
			_grid.Draw(spriteBatch, new Rectangle(_backgroundRect.Right - (int)((float)_grid.Width * Screen.Adjuster.ScaleFactor.Y), _backgroundRect.Y, (int)((float)_grid.Width * Screen.Adjuster.ScaleFactor.Y), (int)vector.Y), Color.White);
			PlayerInventory playerInventory = _hud.PlayerInventory;
			Vector2 vector2 = new Vector2((float)_backgroundRect.X + 299f * Screen.Adjuster.ScaleFactor.Y, (float)_backgroundRect.Y + 33f * Screen.Adjuster.ScaleFactor.Y);
			float num = 59f * Screen.Adjuster.ScaleFactor.Y;
			int num2 = (int)(64f * Screen.Adjuster.ScaleFactor.Y);
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					Vector2 vector3 = vector2 + num * new Vector2(j, i);
					_inventoryItemLocations[j + i * 4] = new Rectangle((int)vector3.X, (int)vector3.Y, (int)num, (int)num);
					InventoryItem inventoryItem = playerInventory.Inventory[i * 4 + j];
					if (inventoryItem != null && inventoryItem != _holdingItem && (_holdingItem == null || SelectedItem != inventoryItem || _mousePointerActive))
					{
						inventoryItem.Draw2D(spriteBatch, new Rectangle((int)vector3.X, (int)vector3.Y, num2, num2));
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
			InventoryItem[] inventory = CurrentCrate.Inventory;
			Vector2 vector4 = new Vector2((float)_backgroundRect.X + 17f * Screen.Adjuster.ScaleFactor.Y, (float)_backgroundRect.Y + 33f * Screen.Adjuster.ScaleFactor.Y);
			spriteBatch.DrawOutlinedText(_smallFont, CRATE, new Vector2((float)_backgroundRect.X + (85f - _smallFont.MeasureString(CRATE).X / 2f) * Screen.Adjuster.ScaleFactor.Y, (float)_backgroundRect.Y + 5f * Screen.Adjuster.ScaleFactor.Y), CMZColors.MenuOrange, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			spriteBatch.DrawOutlinedText(_smallFont, INVENTORY, new Vector2((float)_backgroundRect.X + (369f - _smallFont.MeasureString(INVENTORY).X / 2f) * Screen.Adjuster.ScaleFactor.Y, (float)_backgroundRect.Y + 5f * Screen.Adjuster.ScaleFactor.Y), CMZColors.MenuOrange, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			for (int k = 0; k < 8; k++)
			{
				for (int l = 0; l < 4; l++)
				{
					Vector2 vector5 = vector4 + num * new Vector2(l, k);
					_crateItemLocations[l + k * 4] = new Rectangle((int)vector5.X, (int)vector5.Y, (int)num, (int)num);
					InventoryItem inventoryItem2 = inventory[k * 4 + l];
					if (inventoryItem2 != null && inventoryItem2 != _holdingItem && (_holdingItem == null || SelectedItem != inventoryItem2 || _mousePointerActive))
					{
						inventoryItem2.Draw2D(spriteBatch, new Rectangle((int)vector5.X, (int)vector5.Y, num2, num2));
					}
				}
			}
			Vector2 value = (!_selectorInCrateGrid) ? vector2 : vector4;
			Rectangle destinationRectangle;
			if (_selectedLocation.Y < 8)
			{
				Vector2 vector6 = value + num * new Vector2(_selectedLocation.X, _selectedLocation.Y);
				destinationRectangle = new Rectangle((int)vector6.X, (int)vector6.Y, num2, num2);
			}
			else
			{
				Rectangle rectangle2 = array[GetTrayIndexFromRow(_selectedLocation.Y)];
				destinationRectangle = new Rectangle(rectangle2.Left + (int)(7f * Screen.Adjuster.ScaleFactor.Y + num * (float)_selectedLocation.X), (int)((float)rectangle2.Top + 7f * Screen.Adjuster.ScaleFactor.Y), num2, num2);
			}
			if (!_mousePointerActive || _hitTestTrue)
			{
				_gridSelector.Draw(spriteBatch, destinationRectangle, (_holdingItem == null) ? Color.White : Color.Red);
			}
			if (_holdingItem != null)
			{
				if (_mousePointerActive)
				{
					_holdingItem.Draw2D(spriteBatch, new Rectangle((int)((float)_mousePointerLocation.X - num / 2f), (int)((float)_mousePointerLocation.Y - num / 2f), (int)num, (int)num));
				}
				else
				{
					_holdingItem.Draw2D(spriteBatch, new Rectangle(destinationRectangle.X, destinationRectangle.Y, (int)num, (int)num));
				}
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
				if (trayItem != null && trayItem != _holdingItem && (_holdingItem == null || SelectedItem != trayItem || _mousePointerActive))
				{
					trayItem.Draw2D(spriteBatch, new Rectangle((int)vector.X, (int)vector.Y, size, size));
				}
			}
		}

		private bool SwapSelectedItemLocation()
		{
			if (!_selectorInCrateGrid)
			{
				for (int i = 0; i < CurrentCrate.Inventory.Length; i++)
				{
					if (CurrentCrate.Inventory[i] != null && !IsSlotLocked(i))
					{
						int stackCount = SelectedItem.StackCount;
						InventoryItem selectedItem = SelectedItem;
						CurrentCrate.Inventory[i].Stack(selectedItem);
						SelectedItem = selectedItem;
						if (selectedItem.StackCount != stackCount)
						{
							ItemCrateMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, CurrentCrate.Inventory[i], CurrentCrate, i);
						}
					}
				}
				if (SelectedItem.StackCount <= 0)
				{
					SelectedItem = null;
					return true;
				}
				for (int j = 0; j < CurrentCrate.Inventory.Length; j++)
				{
					if (CurrentCrate.Inventory[j] == null && !IsSlotLocked(j))
					{
						ItemCrateMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, SelectedItem, CurrentCrate, j);
						SelectedItem = null;
						return true;
					}
				}
			}
			else
			{
				SelectedItem = _hud.PlayerInventory.TrayManager.Stack(SelectedItem);
				for (int k = 0; k < _hud.PlayerInventory.Inventory.Length; k++)
				{
					if (_hud.PlayerInventory.Inventory[k] != null)
					{
						InventoryItem selectedItem2 = SelectedItem;
						_hud.PlayerInventory.Inventory[k].Stack(selectedItem2);
						SelectedItem = selectedItem2;
					}
				}
				if (SelectedItem.StackCount <= 0)
				{
					SelectedItem = null;
					_hud.PlayerInventory.DiscoverRecipies();
					return true;
				}
				if (_hud.PlayerInventory.TrayManager.PlaceInEmptySlot(SelectedItem))
				{
					_hud.PlayerInventory.DiscoverRecipies();
					SelectedItem = null;
					return true;
				}
				for (int l = 0; l < _hud.PlayerInventory.Inventory.Length; l++)
				{
					if (_hud.PlayerInventory.Inventory[l] == null)
					{
						_hud.PlayerInventory.Inventory[l] = SelectedItem;
						_hud.PlayerInventory.DiscoverRecipies();
						SelectedItem = null;
						return true;
					}
				}
			}
			return false;
		}

		private bool SwapHoldingItemLocation()
		{
			if (!_selectorInCrateGrid)
			{
				for (int i = 0; i < CurrentCrate.Inventory.Length; i++)
				{
					if (CurrentCrate.Inventory[i] != null && !IsSlotLocked(i))
					{
						int stackCount = _holdingItem.StackCount;
						CurrentCrate.Inventory[i].Stack(_holdingItem);
						if (_holdingItem.StackCount != stackCount)
						{
							ItemCrateMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, CurrentCrate.Inventory[i], CurrentCrate, i);
						}
					}
				}
				if (_holdingItem.StackCount <= 0)
				{
					_holdingItem = null;
					return true;
				}
				for (int j = 0; j < CurrentCrate.Inventory.Length; j++)
				{
					if (CurrentCrate.Inventory[j] == null && !IsSlotLocked(j))
					{
						ItemCrateMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, _holdingItem, CurrentCrate, j);
						_holdingItem = null;
						return true;
					}
				}
			}
			else
			{
				_holdingItem = _hud.PlayerInventory.TrayManager.Stack(_holdingItem);
				for (int k = 0; k < _hud.PlayerInventory.Inventory.Length; k++)
				{
					if (_hud.PlayerInventory.Inventory[k] != null)
					{
						_hud.PlayerInventory.Inventory[k].Stack(_holdingItem);
					}
				}
				if (_holdingItem.StackCount <= 0)
				{
					_holdingItem = null;
					_hud.PlayerInventory.DiscoverRecipies();
					return true;
				}
				if (_hud.PlayerInventory.TrayManager.PlaceInEmptySlot(_holdingItem))
				{
					_hud.PlayerInventory.DiscoverRecipies();
					_holdingItem = null;
					return true;
				}
				for (int l = 0; l < _hud.PlayerInventory.Inventory.Length; l++)
				{
					if (_hud.PlayerInventory.Inventory[l] == null)
					{
						_hud.PlayerInventory.Inventory[l] = _holdingItem;
						_hud.PlayerInventory.DiscoverRecipies();
						_holdingItem = null;
						return true;
					}
				}
			}
			return false;
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			if (CurrentCrate.Destroyed)
			{
				ForceClose();
				return false;
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
			if (controller.PressedButtons.Y || ((inputManager.Keyboard.IsKeyDown(Keys.LeftShift) || inputManager.Keyboard.IsKeyDown(Keys.RightShift)) && inputManager.Mouse.LeftButtonPressed && _hitTestTrue))
			{
				if (_holdingItem != null)
				{
					if (SwapHoldingItemLocation())
					{
						SoundManager.Instance.PlayInstance("Click");
					}
				}
				else if (SelectedItem != null && !IsSelectedSlotLocked() && SwapSelectedItemLocation())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			else if (controller.PressedButtons.A || inputManager.Keyboard.WasKeyPressed(Keys.Enter) || (inputManager.Mouse.LeftButtonPressed && _hitTestTrue))
			{
				if (!IsSelectedSlotLocked())
				{
					if (_holdingItem == null)
					{
						if (SelectedItem != null)
						{
							_holdingItem = SelectedItem;
							if (_selectorInCrateGrid)
							{
								SelectedItem = null;
							}
							else
							{
								_hud.PlayerInventory.Remove(_holdingItem);
							}
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
				else
				{
					SoundManager.Instance.PlayInstance("Error");
				}
			}
			else if (controller.PressedButtons.RightStick || (inputManager.Mouse.RightButtonPressed && _hitTestTrue))
			{
				if (!IsSelectedSlotLocked())
				{
					if (_holdingItem == null)
					{
						if (SelectedItem != null)
						{
							SoundManager.Instance.PlayInstance("Click");
							if (SelectedItem.StackCount == 1)
							{
								_holdingItem = SelectedItem;
								if (_selectorInCrateGrid)
								{
									SelectedItem = null;
								}
								else
								{
									_hud.PlayerInventory.Remove(_holdingItem);
								}
							}
							else
							{
								InventoryItem selectedItem2 = SelectedItem;
								_holdingItem = selectedItem2.Split();
								SelectedItem = selectedItem2;
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
											InventoryItem selectedItem3 = SelectedItem;
											selectedItem3.Stack(_holdingItem.PopOneItem());
											SelectedItem = selectedItem3;
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
									InventoryItem selectedItem4 = SelectedItem;
									InventoryItem item = selectedItem4.Split();
									_holdingItem.Stack(item);
									selectedItem4.Stack(item);
									SelectedItem = selectedItem4;
								}
								else if (_holdingItem.StackCount < _holdingItem.MaxStackCount)
								{
									_holdingItem.Stack(SelectedItem);
									SelectedItem = null;
								}
							}
							else
							{
								InventoryItem selectedItem5 = SelectedItem;
								SelectedItem = _holdingItem;
								_holdingItem = selectedItem5;
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
				else
				{
					SoundManager.Instance.PlayInstance("Error");
				}
			}
			else if (controller.PressedButtons.Start)
			{
				_game.GameScreen.ShowInGameMenu();
				SoundManager.Instance.PlayInstance("Click");
			}
			else if (controller.PressedButtons.Back || inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				SoundManager.Instance.PlayInstance("Click");
				_holdingItem = null;
				PopMe();
				CrateFocusMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, IntVector3.Zero, Point.Zero);
			}
			else if (controller.PressedButtons.B)
			{
				SoundManager.Instance.PlayInstance("Click");
				if (_holdingItem != null)
				{
					_hud.PlayerInventory.AddInventoryItem(_holdingItem);
					_holdingItem = null;
				}
				else
				{
					PopMe();
					CrateFocusMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, IntVector3.Zero, Point.Zero);
				}
			}
			else if (inputManager.Mouse.LeftButtonPressed && !_backgroundRect.Contains(inputManager.Mouse.Position) && _holdingItem != null)
			{
				_mousePointerActive = true;
				SoundManager.Instance.PlayInstance("Click");
				Vector3 localPosition = _game.LocalPlayer.LocalPosition;
				localPosition.Y += 1f;
				PickupManager.Instance.CreatePickup(_holdingItem, localPosition, true);
				SoundManager.Instance.PlayInstance("dropitem");
				_holdingItem = null;
			}
			else if (inputManager.Mouse.RightButtonPressed && !_backgroundRect.Contains(inputManager.Mouse.Position) && _holdingItem != null)
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
				_mousePointerActive = true;
				SoundManager.Instance.PlayInstance("Click");
				Vector3 localPosition2 = _game.LocalPlayer.LocalPosition;
				localPosition2.Y += 1f;
				PickupManager.Instance.CreatePickup(item2, localPosition2, true);
				SoundManager.Instance.PlayInstance("dropitem");
			}
			else if (controller.PressedButtons.X || inputManager.Keyboard.WasKeyPressed(Keys.Q))
			{
				if (!IsSelectedSlotLocked())
				{
					SoundManager.Instance.PlayInstance("Click");
					if (_holdingItem != null)
					{
						Vector3 localPosition3 = _game.LocalPlayer.LocalPosition;
						localPosition3.Y += 1f;
						PickupManager.Instance.CreatePickup(_holdingItem, localPosition3, true);
						SoundManager.Instance.PlayInstance("dropitem");
						_holdingItem = null;
					}
					else if (SelectedItem != null)
					{
						if (_selectorInCrateGrid)
						{
							Vector3 localPosition4 = _game.LocalPlayer.LocalPosition;
							localPosition4.Y += 1f;
							PickupManager.Instance.CreatePickup(SelectedItem, localPosition4, true);
							SoundManager.Instance.PlayInstance("dropitem");
							SelectedItem = null;
						}
						else
						{
							_hud.PlayerInventory.DropItem(SelectedItem);
						}
					}
				}
				else
				{
					SoundManager.Instance.PlayInstance("Error");
				}
			}
			else if (controller.CurrentState.ThumbSticks.Right.Y < -0.2f && controller.LastState.ThumbSticks.Right.Y >= -0.2f)
			{
				itemCountWaitScrollTimer.Reset();
				itemCountAutoScrollTimer.Reset();
				if (ItemCountDown())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			else if (inputManager.Mouse.DeltaWheel < 0 && _hitTestTrue)
			{
				_mousePointerActive = true;
				itemCountWaitScrollTimer.Reset();
				itemCountAutoScrollTimer.Reset();
				if (ItemCountDown())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			else if (inputManager.Mouse.DeltaWheel > 0 && _hitTestTrue)
			{
				_mousePointerActive = true;
				itemCountWaitScrollTimer.Reset();
				itemCountAutoScrollTimer.Reset();
				if (ItemCountUp())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			else if (controller.CurrentState.ThumbSticks.Right.Y > 0.2f && controller.LastState.ThumbSticks.Right.Y <= 0.2f)
			{
				itemCountWaitScrollTimer.Reset();
				itemCountAutoScrollTimer.Reset();
				if (ItemCountUp())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			if (controller.PressedDPad.Down || (controller.CurrentState.ThumbSticks.Left.Y < -0.2f && controller.LastState.ThumbSticks.Left.Y >= -0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Down))
			{
				_mousePointerActive = false;
				waitScrollTimer.Reset();
				autoScrollTimer.Reset();
				if (SelectDown())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
				CrateFocusMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, _selectorInCrateGrid ? CurrentCrate.Location : IntVector3.Zero, _selectedLocation);
			}
			if (controller.PressedDPad.Up || (controller.CurrentState.ThumbSticks.Left.Y > 0.2f && controller.LastState.ThumbSticks.Left.Y <= 0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Up))
			{
				_mousePointerActive = false;
				waitScrollTimer.Reset();
				autoScrollTimer.Reset();
				if (SelectUp())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
				CrateFocusMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, _selectorInCrateGrid ? CurrentCrate.Location : IntVector3.Zero, _selectedLocation);
			}
			if (controller.PressedButtons.LeftShoulder || controller.PressedDPad.Left || (controller.CurrentState.ThumbSticks.Left.X < -0.2f && controller.LastState.ThumbSticks.Left.X >= -0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Left))
			{
				_mousePointerActive = false;
				waitScrollTimer.Reset();
				autoScrollTimer.Reset();
				if (SelectLeft())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
				CrateFocusMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, _selectorInCrateGrid ? CurrentCrate.Location : IntVector3.Zero, _selectedLocation);
			}
			if (controller.PressedButtons.RightShoulder || controller.PressedDPad.Right || (controller.CurrentState.ThumbSticks.Left.X > 0.2f && controller.LastState.ThumbSticks.Left.X <= 0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Right))
			{
				_mousePointerActive = false;
				waitScrollTimer.Reset();
				autoScrollTimer.Reset();
				if (SelectRight())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
				CrateFocusMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, _selectorInCrateGrid ? CurrentCrate.Location : IntVector3.Zero, _selectedLocation);
			}
			itemCountWaitScrollTimer.Update(gameTime.ElapsedGameTime);
			if (itemCountWaitScrollTimer.Expired && !controller.PressedButtons.A)
			{
				if (controller.CurrentState.ThumbSticks.Right.Y < -0.2f)
				{
					itemCountAutoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (itemCountAutoScrollTimer.Expired)
					{
						itemCountAutoScrollTimer.Reset();
						if (ItemCountDown())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
				else if (controller.CurrentState.ThumbSticks.Right.Y > 0.2f)
				{
					itemCountAutoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (itemCountAutoScrollTimer.Expired)
					{
						itemCountAutoScrollTimer.Reset();
						if (ItemCountUp())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
			}
			waitScrollTimer.Update(gameTime.ElapsedGameTime);
			if (waitScrollTimer.Expired)
			{
				if (controller.CurrentState.ThumbSticks.Left.Y < -0.2f || inputManager.Keyboard.IsKeyDown(Keys.Up))
				{
					autoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (autoScrollTimer.Expired)
					{
						autoScrollTimer.Reset();
						if (SelectDown())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
				else if (controller.CurrentState.ThumbSticks.Left.Y > 0.2f || inputManager.Keyboard.IsKeyDown(Keys.Down))
				{
					autoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (autoScrollTimer.Expired)
					{
						autoScrollTimer.Reset();
						if (SelectUp())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
				else if (controller.CurrentState.ThumbSticks.Left.X < -0.2f || inputManager.Keyboard.IsKeyDown(Keys.Left))
				{
					autoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (autoScrollTimer.Expired)
					{
						autoScrollTimer.Reset();
						if (SelectLeft())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
				else if (controller.CurrentState.ThumbSticks.Left.X > 0.2f || inputManager.Keyboard.IsKeyDown(Keys.Right))
				{
					autoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (autoScrollTimer.Expired)
					{
						autoScrollTimer.Reset();
						if (SelectRight())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
			}
			if (inputManager.Mouse.Position != inputManager.Mouse.LastPosition)
			{
				_mousePointerActive = true;
				_mousePointerLocation = inputManager.Mouse.Position;
				hoverItem = InventoryHitTest(inputManager.Mouse.Position);
				if (hoverItem >= 0)
				{
					_hitTestTrue = true;
					_selectedLocation.Y = hoverItem / 4;
					if (_selectedLocation.Y > 7)
					{
						_selectedLocation.X = hoverItem % 8;
					}
					else
					{
						_selectedLocation.X = hoverItem % 4;
					}
					_selectorInCrateGrid = false;
				}
				else
				{
					hoverItem = CrateHitTest(inputManager.Mouse.Position);
					if (hoverItem >= 0)
					{
						_hitTestTrue = true;
						_selectedLocation.Y = hoverItem / 4;
						if (_selectedLocation.Y > 7)
						{
							_selectedLocation.Y = 8;
							_selectedLocation.X = hoverItem % 8;
						}
						else
						{
							_selectedLocation.X = hoverItem % 4;
						}
						_selectorInCrateGrid = true;
					}
					else
					{
						_hitTestTrue = false;
					}
				}
			}
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		public bool ItemCountDown()
		{
			if (IsSelectedSlotLocked())
			{
				return false;
			}
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
				else if (_holdingItem.StackCount > 1)
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
				InventoryItem selectedItem = SelectedItem;
				if (_holdingItem.StackCount == 1)
				{
					selectedItem.Stack(_holdingItem);
					_holdingItem = null;
				}
				else if (_holdingItem.StackCount > 1)
				{
					selectedItem.Stack(_holdingItem.PopOneItem());
				}
				SelectedItem = selectedItem;
			}
			return true;
		}

		public bool ItemCountUp()
		{
			if (IsSelectedSlotLocked())
			{
				return false;
			}
			if (_holdingItem == null)
			{
				if (SelectedItem == null)
				{
					return false;
				}
				if (SelectedItem.StackCount == 1)
				{
					_holdingItem = SelectedItem;
					if (_selectorInCrateGrid)
					{
						SelectedItem = null;
					}
					else
					{
						_hud.PlayerInventory.Remove(_holdingItem);
					}
				}
				else if (SelectedItem.StackCount > 1)
				{
					InventoryItem selectedItem = SelectedItem;
					_holdingItem = selectedItem.PopOneItem();
					SelectedItem = selectedItem;
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
				InventoryItem selectedItem2 = SelectedItem;
				if (SelectedItem.StackCount == 1)
				{
					_holdingItem.Stack(selectedItem2);
					SelectedItem = null;
				}
				else if (SelectedItem.StackCount > 1)
				{
					_holdingItem.Stack(selectedItem2.PopOneItem());
					SelectedItem = selectedItem2;
				}
			}
			return true;
		}

		public bool SelectDown()
		{
			_selectedLocation.Y++;
			if (_selectedLocation.Y > 8)
			{
				_selectedLocation.Y = 0;
			}
			return true;
		}

		public bool SelectUp()
		{
			_selectedLocation.Y--;
			if (_selectedLocation.Y < 0)
			{
				_selectedLocation.Y = 8;
			}
			return true;
		}

		public bool SelectLeft()
		{
			_selectedLocation.X--;
			if (_selectedLocation.X < 0)
			{
				_selectorInCrateGrid = !_selectorInCrateGrid;
				_selectedLocation.X = 3;
			}
			return true;
		}

		public bool SelectRight()
		{
			_selectedLocation.X++;
			if (_selectedLocation.X > 3)
			{
				_selectorInCrateGrid = !_selectorInCrateGrid;
				_selectedLocation.X = 0;
			}
			return true;
		}
	}
}
