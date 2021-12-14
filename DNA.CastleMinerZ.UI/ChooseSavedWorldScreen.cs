using DNA.Audio;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DNA.CastleMinerZ.UI
{
	public class ChooseSavedWorldScreen : ScrollingListScreen
	{
		private enum SortBy
		{
			DateAsc,
			DateDesc,
			NameAsc,
			NameDesc,
			CreatorAsc,
			CreatorDesc,
			OwnerAsc,
			OwnerDesc
		}

		public class SavedWorldItem : ListItemControl
		{
			public WorldInfo World;

			private SpriteFont _largeFont = CastleMinerZGame.Instance._medLargeFont;

			private SpriteFont _medFont = CastleMinerZGame.Instance._medFont;

			private SpriteFont _smallFont = CastleMinerZGame.Instance._smallFont;

			private string _lastPlayedDate;

			public SavedWorldItem(WorldInfo world, Size itemSize)
				: base(itemSize)
			{
				_lastPlayedDate = world.LastPlayedDate.ToString();
				World = world;
			}

			protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
			{
				base.OnDraw(device, spriteBatch, gameTime);
				Color color = TextColor;
				if (base.CaptureInput || Selected)
				{
					color = TextPressedColor;
				}
				else if (base.Hovering)
				{
					color = TextHoverColor;
				}
				Vector2 position = new Vector2((float)base.LocalPosition.X + 10f * Screen.Adjuster.ScaleFactor.X, (float)base.LocalPosition.Y + 5f * Screen.Adjuster.ScaleFactor.X);
				spriteBatch.DrawString(_medFont, World.Name, position, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				position.Y += (float)_medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				int num = (int)(215f * Screen.Adjuster.ScaleFactor.X);
				position.X += num;
				spriteBatch.DrawString(_smallFont, _lastPlayedDate, position, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				position.X += (int)(161f * Screen.Adjuster.ScaleFactor.X);
				spriteBatch.DrawString(_smallFont, World.CreatorGamerTag, position, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				position.X += (int)(161f * Screen.Adjuster.ScaleFactor.X);
				if ((float)Size.Width > position.X)
				{
					spriteBatch.DrawString(_smallFont, World.OwnerGamerTag, position, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				}
			}

			public void DrawSelectedInfo(SpriteBatch spriteBatch, Vector2 loc)
			{
				spriteBatch.DrawOutlinedText(_medFont, World.Name, loc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				loc.Y += (float)_medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawOutlinedText(_medFont, Strings.Created_By + ": " + World.CreatorGamerTag, loc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				loc.Y += (float)_medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawOutlinedText(_medFont, Strings.Last_Played + ": " + _lastPlayedDate, loc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				loc.Y += (float)_medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawOutlinedText(_medFont, Strings.Hosted_By + ": " + World.OwnerGamerTag, loc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
			}
		}

		private CastleMinerZGame _game = CastleMinerZGame.Instance;

		private FrameButtonControl _eraseSaves = new FrameButtonControl();

		private PCDialogScreen _deleteStorageDialog;

		public PCDialogScreen _takeOverTerrain;

		public PCDialogScreen _infiniteModeConversion;

		private PCDialogScreen _deleteWorldDialog;

		private SortBy _currentSort = SortBy.DateDesc;

		private FrameButtonControl _dateButton = new FrameButtonControl();

		private FrameButtonControl _creatorButton = new FrameButtonControl();

		private FrameButtonControl _nameButton = new FrameButtonControl();

		private FrameButtonControl _ownerButton = new FrameButtonControl();

		private PCKeyboardInputScreen _keyboardInputScreen;

		private ButtonControl _deleteButton;

		private ButtonControl _renameButton;

		private ButtonControl _newWorldButton;

		private Rectangle prevScreenRect;

		private WorldManager WorldManager
		{
			get
			{
				return _game.FrontEnd.WorldManager;
			}
		}

		public ButtonControl DeleteButton
		{
			set
			{
				if (_deleteButton != null)
				{
					base.Controls.Remove(_deleteButton);
				}
				_deleteButton = value;
				_deleteButton.Pressed += _deleteButton_Pressed;
				base.Controls.Add(_deleteButton);
			}
		}

		public ButtonControl RenameButton
		{
			set
			{
				if (_renameButton != null)
				{
					base.Controls.Remove(_renameButton);
				}
				_renameButton = value;
				_renameButton.Pressed += _renameButton_Pressed;
				base.Controls.Add(_renameButton);
			}
		}

		public ButtonControl NewWorldButton
		{
			set
			{
				if (_newWorldButton != null)
				{
					base.Controls.Remove(_renameButton);
				}
				_newWorldButton = value;
				_newWorldButton.Pressed += _newWorldButton_Pressed;
				base.Controls.Add(_newWorldButton);
			}
		}

		private void _deleteButton_Pressed(object sender, EventArgs e)
		{
			_game.FrontEnd._uiGroup.ShowPCDialogScreen(_deleteWorldDialog, delegate
			{
				if (_deleteWorldDialog.OptionSelected != -1)
				{
					ChooseSavedWorldScreen chooseSavedWorldScreen = this;
					SavedWorldItem selected = (SavedWorldItem)base.SelectedItem;
					WaitScreen.DoWait(_game.FrontEnd._uiGroup, Strings.Deleting_World___, delegate
					{
						chooseSavedWorldScreen.WorldManager.Delete(selected.World);
						chooseSavedWorldScreen.Items.Remove(selected);
						chooseSavedWorldScreen._updateControlsOnSort();
						chooseSavedWorldScreen._game.SaveDevice.Flush();
						if (chooseSavedWorldScreen.Items.Count == 0)
						{
							chooseSavedWorldScreen._deleteButton.Visible = false;
							chooseSavedWorldScreen._renameButton.Visible = false;
						}
					}, null);
				}
			});
		}

		private void _renameButton_Pressed(object sender, EventArgs e)
		{
			SavedWorldItem selected = (SavedWorldItem)base.SelectedItem;
			_keyboardInputScreen.Title = Strings.Rename + " " + selected.World.Name;
			_keyboardInputScreen.DefaultText = selected.World.Name;
			CastleMinerZGame.Instance.FrontEnd._uiGroup.ShowPCDialogScreen(_keyboardInputScreen, delegate
			{
				if (_keyboardInputScreen.OptionSelected != -1)
				{
					string textInput = _keyboardInputScreen.TextInput;
					if (textInput != null)
					{
						if (textInput.Length > 25)
						{
							selected.World.Name = textInput.Substring(0, 25);
						}
						else
						{
							selected.World.Name = textInput;
						}
						selected.World.SaveToStorage(Screen.CurrentGamer, CastleMinerZGame.Instance.SaveDevice);
						_resetSortButtonText();
					}
				}
			});
		}

		private void _newWorldButton_Pressed(object sender, EventArgs e)
		{
			if (ClickSound != null)
			{
				SoundManager.Instance.PlayInstance(ClickSound);
			}
			_game.FrontEnd.startWorld();
		}

		public ChooseSavedWorldScreen()
			: base(false, new Size(700, 60), new Rectangle(10, 60, Screen.Adjuster.ScreenRect.Width - 10, Screen.Adjuster.ScreenRect.Height - 60))
		{
			ClickSound = "Click";
			Color color = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			_deleteStorageDialog = new PCDialogScreen(Strings.Erase_Storage, Strings.Are_you_sure_you_want_to_delete_everything_, null, true, _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_deleteStorageDialog.UseDefaultValues();
			_eraseSaves.Size = new Size(300, 40);
			_eraseSaves.Text = Strings.Erase_Storage;
			_eraseSaves.Font = _game._medFont;
			_eraseSaves.Frame = _game.ButtonFrame;
			_eraseSaves.Pressed += _eraseSaves_Pressed;
			_eraseSaves.ButtonColor = color;
			base.Controls.Add(_eraseSaves);
			base.SelectButton = new FrameButtonControl
			{
				LocalPosition = new Point(900, 170),
				Size = new Size(300, 40),
				Text = Strings.Start_Game,
				Font = _game._medFont,
				Frame = _game.ButtonFrame,
				ButtonColor = color
			};
			DeleteButton = new FrameButtonControl
			{
				LocalPosition = new Point(900, 215),
				Size = new Size(300, 40),
				Text = Strings.Delete_World,
				Font = _game._medFont,
				Frame = _game.ButtonFrame,
				ButtonColor = color
			};
			RenameButton = new FrameButtonControl
			{
				LocalPosition = new Point(900, 260),
				Size = new Size(300, 40),
				Text = Strings.Rename_World,
				Font = _game._medFont,
				Frame = _game.ButtonFrame,
				ButtonColor = color
			};
			NewWorldButton = new FrameButtonControl
			{
				LocalPosition = new Point(900, 125),
				Size = new Size(300, 40),
				Text = Strings.New_World,
				Font = _game._medFont,
				Frame = _game.ButtonFrame,
				ButtonColor = color
			};
			base.BackButton = new ImageButtonControl
			{
				Image = _game._uiSprites["BackArrow"],
				Font = _game._medFont,
				LocalPosition = new Point(15, 15),
				Text = " " + Strings.Back,
				ImageDefaultColor = color
			};
			Point localPosition = new Point(40, 100);
			_nameButton.LocalPosition = localPosition;
			_nameButton.Size = new Size(217, 18);
			_nameButton.Text = "SERVER NAME";
			_nameButton.Font = _game._smallFont;
			_nameButton.TextAlignment = FrameButtonControl.Alignment.Left;
			_nameButton.Frame = _game.ButtonFrame;
			_nameButton.Pressed += _nameButton_Pressed;
			base.Controls.Add(_nameButton);
			localPosition.X += _nameButton.Size.Width + 1;
			_dateButton.LocalPosition = localPosition;
			_dateButton.Size = new Size(160, 18);
			_dateButton.Text = "DATE \u02c5";
			_dateButton.Font = _game._smallFont;
			_dateButton.TextAlignment = FrameButtonControl.Alignment.Left;
			_dateButton.Frame = _game.ButtonFrame;
			_dateButton.Pressed += _dateButton_Pressed;
			base.Controls.Add(_dateButton);
			localPosition.X += _dateButton.Size.Width + 1;
			_creatorButton.LocalPosition = localPosition;
			_creatorButton.Size = new Size(160, 18);
			_creatorButton.Text = "CREATED BY";
			_creatorButton.Font = _game._smallFont;
			_creatorButton.TextAlignment = FrameButtonControl.Alignment.Left;
			_creatorButton.Frame = _game.ButtonFrame;
			_creatorButton.Pressed += _creatorButton_Pressed;
			base.Controls.Add(_creatorButton);
			localPosition.X += _creatorButton.Size.Width + 1;
			_ownerButton.LocalPosition = localPosition;
			_ownerButton.Size = new Size(160, 18);
			_ownerButton.Text = "HOST";
			_ownerButton.Font = _game._smallFont;
			_ownerButton.TextAlignment = FrameButtonControl.Alignment.Left;
			_ownerButton.Frame = _game.ButtonFrame;
			_ownerButton.Pressed += _ownerButton_Pressed;
			base.Controls.Add(_ownerButton);
			_deleteWorldDialog = new PCDialogScreen(Strings.Delete_World, Strings.Are_you_sure_you_want_to_delete_this_world_, null, true, _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_deleteWorldDialog.UseDefaultValues();
			_takeOverTerrain = new PCDialogScreen(Strings.Take_Over_World, Strings.This_world_belongs_to_someone_else_Would_you_like_to_make_your_own_copy_You_will_be_able_to_make_changes_locally_and_host_it_yourself, null, true, _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_takeOverTerrain.UseDefaultValues();
			_infiniteModeConversion = new PCDialogScreen(Strings.Creative_Mode, Strings.Are_you_sure_you_want_to_play_this_world_in_Creative_Mode__You_will_not_be_able_to_load_it_in_normal_mode_again_, null, true, _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_infiniteModeConversion.UseDefaultValues();
			_keyboardInputScreen = new PCKeyboardInputScreen(CastleMinerZGame.Instance, Strings.Rename, Strings.Enter_A_New_Name, CastleMinerZGame.Instance.DialogScreenImage, CastleMinerZGame.Instance._myriadMed, true, _game.ButtonFrame);
			_keyboardInputScreen.ClickSound = "Click";
			_keyboardInputScreen.OpenSound = "Popup";
		}

		private void _ownerButton_Pressed(object sender, EventArgs e)
		{
			_sortByOwner(_currentSort, Items);
			_updateControlsOnSort();
		}

		private void _creatorButton_Pressed(object sender, EventArgs e)
		{
			_sortByCreator(_currentSort, Items);
			_updateControlsOnSort();
		}

		private void _eraseSaves_Pressed(object sender, EventArgs e)
		{
			_game.FrontEnd._uiGroup.ShowPCDialogScreen(_deleteStorageDialog, delegate
			{
				if (_deleteStorageDialog.OptionSelected != -1)
				{
					WaitScreen.DoWait(_game.FrontEnd._uiGroup, Strings.Deleting_Storage___, delegate
					{
						_game.SaveDevice.DeleteStorage();
					}, null);
					_game.FrontEnd.PopToStartScreen();
				}
			});
		}

		public void Populate()
		{
			WorldInfo[] worlds = WorldManager.GetWorlds();
			List<ListItemControl> list = new List<ListItemControl>();
			WorldInfo[] array = worlds;
			foreach (WorldInfo worldInfo in array)
			{
				if (worldInfo.InfiniteResourceMode == _game.InfiniteResourceMode || _game.InfiniteResourceMode)
				{
					list.Add(new SavedWorldItem(worldInfo, _itemSize));
				}
			}
			switch (_currentSort)
			{
			case SortBy.DateAsc:
				_sortByDate(SortBy.DateDesc, list);
				break;
			case SortBy.DateDesc:
				_sortByDate(SortBy.DateAsc, list);
				break;
			case SortBy.NameAsc:
				_sortByName(SortBy.NameDesc, list);
				break;
			case SortBy.NameDesc:
				_sortByName(SortBy.NameAsc, list);
				break;
			case SortBy.CreatorAsc:
				_sortByCreator(SortBy.CreatorDesc, list);
				break;
			case SortBy.CreatorDesc:
				_sortByCreator(SortBy.CreatorAsc, list);
				break;
			case SortBy.OwnerAsc:
				_sortByOwner(SortBy.OwnerDesc, list);
				break;
			case SortBy.OwnerDesc:
				_sortByOwner(SortBy.OwnerAsc, list);
				break;
			}
			Items = list;
			if (Items.Count == 0)
			{
				FrameButtonControl creatorButton = _creatorButton;
				FrameButtonControl nameButton = _nameButton;
				FrameButtonControl dateButton = _dateButton;
				FrameButtonControl ownerButton = _ownerButton;
				ButtonControl deleteButton = _deleteButton;
				ButtonControl renameButton = _renameButton;
				bool flag2 = _eraseSaves.Visible = false;
				bool flag4 = renameButton.Visible = flag2;
				bool flag6 = deleteButton.Visible = flag4;
				bool flag8 = ownerButton.Visible = flag6;
				bool flag10 = dateButton.Visible = flag8;
				bool visible = nameButton.Visible = flag10;
				creatorButton.Visible = visible;
			}
			else
			{
				FrameButtonControl creatorButton2 = _creatorButton;
				FrameButtonControl nameButton2 = _nameButton;
				FrameButtonControl dateButton2 = _dateButton;
				FrameButtonControl ownerButton2 = _ownerButton;
				ButtonControl deleteButton2 = _deleteButton;
				ButtonControl renameButton2 = _renameButton;
				bool flag13 = _eraseSaves.Visible = true;
				bool flag15 = renameButton2.Visible = flag13;
				bool flag17 = deleteButton2.Visible = flag15;
				bool flag19 = ownerButton2.Visible = flag17;
				bool flag21 = dateButton2.Visible = flag19;
				bool visible2 = nameButton2.Visible = flag21;
				creatorButton2.Visible = visible2;
			}
			base.OnPushed();
		}

		private void _resetSortButtonText()
		{
			_nameButton.Text = "SERVER NAME";
			_dateButton.Text = "DATE";
			_creatorButton.Text = "CREATED BY";
			_ownerButton.Text = "HOST";
		}

		private void _dateButton_Pressed(object sender, EventArgs e)
		{
			_sortByDate(_currentSort, Items);
			_updateControlsOnSort();
		}

		private void _sortByDate(SortBy sort, List<ListItemControl> items)
		{
			_resetSortButtonText();
			if (sort == SortBy.DateDesc)
			{
				_dateButton.Text = "DATE \u02c4";
				_currentSort = SortBy.DateAsc;
				items.Sort(SortByDate);
			}
			else
			{
				_dateButton.Text = "DATE \u02c5";
				_currentSort = SortBy.DateDesc;
				items.Sort(SortByDate);
				items.Reverse();
			}
		}

		private void _sortByCreator(SortBy sort, List<ListItemControl> items)
		{
			_resetSortButtonText();
			if (sort == SortBy.CreatorDesc)
			{
				_creatorButton.Text = "CREATED BY \u02c4";
				_currentSort = SortBy.CreatorAsc;
				items.Sort(SortByCreator);
			}
			else
			{
				_creatorButton.Text = "CREATED BY \u02c5";
				_currentSort = SortBy.CreatorDesc;
				items.Sort(SortByCreator);
				items.Reverse();
			}
		}

		private void _sortByOwner(SortBy sort, List<ListItemControl> items)
		{
			_resetSortButtonText();
			if (sort == SortBy.OwnerDesc)
			{
				_ownerButton.Text = "HOST \u02c4";
				_currentSort = SortBy.OwnerAsc;
				items.Sort(SortByOwner);
			}
			else
			{
				_ownerButton.Text = "HOST \u02c5";
				_currentSort = SortBy.OwnerDesc;
				items.Sort(SortByOwner);
				items.Reverse();
			}
		}

		private void _nameButton_Pressed(object sender, EventArgs e)
		{
			_sortByName(_currentSort, Items);
			_updateControlsOnSort();
		}

		private void _sortByName(SortBy sort, List<ListItemControl> items)
		{
			_resetSortButtonText();
			if (sort == SortBy.NameAsc)
			{
				_nameButton.Text = "SERVER NAME \u02c5";
				_currentSort = SortBy.NameDesc;
				items.Sort(SortByName);
				items.Reverse();
			}
			else
			{
				_nameButton.Text = "SERVER NAME \u02c4";
				_currentSort = SortBy.NameAsc;
				items.Sort(SortByName);
			}
		}

		public static int SortByName(ListItemControl a, ListItemControl b)
		{
			if (a == null)
			{
				if (b == null)
				{
					return 0;
				}
				return -1;
			}
			if (b == null)
			{
				return 1;
			}
			SavedWorldItem savedWorldItem = (SavedWorldItem)a;
			SavedWorldItem savedWorldItem2 = (SavedWorldItem)b;
			return string.Compare(savedWorldItem.World.Name, savedWorldItem2.World.Name, true);
		}

		public static int SortByCreator(ListItemControl a, ListItemControl b)
		{
			if (a == null)
			{
				if (b == null)
				{
					return 0;
				}
				return -1;
			}
			if (b == null)
			{
				return 1;
			}
			SavedWorldItem savedWorldItem = (SavedWorldItem)a;
			SavedWorldItem savedWorldItem2 = (SavedWorldItem)b;
			return string.Compare(savedWorldItem.World.CreatorGamerTag, savedWorldItem2.World.CreatorGamerTag, true);
		}

		public static int SortByOwner(ListItemControl a, ListItemControl b)
		{
			if (a == null)
			{
				if (b == null)
				{
					return 0;
				}
				return -1;
			}
			if (b == null)
			{
				return 1;
			}
			SavedWorldItem savedWorldItem = (SavedWorldItem)a;
			SavedWorldItem savedWorldItem2 = (SavedWorldItem)b;
			if (savedWorldItem.World.OwnerGamerTag == Screen.CurrentGamer.Gamertag)
			{
				if (savedWorldItem2.World.OwnerGamerTag == Screen.CurrentGamer.Gamertag)
				{
					return 0;
				}
				return 1;
			}
			if (savedWorldItem2.World.OwnerGamerTag == Screen.CurrentGamer.Gamertag)
			{
				return -1;
			}
			return string.Compare(savedWorldItem.World.OwnerGamerTag, savedWorldItem2.World.OwnerGamerTag, true);
		}

		public static int SortByDate(ListItemControl a, ListItemControl b)
		{
			if (a == null)
			{
				if (b == null)
				{
					return 0;
				}
				return -1;
			}
			if (b == null)
			{
				return 1;
			}
			SavedWorldItem savedWorldItem = (SavedWorldItem)a;
			SavedWorldItem savedWorldItem2 = (SavedWorldItem)b;
			return DateTime.Compare(savedWorldItem.World.LastPlayedDate, savedWorldItem2.World.LastPlayedDate);
		}

		public override void OnPushed()
		{
			Populate();
			base.OnPushed();
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (Screen.Adjuster.ScreenRect != prevScreenRect)
			{
				prevScreenRect = Screen.Adjuster.ScreenRect;
				int num = (int)(540f * Screen.Adjuster.ScaleFactor.X);
				_selectButton.Scale = (_deleteButton.Scale = (_newWorldButton.Scale = (_eraseSaves.Scale = (_renameButton.Scale = (_nameButton.Scale = (_dateButton.Scale = (_creatorButton.Scale = (_ownerButton.Scale = Screen.Adjuster.ScaleFactor.X))))))));
				Point localPosition = new Point(40, 100);
				_nameButton.LocalPosition = localPosition;
				localPosition.X += _nameButton.Size.Width + 1;
				_dateButton.LocalPosition = localPosition;
				localPosition.X += _dateButton.Size.Width + 1;
				_creatorButton.LocalPosition = localPosition;
				localPosition.X += _creatorButton.Size.Width + 1;
				_ownerButton.LocalPosition = localPosition;
				int x = (int)(740f * Screen.Adjuster.ScaleFactor.X) + num / 2 - _deleteButton.Size.Width / 2;
				int num2 = _selectButton.Size.Height + (int)(5f * Screen.Adjuster.ScaleFactor.X);
				_selectButton.LocalPosition = new Point(x, _selectButton.LocalPosition.Y);
				_newWorldButton.LocalPosition = new Point(x, _selectButton.LocalPosition.Y + num2);
				_deleteButton.LocalPosition = new Point(x, _newWorldButton.LocalPosition.Y + num2);
				_renameButton.LocalPosition = new Point(x, _deleteButton.LocalPosition.Y + num2);
				_eraseSaves.LocalPosition = new Point(x, _renameButton.LocalPosition.Y + num2);
				_itemSize.Width = (int)(700f * Screen.Adjuster.ScaleFactor.X);
				_itemSize.Height = (int)(60f * Screen.Adjuster.ScaleFactor.X);
				int num3 = _nameButton.LocalPosition.Y + _nameButton.Size.Height + (int)(5f * Screen.Adjuster.ScaleFactor.X);
				_drawArea = new Rectangle((int)(10f * Screen.Adjuster.ScaleFactor.X), num3, (int)((float)Screen.Adjuster.ScreenRect.Width - 10f * Screen.Adjuster.ScaleFactor.X), Screen.Adjuster.ScreenRect.Height - num3);
				for (int i = 0; i < Items.Count; i++)
				{
					Items[i].Size = _itemSize;
				}
				_updateControlsOnSort();
			}
			base.OnUpdate(game, gameTime);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			SpriteFont largeFont = _game._largeFont;
			spriteBatch.Begin();
			if (Items.Count == 0)
			{
				string text = "No Saved Worlds";
				Vector2 vector = largeFont.MeasureString(text);
				int lineSpacing = largeFont.LineSpacing;
				spriteBatch.DrawOutlinedText(largeFont, text, new Vector2(75f * Screen.Adjuster.ScaleFactor.X, 170f), CMZColors.MenuGreen, Color.Black, 2);
			}
			else
			{
				string text = Strings.Choose_A_Server;
				Vector2 vector = largeFont.MeasureString(text);
				int lineSpacing2 = largeFont.LineSpacing;
				spriteBatch.DrawOutlinedText(largeFont, text, new Vector2((float)(Screen.Adjuster.ScreenRect.Width / 2) - vector.X / 2f, 10f), CMZColors.MenuGreen, Color.Black, 1);
				SavedWorldItem savedWorldItem = (SavedWorldItem)base.SelectedItem;
				savedWorldItem.DrawSelectedInfo(spriteBatch, new Vector2(_eraseSaves.LocalPosition.X, _eraseSaves.LocalPosition.Y + _eraseSaves.Size.Height + 5));
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}
	}
}
