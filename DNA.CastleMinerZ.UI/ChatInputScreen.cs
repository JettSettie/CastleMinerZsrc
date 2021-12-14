using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Net;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.UI
{
	public class ChatInputScreen : UIControlScreen
	{
		private class ChatInputGroup : UIControlGroup
		{
			public TextEditControl _textEditControl = new TextEditControl();

			public MenuBarControl UpperBar = new MenuBarControl();

			private CastleMinerZGame _game = CastleMinerZGame.Instance;

			private FrameButtonControl _closeWindow = new FrameButtonControl();

			public int Width = 400;

			public int Height = 60;

			public event EventHandler Close;

			public ChatInputGroup()
			{
				UpperBar.LocalPosition = Point.Zero;
				UpperBar.Size = new Size(Width, 20);
				UpperBar.Frame = _game.ButtonFrame;
				UpperBar.Text = Strings.Chat;
				UpperBar.Font = _game._myriadSmall;
				UpperBar.ButtonColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);
				base.Children.Add(UpperBar);
				_closeWindow.LocalPosition = new Point(Width - 19, 2);
				_closeWindow.Size = new Size(17, 17);
				_closeWindow.Frame = _game.ButtonFrame;
				_closeWindow.Text = "x";
				_closeWindow.Font = _game._smallFont;
				_closeWindow.Pressed += _closeWindow_Pressed;
				base.Children.Add(_closeWindow);
				_textEditControl.LocalPosition = new Point(10, 30);
				_textEditControl.Size = new Size(Width - 20, 200);
				_textEditControl.Font = _game._myriadSmall;
				_textEditControl.Frame = _game.ButtonFrame;
				_textEditControl.EnterPressed += _textEditControl_EnterPressed;
				base.Children.Add(_textEditControl);
			}

			private void _textEditControl_EnterPressed(object sender, EventArgs e)
			{
				if (!string.IsNullOrWhiteSpace(_textEditControl.Text))
				{
					BroadcastTextMessage.Send(_game.MyNetworkGamer, _game.MyNetworkGamer.Gamertag + ": " + _textEditControl.Text);
				}
				if (this.Close != null)
				{
					this.Close(this, new EventArgs());
				}
			}

			private void _sendButton_Pressed(object sender, EventArgs e)
			{
				if (!string.IsNullOrWhiteSpace(_textEditControl.Text))
				{
					BroadcastTextMessage.Send(_game.MyNetworkGamer, _game.MyNetworkGamer.Gamertag + ": " + _textEditControl.Text);
				}
				if (this.Close != null)
				{
					this.Close(this, new EventArgs());
				}
			}

			public void OnPushed()
			{
				_textEditControl.Text = "";
				_textEditControl.HasFocus = true;
			}

			private void _cancelButton_Pressed(object sender, EventArgs e)
			{
				if (this.Close != null)
				{
					this.Close(this, new EventArgs());
				}
			}

			private void _closeWindow_Pressed(object sender, EventArgs e)
			{
				if (this.Close != null)
				{
					this.Close(this, new EventArgs());
				}
			}

			protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
			{
				_game.ButtonFrame.Draw(spriteBatch, new Rectangle(base.ScreenPosition.X, base.ScreenPosition.Y + 20, Width, Height - 20), new Color(0.75f, 0.75f, 0.75f, 0.75f));
				base.OnDraw(device, spriteBatch, gameTime);
			}
		}

		private ChatInputGroup _chatInput = new ChatInputGroup();

		private Vector2 _chatPosition = new Vector2(0f, 0f);

		private CastleMinerZGame _game = CastleMinerZGame.Instance;

		public ChatInputScreen()
			: base(true)
		{
			base.Controls.Add(_chatInput);
			_chatInput.Close += _chatInput_Close;
		}

		public override void OnPushed()
		{
			_chatInput.OnPushed();
			base.OnPushed();
		}

		private void _chatInput_Close(object sender, EventArgs e)
		{
			PopMe();
		}

		protected override bool OnInput(InputManager inputManager, GameTime gameTime)
		{
			if (_chatInput.UpperBar.DragMenu)
			{
				_chatPosition += inputManager.Mouse.DeltaPosition;
				float num = Screen.Adjuster.ScreenRect.Width - _chatInput.Width;
				float num2 = Screen.Adjuster.ScreenRect.Height - _chatInput.Height;
				if (_chatPosition.X < 0f)
				{
					_chatPosition.X = 0f;
				}
				else if (_chatPosition.X > num)
				{
					_chatPosition.X = num;
				}
				if (_chatPosition.Y < 0f)
				{
					_chatPosition.Y = 0f;
				}
				else if (_chatPosition.Y > num2)
				{
					_chatPosition.Y = num2;
				}
			}
			return base.OnInput(inputManager, gameTime);
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			_chatInput.LocalPosition = new Point((int)_chatPosition.X, (int)_chatPosition.Y);
			_chatInput._textEditControl.HasFocus = true;
			base.Update(game, gameTime);
		}
	}
}
