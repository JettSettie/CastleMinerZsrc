using DNA.Audio;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.UI
{
	public class ControlsTab : TabControl.TabPage
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

			private ControlsTab _controllerScreen;

			private TextRegionControl textRegion;

			private SpriteFont _font;

			public SelectButtonDialog(SpriteFont font, CastleMinerZControllerMapping.CMZControllerFunctions function, FrameButtonControl hotkeyButton, ControlsTab controllerScreen)
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

		private SpriteFont _menuButtonFont;

		private InputBinding _binding = CastleMinerZGame.Instance._controllerMapping.Binding;

		private bool inGame;

		private CastleMinerZControllerMapping.CMZControllerFunctions[] _buttonOrder = new CastleMinerZControllerMapping.CMZControllerFunctions[28]
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
			CastleMinerZControllerMapping.CMZControllerFunctions.SwitchTray,
			CastleMinerZControllerMapping.CMZControllerFunctions.UseItem1,
			CastleMinerZControllerMapping.CMZControllerFunctions.UseItem2,
			CastleMinerZControllerMapping.CMZControllerFunctions.UseItem3,
			CastleMinerZControllerMapping.CMZControllerFunctions.UseItem4,
			CastleMinerZControllerMapping.CMZControllerFunctions.UseItem5,
			CastleMinerZControllerMapping.CMZControllerFunctions.UseItem6,
			CastleMinerZControllerMapping.CMZControllerFunctions.UseItem7,
			CastleMinerZControllerMapping.CMZControllerFunctions.UseItem8,
			CastleMinerZControllerMapping.CMZControllerFunctions.Count,
			CastleMinerZControllerMapping.CMZControllerFunctions.Shoulder_Activate,
			CastleMinerZControllerMapping.CMZControllerFunctions.Shoulder,
			CastleMinerZControllerMapping.CMZControllerFunctions.Activate,
			CastleMinerZControllerMapping.CMZControllerFunctions.NextItem,
			CastleMinerZControllerMapping.CMZControllerFunctions.PreviousItem,
			CastleMinerZControllerMapping.CMZControllerFunctions.BlockUI,
			CastleMinerZControllerMapping.CMZControllerFunctions.DropQuickBarItem,
			CastleMinerZControllerMapping.CMZControllerFunctions.FlyMode,
			CastleMinerZControllerMapping.CMZControllerFunctions.TextChat
		};

		private string[] _buttonLabels = new string[28]
		{
			Strings.Use_Shoot,
			Strings.Move_forward,
			Strings.Move_backward,
			Strings.Strafe_left,
			Strings.Strafe_right,
			Strings.Sprint,
			Strings.Jump,
			Strings.Reload,
			Strings.Show_Player_List,
			Strings.Switch_Tray,
			Strings.Slot_1,
			Strings.Slot_2,
			Strings.Slot_3,
			Strings.Slot_4,
			Strings.Slot_5,
			Strings.Slot_6,
			Strings.Slot_7,
			Strings.Slot_8,
			null,
			Strings.Activate_Shoulder,
			Strings.Shoulder,
			Strings.Activate,
			Strings.Next_Item,
			Strings.Previous_Item,
			Strings.Opens_your_inventory,
			Strings.Drop_an_item_from_the_quick_bar,
			Strings.Fly_Mode,
			Strings.Show_Chat
		};

		private TextControl _esc;

		private TextControl _opensMenu;

		private TextControl _invertY;

		private TextControl _sensitivityLabel;

		private TextControl _pressToRebind;

		private TrackBarControl _sensitivityControl = new TrackBarControl();

		private BindingScreenButtonControl[] _buttons;

		private TextControl[] _labels;

		private ScreenGroup _uiGroup;

		private FrameButtonControl _defaultButton = new FrameButtonControl();

		private FrameButtonControl invertYButton = new FrameButtonControl();

		private Rectangle _prevScreenSize = Rectangle.Empty;

		public ControlsTab(bool InGame, ScreenGroup uiGroup)
			: base(Strings.Controls)
		{
			_game = CastleMinerZGame.Instance;
			_controlsFont = _game._smallFont;
			_menuButtonFont = _game._medFont;
			inGame = InGame;
			_uiGroup = uiGroup;
			Color buttonColor = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			_defaultButton.LocalPosition = new Point(690, 595);
			_defaultButton.Size = new Size(225, _menuButtonFont.LineSpacing);
			_defaultButton.Text = Strings.Reset_To_Default;
			_defaultButton.Font = _menuButtonFont;
			_defaultButton.Frame = _game.ButtonFrame;
			_defaultButton.Pressed += _defaultButton_Pressed;
			_defaultButton.ButtonColor = buttonColor;
			base.Children.Add(_defaultButton);
			invertYButton.LocalPosition = new Point(310, 595);
			invertYButton.Size = new Size(135, _controlsFont.LineSpacing);
			invertYButton.Text = (_game.PlayerStats.InvertYAxis ? Strings.Inverted : Strings.Regular);
			invertYButton.Font = _controlsFont;
			invertYButton.Frame = _game.ButtonFrame;
			invertYButton.Pressed += invertYButton_Pressed;
			invertYButton.ButtonColor = buttonColor;
			base.Children.Add(invertYButton);
			_buttons = new BindingScreenButtonControl[_buttonOrder.Length - 1];
			_labels = new TextControl[_buttonOrder.Length - 1];
			int num = 0;
			for (int i = 0; i < _buttonOrder.Length; i++)
			{
				if (_buttonLabels[i] != null)
				{
					BindingScreenButtonControl bindingScreenButtonControl = new BindingScreenButtonControl(_buttonOrder[i]);
					bindingScreenButtonControl.Size = new Size(135, _controlsFont.LineSpacing);
					bindingScreenButtonControl.Text = InputBinding.KeyString(_binding.GetBinding((int)_buttonOrder[i], InputBinding.Slot.KeyMouse1));
					bindingScreenButtonControl.Font = _controlsFont;
					bindingScreenButtonControl.Frame = _game.ButtonFrame;
					bindingScreenButtonControl.Pressed += _bindingBtn_Pressed;
					bindingScreenButtonControl.ButtonColor = buttonColor;
					base.Children.Add(bindingScreenButtonControl);
					_buttons[num] = bindingScreenButtonControl;
					TextControl textControl = new TextControl(_buttonLabels[i], _controlsFont);
					base.Children.Add(textControl);
					_labels[num] = textControl;
					num++;
				}
			}
			_esc = new TextControl(_controlsFont, "Esc", Point.Zero, CMZColors.MenuGreen);
			_opensMenu = new TextControl(Strings.Opens_the_menu__Pauses_offline_games, _controlsFont);
			_invertY = new TextControl(Strings.Invert_Y_Axis + ":", _controlsFont);
			_sensitivityLabel = new TextControl(Strings.Controller_Sensitivity, _controlsFont);
			_pressToRebind = new TextControl(Strings.Press_a_button_to_rebind_keys, _menuButtonFont);
			base.Children.Add(_esc);
			base.Children.Add(_opensMenu);
			base.Children.Add(_invertY);
			base.Children.Add(_sensitivityLabel);
			base.Children.Add(_pressToRebind);
			_sensitivityControl.Size = new Size(185, _controlsFont.LineSpacing);
			_sensitivityControl.MinValue = 0;
			_sensitivityControl.MaxValue = 100;
			_sensitivityControl.FillColor = CMZColors.MenuGreen;
			base.Children.Add(_sensitivityControl);
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

		public void ResetAllButtonText()
		{
			for (int i = 0; i < _buttons.Length; i++)
			{
				_buttons[i].Text = InputBinding.KeyString(_binding.GetBinding((int)_buttons[i].Function, InputBinding.Slot.KeyMouse1));
			}
		}

		public override void OnSelected()
		{
			if (!_binding.Initialized)
			{
				CastleMinerZGame.Instance._controllerMapping.SetToDefault();
			}
			invertYButton.Text = (_game.PlayerStats.InvertYAxis ? Strings.Inverted : Strings.Regular);
			_sensitivityControl.Value = (int)(100f * (_game.PlayerStats.controllerSensitivity - 0.01f));
			ResetAllButtonText();
			base.OnSelected();
		}

		public override void OnLostFocus()
		{
			try
			{
				_game.SavePlayerStats(_game.PlayerStats);
			}
			catch
			{
			}
			base.OnLostFocus();
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (base.SelectedTab)
			{
				_game.PlayerStats.controllerSensitivity = 0.01f + (float)_sensitivityControl.Value / 100f;
			}
			if (_prevScreenSize != Screen.Adjuster.ScreenRect)
			{
				_prevScreenSize = Screen.Adjuster.ScreenRect;
				int num = 40;
				if (_buttons.Length > 1)
				{
					num = (int)((float)_buttons[0].Size.Height * 1.3f);
				}
				int num2 = (int)((float)num * Screen.Adjuster.ScaleFactor.Y);
				Point localPosition = new Point(0, (int)(55f * Screen.Adjuster.ScaleFactor.Y));
				int num3 = (int)(300f * Screen.Adjuster.ScaleFactor.Y);
				int num4 = 0;
				_pressToRebind.LocalPosition = new Point((int)(300f * Screen.Adjuster.ScaleFactor.Y), _pressToRebind.LocalPosition.Y);
				for (int i = 0; i < _buttonLabels.Length; i++)
				{
					if (_buttonLabels[i] == null)
					{
						localPosition = new Point((int)(475f * Screen.Adjuster.ScaleFactor.Y), _buttons[0].LocalPosition.Y);
						continue;
					}
					_buttons[num4].LocalPosition = new Point(localPosition.X + num3, localPosition.Y);
					_labels[num4].LocalPosition = localPosition;
					_labels[num4].Scale = (_buttons[num4].Scale = Screen.Adjuster.ScaleFactor.Y);
					num4++;
					localPosition.Y += num2;
				}
				_esc.Scale = (_opensMenu.Scale = (_sensitivityLabel.Scale = (_invertY.Scale = (_defaultButton.Scale = (invertYButton.Scale = (_pressToRebind.Scale = Screen.Adjuster.ScaleFactor.Y))))));
				_esc.LocalPosition = new Point((int)((double)localPosition.X + (double)num3 * 1.2), localPosition.Y);
				_opensMenu.LocalPosition = localPosition;
				localPosition.Y += num2;
				invertYButton.LocalPosition = new Point(localPosition.X + num3, localPosition.Y);
				_invertY.LocalPosition = localPosition;
				localPosition.Y += num2;
				_sensitivityControl.LocalPosition = new Point((int)((double)localPosition.X + (double)num3 * 0.85), localPosition.Y + (int)(10f * Screen.Adjuster.ScaleFactor.Y));
				_sensitivityControl.Size = new Size((int)(185f * Screen.Adjuster.ScaleFactor.Y), num2);
				_sensitivityLabel.LocalPosition = new Point(localPosition.X, localPosition.Y);
				localPosition.Y += num2;
				_defaultButton.LocalPosition = new Point((int)(140f * Screen.Adjuster.ScaleFactor.Y), Size.Height - (int)(40f * Screen.Adjuster.ScaleFactor.Y));
			}
			base.OnUpdate(game, gameTime);
		}
	}
}
