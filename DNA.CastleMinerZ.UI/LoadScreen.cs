using DNA.Drawing.UI;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.UI
{
	public class LoadScreen : Screen
	{
		private OneShotTimer preBlackness = new OneShotTimer(TimeSpan.FromSeconds(2.0));

		private OneShotTimer fadeIn = new OneShotTimer(TimeSpan.FromSeconds(2.0));

		private OneShotTimer display = new OneShotTimer(TimeSpan.FromSeconds(2.0));

		private OneShotTimer fadeOut = new OneShotTimer(TimeSpan.FromSeconds(2.0));

		private OneShotTimer postBlackness = new OneShotTimer(TimeSpan.FromSeconds(2.0));

		private Texture2D _image;

		public bool Finished;

		public LoadScreen(Texture2D loadScreen, TimeSpan totalTime)
			: base(true, false)
		{
			display.MaxTime = totalTime - TimeSpan.FromSeconds(8.0);
			_image = loadScreen;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle destinationRectangle = Screen.Adjuster.TransformClipped(new Rectangle(0, 0, 1280, 720));
			spriteBatch.Begin();
			if (preBlackness.Expired)
			{
				if (fadeIn.Expired)
				{
					if (display.Expired)
					{
						if (fadeOut.Expired)
						{
							if (postBlackness.Expired)
							{
								Finished = true;
							}
							else
							{
								spriteBatch.Draw(_image, destinationRectangle, Color.Black);
							}
						}
						else
						{
							spriteBatch.Draw(_image, destinationRectangle, Color.Lerp(Color.White, Color.Black, fadeOut.PercentComplete));
						}
					}
					else
					{
						spriteBatch.Draw(_image, destinationRectangle, Color.White);
					}
				}
				else
				{
					spriteBatch.Draw(_image, destinationRectangle, Color.Lerp(Color.Black, Color.White, fadeIn.PercentComplete));
				}
			}
			else
			{
				spriteBatch.Draw(_image, destinationRectangle, Color.Black);
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (preBlackness.Expired)
			{
				if (fadeIn.Expired)
				{
					if (display.Expired)
					{
						if (fadeOut.Expired)
						{
							if (postBlackness.Expired)
							{
								Finished = true;
							}
							else
							{
								postBlackness.Update(gameTime.ElapsedGameTime);
							}
						}
						else
						{
							fadeOut.Update(gameTime.ElapsedGameTime);
						}
					}
					else
					{
						display.Update(gameTime.ElapsedGameTime);
					}
				}
				else
				{
					fadeIn.Update(gameTime.ElapsedGameTime);
				}
			}
			else
			{
				preBlackness.Update(gameTime.ElapsedGameTime);
			}
			base.OnUpdate(game, gameTime);
		}
	}
}
