using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DNA.CastleMinerZ.UI
{
	public class Tier1Item : IEquatable<Tier1Item>
	{
		private string _title;

		private Vector2 _location;

		private Rectangle _scaledLocation;

		private Sprite _icon;

		private List<Tier2Item> _items;

		public Tier2Item SelectedTier2Item;

		private CraftingScreen _craftingScreen;

		private SpriteFont _font;

		private string Title
		{
			get
			{
				return _title;
			}
		}

		public Tier1Item(string title, Vector2 location, Sprite icon, CraftingScreen craftingScreen)
		{
			_title = title;
			_location = location;
			_icon = icon;
			_craftingScreen = craftingScreen;
			_font = CastleMinerZGame.Instance._medFont;
		}

		public void SetItems(List<Tier2Item> items)
		{
			_items = items;
			Vector2 value = new Vector2(263f, 7f);
			for (int i = 0; i < _items.Count; i++)
			{
				_items[i].Location = _location + value;
				value.Y += 31f;
			}
			SelectedTier2Item = _items[0];
		}

		public bool Equals(Tier1Item other)
		{
			return Title == other.Title;
		}

		public void UpdateScaledLocation(Rectangle backgroundRectangle)
		{
			_scaledLocation = new Rectangle((int)((float)backgroundRectangle.X + _location.X * Screen.Adjuster.ScaleFactor.Y), (int)((float)backgroundRectangle.Y + _location.Y * Screen.Adjuster.ScaleFactor.Y), (int)(243f * Screen.Adjuster.ScaleFactor.Y), (int)(36f * Screen.Adjuster.ScaleFactor.Y));
			for (int i = 0; i < _items.Count; i++)
			{
				_items[i].UpdateScaledLocation(backgroundRectangle);
			}
		}

		public bool CheckInput(InputManager inputManager)
		{
			if (_craftingScreen.SelectedTier1Item == this)
			{
				for (int i = 0; i < _items.Count; i++)
				{
					if (_items[i].CheckInput(inputManager))
					{
						SelectedTier2Item = _items[i];
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
			bool flag = _craftingScreen.SelectedTier1Item == this;
			Color color = CMZColors.MenuAqua;
			if (flag)
			{
				color = Color.White;
			}
			spriteBatch.DrawString(_font, _title, new Vector2((float)_scaledLocation.Right - (50f + _font.MeasureString(_title).X) * Screen.Adjuster.ScaleFactor.Y, _scaledLocation.Top), color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
			int num = (int)(35f * Screen.Adjuster.ScaleFactor.Y);
			spriteBatch.Draw(_icon, new Rectangle(_scaledLocation.Right - num, _scaledLocation.Top, num, num), color);
			if (flag)
			{
				spriteBatch.Draw(_craftingScreen._craftSelector, _scaledLocation, color);
			}
			if (flag)
			{
				for (int i = 0; i < _items.Count; i++)
				{
					_items[i].Draw(spriteBatch);
				}
			}
		}
	}
}
