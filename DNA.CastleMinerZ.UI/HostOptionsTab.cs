using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Net;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.UI
{
	public class HostOptionsTab : TabControl.TabPage
	{
		private struct PlayerItem
		{
			public Player Player;

			public PlayerItem(Player player)
			{
				Player = player;
			}

			public override string ToString()
			{
				if (Player == null)
				{
					return Strings.Choose_a_Player;
				}
				return Player.Gamer.Gamertag;
			}
		}

		private DropListControl<PlayerItem> _playerDropList = new DropListControl<PlayerItem>();

		private FrameButtonControl _kickPlayerButton = new FrameButtonControl();

		private FrameButtonControl _banPlayerButton = new FrameButtonControl();

		private FrameButtonControl _clearBanListButton = new FrameButtonControl();

		private FrameButtonControl _restartButton = new FrameButtonControl();

		private TextControl _pvpLabel;

		private DropListControl<string> _pvpDropList = new DropListControl<string>();

		private TextControl _passwordLabel;

		private TextEditControl _passwordTextbox = new TextEditControl();

		private TextControl _serverMessageLabel;

		private TextEditControl _serverMessageTextbox = new TextEditControl();

		private TextControl _whoCanJoinLabel;

		private DropListControl<string> _whoCanJoinDropList = new DropListControl<string>();

		private PCDialogScreen _restartDialog;

		private bool _inGame;

		private CastleMinerZGame _game;

		private SpriteFont _controlsFont;

		private Rectangle prevScreenSize;

		public HostOptionsTab(bool inGame)
			: base(Strings.Host_Options)
		{
			_inGame = inGame;
			_game = CastleMinerZGame.Instance;
			_controlsFont = _game._medFont;
			Color buttonColor = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			_restartDialog = new PCDialogScreen(Strings.Restart_Game, Strings.Are_you_sure_you_want_to_restart_this_game_, null, true, _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_restartDialog.UseDefaultValues();
			_playerDropList.Frame = _game.ButtonFrame;
			_playerDropList.DropArrow = _game._uiSprites["DropArrow"];
			_playerDropList.Font = _controlsFont;
			base.Children.Add(_playerDropList);
			_kickPlayerButton.Size = new Size(200, _controlsFont.LineSpacing);
			_kickPlayerButton.Text = Strings.Kick_Player;
			_kickPlayerButton.Font = _controlsFont;
			_kickPlayerButton.Frame = _game.ButtonFrame;
			_kickPlayerButton.Pressed += _kickPlayerButton_Pressed;
			base.Children.Add(_kickPlayerButton);
			_banPlayerButton.Size = new Size(200, _controlsFont.LineSpacing);
			_banPlayerButton.Text = Strings.Ban_Player;
			_banPlayerButton.Font = _controlsFont;
			_banPlayerButton.Frame = _game.ButtonFrame;
			_banPlayerButton.Pressed += _banPlayerButton_Pressed;
			base.Children.Add(_banPlayerButton);
			_clearBanListButton.Size = new Size(200, _controlsFont.LineSpacing);
			_clearBanListButton.Text = Strings.Clear_Ban_List;
			_clearBanListButton.Font = _controlsFont;
			_clearBanListButton.Frame = _game.ButtonFrame;
			_clearBanListButton.Pressed += _clearBanListButton_Pressed;
			base.Children.Add(_clearBanListButton);
			_restartButton.Size = new Size(135, _controlsFont.LineSpacing);
			_restartButton.Text = Strings.Restart;
			_restartButton.Font = _controlsFont;
			_restartButton.Frame = _game.ButtonFrame;
			_restartButton.Pressed += _restartButton_Pressed;
			_restartButton.ButtonColor = buttonColor;
			base.Children.Add(_restartButton);
			_pvpLabel = new TextControl("PVP:", _controlsFont);
			base.Children.Add(_pvpLabel);
			_pvpDropList.Items.Add(Strings.Off);
			_pvpDropList.Items.Add(Strings.On);
			_pvpDropList.Items.Add(Strings.Non_Friends_Only);
			_pvpDropList.Frame = _game.ButtonFrame;
			_pvpDropList.DropArrow = _game._uiSprites["DropArrow"];
			_pvpDropList.Font = _controlsFont;
			_pvpDropList.SelectedIndexChanged += _pvpStr_SelectedIndexChanged;
			_passwordLabel = new TextControl(Strings.Password + ":", _controlsFont);
			base.Children.Add(_passwordLabel);
			_passwordTextbox.Frame = _game.ButtonFrame;
			_passwordTextbox.Font = _controlsFont;
			_passwordTextbox.Size = new Size(200, 40);
			base.Children.Add(_passwordTextbox);
			_serverMessageLabel = new TextControl(Strings.Server_Message + ":", _controlsFont);
			base.Children.Add(_serverMessageLabel);
			_serverMessageTextbox.Frame = _game.ButtonFrame;
			_serverMessageTextbox.Font = _controlsFont;
			_serverMessageTextbox.Size = new Size(200, 40);
			base.Children.Add(_serverMessageTextbox);
			_whoCanJoinLabel = new TextControl(Strings.Who_can_join, _controlsFont);
			base.Children.Add(_whoCanJoinLabel);
			_whoCanJoinDropList.Items.Add(Strings.Everyone);
			_whoCanJoinDropList.Items.Add(Strings.Friends_only);
			_whoCanJoinDropList.Items.Add(Strings.Invitation_only);
			_whoCanJoinDropList.Frame = _game.ButtonFrame;
			_whoCanJoinDropList.DropArrow = _game._uiSprites["DropArrow"];
			_whoCanJoinDropList.Font = _controlsFont;
			_whoCanJoinDropList.SelectedIndexChanged += _whoCanJoinDropList_SelectedIndexChanged;
			base.Children.Add(_whoCanJoinDropList);
			base.Children.Add(_pvpDropList);
		}

		public override void OnSelected()
		{
			_pvpDropList.SelectedIndex = (int)_game.PVPState;
			_whoCanJoinDropList.SelectedIndex = (int)_game.JoinGamePolicy;
			_passwordTextbox.Text = _game.CurrentWorld.ServerPassword;
			_serverMessageTextbox.Text = _game.ServerMessage;
			_playerDropList.Items.Clear();
			_playerDropList.Items.Add(new PlayerItem(null));
			for (int i = 0; i < _game.CurrentNetworkSession.RemoteGamers.Count; i++)
			{
				NetworkGamer networkGamer = _game.CurrentNetworkSession.RemoteGamers[i];
				if (networkGamer.Tag != null)
				{
					Player player = (Player)networkGamer.Tag;
					_playerDropList.Items.Add(new PlayerItem(player));
				}
			}
			base.OnSelected();
		}

		private void _setPassword(string password)
		{
			if (string.IsNullOrWhiteSpace(password))
			{
				if (!string.IsNullOrWhiteSpace(_game.CurrentWorld.ServerPassword))
				{
					if (_game.IsOnlineGame)
					{
						_game.CurrentNetworkSession.UpdateHostSession(null, false, null, null);
					}
					_game.CurrentWorld.ServerPassword = "";
					_game.CurrentNetworkSession.Password = null;
				}
			}
			else if (_game.CurrentWorld.ServerPassword != _passwordTextbox.Text)
			{
				if (_game.IsOnlineGame)
				{
					_game.CurrentNetworkSession.UpdateHostSession(null, true, null, null);
				}
				_game.CurrentWorld.ServerPassword = password;
				_game.CurrentNetworkSession.Password = password;
			}
		}

		private void _setServerMessage(string name)
		{
			if (!string.IsNullOrWhiteSpace(name) && _game.ServerMessage != name.Trim())
			{
				_game.ServerMessage = name.Trim();
			}
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (base.SelectedTab)
			{
				FrameButtonControl kickPlayerButton = _kickPlayerButton;
				bool enabled = _banPlayerButton.Enabled = (_playerDropList.SelectedIndex > 0);
				kickPlayerButton.Enabled = enabled;
				_clearBanListButton.Enabled = (_game.PlayerStats.BanList.Count > 0);
				if (!_passwordTextbox.HasFocus)
				{
					_setPassword(_passwordTextbox.Text);
				}
				if (!_serverMessageTextbox.HasFocus)
				{
					_setServerMessage(_serverMessageTextbox.Text);
				}
			}
			if (prevScreenSize != Screen.Adjuster.ScreenRect)
			{
				prevScreenSize = Screen.Adjuster.ScreenRect;
				_playerDropList.Scale = (_kickPlayerButton.Scale = (_banPlayerButton.Scale = (_clearBanListButton.Scale = (_restartButton.Scale = (_pvpLabel.Scale = (_pvpDropList.Scale = (_passwordLabel.Scale = (_passwordTextbox.Scale = (_serverMessageLabel.Scale = (_serverMessageTextbox.Scale = (_whoCanJoinLabel.Scale = (_whoCanJoinDropList.Scale = Screen.Adjuster.ScaleFactor.Y))))))))))));
				int num = (int)(50f * Screen.Adjuster.ScaleFactor.Y);
				Point localPosition = new Point(0, (int)(75f * Screen.Adjuster.ScaleFactor.Y));
				int num2 = (int)(210f * Screen.Adjuster.ScaleFactor.Y);
				_restartButton.LocalPosition = new Point((int)(140f * Screen.Adjuster.ScaleFactor.Y), Size.Height - (int)(40f * Screen.Adjuster.ScaleFactor.Y));
				_pvpLabel.LocalPosition = localPosition;
				_pvpDropList.LocalPosition = new Point(localPosition.X + num2, localPosition.Y);
				localPosition.Y += num;
				_serverMessageLabel.LocalPosition = localPosition;
				_serverMessageTextbox.LocalPosition = new Point(localPosition.X + num2, localPosition.Y);
				localPosition.Y += num;
				_passwordLabel.LocalPosition = localPosition;
				_passwordTextbox.LocalPosition = new Point(localPosition.X + num2, localPosition.Y);
				localPosition.Y += num;
				_whoCanJoinLabel.LocalPosition = localPosition;
				_whoCanJoinDropList.LocalPosition = new Point(localPosition.X + num2, localPosition.Y);
				localPosition.Y += num;
				_playerDropList.LocalPosition = localPosition;
				_kickPlayerButton.LocalPosition = new Point(localPosition.X + num2, localPosition.Y);
				_banPlayerButton.LocalPosition = new Point(localPosition.X + num2 * 2, localPosition.Y);
				_clearBanListButton.LocalPosition = new Point(localPosition.X + num2 * 3, localPosition.Y);
			}
			base.OnUpdate(game, gameTime);
		}

		public override void OnLostFocus()
		{
			_setPassword(_passwordTextbox.Text);
			_setServerMessage(_serverMessageTextbox.Text);
			base.OnLostFocus();
		}

		private void _whoCanJoinDropList_SelectedIndexChanged(object sender, EventArgs e)
		{
			_game.JoinGamePolicy = (JoinGamePolicy)_whoCanJoinDropList.SelectedIndex;
		}

		private void _pvpStr_SelectedIndexChanged(object sender, EventArgs e)
		{
			_game.PVPState = (CastleMinerZGame.PVPEnum)_pvpDropList.SelectedIndex;
			string message = "";
			switch (_game.PVPState)
			{
			case CastleMinerZGame.PVPEnum.Everyone:
				message = "PVP: " + Strings.Everyone;
				break;
			case CastleMinerZGame.PVPEnum.NotFriends:
				message = "PVP: " + Strings.Non_Friends_Only;
				break;
			case CastleMinerZGame.PVPEnum.Off:
				message = "PVP: " + Strings.Off;
				break;
			}
			BroadcastTextMessage.Send(_game.MyNetworkGamer, message);
		}

		private void _restartButton_Pressed(object sender, EventArgs e)
		{
			_game.GameScreen._uiGroup.ShowPCDialogScreen(_restartDialog, delegate
			{
				if (_restartDialog.OptionSelected != -1)
				{
					RestartLevelMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer);
					BroadcastTextMessage.Send(_game.MyNetworkGamer, _game.LocalPlayer.Gamer.Gamertag + " " + Strings.Has_Restarted_The_Game);
					_game.GameScreen.PopToHUD();
				}
			});
		}

		private void _clearBanListButton_Pressed(object sender, EventArgs e)
		{
			_game.PlayerStats.BanList.Clear();
		}

		private void _banPlayerButton_Pressed(object sender, EventArgs e)
		{
			BroadcastTextMessage.Send(_game.MyNetworkGamer, _playerDropList.SelectedItem.Player.Gamer.Gamertag + " " + Strings.has_been_banned_by_the_host);
			KickMessage.Send(_game.MyNetworkGamer, _playerDropList.SelectedItem.Player.Gamer, true);
			_game.PlayerStats.BanList[_playerDropList.SelectedItem.Player.Gamer.AlternateAddress] = DateTime.UtcNow;
			_game.SaveData();
		}

		private void _kickPlayerButton_Pressed(object sender, EventArgs e)
		{
			BroadcastTextMessage.Send(_game.MyNetworkGamer, _playerDropList.SelectedItem.Player.Gamer.Gamertag + " " + Strings.has_been_kicked_by_the_host);
			KickMessage.Send(_game.MyNetworkGamer, _playerDropList.SelectedItem.Player.Gamer, false);
		}
	}
}
