using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	internal class GameModeMenu : MenuScreen
	{
		private CastleMinerZGame _game;

		private TextRegionElement _descriptionText;

		private MenuItemElement SurvivalControl;

		private MenuItemElement DragonEnduranceControl;

		private MenuItemElement CreativeControl;

		private MenuItemElement ExplorationControl;

		private MenuItemElement ScavengerControl;

		public GameModeMenu(CastleMinerZGame game)
			: base(game._largeFont, CMZColors.MenuGreen, Color.White, false)
		{
			_game = game;
			SpriteFont largeFont = _game._largeFont;
			SelectSound = "Click";
			ClickSound = "Click";
			_descriptionText = new TextRegionElement(_game._medLargeFont);
			HorizontalAlignment = HorizontalAlignmentTypes.Right;
			VerticalAlignment = VerticalAlignmentTypes.Top;
			LineSpacing = -10;
			AddMenuItem(Strings.Endurance, Strings.Earn_awards_by_seeing_how_far_you_can_travel_from_the_start_point__Changes_to_the_world_will_not_be_saved_in_this_mode_, GameModeTypes.Endurance);
			SurvivalControl = AddMenuItem(Strings.Survival, Strings.Mine_resources_and_build_your_fortress_while_defending_yourself_from_the_undead_horde__Your_creations_will_be_saved_in_this_mode__You_can_play_with_or_without_enemies_, GameModeTypes.Survival);
			DragonEnduranceControl = AddMenuItem(Strings.Dragon_Endurance, Strings.Fend_off_wave_after_wave_of_dragons__Unlock_this_mode_by_defeating_the_undead_dragon_in_Endurance_Mode__Your_creations_will_be_saved_in_this_mode_, GameModeTypes.DragonEndurance);
			CreativeControl = AddMenuItem(Strings.Creative, Strings.Build_structures_with_unlimited_resources__You_can_play_with_or_without_enemies, GameModeTypes.Creative);
			ExplorationControl = AddMenuItem(Strings.Exploration, Strings.Exploration_description, GameModeTypes.Exploration);
			ScavengerControl = AddMenuItem(Strings.Scavenger, Strings.Scavenger_description, GameModeTypes.Scavenger);
			AddMenuItem(Strings.Back, Strings.Back_to_main_menu_, null);
		}

		protected override void OnMenuItemFocus(MenuItemElement selectedControl)
		{
			_descriptionText.Text = selectedControl.Description;
			base.OnMenuItemFocus(selectedControl);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			int num = (int)(512f * Screen.Adjuster.ScaleFactor.Y);
			int num2 = _game.Logo.Height * num / _game.Logo.Width;
			DrawArea = new Rectangle(0, (int)((double)num2 * 0.75), (int)((float)(Screen.Adjuster.ScreenRect.Width / 2) - 125f * Screen.Adjuster.ScaleFactor.X), Screen.Adjuster.ScreenRect.Height - num2);
			_descriptionText.Location = new Vector2((float)Screen.Adjuster.ScreenRect.Center.X + 50f * Screen.Adjuster.ScaleFactor.X, (float)DrawArea.Value.Y + 20f * Screen.Adjuster.ScaleFactor.Y);
			_descriptionText.Size = new Vector2((float)Screen.Adjuster.ScreenRect.Right - _descriptionText.Location.X - 10f, (float)Screen.Adjuster.ScreenRect.Bottom - _descriptionText.Location.Y);
			string choose_a_Game_Mode = Strings.Choose_a_Game_Mode;
			spriteBatch.Begin();
			spriteBatch.DrawOutlinedText(_game._largeFont, choose_a_Game_Mode, new Vector2((float)Screen.Adjuster.ScreenRect.Center.X - _game._largeFont.MeasureString(choose_a_Game_Mode).X / 2f, 0f), CMZColors.MenuGreen, Color.Black, 2);
			_descriptionText.Draw(device, spriteBatch, gameTime, false);
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			bool flag = !CastleMinerZGame.TrialMode;
			DragonEnduranceControl.TextColor = (((CastleMinerZGame.Instance.PlayerStats.UndeadDragonKills > 0 || _game.LicenseServices.GetAddOn(AddOnIDs.DragonEndurance).HasValue) && flag) ? CMZColors.MenuGreen : Color.Gray);
			SurvivalControl.TextColor = (flag ? CMZColors.MenuGreen : Color.Gray);
			CreativeControl.TextColor = (flag ? CMZColors.MenuGreen : Color.Gray);
			ExplorationControl.TextColor = (flag ? CMZColors.MenuGreen : Color.Gray);
			ScavengerControl.TextColor = (flag ? CMZColors.MenuGreen : Color.Gray);
			base.OnUpdate(game, gameTime);
		}
	}
}
