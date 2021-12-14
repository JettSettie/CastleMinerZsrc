using DNA.Audio;
using DNA.CastleMinerZ.Inventory;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DNA.CastleMinerZ.UI
{
	public class Tier2Item : IEquatable<Tier2Item>
	{
		private string _title;

		public Vector2 Location;

		private Rectangle _scaledLocation;

		private List<Recipe> _items;

		private Point[] _itemLocations;

		private Rectangle[] _scaledItemLocations;

		private int _selectedIndex;

		private CraftingScreen _craftingScreen;

		private SpriteFont _font;

		private Tier1Item _tier1Item;

		private Rectangle _backgroundRectangle;

		private string Title
		{
			get
			{
				return _title;
			}
		}

		public InventoryItem SelectedItem
		{
			get
			{
				return _items[_selectedIndex].Result;
			}
		}

		public List<InventoryItem> SelectedItemIngredients
		{
			get
			{
				return _items[_selectedIndex].Ingredients;
			}
		}

		private PlayerInventory Inventory
		{
			get
			{
				return CastleMinerZGame.Instance.GameScreen.HUD.PlayerInventory;
			}
		}

		public Tier2Item(string title, Recipe.RecipeTypes recipeType, Tier1Item tier1Item, CraftingScreen craftingScreen)
		{
			_title = title;
			_items = Recipe.GetRecipes(recipeType);
			_craftingScreen = craftingScreen;
			_font = CastleMinerZGame.Instance._smallFont;
			_tier1Item = tier1Item;
			_itemLocations = new Point[_items.Count];
			_scaledItemLocations = new Rectangle[_items.Count];
			Point point = new Point(393, 40);
			for (int i = 0; i < _itemLocations.Length; i++)
			{
				_itemLocations[i] = new Point(point.X, point.Y);
				if (i > 0)
				{
					for (int num = i - 1; num >= 0; num--)
					{
						if (_items[i].Result.ItemClass.ID == _items[num].Result.ItemClass.ID)
						{
							_itemLocations[i] = new Point(_itemLocations[num].X + 65, _itemLocations[num].Y);
							point.Y -= 65;
							break;
						}
					}
				}
				point.Y += 65;
			}
		}

		public bool Equals(Tier2Item other)
		{
			return Title == other.Title;
		}

		public void UpdateScaledLocation(Rectangle backgroundRectangle)
		{
			_scaledLocation = new Rectangle((int)((float)backgroundRectangle.X + Location.X * Screen.Adjuster.ScaleFactor.Y), (int)((float)backgroundRectangle.Y + Location.Y * Screen.Adjuster.ScaleFactor.Y), (int)(104f * Screen.Adjuster.ScaleFactor.Y), (int)(30f * Screen.Adjuster.ScaleFactor.Y));
			_backgroundRectangle = backgroundRectangle;
			for (int i = 0; i < _itemLocations.Length; i++)
			{
				_scaledItemLocations[i] = new Rectangle((int)((float)backgroundRectangle.X + (float)_itemLocations[i].X * Screen.Adjuster.ScaleFactor.Y), (int)((float)backgroundRectangle.Y + (float)_itemLocations[i].Y * Screen.Adjuster.ScaleFactor.Y), (int)(64f * Screen.Adjuster.ScaleFactor.Y), (int)(64f * Screen.Adjuster.ScaleFactor.Y));
			}
		}

		private int _hitTest(MouseInput mouse)
		{
			for (int i = 0; i < _scaledItemLocations.Length; i++)
			{
				if (_scaledItemLocations[i].Contains(mouse.Position))
				{
					return i;
				}
			}
			return -1;
		}

		public bool CheckInput(InputManager inputManager)
		{
			if (_tier1Item.SelectedTier2Item == this)
			{
				int num = _hitTest(inputManager.Mouse);
				if (num >= 0)
				{
					_selectedIndex = num;
					if (inputManager.Mouse.LeftButtonPressed)
					{
						if (Inventory.CanCraft(_items[_selectedIndex]))
						{
							SoundManager.Instance.PlayInstance("craft");
							CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(_items[_selectedIndex].Result.ItemClass.ID);
							if (inputManager.Keyboard.IsKeyDown(Keys.LeftShift) || inputManager.Keyboard.IsKeyDown(Keys.RightShift))
							{
								while (Inventory.CanCraft(_items[_selectedIndex]))
								{
									Inventory.Craft(_items[_selectedIndex]);
									itemStats.Crafted++;
									if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance)
									{
										CastleMinerZGame.Instance.PlayerStats.TotalItemsCrafted++;
									}
								}
							}
							else
							{
								Inventory.Craft(_items[_selectedIndex]);
								itemStats.Crafted++;
								if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance)
								{
									CastleMinerZGame.Instance.PlayerStats.TotalItemsCrafted++;
								}
							}
						}
						else
						{
							SoundManager.Instance.PlayInstance("Error");
						}
					}
				}
			}
			if (_scaledLocation.Contains(inputManager.Mouse.Position))
			{
				return true;
			}
			return false;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			Color color = CMZColors.MenuAqua;
			bool flag = _tier1Item.SelectedTier2Item == this;
			spriteBatch.Draw(_craftingScreen._tier2Back, _scaledLocation, Color.White);
			if (flag)
			{
				color = Color.White;
			}
			spriteBatch.DrawString(_font, _title, new Vector2((float)_scaledLocation.Left + 10f * Screen.Adjuster.ScaleFactor.Y, _scaledLocation.Top), color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
			if (!flag)
			{
				return;
			}
			for (int i = 0; i < _scaledItemLocations.Length; i++)
			{
				spriteBatch.Draw(_craftingScreen._gridSquare, _scaledItemLocations[i], Inventory.CanCraft(_items[i]) ? Color.White : new Color(0.25f, 0.25f, 0.25f, 0.5f));
				_items[i].Result.Draw2D(spriteBatch, new Rectangle(_scaledItemLocations[i].X, _scaledItemLocations[i].Y, (int)((float)_scaledItemLocations[i].Width - 5f * Screen.Adjuster.ScaleFactor.Y), (int)((float)_scaledItemLocations[i].Height - 5f * Screen.Adjuster.ScaleFactor.Y)), Inventory.CanCraft(_items[i]) ? Color.White : new Color(0.25f, 0.25f, 0.25f, 0.5f), true);
				if (_selectedIndex == i)
				{
					spriteBatch.Draw(_craftingScreen._gridSelector, _scaledItemLocations[i], Color.White);
					_items[i].Result.Draw2D(spriteBatch, new Rectangle(_backgroundRectangle.X + (int)(32f * Screen.Adjuster.ScaleFactor.Y), _backgroundRectangle.Y + (int)(235f * Screen.Adjuster.ScaleFactor.Y), (int)(130f * Screen.Adjuster.ScaleFactor.Y), (int)(130f * Screen.Adjuster.ScaleFactor.Y)), Color.White, false);
				}
			}
			spriteBatch.Draw(_craftingScreen._craftSelector, new Rectangle(_scaledLocation.X, _scaledLocation.Y, (int)((float)_scaledLocation.Width - 8f * Screen.Adjuster.ScaleFactor.Y), (int)((float)_scaledLocation.Height - 5f * Screen.Adjuster.ScaleFactor.Y)), color);
			Point point = new Point(39, 521);
			for (int j = 0; j < _items[_selectedIndex].Ingredients.Count; j++)
			{
				int num = (int)(64f * Screen.Adjuster.ScaleFactor.Y);
				Rectangle rectangle = new Rectangle((int)((float)_backgroundRectangle.X + (float)point.X * Screen.Adjuster.ScaleFactor.Y), (int)((float)_backgroundRectangle.Y + (float)point.Y * Screen.Adjuster.ScaleFactor.Y), num, num);
				spriteBatch.Draw(_craftingScreen._gridSquare, rectangle, Color.White);
				rectangle.Width = (int)((float)rectangle.Width - 5f * Screen.Adjuster.ScaleFactor.Y);
				rectangle.Height = (int)((float)rectangle.Height - 5f * Screen.Adjuster.ScaleFactor.Y);
				_items[_selectedIndex].Ingredients[j].Draw2D(spriteBatch, rectangle, Inventory.CanConsume(_items[_selectedIndex].Ingredients[j].ItemClass, _items[_selectedIndex].Ingredients[j].StackCount) ? Color.White : new Color(0.25f, 0.25f, 0.25f, 0.5f), true);
				point.X += 65;
			}
		}
	}
}
