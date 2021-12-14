using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class InGameMenu : MenuScreen
	{
		private CastleMinerZGame _game;

		private MenuItemElement inviteControl;

		private MenuItemElement teleport;

		public InGameMenu(CastleMinerZGame game)
			: base(game._largeFont, CMZColors.MenuGreen, Color.White, true)
		{
			SpriteFont largeFont = game._largeFont;
			_game = game;
			SelectSound = "Click";
			ClickSound = "Click";
			AddMenuItem(Strings.Return_To_Game, InGameMenuItems.Return);
			AddMenuItem(Strings.Inventory, InGameMenuItems.MyBlocks);
			if (_game.GameMode != 0 && _game.Difficulty != GameDifficultyTypes.HARDCORE)
			{
				teleport = AddMenuItem(Strings.Teleport, InGameMenuItems.Teleport);
			}
			inviteControl = AddMenuItem(Strings.Invite_Friends, InGameMenuItems.Invite);
			AddMenuItem(Strings.Options, InGameMenuItems.Options);
			AddMenuItem(Strings.Main_Menu, InGameMenuItems.Quit);
			inviteControl.Visible = false;
		}

		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			spriteBatch.Begin();
			spriteBatch.Draw(_game.DummyTexture, Screen.Adjuster.ScreenRect, new Color(0f, 0f, 0f, 0.5f));
			if (CastleMinerZGame.Instance.IsOnlineGame && CastleMinerZGame.Instance.CurrentNetworkSession != null)
			{
				string text = Strings.Server_Message + ": " + CastleMinerZGame.Instance.CurrentNetworkSession.ServerMessage;
				spriteBatch.DrawOutlinedText(_game._consoleFont, text, new Vector2(screenRect.Left + 22, screenRect.Top), Color.White, Color.Black, 1);
			}
			spriteBatch.End();
			base.Draw(device, spriteBatch, gameTime);
		}

		public override void OnPushed()
		{
			if (!CastleMinerZGame.Instance.IsOnlineGame)
			{
				CastleMinerZGame.Instance.GameScreen.mainScene.DoUpdate = false;
				CastleMinerZGame.Instance.GameScreen.DoUpdate = false;
			}
			base.OnPushed();
		}

		public override void OnPoped()
		{
			if (!CastleMinerZGame.Instance.IsOnlineGame)
			{
				CastleMinerZGame.Instance.GameScreen.mainScene.DoUpdate = true;
				CastleMinerZGame.Instance.GameScreen.DoUpdate = true;
			}
			base.OnPoped();
		}
	}
}
