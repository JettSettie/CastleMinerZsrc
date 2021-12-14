using DNA.CastleMinerZ.Globalization;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class TeleportMenu : MenuScreen
	{
		private MenuItemElement toPLayer;

		private CastleMinerZGame _game;

		public TeleportMenu(CastleMinerZGame game)
			: base(game._largeFont, false)
		{
			TextColor = CMZColors.MenuGreen;
			SelectedColor = Color.White;
			_game = game;
			ClickSound = "Click";
			SelectSound = "Click";
			AddMenuItem(Strings.Return_To_Game, TeleportMenuItems.Quit);
			AddMenuItem(Strings.Teleport_To_Surface, TeleportMenuItems.Surface);
			AddMenuItem(Strings.Teleport_To_Start, TeleportMenuItems.Origin);
			toPLayer = AddMenuItem(Strings.Teleport_To_Player, TeleportMenuItems.Player);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			toPLayer.Visible = (_game.IsOnlineGame && _game.PVPState == CastleMinerZGame.PVPEnum.Off);
			base.OnUpdate(game, gameTime);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			spriteBatch.Begin();
			spriteBatch.Draw(_game.DummyTexture, Screen.Adjuster.ScreenRect, new Color(0f, 0f, 0f, 0.5f));
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}
	}
}
