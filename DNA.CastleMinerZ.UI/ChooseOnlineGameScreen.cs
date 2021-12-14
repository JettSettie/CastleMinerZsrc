using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Net;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DNA.CastleMinerZ.UI
{
	public class ChooseOnlineGameScreen : ScrollingListScreen
	{
		private enum SortBy
		{
			DateAsc,
			DateDesc,
			NumPlayersAsc,
			NumPlayersDesc,
			ModeAsc,
			ModeDesc,
			NameAsc,
			NameDesc,
			MaxPlayersAsc,
			MaxPlayersDesc,
			NumFriendsAsc,
			NumFriendsDesc
		}

		private enum SortDirection
		{
			Ascending = 1,
			Descending = -1
		}

		public class OnlineGameMenuItem : ListItemControl
		{
			private ServerInfo _serverInfo;

			public string Password = "";

			private SpriteFont _largeFont = CastleMinerZGame.Instance._medLargeFont;

			private SpriteFont _medFont = CastleMinerZGame.Instance._medFont;

			private SpriteFont _smallFont = CastleMinerZGame.Instance._smallFont;

			private string _dateCreated;

			public AvailableNetworkSession NetworkSession
			{
				get
				{
					return _serverInfo.Session;
				}
			}

			public string GameModeString
			{
				get
				{
					return _serverInfo.GameModeString;
				}
			}

			public int NumFriends
			{
				get
				{
					return _serverInfo.NumFriends;
				}
			}

			public int Proximity
			{
				get
				{
					return _serverInfo.Proximity;
				}
			}

			protected override void OnUpdate(DNAGame game, GameTime gameTime)
			{
				Active = _serverInfo.IsOnline;
				base.OnUpdate(game, gameTime);
			}

			public OnlineGameMenuItem(AvailableNetworkSession networkSession, Size itemSize)
				: base(itemSize)
			{
				networkSession.ConvertToIPV4();
				_dateCreated = networkSession.DateCreated.ToString();
				_serverInfo = new ServerInfo(networkSession);
			}

			public void UpdateServerInfo(HostDiscovery hostDiscovery)
			{
				_serverInfo.RefreshServerStatus(hostDiscovery);
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
				if (_serverInfo.IsOnline)
				{
					spriteBatch.DrawString(_medFont, NetworkSession.ServerMessage, position, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				}
				else
				{
					spriteBatch.DrawString(_medFont, _serverInfo.ServerName, position, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				}
				position.Y += (float)_medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				string hostGamertag = NetworkSession.HostGamertag;
				if (!hostGamertag.Equals("[unknown]", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(hostGamertag))
				{
					spriteBatch.DrawString(_smallFont, Strings.Hosted_By + ": " + hostGamertag, position, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				}
				int num = 237;
				position.X += (float)num * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawString(_smallFont, _dateCreated, position, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				position.X += 175f * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawString(_smallFont, _serverInfo.NumberPlayerString, position, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				position.X += 30f * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawString(_smallFont, "/", position, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				position.X += (30f + _smallFont.MeasureString("/").X) * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawString(_smallFont, _serverInfo.MaxPlayersString, new Vector2(position.X - _smallFont.MeasureString(_serverInfo.MaxPlayersString).X, position.Y), color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				position.X += 35f * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawString(_smallFont, _serverInfo.GameModeString, position, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				position.X += 106f * Screen.Adjuster.ScaleFactor.X;
				if ((float)Size.Width > position.X)
				{
					spriteBatch.DrawString(_smallFont, _serverInfo.NumFriendsStr, position, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				}
			}

			public void DrawSelected(SpriteBatch spriteBatch, Vector2 loc)
			{
				Color color = TextColor;
				if (base.CaptureInput || Selected)
				{
					color = TextPressedColor;
				}
				else if (base.Hovering)
				{
					color = TextHoverColor;
				}
				spriteBatch.DrawOutlinedText(_medFont, NetworkSession.ServerMessage, loc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				loc.Y += (float)_medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				string hostGamertag = NetworkSession.HostGamertag;
				if (!hostGamertag.Equals("[unknown]", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(hostGamertag))
				{
					spriteBatch.DrawOutlinedText(_medFont, Strings.Hosted_By + ": " + hostGamertag, loc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				}
				loc.Y += (float)_medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawOutlinedText(_medFont, Strings.Created + ": " + _dateCreated, loc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				loc.Y += (float)_medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawOutlinedText(_medFont, Strings.Players + ": ", loc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				Vector2 location = loc;
				location.X += 100f * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawOutlinedText(_medFont, _serverInfo.NumberPlayerString, location, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				location.X += 40f * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawOutlinedText(_medFont, "/", location, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				location.X += (40f + _medFont.MeasureString("/").X) * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawOutlinedText(_medFont, _serverInfo.MaxPlayersString, new Vector2(location.X - _medFont.MeasureString(_serverInfo.MaxPlayersString).X, loc.Y), Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				loc.Y += (float)_medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawString(_medFont, Strings.Game_Mode + ": " + _serverInfo.GameModeString, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				loc.Y += (float)_medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawString(_medFont, "PVP: " + _serverInfo.PVPstr, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				if (_serverInfo.GameMode == GameModeTypes.Survival || _serverInfo.GameMode == GameModeTypes.Scavenger)
				{
					loc.Y += (float)_medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
					spriteBatch.DrawString(_medFont, Strings.Difficulty + ": " + _serverInfo.GameDifficultyString, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				}
				if (_serverInfo.PasswordProtected)
				{
					loc.Y += (float)_medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X * 2f;
					spriteBatch.DrawString(_medFont, Strings.Server_Requires_A_Password, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				}
			}
		}

		private CastleMinerZGame _game = CastleMinerZGame.Instance;

		private HostDiscovery _hostDiscovery;

		private SortBy _currentSort = SortBy.NumFriendsDesc;

		private FrameButtonControl _dateButton = new FrameButtonControl();

		private FrameButtonControl _numPLayersButton = new FrameButtonControl();

		private FrameButtonControl _modeButton = new FrameButtonControl();

		private FrameButtonControl _nameButton = new FrameButtonControl();

		private FrameButtonControl _MaxPlayersButton = new FrameButtonControl();

		private FrameButtonControl _numberFriendsButton = new FrameButtonControl();

		private FrameButtonControl _refreshButton = new FrameButtonControl();

		private PCKeyboardInputScreen _serverPasswordScreen;

		private Rectangle prevScreenRect;

		public ChooseOnlineGameScreen()
			: base(false, new Size(692, 60), new Rectangle(50, 200, 1180, 500))
		{
			ClickSound = "Click";
			_serverPasswordScreen = new PCKeyboardInputScreen(_game, Strings.Server_Password, Strings.Enter_a_password_for_this_server + ": ", _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_serverPasswordScreen.ClickSound = "Click";
			_serverPasswordScreen.OpenSound = "Popup";
			Color color = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			base.SelectButton = new FrameButtonControl
			{
				LocalPosition = new Point(900, 125),
				Size = new Size(300, 40),
				Text = Strings.Join_Game,
				Font = _game._medFont,
				Frame = _game.ButtonFrame,
				ButtonColor = color
			};
			_refreshButton = new FrameButtonControl();
			_refreshButton.LocalPosition = new Point(900, 170);
			_refreshButton.Size = new Size(300, 40);
			_refreshButton.Text = Strings.Search_Again;
			_refreshButton.Font = _game._medFont;
			_refreshButton.Frame = _game.ButtonFrame;
			_refreshButton.Pressed += _refreshButton_Pressed;
			_refreshButton.ButtonColor = color;
			base.Controls.Add(_refreshButton);
			base.BackButton = new ImageButtonControl
			{
				Image = _game._uiSprites["BackArrow"],
				Font = _game._medFont,
				LocalPosition = new Point(15, 15),
				Text = " " + Strings.Back,
				ImageDefaultColor = color
			};
			int num = 237;
			Point localPosition = new Point(40, 100);
			_nameButton.LocalPosition = localPosition;
			_nameButton.Size = new Size(num - 1, 18);
			_nameButton.Text = "SERVER NAME";
			_nameButton.Font = _game._smallFont;
			_nameButton.TextAlignment = FrameButtonControl.Alignment.Left;
			_nameButton.Frame = _game.ButtonFrame;
			_nameButton.Pressed += _nameButton_Pressed;
			base.Controls.Add(_nameButton);
			localPosition.X += num;
			_dateButton.LocalPosition = localPosition;
			_dateButton.Size = new Size(149, 18);
			_dateButton.Text = "DATE \u02c5";
			_dateButton.Font = _game._smallFont;
			_dateButton.TextAlignment = FrameButtonControl.Alignment.Left;
			_dateButton.Frame = _game.ButtonFrame;
			_dateButton.Pressed += _dateButton_Pressed;
			base.Controls.Add(_dateButton);
			localPosition.X += 150;
			_numPLayersButton.LocalPosition = localPosition;
			_numPLayersButton.Size = new Size(74, 18);
			_numPLayersButton.Text = "PLAYERS";
			_numPLayersButton.Font = _game._smallFont;
			_numPLayersButton.TextAlignment = FrameButtonControl.Alignment.Left;
			_numPLayersButton.Frame = _game.ButtonFrame;
			_numPLayersButton.Pressed += _numPLayersButton_Pressed;
			base.Controls.Add(_numPLayersButton);
			localPosition.X += 75;
			_MaxPlayersButton.LocalPosition = localPosition;
			_MaxPlayersButton.Size = new Size(54, 18);
			_MaxPlayersButton.Text = "MAX";
			_MaxPlayersButton.Font = _game._smallFont;
			_MaxPlayersButton.TextAlignment = FrameButtonControl.Alignment.Left;
			_MaxPlayersButton.Frame = _game.ButtonFrame;
			_MaxPlayersButton.Pressed += _MaxPlayersButton_Pressed;
			base.Controls.Add(_MaxPlayersButton);
			localPosition.X += 55;
			_modeButton.LocalPosition = localPosition;
			_modeButton.Size = new Size(105, 18);
			_modeButton.Text = "GAME MODE";
			_modeButton.Font = _game._smallFont;
			_modeButton.TextAlignment = FrameButtonControl.Alignment.Left;
			_modeButton.Frame = _game.ButtonFrame;
			_modeButton.Pressed += _modeButton_Pressed;
			base.Controls.Add(_modeButton);
			localPosition.X += 106;
			_numberFriendsButton.LocalPosition = localPosition;
			_numberFriendsButton.Size = new Size(70, 18);
			_numberFriendsButton.Text = "FRIENDS";
			_numberFriendsButton.Font = _game._smallFont;
			_numberFriendsButton.TextAlignment = FrameButtonControl.Alignment.Left;
			_numberFriendsButton.Frame = _game.ButtonFrame;
			_numberFriendsButton.Pressed += _numberFriendsButton_Pressed;
			base.Controls.Add(_numberFriendsButton);
		}

		private void _refreshButton_Pressed(object sender, EventArgs e)
		{
			_game.GetNetworkSessions(delegate(AvailableNetworkSessionCollection result)
			{
				Populate(result);
			});
		}

		private void _numberFriendsButton_Pressed(object sender, EventArgs e)
		{
			_sortByFriendCount(_currentSort, Items);
			_updateControlsOnSort();
		}

		public void ShutdownHostDiscovery()
		{
			if (_hostDiscovery != null)
			{
				_hostDiscovery.Shutdown();
			}
			_hostDiscovery = null;
		}

		public override void OnPoped()
		{
			ShutdownHostDiscovery();
			base.OnPoped();
		}

		public override void OnPushed()
		{
			_currentSort = SortBy.NumFriendsDesc;
			_hostDiscovery = NetworkSession.GetHostDiscoveryObject("CastleMinerZSteam", 3, DNAGame.GetLocalID());
			base.OnPushed();
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (_hostDiscovery != null)
			{
				_hostDiscovery.Update();
			}
			for (int i = 0; i < Items.Count; i++)
			{
				OnlineGameMenuItem onlineGameMenuItem = Items[i] as OnlineGameMenuItem;
				if (onlineGameMenuItem != null)
				{
					onlineGameMenuItem.UpdateServerInfo(_hostDiscovery);
				}
			}
			if (Screen.Adjuster.ScreenRect != prevScreenRect)
			{
				prevScreenRect = Screen.Adjuster.ScreenRect;
				int num = (int)(540f * Screen.Adjuster.ScaleFactor.X);
				_selectButton.Scale = (_refreshButton.Scale = (_MaxPlayersButton.Scale = (_numPLayersButton.Scale = (_nameButton.Scale = (_dateButton.Scale = (_modeButton.Scale = (_numberFriendsButton.Scale = Screen.Adjuster.ScaleFactor.X)))))));
				Point localPosition = new Point(40, 100);
				_nameButton.LocalPosition = localPosition;
				localPosition.X += _nameButton.Size.Width + 1;
				_dateButton.LocalPosition = localPosition;
				localPosition.X += _dateButton.Size.Width + 1;
				_numPLayersButton.LocalPosition = localPosition;
				localPosition.X += _numPLayersButton.Size.Width + 1;
				_MaxPlayersButton.LocalPosition = localPosition;
				localPosition.X += _MaxPlayersButton.Size.Width + 1;
				_modeButton.LocalPosition = localPosition;
				localPosition.X += _modeButton.Size.Width + 1;
				_numberFriendsButton.LocalPosition = localPosition;
				int x = (int)(740f * Screen.Adjuster.ScaleFactor.X) + num / 2 - _selectButton.Size.Width / 2;
				int num2 = _selectButton.Size.Height + (int)(5f * Screen.Adjuster.ScaleFactor.X);
				_selectButton.LocalPosition = new Point(x, _selectButton.LocalPosition.Y);
				_refreshButton.LocalPosition = new Point(x, _selectButton.LocalPosition.Y + num2);
				_itemSize.Width = (int)(700f * Screen.Adjuster.ScaleFactor.X);
				_itemSize.Height = (int)(60f * Screen.Adjuster.ScaleFactor.X);
				for (int j = 0; j < Items.Count; j++)
				{
					Items[j].Size = _itemSize;
				}
				int num3 = _nameButton.LocalPosition.Y + _nameButton.Size.Height + (int)(5f * Screen.Adjuster.ScaleFactor.X);
				_drawArea = new Rectangle((int)(10f * Screen.Adjuster.ScaleFactor.X), num3, (int)((float)Screen.Adjuster.ScreenRect.Width - 10f * Screen.Adjuster.ScaleFactor.X), Screen.Adjuster.ScreenRect.Height - num3);
				_updateControlsOnSort();
			}
			base.OnUpdate(game, gameTime);
		}

		public override void Click()
		{
			OnlineGameMenuItem item = (OnlineGameMenuItem)base.SelectedItem;
			if (item.NetworkSession.PasswordProtected)
			{
				_serverPasswordScreen.DefaultText = item.Password;
				_game.FrontEnd.ShowPCDialogScreen(_serverPasswordScreen, delegate
				{
					if (_serverPasswordScreen.OptionSelected != -1)
					{
						string textInput = _serverPasswordScreen.TextInput;
						if (!string.IsNullOrWhiteSpace(textInput))
						{
							item.Password = textInput;
						}
						base.Click();
					}
				});
			}
			else
			{
				base.Click();
			}
		}

		public void Populate(AvailableNetworkSessionCollection sessions)
		{
			List<ListItemControl> list = new List<ListItemControl>();
			foreach (AvailableNetworkSession session in sessions)
			{
				if (session.HostGamertag != Screen.CurrentGamer.Gamertag)
				{
					list.Add(new OnlineGameMenuItem(session, _itemSize));
				}
			}
			switch (_currentSort)
			{
			case SortBy.NumPlayersAsc:
				_sortByNumPlayers(SortBy.NumPlayersDesc, list);
				break;
			case SortBy.NumPlayersDesc:
				_sortByNumPlayers(SortBy.NumPlayersAsc, list);
				break;
			case SortBy.DateAsc:
				_sortByDate(SortBy.DateDesc, list);
				break;
			case SortBy.DateDesc:
				_sortByDate(SortBy.DateAsc, list);
				break;
			case SortBy.MaxPlayersAsc:
				_sortByMaxPLayers(SortBy.MaxPlayersDesc, list);
				break;
			case SortBy.MaxPlayersDesc:
				_sortByMaxPLayers(SortBy.MaxPlayersAsc, list);
				break;
			case SortBy.ModeAsc:
				_sortByMode(SortBy.ModeDesc, list);
				break;
			case SortBy.ModeDesc:
				_sortByMode(SortBy.ModeAsc, list);
				break;
			case SortBy.NameAsc:
				_sortByName(SortBy.NameDesc, list);
				break;
			case SortBy.NameDesc:
				_sortByName(SortBy.NameAsc, list);
				break;
			case SortBy.NumFriendsAsc:
				_sortByFriendCount(SortBy.NumFriendsDesc, list);
				break;
			case SortBy.NumFriendsDesc:
				_sortByFriendCount(SortBy.NumFriendsAsc, list);
				break;
			}
			Items = list;
			if (Items.Count == 0)
			{
				FrameButtonControl modeButton = _modeButton;
				FrameButtonControl nameButton = _nameButton;
				FrameButtonControl dateButton = _dateButton;
				FrameButtonControl numPLayersButton = _numPLayersButton;
				FrameButtonControl maxPlayersButton = _MaxPlayersButton;
				bool flag2 = _numberFriendsButton.Visible = false;
				bool flag4 = maxPlayersButton.Visible = flag2;
				bool flag6 = numPLayersButton.Visible = flag4;
				bool flag8 = dateButton.Visible = flag6;
				bool visible = nameButton.Visible = flag8;
				modeButton.Visible = visible;
			}
			else
			{
				FrameButtonControl modeButton2 = _modeButton;
				FrameButtonControl nameButton2 = _nameButton;
				FrameButtonControl dateButton2 = _dateButton;
				FrameButtonControl numPLayersButton2 = _numPLayersButton;
				FrameButtonControl maxPlayersButton2 = _MaxPlayersButton;
				bool flag11 = _numberFriendsButton.Visible = true;
				bool flag13 = maxPlayersButton2.Visible = flag11;
				bool flag15 = numPLayersButton2.Visible = flag13;
				bool flag17 = dateButton2.Visible = flag15;
				bool visible2 = nameButton2.Visible = flag17;
				modeButton2.Visible = visible2;
			}
			UpdateAfterPopulate();
		}

		private void _resetSortButtonText()
		{
			_nameButton.Text = "SERVER NAME";
			_dateButton.Text = "DATE";
			_numPLayersButton.Text = "PLAYERS";
			_MaxPlayersButton.Text = "MAX";
			_modeButton.Text = "GAME MODE";
			_numberFriendsButton.Text = "FRIENDS";
		}

		private void _modeButton_Pressed(object sender, EventArgs e)
		{
			_sortByMode(_currentSort, Items);
			_updateControlsOnSort();
		}

		private void _sortByMode(SortBy sort, List<ListItemControl> items)
		{
			_resetSortButtonText();
			if (sort == SortBy.ModeAsc)
			{
				_modeButton.Text = "GAME MODE \u02c5";
				_currentSort = SortBy.ModeDesc;
				items.Sort(SortByGameModeDesc);
			}
			else
			{
				_modeButton.Text = "GAME MODE \u02c4";
				_currentSort = SortBy.ModeAsc;
				items.Sort(SortByGameModeAsc);
			}
		}

		private void _MaxPlayersButton_Pressed(object sender, EventArgs e)
		{
			_sortByMaxPLayers(_currentSort, Items);
			_updateControlsOnSort();
		}

		private void _sortByMaxPLayers(SortBy sort, List<ListItemControl> items)
		{
			_resetSortButtonText();
			if (sort == SortBy.MaxPlayersDesc)
			{
				_MaxPlayersButton.Text = "MAX \u02c4";
				_currentSort = SortBy.MaxPlayersAsc;
				items.Sort(SortByMaxAsc);
			}
			else
			{
				_MaxPlayersButton.Text = "MAX \u02c5";
				_currentSort = SortBy.MaxPlayersDesc;
				items.Sort(SortByMaxDesc);
			}
		}

		private void _numPLayersButton_Pressed(object sender, EventArgs e)
		{
			_sortByNumPlayers(_currentSort, Items);
			_updateControlsOnSort();
		}

		private void _sortByNumPlayers(SortBy sort, List<ListItemControl> items)
		{
			_resetSortButtonText();
			if (sort == SortBy.NumPlayersAsc)
			{
				_numPLayersButton.Text = "PLAYERS \u02c5";
				_currentSort = SortBy.NumPlayersDesc;
				items.Sort(SortByPlayersDesc);
			}
			else
			{
				_numPLayersButton.Text = "PLAYERS \u02c4";
				_currentSort = SortBy.NumPlayersAsc;
				items.Sort(SortByPlayersAsc);
			}
		}

		private void _sortByFriendCount(SortBy sort, List<ListItemControl> items)
		{
			_resetSortButtonText();
			if (sort == SortBy.NumFriendsAsc)
			{
				_numberFriendsButton.Text = "FRIENDS \u02c5";
				_currentSort = SortBy.NumFriendsDesc;
				items.Sort(SortByFriendCountDesc);
			}
			else
			{
				_numberFriendsButton.Text = "FRIENDS \u02c4";
				_currentSort = SortBy.NumFriendsAsc;
				items.Sort(SortByFriendCountAsc);
			}
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
				items.Sort(SortByDateAsc);
			}
			else
			{
				_dateButton.Text = "DATE \u02c5";
				_currentSort = SortBy.DateDesc;
				items.Sort(SortByDateDesc);
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
				items.Sort(SortByNameDesc);
			}
			else
			{
				_nameButton.Text = "SERVER NAME \u02c4";
				_currentSort = SortBy.NameAsc;
				items.Sort(SortByNameAsc);
			}
		}

		private static bool SortCheckForNulls(ListItemControl a, ListItemControl b, out int comp)
		{
			bool result = true;
			if (a == null)
			{
				if (b == null)
				{
					comp = 0;
				}
				else
				{
					comp = -1;
				}
			}
			else if (b == null)
			{
				comp = 1;
			}
			else
			{
				result = false;
				comp = 0;
			}
			return result;
		}

		private static int SortSubSort(OnlineGameMenuItem one, OnlineGameMenuItem two, int comp)
		{
			if (comp != 0)
			{
				return comp;
			}
			return Math.Sign(one.Proximity - two.Proximity);
		}

		private static int SortByNameAsc(ListItemControl a, ListItemControl b)
		{
			return SortByName(a, b, SortDirection.Ascending);
		}

		private static int SortByNameDesc(ListItemControl a, ListItemControl b)
		{
			return SortByName(a, b, SortDirection.Descending);
		}

		private static int SortByName(ListItemControl a, ListItemControl b, SortDirection direction)
		{
			int comp;
			if (!SortCheckForNulls(a, b, out comp))
			{
				OnlineGameMenuItem onlineGameMenuItem = (OnlineGameMenuItem)a;
				OnlineGameMenuItem onlineGameMenuItem2 = (OnlineGameMenuItem)b;
				comp = string.Compare(onlineGameMenuItem.NetworkSession.ServerMessage, onlineGameMenuItem2.NetworkSession.ServerMessage, true) * (int)direction;
				return SortSubSort(onlineGameMenuItem, onlineGameMenuItem2, comp);
			}
			return comp;
		}

		private static int SortByDateAsc(ListItemControl a, ListItemControl b)
		{
			return SortByDate(a, b, SortDirection.Ascending);
		}

		private static int SortByDateDesc(ListItemControl a, ListItemControl b)
		{
			return SortByDate(a, b, SortDirection.Descending);
		}

		private static int SortByDate(ListItemControl a, ListItemControl b, SortDirection direction)
		{
			int comp;
			if (!SortCheckForNulls(a, b, out comp))
			{
				OnlineGameMenuItem onlineGameMenuItem = (OnlineGameMenuItem)a;
				OnlineGameMenuItem onlineGameMenuItem2 = (OnlineGameMenuItem)b;
				comp = DateTime.Compare(onlineGameMenuItem.NetworkSession.DateCreated, onlineGameMenuItem2.NetworkSession.DateCreated) * (int)direction;
				return SortSubSort(onlineGameMenuItem, onlineGameMenuItem2, comp);
			}
			return comp;
		}

		private static int SortByFriendCountAsc(ListItemControl a, ListItemControl b)
		{
			return SortByFriendCount(a, b, SortDirection.Ascending);
		}

		private static int SortByFriendCountDesc(ListItemControl a, ListItemControl b)
		{
			return SortByFriendCount(a, b, SortDirection.Descending);
		}

		private static int SortByFriendCount(ListItemControl a, ListItemControl b, SortDirection direction)
		{
			int comp;
			if (!SortCheckForNulls(a, b, out comp))
			{
				OnlineGameMenuItem onlineGameMenuItem = (OnlineGameMenuItem)a;
				OnlineGameMenuItem onlineGameMenuItem2 = (OnlineGameMenuItem)b;
				comp = Math.Sign(onlineGameMenuItem.NumFriends - onlineGameMenuItem2.NumFriends) * (int)direction;
				return SortSubSort(onlineGameMenuItem, onlineGameMenuItem2, comp);
			}
			return comp;
		}

		private static int SortByPlayersAsc(ListItemControl a, ListItemControl b)
		{
			return SortByPlayers(a, b, SortDirection.Ascending);
		}

		private static int SortByPlayersDesc(ListItemControl a, ListItemControl b)
		{
			return SortByPlayers(a, b, SortDirection.Descending);
		}

		private static int SortByPlayers(ListItemControl a, ListItemControl b, SortDirection direction)
		{
			int comp;
			if (!SortCheckForNulls(a, b, out comp))
			{
				OnlineGameMenuItem onlineGameMenuItem = (OnlineGameMenuItem)a;
				OnlineGameMenuItem onlineGameMenuItem2 = (OnlineGameMenuItem)b;
				comp = Math.Sign(onlineGameMenuItem.NetworkSession.CurrentGamerCount - onlineGameMenuItem2.NetworkSession.CurrentGamerCount) * (int)direction;
				return SortSubSort(onlineGameMenuItem, onlineGameMenuItem2, comp);
			}
			return comp;
		}

		private static int SortByMaxAsc(ListItemControl a, ListItemControl b)
		{
			return SortByMax(a, b, SortDirection.Ascending);
		}

		private static int SortByMaxDesc(ListItemControl a, ListItemControl b)
		{
			return SortByMax(a, b, SortDirection.Descending);
		}

		private static int SortByMax(ListItemControl a, ListItemControl b, SortDirection direction)
		{
			int comp;
			if (!SortCheckForNulls(a, b, out comp))
			{
				OnlineGameMenuItem onlineGameMenuItem = (OnlineGameMenuItem)a;
				OnlineGameMenuItem onlineGameMenuItem2 = (OnlineGameMenuItem)b;
				comp = Math.Sign(onlineGameMenuItem.NetworkSession.MaxGamerCount - onlineGameMenuItem2.NetworkSession.MaxGamerCount) * (int)direction;
				return SortSubSort(onlineGameMenuItem, onlineGameMenuItem2, comp);
			}
			return comp;
		}

		private static int SortByGameModeAsc(ListItemControl a, ListItemControl b)
		{
			return SortByGameMode(a, b, SortDirection.Ascending);
		}

		private static int SortByGameModeDesc(ListItemControl a, ListItemControl b)
		{
			return SortByGameMode(a, b, SortDirection.Descending);
		}

		private static int SortByGameMode(ListItemControl a, ListItemControl b, SortDirection direction)
		{
			int comp;
			if (!SortCheckForNulls(a, b, out comp))
			{
				OnlineGameMenuItem onlineGameMenuItem = (OnlineGameMenuItem)a;
				OnlineGameMenuItem onlineGameMenuItem2 = (OnlineGameMenuItem)b;
				comp = string.Compare(onlineGameMenuItem.GameModeString, onlineGameMenuItem2.GameModeString, true) * (int)direction;
				return SortSubSort(onlineGameMenuItem, onlineGameMenuItem2, comp);
			}
			return comp;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			SpriteFont largeFont = _game._largeFont;
			spriteBatch.Begin();
			if (Items.Count == 0)
			{
				string no_Servers_Available = Strings.No_Servers_Available;
				Vector2 vector = largeFont.MeasureString(no_Servers_Available);
				int lineSpacing = largeFont.LineSpacing;
				spriteBatch.DrawOutlinedText(largeFont, no_Servers_Available, new Vector2((float)Screen.Adjuster.ScreenRect.Center.X - vector.X / 2f, (float)Screen.Adjuster.ScreenRect.Center.Y - vector.Y / 2f), CMZColors.MenuGreen, Color.Black, 1);
			}
			else
			{
				string no_Servers_Available = Strings.Choose_A_Server;
				Vector2 vector = largeFont.MeasureString(no_Servers_Available);
				spriteBatch.DrawOutlinedText(largeFont, no_Servers_Available, new Vector2((float)Screen.Adjuster.ScreenRect.Center.X - vector.X / 2f, 10f), CMZColors.MenuGreen, Color.Black, 1);
				OnlineGameMenuItem onlineGameMenuItem = (OnlineGameMenuItem)base.SelectedItem;
				onlineGameMenuItem.DrawSelected(spriteBatch, new Vector2(_refreshButton.LocalPosition.X, _refreshButton.LocalPosition.Y + _refreshButton.Size.Height + 5));
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}
	}
}
