using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;

namespace DNA.CastleMinerZ.UI
{
	public class MainMenu : MenuScreen
	{
		private CastleMinerZGame _game;

		private MenuItemElement hostOnlineControl;

		private MenuItemElement joinOnlineControl;

		private MenuItemElement purchaseControl;

		private StringBuilder builder = new StringBuilder();

		private Rectangle _nameRect = Rectangle.Empty;

		public MainMenu(CastleMinerZGame game)
			: base(game._largeFont, CMZColors.MenuGreen, Color.White, false)
		{
			SpriteFont largeFont = game._largeFont;
			_game = game;
			ClickSound = "Click";
			SelectSound = "Click";
			HorizontalAlignment = HorizontalAlignmentTypes.Right;
			VerticalAlignment = VerticalAlignmentTypes.Top;
			LineSpacing = -10;
			hostOnlineControl = AddMenuItem(Strings.Host_Online, MainMenuItems.HostOnline);
			joinOnlineControl = AddMenuItem(Strings.Join_Online, MainMenuItems.JoinOnline);
			AddMenuItem(Strings.Play_Offline, MainMenuItems.PlayOffline);
			purchaseControl = AddMenuItem(Strings.Purchase, MainMenuItems.Purchase);
			AddMenuItem(Strings.Options, MainMenuItems.Options);
			AddMenuItem(Strings.Exit, MainMenuItems.Quit);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			spriteBatch.Begin();
			string gamertag = Screen.CurrentGamer.Gamertag;
			spriteBatch.DrawOutlinedText(_game._myriadMed, gamertag, Vector2.Zero, Color.White, Color.Black, 1);
			_nameRect = new Rectangle(0, 0, (int)_game._myriadMed.MeasureString(gamertag).X, (int)_game._myriadMed.MeasureString(gamertag).Y);
			int num = (int)(512f * Screen.Adjuster.ScaleFactor.Y);
			int num2 = _game.Logo.Height * num / _game.Logo.Width;
			_game.Logo.Draw(spriteBatch, new Rectangle(Screen.Adjuster.ScreenRect.Center.X - num / 2, 0, num, num2), Color.White);
			DrawArea = new Rectangle(0, (int)((double)num2 * 0.75), (int)((float)(Screen.Adjuster.ScreenRect.Width / 2) - 125f * Screen.Adjuster.ScaleFactor.X), Screen.Adjuster.ScreenRect.Height - num2);
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override bool OnPlayerInput(InputManager input, GameController controller, KeyboardInput chatpad, GameTime gameTime)
		{
			if (controller.PressedButtons.B || controller.PressedButtons.Back || input.Keyboard.WasKeyPressed(Keys.Escape))
			{
				_game.FrontEnd.ConfirmExit();
				return false;
			}
			return base.OnPlayerInput(input, controller, chatpad, gameTime);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			purchaseControl.Visible = CastleMinerZGame.TrialMode;
			bool flag = !CastleMinerZGame.TrialMode;
			hostOnlineControl.TextColor = (flag ? CMZColors.MenuGreen : Color.Gray);
			joinOnlineControl.TextColor = (flag ? CMZColors.MenuGreen : Color.Gray);
			base.OnUpdate(game, gameTime);
		}
	}
}
