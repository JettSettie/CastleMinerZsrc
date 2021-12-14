using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class MenuBackdropScreen : Screen
	{
		private CastleMinerZGame _game;

		public MenuBackdropScreen(CastleMinerZGame game)
			: base(false, false)
		{
			_game = game;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			try
			{
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
				spriteBatch.Draw(_game.MenuBackdrop, screenRect, Color.White);
				spriteBatch.End();
			}
			catch
			{
			}
			base.OnDraw(device, spriteBatch, gameTime);
		}
	}
}
