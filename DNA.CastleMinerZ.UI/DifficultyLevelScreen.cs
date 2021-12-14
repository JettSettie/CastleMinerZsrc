using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	internal class DifficultyLevelScreen : MenuScreen
	{
		private CastleMinerZGame _game;

		private MenuItemElement HardcoreControl;

		private TextRegionElement _descriptionText;

		public DifficultyLevelScreen(CastleMinerZGame game)
			: base(game._largeFont, CMZColors.MenuGreen, Color.White, false)
		{
			_game = game;
			SpriteFont largeFont = _game._largeFont;
			SelectSound = "Click";
			ClickSound = "Click";
			HorizontalAlignment = HorizontalAlignmentTypes.Right;
			VerticalAlignment = VerticalAlignmentTypes.Top;
			LineSpacing = -10;
			_descriptionText = new TextRegionElement(_game._medLargeFont);
			AddMenuItem(Strings.No_Enemies, Strings.Build_freely_without_enemy_attacks_, GameDifficultyTypes.NOENEMIES);
			AddMenuItem(Strings.Easy, Strings.Enemies_do_less_damage__Zombies_only_appear_at_night__Dragons_will_not_damage_your_structures_, GameDifficultyTypes.EASY);
			AddMenuItem(Strings.Normal, Strings.Enemies_appear_when_you_cover_fresh_ground_and_swarm_at_night__Dragons_will_damage_your_structures_, GameDifficultyTypes.HARD);
			HardcoreControl = AddMenuItem(Strings.Hardcore, Strings.Start_with_nothing_and_drop_all_items_when_you_die_, GameDifficultyTypes.HARDCORE);
			AddMenuItem(Strings.Back, Strings.Back_to_main_menu_, null);
			base.SelectedIndex = 2;
		}

		protected override void OnMenuItemFocus(MenuItemElement selectedControl)
		{
			_descriptionText.Text = selectedControl.Description;
			base.OnMenuItemFocus(selectedControl);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			int num = (int)(512f * Screen.Adjuster.ScaleFactor.Y);
			int num2 = _game.Logo.Height * num / _game.Logo.Width;
			DrawArea = new Rectangle(0, (int)((double)num2 * 0.75), (int)((float)(Screen.Adjuster.ScreenRect.Width / 2) - 125f * Screen.Adjuster.ScaleFactor.X), Screen.Adjuster.ScreenRect.Height - num2);
			_descriptionText.Location = new Vector2((float)Screen.Adjuster.ScreenRect.Center.X + 50f * Screen.Adjuster.ScaleFactor.X, (float)DrawArea.Value.Y + 20f * Screen.Adjuster.ScaleFactor.Y);
			_descriptionText.Size = new Vector2((float)Screen.Adjuster.ScreenRect.Right - _descriptionText.Location.X - 10f, (float)Screen.Adjuster.ScreenRect.Bottom - _descriptionText.Location.Y);
			string choose_a_Difficulty_Level = Strings.Choose_a_Difficulty_Level;
			spriteBatch.Begin();
			spriteBatch.DrawOutlinedText(_game._largeFont, choose_a_Difficulty_Level, new Vector2((float)Screen.Adjuster.ScreenRect.Center.X - _game._largeFont.MeasureString(choose_a_Difficulty_Level).X / 2f, 0f), CMZColors.MenuGreen, Color.Black, 2);
			_descriptionText.Draw(device, spriteBatch, gameTime, false);
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			HardcoreControl.Visible = !_game.InfiniteResourceMode;
			base.OnUpdate(game, gameTime);
		}
	}
}
