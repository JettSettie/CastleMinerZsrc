using DNA.CastleMinerZ.Globalization;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	internal class OptionsMenu : MenuScreen
	{
		private CastleMinerZGame _game;

		public PCDialogScreen _deleteStorageDialog;

		private ControllerScreen _controllerScreen;

		private SettingsMenu _settingsMenu;

		private SpriteBatch SpriteBatch;

		private ScreenGroup _uiGroup;

		private TextRegionElement _descriptionText;

		private bool Cancel;

		public OptionsMenu(CastleMinerZGame game, ScreenGroup uiGroup, SpriteBatch spriteBatch)
			: base(game._largeFont, CMZColors.MenuGreen, Color.White, false)
		{
			SpriteBatch = spriteBatch;
			SpriteFont largeFont = game._largeFont;
			_uiGroup = uiGroup;
			_game = game;
			ClickSound = "Click";
			SelectSound = "Click";
			HorizontalAlignment = HorizontalAlignmentTypes.Right;
			VerticalAlignment = VerticalAlignmentTypes.Top;
			LineSpacing = -10;
			_descriptionText = new TextRegionElement(_game._medLargeFont);
			AddMenuItem(Strings.Controls, Strings.View_in_game_controls_and_settings, OptionsMenuItems.Controls);
			AddMenuItem(Strings.Erase_Storage, Strings.Erase_all_worlds_and_stats_, OptionsMenuItems.EraseStorage);
			AddMenuItem(Strings.Settings, Strings.Change_game_settings_such_as_volume_and_brightness_, OptionsMenuItems.Settings);
			AddMenuItem(Strings.Release_Notes, Strings.View_the_release_notes_, OptionsMenuItems.ReleaseNotes);
			AddMenuItem(Strings.Back, Strings.Back_to_main_menu_, OptionsMenuItems.Back);
			_deleteStorageDialog = new PCDialogScreen(Strings.Erase_Storage, Strings.Are_you_sure_you_want_to_delete_everything_, null, true, _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_deleteStorageDialog.UseDefaultValues();
			_controllerScreen = new ControllerScreen(_game, false, _uiGroup);
			_settingsMenu = new SettingsMenu(_game);
			base.MenuItemSelected += OptionsMenu_MenuItemSelected;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			int num = (int)(512f * Screen.Adjuster.ScaleFactor.Y);
			int num2 = _game.Logo.Height * num / _game.Logo.Width;
			DrawArea = new Rectangle(0, (int)((double)num2 * 0.75), (int)((float)(Screen.Adjuster.ScreenRect.Width / 2) - 125f * Screen.Adjuster.ScaleFactor.X), Screen.Adjuster.ScreenRect.Height - num2);
			_descriptionText.Location = new Vector2((float)Screen.Adjuster.ScreenRect.Center.X + 50f * Screen.Adjuster.ScaleFactor.X, (float)DrawArea.Value.Y + 20f * Screen.Adjuster.ScaleFactor.Y);
			_descriptionText.Size = new Vector2((float)Screen.Adjuster.ScreenRect.Right - _descriptionText.Location.X - 10f, (float)Screen.Adjuster.ScreenRect.Bottom - _descriptionText.Location.Y);
			spriteBatch.Begin();
			_descriptionText.Draw(device, spriteBatch, gameTime, false);
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override void OnMenuItemFocus(MenuItemElement selectedControl)
		{
			_descriptionText.Text = selectedControl.Description;
			base.OnMenuItemFocus(selectedControl);
		}

		private void OptionsMenu_MenuItemSelected(object sender, SelectedMenuItemArgs e)
		{
			switch ((OptionsMenuItems)e.MenuItem.Tag)
			{
			case OptionsMenuItems.Controls:
				_uiGroup.PushScreen(_controllerScreen);
				break;
			case OptionsMenuItems.EraseStorage:
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
				break;
			case OptionsMenuItems.Back:
				PopMe();
				break;
			case OptionsMenuItems.Settings:
				_uiGroup.PushScreen(_settingsMenu);
				break;
			case OptionsMenuItems.ReleaseNotes:
				_game.FrontEnd.PushReleaseNotesScreen();
				break;
			}
		}
	}
}
