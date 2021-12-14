using DNA.Audio;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace DNA.CastleMinerZ.UI
{
	internal class ControllerScreen : UIControlScreen
	{
		private class BindingScreenButtonControl : FrameButtonControl
		{
			public CastleMinerZControllerMapping.CMZControllerFunctions Function;

			public BindingScreenButtonControl(CastleMinerZControllerMapping.CMZControllerFunctions function)
			{
				Function = function;
			}
		}

		private class SelectButtonDialog : UIControlScreen
		{
			private Rectangle _window;

			private CastleMinerZControllerMapping.CMZControllerFunctions _function;

			private FrameButtonControl _hotkeyButton;

			private InputBinding _binding = CastleMinerZGame.Instance._controllerMapping.Binding;

			private FrameButtonControl cancelButton = new FrameButtonControl();

			private InputBinding.Bindable _currentBinding;

			private ControllerScreen _controllerScreen;

			private TextRegionControl textRegion;

			private SpriteFont _font;

			public SelectButtonDialog(SpriteFont font, CastleMinerZControllerMapping.CMZControllerFunctions function, FrameButtonControl hotkeyButton, ControllerScreen controllerScreen)
				: base(true)
			{
				_font = font;
				_window = new Rectangle(Screen.Adjuster.ScreenRect.Width / 2 - 150, Screen.Adjuster.ScreenRect.Height / 2 - 75, 300, 150);
				_function = function;
				_hotkeyButton = hotkeyButton;
				_currentBinding = _binding.GetBinding((int)_function, InputBinding.Slot.KeyMouse1);
				_controllerScreen = controllerScreen;
				textRegion = new TextRegionControl(font);
				textRegion.LocalBounds = new Rectangle(_window.Left + 10, _window.Top + 30, _window.Width - 20, _window.Height - 40);
				textRegion.Text = Strings.Press_the_key_you_would_like_to_set_for_this_control;
				textRegion.Color = Color.Black;
				base.Controls.Add(textRegion);
				cancelButton.Font = font;
				cancelButton.Frame = CastleMinerZGame.Instance.ButtonFrame;
				cancelButton.Text = Strings.Cancel;
				cancelButton.Size = new Size(100, font.LineSpacing);
				cancelButton.LocalPosition = new Point(_window.Right - 110, _window.Bottom - font.LineSpacing - 10);
				cancelButton.Pressed += cancelButton_Pressed;
				base.Controls.Add(cancelButton);
			}

			public override void OnPushed()
			{
				SoundManager.Instance.PlayInstance("Popup");
				_binding.InitBindableSensor();
				base.OnPushed();
			}

			protected override void OnUpdate(DNAGame game, GameTime gameTime)
			{
				_window = new Rectangle(Screen.Adjuster.ScreenRect.Width / 2 - 150, Screen.Adjuster.ScreenRect.Height / 2 - 75, 300, 150);
				textRegion.LocalBounds = new Rectangle(_window.Left + 10, _window.Top + 30, _window.Width - 20, _window.Height - 40);
				cancelButton.LocalPosition = new Point(_window.Right - 110, _window.Bottom - _font.LineSpacing - 10);
				base.OnUpdate(game, gameTime);
			}

			protected override bool OnInput(InputManager inputManager, GameTime gameTime)
			{
				if (!inputManager.Mouse.LeftButtonPressed || !cancelButton.HitTest(inputManager.Mouse.Position))
				{
					InputBinding.Bindable bindable = _binding.SenseBindable(InputBinding.Slot.KeyMouse1, inputManager.Keyboard, inputManager.Mouse, inputManager.Controllers[0]);
					switch (bindable)
					{
					case InputBinding.Bindable.KeyEscape:
						SoundManager.Instance.PlayInstance("Click");
						PopMe();
						break;
					case InputBinding.Bindable.KeyD1:
					case InputBinding.Bindable.KeyD2:
					case InputBinding.Bindable.KeyD3:
					case InputBinding.Bindable.KeyD4:
					case InputBinding.Bindable.KeyD5:
					case InputBinding.Bindable.KeyD6:
					case InputBinding.Bindable.KeyD7:
					case InputBinding.Bindable.KeyD8:
						SoundManager.Instance.PlayInstance("Error");
						_binding.InitBindableSensor();
						break;
					default:
						_binding.Bind((int)_function, InputBinding.Slot.KeyMouse1, bindable);
						_controllerScreen.ResetAllButtonText();
						SoundManager.Instance.PlayInstance("Click");
						PopMe();
						break;
					case InputBinding.Bindable.None:
						break;
					}
				}
				return base.OnInput(inputManager, gameTime);
			}

			private void cancelButton_Pressed(object sender, EventArgs e)
			{
				PopMe();
			}

			protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
			{
				spriteBatch.Begin();
				CastleMinerZGame.Instance.ButtonFrame.Draw(spriteBatch, _window, new Color(0.87f, 0.87f, 0.87f, 0.87f));
				spriteBatch.Draw(CastleMinerZGame.Instance.DummyTexture, new Rectangle(_window.X, _window.Y, _window.Width, 20), new Color(0f, 0f, 0f, 0.87f));
				spriteBatch.End();
				base.OnDraw(device, spriteBatch, gameTime);
			}
		}

		private CastleMinerZGame _game;

		private SpriteFont _controlsFont;

		private SpriteFont _smallControlsFont;

		private InputBinding _binding = CastleMinerZGame.Instance._controllerMapping.Binding;

		private bool inGame;

		private CastleMinerZControllerMapping.CMZControllerFunctions[] _buttonOrder = new CastleMinerZControllerMapping.CMZControllerFunctions[18]
		{
			CastleMinerZControllerMapping.CMZControllerFunctions.Use,
			CastleMinerZControllerMapping.CMZControllerFunctions.MoveForward,
			CastleMinerZControllerMapping.CMZControllerFunctions.MoveBackward,
			CastleMinerZControllerMapping.CMZControllerFunctions.StrafeLeft,
			CastleMinerZControllerMapping.CMZControllerFunctions.StrafeRight,
			CastleMinerZControllerMapping.CMZControllerFunctions.Sprint,
			CastleMinerZControllerMapping.CMZControllerFunctions.Jump,
			CastleMinerZControllerMapping.CMZControllerFunctions.Reload,
			CastleMinerZControllerMapping.CMZControllerFunctions.PlayersScreen,
			CastleMinerZControllerMapping.CMZControllerFunctions.TextChat,
			CastleMinerZControllerMapping.CMZControllerFunctions.Count,
			CastleMinerZControllerMapping.CMZControllerFunctions.Shoulder_Activate,
			CastleMinerZControllerMapping.CMZControllerFunctions.Shoulder,
			CastleMinerZControllerMapping.CMZControllerFunctions.Activate,
			CastleMinerZControllerMapping.CMZControllerFunctions.NextItem,
			CastleMinerZControllerMapping.CMZControllerFunctions.PreviousItem,
			CastleMinerZControllerMapping.CMZControllerFunctions.BlockUI,
			CastleMinerZControllerMapping.CMZControllerFunctions.DropQuickBarItem
		};

		private string[] _buttonLabels = new string[18]
		{
			Strings.Use_Shoot,
			Strings.Move_forward,
			Strings.Move_backward,
			Strings.Strafe_left,
			Strings.Strafe_right,
			Strings.Jump,
			Strings.Coming_soon,
			Strings.Reload,
			Strings.Show_Player_List,
			Strings.Show_Chat,
			null,
			Strings.Activate_Shoulder,
			Strings.Shoulder,
			Strings.Activate,
			Strings.Next_Item,
			Strings.Previous_Item,
			Strings.Opens_your_inventory,
			Strings.Drop_an_item_from_the_quick_bar
		};

		private BindingScreenButtonControl[] _buttons;

		private float[] _buttonLabelLengths;

		private float[] _smallButtonLabelLengths;

		private ScreenGroup _uiGroup;

		private FrameButtonControl _defaultButton = new FrameButtonControl();

		private int _normalFontWidth;

		private int _smallFontWidth;

		private int _normalLeftSize;

		private int _smallLeftSize;

		private FrameButtonControl invertYButton = new FrameButtonControl();

		public ControllerScreen(CastleMinerZGame game, bool InGame, ScreenGroup uiGroup)
			: base(false)
		{
			_game = game;
			_controlsFont = _game._medFont;
			_smallControlsFont = _game._smallFont;
			inGame = InGame;
			_uiGroup = uiGroup;
			float num = _controlsFont.MeasureString(Strings.Invert_Y_Axis).X;
			float num2 = Math.Max(_controlsFont.MeasureString(Strings.Selects_the_appropriate_quickbar_item).X, _controlsFont.MeasureString(Strings.Opens_the_menu__Pauses_offline_games).X);
			float num3 = _smallControlsFont.MeasureString(Strings.Invert_Y_Axis).X;
			float num4 = Math.Max(_smallControlsFont.MeasureString(Strings.Selects_the_appropriate_quickbar_item).X, _smallControlsFont.MeasureString(Strings.Opens_the_menu__Pauses_offline_games).X);
			bool flag = false;
			_buttonLabelLengths = new float[_buttonLabels.Length];
			_smallButtonLabelLengths = new float[_buttonLabels.Length];
			for (int i = 0; i < _buttonLabels.Length; i++)
			{
				if (_buttonLabels[i] != null)
				{
					_buttonLabelLengths[i] = _controlsFont.MeasureString(_buttonLabels[i]).X;
					_smallButtonLabelLengths[i] = _smallControlsFont.MeasureString(_buttonLabels[i]).X;
					if (flag)
					{
						num2 = Math.Max(num2, _buttonLabelLengths[i]);
						num4 = Math.Max(num4, _smallButtonLabelLengths[i]);
					}
					else
					{
						num = Math.Max(num, _buttonLabelLengths[i]);
						num3 = Math.Max(num3, _smallButtonLabelLengths[i]);
					}
				}
				else
				{
					flag = true;
				}
			}
			_normalFontWidth = (int)(num + num2 + 270f + 10f + 10f);
			_smallFontWidth = (int)(num3 + num4 + 270f + 10f + 10f);
			_normalLeftSize = (int)num + 5;
			_smallLeftSize = (int)num3 + 5;
			Color color = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			ImageButtonControl imageButtonControl = new ImageButtonControl();
			imageButtonControl.Image = _game._uiSprites["BackArrow"];
			imageButtonControl.Font = _game._medFont;
			imageButtonControl.LocalPosition = new Point(15, 15);
			imageButtonControl.Pressed += _backButton_Pressed;
			imageButtonControl.Text = " " + Strings.Back;
			imageButtonControl.ImageDefaultColor = color;
			base.Controls.Add(imageButtonControl);
			_defaultButton.LocalPosition = new Point(690, 595);
			_defaultButton.Size = new Size(225, 40);
			_defaultButton.Text = Strings.Reset_To_Default;
			_defaultButton.Font = _controlsFont;
			_defaultButton.Frame = _game.ButtonFrame;
			_defaultButton.Pressed += _defaultButton_Pressed;
			_defaultButton.ButtonColor = color;
			base.Controls.Add(_defaultButton);
			invertYButton.LocalPosition = new Point(310, 595);
			invertYButton.Size = new Size(135, 40);
			invertYButton.Text = (_game.PlayerStats.InvertYAxis ? Strings.Inverted : Strings.Regular);
			invertYButton.Font = _controlsFont;
			invertYButton.Frame = _game.ButtonFrame;
			invertYButton.Pressed += invertYButton_Pressed;
			invertYButton.ButtonColor = color;
			base.Controls.Add(invertYButton);
			Point localPosition = new Point(Screen.Adjuster.ScreenRect.Center.X - 140, 145);
			int num5 = (int)((float)_controlsFont.LineSpacing * 1.25f);
			_buttons = new BindingScreenButtonControl[_buttonOrder.Length - 1];
			int num6 = 0;
			for (int j = 0; j < _buttonOrder.Length; j++)
			{
				if (_buttonOrder[j] == CastleMinerZControllerMapping.CMZControllerFunctions.Count)
				{
					localPosition = new Point(Screen.Adjuster.ScreenRect.Center.X + 5, 145);
					continue;
				}
				BindingScreenButtonControl bindingScreenButtonControl = new BindingScreenButtonControl(_buttonOrder[j]);
				bindingScreenButtonControl.LocalPosition = localPosition;
				bindingScreenButtonControl.Size = new Size(135, 40);
				bindingScreenButtonControl.Text = InputBinding.KeyString(_binding.GetBinding((int)_buttonOrder[j], InputBinding.Slot.KeyMouse1));
				bindingScreenButtonControl.Font = _controlsFont;
				bindingScreenButtonControl.Frame = _game.ButtonFrame;
				bindingScreenButtonControl.Pressed += _bindingBtn_Pressed;
				base.Controls.Add(bindingScreenButtonControl);
				_buttons[num6++] = bindingScreenButtonControl;
				localPosition.Y += num5;
				bindingScreenButtonControl.ButtonColor = color;
			}
		}

		private void _bindingBtn_Pressed(object sender, EventArgs e)
		{
			BindingScreenButtonControl bindingScreenButtonControl = sender as BindingScreenButtonControl;
			if (bindingScreenButtonControl != null)
			{
				_uiGroup.PushScreen(new SelectButtonDialog(_controlsFont, bindingScreenButtonControl.Function, bindingScreenButtonControl, this));
			}
		}

		private void _defaultButton_Pressed(object sender, EventArgs e)
		{
			_game._controllerMapping.SetToDefault();
			ResetAllButtonText();
		}

		private void invertYButton_Pressed(object sender, EventArgs e)
		{
			_game.PlayerStats.InvertYAxis = !_game.PlayerStats.InvertYAxis;
			invertYButton.Text = (_game.PlayerStats.InvertYAxis ? Strings.Inverted : Strings.Regular);
		}

		private void _backButton_Pressed(object sender, EventArgs e)
		{
			PopMe();
		}

		public void ResetAllButtonText()
		{
			for (int i = 0; i < _buttons.Length; i++)
			{
				_buttons[i].Text = InputBinding.KeyString(_binding.GetBinding((int)_buttons[i].Function, InputBinding.Slot.KeyMouse1));
			}
		}

		public override void OnPushed()
		{
			if (!_binding.Initialized)
			{
				CastleMinerZGame.Instance._controllerMapping.SetToDefault();
			}
			invertYButton.Text = (_game.PlayerStats.InvertYAxis ? Strings.Inverted : Strings.Regular);
			ResetAllButtonText();
			base.OnPushed();
		}

		public override void OnPoped()
		{
			try
			{
				_game.SavePlayerStats(_game.PlayerStats);
				CastleMinerZGame.GlobalSettings.Save();
			}
			catch
			{
			}
			base.OnPoped();
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			if (controller.PressedButtons.B || controller.PressedButtons.Back || inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				PopMe();
			}
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			spriteBatch.Begin();
			if (inGame)
			{
				spriteBatch.Draw(_game.DummyTexture, Screen.Adjuster.ScreenRect, new Color(0f, 0f, 0f, 0.5f));
			}
			float num = (float)_controlsFont.LineSpacing * 1.35f;
			int num2 = Math.Max(55, Screen.Adjuster.ScreenRect.Center.Y - (int)num * 5);
			Vector2 vector = new Vector2(125f, num2);
			Point localPosition = new Point(Screen.Adjuster.ScreenRect.Center.X - _normalFontWidth / 2 + _normalLeftSize, num2);
			int num3 = 0;
			SpriteFont spriteFont = _controlsFont;
			float[] array;
			if (Screen.Adjuster.ScreenRect.Width < _normalFontWidth)
			{
				localPosition.X = Screen.Adjuster.ScreenRect.Center.X - _smallFontWidth / 2 + _smallLeftSize;
				vector.Y = num2 + 7;
				spriteFont = _smallControlsFont;
				array = _smallButtonLabelLengths;
			}
			else
			{
				array = _buttonLabelLengths;
			}
			bool flag = false;
			for (int i = 0; i < _buttonLabels.Length; i++)
			{
				if (_buttonLabels[i] == null)
				{
					flag = true;
					localPosition = new Point(localPosition.X + 145, num2);
					vector = new Vector2(localPosition.X + 140, num2);
					if (Screen.Adjuster.ScreenRect.Width < _normalFontWidth)
					{
						vector.Y = num2 + 7;
					}
					continue;
				}
				_buttons[num3].LocalPosition = localPosition;
				localPosition.Y += (int)num;
				num3++;
				if (!flag)
				{
					vector.X = (float)localPosition.X - array[i] - 5f;
				}
				spriteBatch.DrawOutlinedText(spriteFont, _buttonLabels[i], vector, Color.White, Color.Black, 1);
				vector.Y += num;
			}
			spriteBatch.DrawOutlinedText(_controlsFont, "1 - 8", vector - new Vector2(100f, 0f), CMZColors.MenuGreen, Color.Black, 1);
			spriteBatch.DrawOutlinedText(spriteFont, Strings.Selects_the_appropriate_quickbar_item, vector, Color.White, Color.Black, 1);
			vector.Y += num;
			spriteBatch.DrawOutlinedText(_controlsFont, "Esc", vector - new Vector2(100f, 0f), CMZColors.MenuGreen, Color.Black, 1);
			spriteBatch.DrawOutlinedText(spriteFont, Strings.Opens_the_menu__Pauses_offline_games, vector, Color.White, Color.Black, 1);
			localPosition.Y += (int)num * 2;
			_defaultButton.LocalPosition = localPosition;
			localPosition.X = _buttons[0].LocalPosition.X;
			invertYButton.LocalPosition = localPosition;
			vector.Y += num;
			vector.X = (float)(localPosition.X - 10) - spriteFont.MeasureString(Strings.Invert_Y_Axis).X;
			spriteBatch.DrawOutlinedText(spriteFont, Strings.Invert_Y_Axis + ":", vector, Color.White, Color.Black, 1);
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}
	}
}
