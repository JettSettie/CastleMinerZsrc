using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.UI
{
	public class InGameWaitScreen : Screen
	{
		private SpriteFont _largeFont;

		private string _text;

		private ProgressCallback _callback;

		private CastleMinerZGame _game;

		private bool _spawnOnTop;

		private OneShotTimer textFlashTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		public static void ShowScreen(CastleMinerZGame game, ScreenGroup group, string text, bool spawnontop, ProgressCallback callback)
		{
			if (!callback())
			{
				group.PushScreen(new InGameWaitScreen(game, text, callback, spawnontop));
			}
			else
			{
				game.MakeAboveGround(spawnontop);
			}
		}

		public InGameWaitScreen(CastleMinerZGame game, string text, ProgressCallback callback, bool spawnontop)
			: base(true, false)
		{
			_largeFont = game._largeFont;
			_text = text;
			_callback = callback;
			_game = game;
			_spawnOnTop = spawnontop;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			float loadProgress = _game.LoadProgress;
			string text = _text;
			float num = (float)screenRect.Width * 0.8f;
			float num2 = (float)screenRect.Left + ((float)screenRect.Width - num) / 2f;
			Sprite sprite = _game._uiSprites["Bar"];
			Vector2 vector = _largeFont.MeasureString(text);
			Vector2 location = new Vector2(num2, (float)(screenRect.Height / 2) + vector.Y / 2f);
			float num3 = location.Y + (float)_largeFont.LineSpacing + 6.66666651f;
			Rectangle rectangle = new Rectangle((int)num2, (int)num3, (int)num, _largeFont.LineSpacing);
			int left = rectangle.Left;
			int top = rectangle.Top;
			float num5 = (float)rectangle.Width / (float)sprite.Width;
			spriteBatch.Begin();
			spriteBatch.Draw(destinationRectangle: new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height), texture: _game.MenuBackdrop, color: Color.White);
			_game.Logo.Draw(spriteBatch, new Vector2(screenRect.Center.X - _game.Logo.Width / 2, 0f), Color.White);
			spriteBatch.DrawOutlinedText(_largeFont, text, location, Color.White, Color.Black, 1);
			spriteBatch.Draw(_game.DummyTexture, new Rectangle(left - 2, top - 2, rectangle.Width + 4, rectangle.Height + 4), Color.White);
			spriteBatch.Draw(_game.DummyTexture, new Rectangle(left, top, rectangle.Width, rectangle.Height), Color.Black);
			int num4 = (int)((float)sprite.Width * loadProgress);
			sprite.Draw(spriteBatch, new Rectangle(left, top, (int)((float)rectangle.Width * loadProgress), rectangle.Height), new Rectangle(sprite.Width - num4, 0, num4, sprite.Height), Color.White);
			textFlashTimer.Update(gameTime.ElapsedGameTime);
			Color.Lerp(Color.Red, Color.White, textFlashTimer.PercentComplete);
			if (textFlashTimer.Expired)
			{
				textFlashTimer.Reset();
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (_callback())
			{
				_game.MakeAboveGround(_spawnOnTop);
				PopMe();
			}
			base.OnUpdate(game, gameTime);
		}
	}
}
