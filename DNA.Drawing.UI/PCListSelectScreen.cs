using DNA.Audio;
using DNA.CastleMinerZ;
using DNA.Input;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace DNA.Drawing.UI
{
	public class PCListSelectScreen : PCDialogScreen
	{
		private DNAGame _game;

		private string _defaultText = "";

		private OneShotTimer _drawCursorTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		private bool _drawCursor = true;

		private int _sourceIndex;

		private List<string> _customNamesList = new List<string>();

		private StringBuilder _builder = new StringBuilder();

		private string _description2;

		private string _description3;

		protected int _cursorLine = 1;

		private string _errorMessage;

		private Rectangle[] _itemLocations = new Rectangle[0];

		private int _selectedIndex;

		private int _lastHitTestResult = -1;

		public string DefaultText
		{
			set
			{
				_defaultText = value;
			}
		}

		public string ErrorMessage
		{
			get
			{
				return _errorMessage;
			}
			set
			{
				_errorMessage = value;
			}
		}

		public PCListSelectScreen(DNAGame game, string title, string description1, Texture2D bgImage, SpriteFont font, bool drawBehind, ScalableFrame frame)
			: base(title, description1, null, true, bgImage, font, drawBehind, frame)
		{
			UseDefaultValues();
			_game = game;
		}

		public PCListSelectScreen(DNAGame game, string title, string description1, string description2, Texture2D bgImage, SpriteFont font, bool drawBehind, ScalableFrame frame)
			: base(title, description1, null, true, bgImage, font, drawBehind, frame)
		{
			UseDefaultValues();
			_description2 = description2;
			_game = game;
		}

		public PCListSelectScreen(DNAGame game, string title, string description1, string description2, string description3, Texture2D bgImage, SpriteFont font, bool drawBehind, ScalableFrame frame)
			: base(title, description1, null, true, bgImage, font, drawBehind, frame)
		{
			UseDefaultValues();
			_description2 = description2;
			_description3 = description3;
			_game = game;
		}

		public void Init(int sourceIndex, List<string> names)
		{
			_customNamesList = names;
			_sourceIndex = sourceIndex;
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

		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			base.Draw(device, spriteBatch, gameTime);
			Rectangle titleSafeArea = device.Viewport.TitleSafeArea;
			_drawCursorTimer.Update(gameTime.ElapsedGameTime);
			if (_drawCursorTimer.Expired)
			{
				_drawCursorTimer.Reset();
				_drawCursor = !_drawCursor;
			}
			spriteBatch.Begin();
			Vector2 location = new Vector2((float)(titleSafeArea.Center.X - _bgImage.Width / 2) + DescriptionPadding.X, _endOfDescriptionLoc);
			if (_errorMessage != null)
			{
				location.Y += 35f;
				spriteBatch.DrawOutlinedText(_font, _errorMessage, location, Color.Red, Color.Black, 1);
			}
			if (_itemLocations.Length != _customNamesList.Count)
			{
				_itemLocations = new Rectangle[_customNamesList.Count];
			}
			Rectangle rectangle = new Rectangle((int)location.X + 10, (int)location.Y, _bgImage.Width, _bgImage.Height);
			SpriteFont font = _font;
			int count = _customNamesList.Count;
			int maxGamer = _game.CurrentNetworkSession.MaxGamers;
			_builder.Length = 0;
			Vector2 vector = font.MeasureString(_builder);
			spriteBatch.DrawOutlinedText(font, _builder, new Vector2((float)rectangle.Right - vector.X, (float)rectangle.Bottom - vector.Y), Color.White, Color.Black, 2);
			float[] array = new float[1];
			float num = 0f;
			num += (array[0] = font.MeasureString("XXXXXXXXXXXXXXXXXXX ").X);
			float num2 = ((float)Screen.Adjuster.ScreenRect.Width - num) / 2f + 2f;
			float num3 = rectangle.Top;
			for (int i = 0; i < _customNamesList.Count; i++)
			{
				string text = _customNamesList[i];
				if (_lastHitTestResult == i)
				{
					Color gray = Color.Gray;
					spriteBatch.Draw(CastleMinerZGame.Instance.DummyTexture, new Rectangle((int)num2 - 2, (int)num3, (int)num + 2, font.LineSpacing + 4), gray);
				}
				font.MeasureString(text);
				_itemLocations[i] = new Rectangle((int)num2 - 2, (int)num3, (int)num + 2, font.LineSpacing + 4);
				spriteBatch.DrawOutlinedText(font, text, new Vector2(num2, num3 + 2f), (_sourceIndex == i) ? Color.Red : Color.White, Color.Black, 2);
				num3 += (float)(font.LineSpacing + 4);
				if (i == 15)
				{
					num3 = font.LineSpacing + 10 + rectangle.Top;
					num2 += num + 10f;
				}
			}
			spriteBatch.End();
		}

		public override void OnPoped()
		{
			_defaultText = "";
			base.OnPoped();
		}

		public override void OnPushed()
		{
			base.OnPushed();
		}

		protected override bool OnPlayerInput(InputManager input, GameController controller, KeyboardInput chatpad, GameTime gameTime)
		{
			int num = _hitTest(input.Mouse.Position);
			if (num >= 0 && _selectedIndex != num)
			{
				SoundManager.Instance.PlayInstance("Click");
				_selectedIndex = num;
				_optionSelected = _selectedIndex;
			}
			if (controller.PressedButtons.A || (input.Mouse.LeftButtonPressed && num >= 0))
			{
				if (_sourceIndex == num)
				{
					SoundManager.Instance.PlayInstance("Error");
				}
				else
				{
					PopMe();
					if (Callback != null)
					{
						Callback();
					}
				}
			}
			return base.OnPlayerInput(input, controller, chatpad, gameTime);
		}

		private bool SelectDown()
		{
			_selectedIndex++;
			if (_selectedIndex >= _customNamesList.Count)
			{
				_selectedIndex = _customNamesList.Count - 1;
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
