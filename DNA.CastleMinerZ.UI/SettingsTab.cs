using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.UI
{
	public class SettingsTab : TabControl.TabPage
	{
		private TrackBarControl _musicVolumeTrack = new TrackBarControl();

		private TextControl _musicVolumeLabel;

		private CheckBoxControl _musicMute = new CheckBoxControl();

		private CheckBoxControl _autoClimb = new CheckBoxControl();

		private CheckBoxControl _fadeInactiveTray = new CheckBoxControl();

		private FrameButtonControl _eraseSaves = new FrameButtonControl();

		private PCDialogScreen _deleteStorageDialog;

		private CastleMinerZGame _game;

		private SpriteFont _controlsFont;

		private bool _inGame;

		private ScreenGroup _uiGroup;

		private Rectangle prevScreenSize;

		public SettingsTab(bool inGame, ScreenGroup uiGroup)
			: base(Strings.Controls)
		{
			_game = CastleMinerZGame.Instance;
			_controlsFont = _game._medFont;
			_inGame = inGame;
			_uiGroup = uiGroup;
			Color buttonColor = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			_deleteStorageDialog = new PCDialogScreen(Strings.Erase_Storage, Strings.Are_you_sure_you_want_to_delete_everything_, null, true, _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_deleteStorageDialog.UseDefaultValues();
			if (!_inGame)
			{
				_eraseSaves.Size = new Size(225, _controlsFont.LineSpacing);
				_eraseSaves.Text = Strings.Erase_Storage;
				_eraseSaves.Font = _controlsFont;
				_eraseSaves.Frame = _game.ButtonFrame;
				_eraseSaves.Pressed += _eraseSaves_Pressed;
				_eraseSaves.ButtonColor = buttonColor;
				base.Children.Add(_eraseSaves);
			}
			_autoClimb.Font = _controlsFont;
			_autoClimb.Text = Strings.Auto_Climb;
			_autoClimb.CheckedImage = _game._uiSprites["Checked"];
			_autoClimb.UncheckedImage = _game._uiSprites["Unchecked"];
			_autoClimb.TextColor = Color.White;
			base.Children.Add(_autoClimb);
			_fadeInactiveTray.Font = _controlsFont;
			_fadeInactiveTray.Text = Strings.Fade_Inactive_Tray;
			_fadeInactiveTray.CheckedImage = _game._uiSprites["Checked"];
			_fadeInactiveTray.UncheckedImage = _game._uiSprites["Unchecked"];
			_fadeInactiveTray.TextColor = Color.White;
			base.Children.Add(_fadeInactiveTray);
			_musicMute.Font = _controlsFont;
			_musicMute.Text = Strings.Mute + ":";
			_musicMute.CheckedImage = _game._uiSprites["Checked"];
			_musicMute.UncheckedImage = _game._uiSprites["Unchecked"];
			_musicMute.TextColor = Color.White;
			_musicMute.TextOnRight = false;
			base.Children.Add(_musicMute);
			_musicVolumeLabel = new TextControl(Strings.Music_Volume, _controlsFont);
			_musicVolumeTrack.MinValue = 0;
			_musicVolumeTrack.MaxValue = 100;
			_musicVolumeTrack.FillColor = CMZColors.MenuGreen;
			base.Children.Add(_musicVolumeLabel);
			base.Children.Add(_musicVolumeTrack);
		}

		private void _eraseSaves_Pressed(object sender, EventArgs e)
		{
			_uiGroup.ShowPCDialogScreen(_deleteStorageDialog, delegate
			{
				if (_deleteStorageDialog.OptionSelected != -1)
				{
					WaitScreen.DoWait(_uiGroup, Strings.Deleting_Storage___, delegate
					{
						_game.SaveDevice.DeleteStorage();
					}, null);
					_game.FrontEnd.PopToStartScreen();
				}
			});
		}

		public override void OnSelected()
		{
			_musicVolumeTrack.Value = (int)(_game.PlayerStats.musicVolume * 100f);
			_musicMute.Checked = _game.PlayerStats.musicMute;
			_autoClimb.Checked = _game.PlayerStats.AutoClimb;
			base.OnSelected();
		}

		public override void OnLostFocus()
		{
			try
			{
				_game.SavePlayerStats(_game.PlayerStats);
				CastleMinerZGame.GlobalSettings.Save();
			}
			catch
			{
			}
			base.OnLostFocus();
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (base.SelectedTab)
			{
				_game.PlayerStats.musicMute = _musicMute.Checked;
				_game.PlayerStats.musicVolume = (float)_musicVolumeTrack.Value / 100f;
				if (_musicMute.Checked)
				{
					_game.MusicSounds.SetVolume(0f);
				}
				else
				{
					_game.MusicSounds.SetVolume((float)_musicVolumeTrack.Value / 100f);
				}
				_game.PlayerStats.AutoClimb = _autoClimb.Checked;
				_game.PlayerStats.SecondTrayFaded = _fadeInactiveTray.Checked;
			}
			if (prevScreenSize != Screen.Adjuster.ScreenRect)
			{
				prevScreenSize = Screen.Adjuster.ScreenRect;
				_musicVolumeLabel.Scale = (_musicMute.Scale = (_autoClimb.Scale = (_fadeInactiveTray.Scale = Screen.Adjuster.ScaleFactor.Y)));
				if (!_inGame)
				{
					_eraseSaves.Scale = Screen.Adjuster.ScaleFactor.Y;
				}
				int num = (int)(50f * Screen.Adjuster.ScaleFactor.Y);
				Point localPosition = new Point(0, (int)(75f * Screen.Adjuster.ScaleFactor.Y));
				int num2 = (int)(200f * Screen.Adjuster.ScaleFactor.Y);
				_musicVolumeLabel.LocalPosition = localPosition;
				_musicVolumeTrack.LocalPosition = new Point(localPosition.X + num2, localPosition.Y + (int)(10f * Screen.Adjuster.ScaleFactor.Y));
				_musicVolumeTrack.Size = new Size((int)(185f * Screen.Adjuster.ScaleFactor.Y), num);
				_musicMute.LocalPosition = new Point(localPosition.X + num2 * 2, localPosition.Y + (int)(5f * Screen.Adjuster.ScaleFactor.Y));
				localPosition.Y += num;
				_autoClimb.LocalPosition = localPosition;
				localPosition.Y += num;
				_fadeInactiveTray.LocalPosition = localPosition;
				if (!_inGame)
				{
					_eraseSaves.LocalPosition = new Point((int)(140f * Screen.Adjuster.ScaleFactor.Y), Size.Height - (int)(40f * Screen.Adjuster.ScaleFactor.Y));
				}
			}
			base.OnUpdate(game, gameTime);
		}
	}
}
