using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Net;
using DNA.Drawing;
using DNA.Drawing.UI.Controls;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace DNA.CastleMinerZ.UI
{
	public class PlainChatInputScreen : UIControlScreen
	{
		public TextEditControl _textEditControl = new TextEditControl();

		public int Width = 350;

		private CastleMinerZGame _game = CastleMinerZGame.Instance;

		private float _yLoc;

		public float YLoc
		{
			set
			{
				_yLoc = value;
			}
		}

		public PlainChatInputScreen(float yLoc)
			: base(true)
		{
			_yLoc = yLoc;
			_textEditControl.LocalPosition = new Point(0, (int)_yLoc);
			_textEditControl.Size = new Size(Width - 20, 200);
			_textEditControl.Font = _game._myriadSmall;
			_textEditControl.Frame = _game.ButtonFrame;
			_textEditControl.EnterPressed += _textEditControl_EnterPressed;
			_textEditControl.TextColor = Color.White;
			_textEditControl.FrameColor = new Color(0f, 0f, 0f, 0.5f);
			base.Controls.Add(_textEditControl);
		}

		private void _textEditControl_EnterPressed(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(_textEditControl.Text))
			{
				BroadcastTextMessage.Send(_game.MyNetworkGamer, _game.MyNetworkGamer.Gamertag + ": " + _textEditControl.Text);
			}
			PopMe();
		}

		public override void OnPushed()
		{
			_textEditControl.Text = "";
			_textEditControl.HasFocus = true;
			base.OnPushed();
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			if (inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				PopMe();
			}
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			spriteBatch.Begin();
			if (string.IsNullOrWhiteSpace(_textEditControl.Text))
			{
				spriteBatch.DrawString(_game._myriadSmall, Strings.Type_here, new Vector2(0f, _yLoc), Color.White);
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}
	}
}
