using DNA.CastleMinerZ.Globalization;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DNA.CastleMinerZ.UI
{
	public class SettingsMenu : SettingScreen
	{
		private CastleMinerZGame _game;

		private BarSettingItem BrightnessBar;

		private BarSettingItem MusicVolumeBar;

		private ListSettingItem DrawDistanceBar;

		private BoolSettingItem FadeInactiveTray;

		private BoolSettingItem InvertYaxis;

		private BarSettingItem ControllerSensitivityBar;

		private BoolSettingItem AutoClimb;

		private BoolSettingItem FullScreen;

		public SettingsMenu(CastleMinerZGame game)
			: base(game, game._medLargeFont, Color.White, Color.Red, false)
		{
			_game = game;
			ClickSound = "Click";
			SelectSound = "Click";
			BrightnessBar = new BarSettingItem(Strings.Brightness, _game.Brightness);
			base.MenuItems.Add(BrightnessBar);
			ControllerSensitivityBar = new BarSettingItem(Strings.Controller_Sensitivity, _game.PlayerStats.controllerSensitivity);
			base.MenuItems.Add(ControllerSensitivityBar);
			MusicVolumeBar = new BarSettingItem(Strings.Music_Volume, _game.PlayerStats.musicVolume);
			base.MenuItems.Add(MusicVolumeBar);
			List<object> items = new List<object>
			{
				Strings.Lowest,
				Strings.Low,
				Strings.Medium,
				Strings.High,
				Strings.Ultra
			};
			DrawDistanceBar = new ListSettingItem(Strings.Graphics, items, _game.PlayerStats.DrawDistance);
			base.MenuItems.Add(DrawDistanceBar);
			InvertYaxis = new BoolSettingItem(Strings.Invert_Y_Axis, _game.PlayerStats.InvertYAxis, Strings.Inverted, Strings.Regular);
			base.MenuItems.Add(InvertYaxis);
			FadeInactiveTray = new BoolSettingItem(Strings.Fade_Inactive_Tray, _game.PlayerStats.SecondTrayFaded, Strings.Faded, Strings.Regular);
			base.MenuItems.Add(FadeInactiveTray);
			AutoClimb = new BoolSettingItem(Strings.Fade_Inactive_Tray, _game.PlayerStats.AutoClimb, Strings.On, Strings.Off);
			base.MenuItems.Add(AutoClimb);
			FullScreen = new BoolSettingItem(Strings.Full_Screen, _game.IsFullScreen, Strings.On, Strings.Off);
			base.MenuItems.Add(FullScreen);
			ImageButtonControl imageButtonControl = new ImageButtonControl();
			imageButtonControl.Image = _game._uiSprites["BackArrow"];
			imageButtonControl.Font = _game._medFont;
			imageButtonControl.LocalPosition = new Point(15, 15);
			imageButtonControl.Pressed += _backButton_Pressed;
			imageButtonControl.Text = " " + Strings.Back;
			imageButtonControl.ImageDefaultColor = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			base.Controls.Add(imageButtonControl);
			int num = (int)(Screen.Adjuster.ScaleFactor.Y * 25f);
			int num2 = (int)(Screen.Adjuster.ScaleFactor.X * 25f);
			DrawArea = new Rectangle(Screen.Adjuster.ScreenRect.X + num2, Screen.Adjuster.ScreenRect.Y + num, Screen.Adjuster.ScreenRect.Width - num2 * 2, Screen.Adjuster.ScreenRect.Height - num * 2);
			SelectedColor = CMZColors.MenuGreen;
		}

		private void _backButton_Pressed(object sender, EventArgs e)
		{
			PopMe();
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			DrawArea = new Rectangle(Screen.Adjuster.ScreenRect.X + 15, Screen.Adjuster.ScreenRect.Y + 55, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height - 55);
			_game.Brightness = BrightnessBar.Value / 2f;
			_game.PlayerStats.brightness = BrightnessBar.Value / 2f;
			_game.PlayerStats.InvertYAxis = InvertYaxis.On;
			_game.PlayerStats.SecondTrayFaded = FadeInactiveTray.On;
			_game.PlayerStats.DrawDistance = DrawDistanceBar.Index;
			_game.PlayerStats.musicVolume = MusicVolumeBar.Value;
			_game.MusicSounds.SetVolume(MusicVolumeBar.Value);
			_game.PlayerStats.AutoClimb = AutoClimb.On;
			CastleMinerZGame.GlobalSettings.FullScreen = FullScreen.On;
			_game.IsFullScreen = FullScreen.On;
			if ((double)ControllerSensitivityBar.Value < 0.5)
			{
				_game.PlayerStats.controllerSensitivity = ControllerSensitivityBar.Value + 0.5f;
			}
			else
			{
				_game.PlayerStats.controllerSensitivity = ControllerSensitivityBar.Value * 2f;
			}
			base.OnUpdate(game, gameTime);
		}

		public override void OnPushed()
		{
			BrightnessBar.Value = _game.Brightness * 2f;
			FadeInactiveTray.On = _game.PlayerStats.SecondTrayFaded;
			InvertYaxis.On = _game.PlayerStats.InvertYAxis;
			MusicVolumeBar.Value = _game.PlayerStats.musicVolume;
			AutoClimb.On = _game.PlayerStats.AutoClimb;
			FullScreen.On = _game.IsFullScreen;
			DrawDistanceBar.Index = _game.PlayerStats.DrawDistance;
			if (_game.PlayerStats.controllerSensitivity < 1f)
			{
				ControllerSensitivityBar.Value = _game.PlayerStats.controllerSensitivity - 0.5f;
			}
			else
			{
				ControllerSensitivityBar.Value = _game.PlayerStats.controllerSensitivity / 2f;
			}
			base.OnPushed();
		}

		public override void OnPoped()
		{
			try
			{
				_game.SavePlayerStats(_game.PlayerStats);
			}
			catch
			{
			}
			base.OnPoped();
		}
	}
}
