using DNA.Audio;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using DNA.Input;
using DNA.Net.GamerServices;
using DNA.Text;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace DNA.CastleMinerZ.UI
{
	public class SelectPlayerScreen : UIControlScreen
	{
		public Player PlayerSelected;

		public SelectPlayerCallback _callback;

		private SpriteFont font;

		private int _selectedIndex;

		private bool _showMe;

		private CastleMinerZGame _game;

		private Rectangle[] _itemLocations = new Rectangle[0];

		private int _lastHitTestResult = -1;

		private OneShotTimer flashTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private bool flashDir;

		private StringBuilder _builder = new StringBuilder();

		private List<NetworkGamer> activeGamers = new List<NetworkGamer>();

		private OneShotTimer waitScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		private OneShotTimer autoScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.10000000149011612));

		private SelectPlayerScreen(CastleMinerZGame game, bool showME, bool drawBehind, SelectPlayerCallback callback)
			: base(drawBehind)
		{
			_showMe = showME;
			font = game._medFont;
			_game = game;
			_callback = callback;
			ImageButtonControl imageButtonControl = new ImageButtonControl();
			imageButtonControl.Image = _game._uiSprites["BackArrow"];
			imageButtonControl.Font = _game._medFont;
			imageButtonControl.LocalPosition = new Point(32, 32);
			imageButtonControl.Pressed += _backButton_Pressed;
			imageButtonControl.ImageDefaultColor = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			imageButtonControl.Text = " " + Strings.Back;
			base.Controls.Add(imageButtonControl);
		}

		private void _backButton_Pressed(object sender, EventArgs e)
		{
			PopMe();
		}

		private int _hitTest(Point p)
		{
			for (int i = 0; i < _itemLocations.Length; i++)
			{
				if (_itemLocations[i].Contains(p))
				{
					_lastHitTestResult = i;
					return i;
				}
			}
			_lastHitTestResult = -1;
			return -1;
		}

		public static void SelectPlayer(CastleMinerZGame game, ScreenGroup group, bool showME, bool drawBehind, SelectPlayerCallback callback)
		{
			SelectPlayerScreen screen = new SelectPlayerScreen(game, showME, drawBehind, callback);
			group.PushScreen(screen);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (_itemLocations.Length != activeGamers.Count)
			{
				_itemLocations = new Rectangle[activeGamers.Count];
			}
			flashTimer.Update(gameTime.ElapsedGameTime);
			if (flashTimer.Expired)
			{
				flashTimer.Reset();
				flashDir = !flashDir;
			}
			Rectangle rectangle = new Rectangle(25, 25, Screen.Adjuster.ScreenRect.Width - 50, Screen.Adjuster.ScreenRect.Height - 50);
			spriteBatch.Begin();
			int int_val = activeGamers.Count + ((!_showMe) ? 1 : 0);
			int maxGamers = _game.CurrentNetworkSession.MaxGamers;
			_builder.Length = 0;
			_builder.Append(Strings.Players + " ").Concat(int_val).Append("/")
				.Concat(maxGamers);
			Vector2 vector = font.MeasureString(_builder);
			spriteBatch.DrawOutlinedText(font, _builder, new Vector2((float)rectangle.Right - vector.X, (float)rectangle.Bottom - vector.Y), Color.White, Color.Black, 2);
			float[] array = new float[1];
			float num = 0f;
			num += (array[0] = font.MeasureString("XXXXXXXXXXXXXXXXXXX ").X);
			float num2 = ((float)Screen.Adjuster.ScreenRect.Width - num) / 2f + 2f;
			float num3 = rectangle.Top;
			spriteBatch.DrawOutlinedText(font, Strings.Player, new Vector2(num2, num3), Color.Orange, Color.Black, 2);
			num3 += (float)(font.LineSpacing + 10);
			for (int i = 0; i < activeGamers.Count; i++)
			{
				NetworkGamer networkGamer = activeGamers[i];
				if (networkGamer.Tag == null)
				{
					continue;
				}
				Player player = (Player)networkGamer.Tag;
				if (i == _selectedIndex)
				{
					Color color = Color.Black;
					if (_lastHitTestResult == i)
					{
						color = Color.Gray;
					}
					spriteBatch.Draw(CastleMinerZGame.Instance.DummyTexture, new Rectangle((int)num2 - 2, (int)num3, (int)num + 2, font.LineSpacing + 4), color);
				}
				font.MeasureString(player.Gamer.Gamertag);
				_itemLocations[i] = new Rectangle((int)num2 - 2, (int)num3, (int)num + 2, font.LineSpacing + 4);
				spriteBatch.DrawOutlinedText(font, player.Gamer.Gamertag, new Vector2(num2, num3 + 2f), player.Gamer.IsLocal ? Color.Red : Color.White, Color.Black, 2);
				if (player.Profile != null)
				{
					float num4 = (float)font.LineSpacing * 0.9f;
					float num5 = (float)font.LineSpacing - num4;
					if (player.GamerPicture != null)
					{
						spriteBatch.Draw(player.GamerPicture, new Rectangle((int)(num2 - (float)font.LineSpacing), (int)(num3 + num5), (int)num4, (int)num4), Color.White);
					}
				}
				num3 += (float)(font.LineSpacing + 4);
				if (i == 15)
				{
					num3 = font.LineSpacing + 10 + rectangle.Top;
					num2 += num + 10f;
				}
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			_setActiveGamers();
			base.OnUpdate(game, gameTime);
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			int num = _hitTest(inputManager.Mouse.Position);
			if (num >= 0 && _selectedIndex != num)
			{
				SoundManager.Instance.PlayInstance("Click");
				_selectedIndex = num;
			}
			if (controller.PressedDPad.Down || (controller.CurrentState.ThumbSticks.Left.Y < -0.2f && controller.LastState.ThumbSticks.Left.Y >= -0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Down))
			{
				waitScrollTimer.Reset();
				autoScrollTimer.Reset();
				if (SelectDown())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			if (controller.PressedDPad.Up || (controller.CurrentState.ThumbSticks.Left.Y > 0.2f && controller.LastState.ThumbSticks.Left.Y <= 0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Up))
			{
				waitScrollTimer.Reset();
				autoScrollTimer.Reset();
				if (SelectUp())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			waitScrollTimer.Update(gameTime.ElapsedGameTime);
			if (controller.PressedButtons.A || inputManager.Keyboard.WasKeyPressed(Keys.Enter) || (inputManager.Mouse.LeftButtonPressed && num >= 0))
			{
				PopMe();
				if (_callback != null)
				{
					_callback(PlayerSelected);
				}
			}
			if (controller.PressedButtons.B || controller.PressedButtons.Back || inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				PlayerSelected = null;
				PopMe();
				if (_callback != null)
				{
					_callback(null);
				}
			}
			if (waitScrollTimer.Expired)
			{
				if (controller.CurrentState.ThumbSticks.Left.Y < -0.2f)
				{
					autoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (autoScrollTimer.Expired)
					{
						autoScrollTimer.Reset();
						if (SelectDown())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
				else if (controller.CurrentState.ThumbSticks.Left.Y > 0.2f)
				{
					autoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (autoScrollTimer.Expired)
					{
						autoScrollTimer.Reset();
						if (SelectUp())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
			}
			if (_selectedIndex <= 0)
			{
				_selectedIndex = 0;
			}
			if (_selectedIndex >= activeGamers.Count)
			{
				_selectedIndex = activeGamers.Count - 1;
			}
			SetSelection();
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		private void SetSelection()
		{
			PlayerSelected = null;
			int num = 0;
			for (int i = 0; i < _game.CurrentNetworkSession.AllGamers.Count; i++)
			{
				NetworkGamer networkGamer = _game.CurrentNetworkSession.AllGamers[i];
				if (networkGamer.Tag != null && (_showMe || networkGamer != _game.MyNetworkGamer))
				{
					if (num == _selectedIndex)
					{
						PlayerSelected = (Player)networkGamer.Tag;
					}
					num++;
				}
			}
		}

		private void _setActiveGamers()
		{
			activeGamers.Clear();
			for (int i = 0; i < _game.CurrentNetworkSession.AllGamers.Count; i++)
			{
				NetworkGamer networkGamer = _game.CurrentNetworkSession.AllGamers[i];
				if (networkGamer.Tag != null && (_showMe || networkGamer != _game.MyNetworkGamer))
				{
					activeGamers.Add(networkGamer);
				}
			}
		}

		private bool SelectDown()
		{
			_selectedIndex++;
			if (_selectedIndex >= activeGamers.Count)
			{
				_selectedIndex = activeGamers.Count - 1;
				return false;
			}
			return true;
		}

		private bool SelectUp()
		{
			_selectedIndex--;
			if (_selectedIndex <= 0)
			{
				_selectedIndex = 0;
				return false;
			}
			return true;
		}
	}
}
