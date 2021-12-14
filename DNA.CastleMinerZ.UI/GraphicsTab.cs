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
	public class GraphicsTab : TabControl.TabPage
	{
		private struct Resolution
		{
			public Size ScreenSize;

			public override string ToString()
			{
				return ScreenSize.Width + "x" + ScreenSize.Height;
			}

			public Resolution(Size size)
			{
				ScreenSize = size;
			}
		}

		private static Dictionary<Size, bool> _validResolutions;

		private TrackBarControl _brightnessBar = new TrackBarControl();

		private TextControl _brightnessLabel;

		private DropListControl<string> _viewDistanceDropList = new DropListControl<string>();

		private TextControl _viewDistanceLabel;

		private DropListControl<Resolution> _resolutionDropList = new DropListControl<Resolution>();

		private TextControl _resolutionLabel;

		private CheckBoxControl _fullScreen;

		private DropListControl<string> _textureQualityDropList = new DropListControl<string>();

		private TextControl _textureQualityLabel;

		private TrackBarControl _musicVolumeTrack = new TrackBarControl();

		private TextControl _musicVolumeLabel;

		private CheckBoxControl _musicMute = new CheckBoxControl();

		private CheckBoxControl _autoClimb = new CheckBoxControl();

		private CheckBoxControl _fadeInactiveTray = new CheckBoxControl();

		private bool _inGame;

		private ScreenGroup _uiGroup;

		private CastleMinerZGame _game;

		private SpriteFont _controlsFont;

		private PCDialogScreen _restartDialog;

		private static Size[] _screenSizes;

		private bool _ignoreTextureQualityChange;

		private Rectangle prevScreenSize;

		static GraphicsTab()
		{
			_validResolutions = new Dictionary<Size, bool>();
			_screenSizes = new Size[10]
			{
				new Size(1920, 1200),
				new Size(1920, 1080),
				new Size(1680, 1050),
				new Size(1600, 900),
				new Size(1440, 900),
				new Size(1366, 768),
				new Size(1280, 800),
				new Size(1280, 720),
				new Size(1024, 768),
				new Size(800, 600)
			};
			foreach (DisplayMode supportedDisplayMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
			{
				_validResolutions[new Size(supportedDisplayMode.Width, supportedDisplayMode.Height)] = true;
			}
		}

		public GraphicsTab(bool inGame, ScreenGroup uiGroup)
			: base(Strings.Settings)
		{
			_inGame = inGame;
			_uiGroup = uiGroup;
			_game = CastleMinerZGame.Instance;
			_controlsFont = _game._medFont;
			new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
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
			_brightnessBar.MinValue = 0;
			_brightnessBar.MaxValue = 100;
			_brightnessBar.FillColor = CMZColors.MenuGreen;
			base.Children.Add(_brightnessBar);
			_brightnessLabel = new TextControl(Strings.Brightness, _controlsFont);
			base.Children.Add(_brightnessLabel);
			_viewDistanceDropList.Items.Add(Strings.Lowest);
			_viewDistanceDropList.Items.Add(Strings.Low);
			_viewDistanceDropList.Items.Add(Strings.Medium);
			_viewDistanceDropList.Items.Add(Strings.High);
			_viewDistanceDropList.Items.Add(Strings.Ultra);
			_viewDistanceDropList.SelectedIndexChanged += _viewDistanceDropList_SelectedIndexChanged;
			_viewDistanceDropList.Frame = _game.ButtonFrame;
			_viewDistanceDropList.DropArrow = _game._uiSprites["DropArrow"];
			_viewDistanceDropList.Font = _controlsFont;
			_viewDistanceLabel = new TextControl(Strings.View_Distance, _controlsFont);
			base.Children.Add(_viewDistanceLabel);
			_textureQualityDropList.Items.Add(Strings.High);
			_textureQualityDropList.Items.Add(Strings.Medium);
			_textureQualityDropList.Items.Add(Strings.Low);
			_textureQualityDropList.SelectedIndexChanged += _textureQualityDropList_SelectedIndexChanged;
			_textureQualityDropList.Frame = _game.ButtonFrame;
			_textureQualityDropList.DropArrow = _game._uiSprites["DropArrow"];
			_textureQualityDropList.Font = _controlsFont;
			_textureQualityLabel = new TextControl(Strings.Texture_Quality, _controlsFont);
			base.Children.Add(_textureQualityLabel);
			for (int i = 0; i < _screenSizes.Length; i++)
			{
				if (_validResolutions.ContainsKey(_screenSizes[i]))
				{
					_resolutionDropList.Items.Add(new Resolution(_screenSizes[i]));
				}
			}
			_resolutionDropList.SelectedIndexChanged += _resolutionDropList_SelectedIndexChanged;
			_resolutionDropList.Frame = _game.ButtonFrame;
			_resolutionDropList.DropArrow = _game._uiSprites["DropArrow"];
			_resolutionDropList.Font = _controlsFont;
			_resolutionLabel = new TextControl(Strings.Resolution, _controlsFont);
			base.Children.Add(_resolutionLabel);
			_fullScreen = new CheckBoxControl(_game._uiSprites["Unchecked"], _game._uiSprites["Checked"]);
			_fullScreen.Text = Strings.Full_Screen + ":";
			_fullScreen.TextColor = Color.White;
			_fullScreen.Font = _controlsFont;
			base.Children.Add(_fullScreen);
			base.Children.Add(_resolutionDropList);
			base.Children.Add(_textureQualityDropList);
			base.Children.Add(_viewDistanceDropList);
			_restartDialog = new PCDialogScreen(Strings.Texture_Quality, Strings.You_must_restart_the_game_to_apply_these_changes_, null, false, _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_restartDialog.UseDefaultValues();
		}

		private void _resolutionDropList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (Screen.Adjuster.ScreenSize != _resolutionDropList.SelectedItem.ScreenSize)
			{
				CastleMinerZGame.GlobalSettings.ScreenSize = _resolutionDropList.SelectedItem.ScreenSize;
				_game.ChangeScreenSize(_resolutionDropList.SelectedItem.ScreenSize);
			}
		}

		private void _textureQualityDropList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!_ignoreTextureQualityChange)
			{
				CastleMinerZGame.GlobalSettings.TextureQualityLevel = _textureQualityDropList.SelectedIndex + 1;
				_uiGroup.ShowPCDialogScreen(_restartDialog, null);
			}
			_ignoreTextureQualityChange = false;
		}

		private void _viewDistanceDropList_SelectedIndexChanged(object sender, EventArgs e)
		{
			_game.PlayerStats.DrawDistance = _viewDistanceDropList.SelectedIndex;
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

		public override void OnSelected()
		{
			_musicVolumeTrack.Value = (int)(_game.PlayerStats.musicVolume * 100f);
			_musicMute.Checked = _game.PlayerStats.musicMute;
			_autoClimb.Checked = _game.PlayerStats.AutoClimb;
			_fadeInactiveTray.Checked = _game.PlayerStats.SecondTrayFaded;
			for (int i = 0; i < _resolutionDropList.Items.Count; i++)
			{
				if (_resolutionDropList.Items[i].ScreenSize == Screen.Adjuster.ScreenSize)
				{
					_resolutionDropList.SelectedIndex = i;
					break;
				}
			}
			_viewDistanceDropList.SelectedIndex = _game.PlayerStats.DrawDistance;
			_brightnessBar.Value = (int)(_game.PlayerStats.brightness * 2f * 100f);
			_fullScreen.Checked = _game.IsFullScreen;
			if (_textureQualityDropList.SelectedIndex != CastleMinerZGame.GlobalSettings.TextureQualityLevel - 1)
			{
				_ignoreTextureQualityChange = true;
				_textureQualityDropList.SelectedIndex = CastleMinerZGame.GlobalSettings.TextureQualityLevel - 1;
			}
			base.OnSelected();
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (base.SelectedTab)
			{
				_game.PlayerStats.brightness = (float)_brightnessBar.Value / 100f / 2f;
				_game.Brightness = _game.PlayerStats.brightness;
				_game.IsFullScreen = _fullScreen.Checked;
				CastleMinerZGame.GlobalSettings.FullScreen = _fullScreen.Checked;
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
				_fullScreen.Scale = (_resolutionLabel.Scale = (_resolutionDropList.Scale = (_textureQualityLabel.Scale = (_textureQualityDropList.Scale = (_viewDistanceLabel.Scale = (_viewDistanceDropList.Scale = (_brightnessLabel.Scale = Screen.Adjuster.ScaleFactor.Y)))))));
				_musicVolumeLabel.Scale = (_musicMute.Scale = (_autoClimb.Scale = (_fadeInactiveTray.Scale = Screen.Adjuster.ScaleFactor.Y)));
				int num = (int)(50f * Screen.Adjuster.ScaleFactor.Y);
				Point localPosition = new Point(0, (int)(75f * Screen.Adjuster.ScaleFactor.Y));
				int num2 = (int)(215f * Screen.Adjuster.ScaleFactor.Y);
				_musicVolumeLabel.LocalPosition = localPosition;
				_musicVolumeTrack.LocalPosition = new Point(localPosition.X + num2, localPosition.Y + (int)(10f * Screen.Adjuster.ScaleFactor.Y));
				_musicVolumeTrack.Size = new Size((int)(185f * Screen.Adjuster.ScaleFactor.Y), num);
				_musicMute.LocalPosition = new Point(localPosition.X + num2 * 2, localPosition.Y + (int)(5f * Screen.Adjuster.ScaleFactor.Y));
				localPosition.Y += num;
				_autoClimb.LocalPosition = localPosition;
				localPosition.X += num2;
				_fadeInactiveTray.LocalPosition = localPosition;
				localPosition.X -= num2;
				localPosition.Y += num;
				_brightnessBar.Size = new Size((int)(185f * Screen.Adjuster.ScaleFactor.Y), num);
				_brightnessLabel.LocalPosition = localPosition;
				_brightnessBar.LocalPosition = new Point(localPosition.X + num2, localPosition.Y + (int)(10f * Screen.Adjuster.ScaleFactor.Y));
				_brightnessBar.Size = new Size((int)(185f * Screen.Adjuster.ScaleFactor.Y), num);
				localPosition.Y += num;
				_viewDistanceLabel.LocalPosition = localPosition;
				_viewDistanceDropList.LocalPosition = new Point(localPosition.X + num2, localPosition.Y);
				localPosition.Y += num;
				_textureQualityLabel.LocalPosition = localPosition;
				_textureQualityDropList.LocalPosition = new Point(localPosition.X + num2, localPosition.Y);
				localPosition.Y += num;
				_resolutionLabel.LocalPosition = localPosition;
				_resolutionDropList.LocalPosition = new Point(localPosition.X + num2, localPosition.Y);
				_fullScreen.LocalPosition = new Point(localPosition.X + num2 * 2, localPosition.Y);
			}
			base.OnUpdate(game, gameTime);
		}
	}
}
