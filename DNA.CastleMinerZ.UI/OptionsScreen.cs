using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace DNA.CastleMinerZ.UI
{
	public class OptionsScreen : UIControlScreen
	{
		private ControlsTab _controlsTab;

		private HostOptionsTab _hostOptionsTab;

		private GraphicsTab _graphicsTab;

		private TabControl tabControl = new TabControl();

		private CastleMinerZGame _game = CastleMinerZGame.Instance;

		private bool _inGame;

		private FrameButtonControl _backButton = new FrameButtonControl();

		private Rectangle prevScreenSize;

		public OptionsScreen(bool inGame, ScreenGroup uiGroup)
			: base(false)
		{
			_inGame = inGame;
			Color color = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			tabControl.Font = _game._medFont;
			tabControl.Size = new Size(Screen.Adjuster.ScreenRect.Width, (int)(620f * Screen.Adjuster.ScaleFactor.Y));
			tabControl.LocalPosition = new Point(Screen.Adjuster.ScreenRect.Left, Screen.Adjuster.ScreenRect.Top + (int)(100f * Screen.Adjuster.ScaleFactor.Y));
			tabControl.TextColor = color;
			tabControl.TextSelectedColor = color;
			tabControl.TextHoverColor = Color.Gray;
			tabControl.TextPressedColor = Color.Black;
			tabControl.BarColor = color;
			tabControl.BarSelectedColor = color;
			tabControl.BarHoverColor = Color.Black;
			tabControl.BarPressedColor = Color.Black;
			tabControl.Size = new Size(960, 620);
			base.Controls.Add(tabControl);
			_controlsTab = new ControlsTab(inGame, uiGroup);
			_graphicsTab = new GraphicsTab(inGame, uiGroup);
			if (inGame)
			{
				_hostOptionsTab = new HostOptionsTab(inGame);
			}
			tabControl.Tabs.Add(_graphicsTab);
			tabControl.Tabs.Add(_controlsTab);
			if (inGame)
			{
				tabControl.Tabs.Add(_hostOptionsTab);
			}
			_backButton.Size = new Size(135, _game._medFont.LineSpacing);
			_backButton.Text = Strings.Back;
			_backButton.Font = _game._medFont;
			_backButton.Frame = _game.ButtonFrame;
			_backButton.Pressed += _backButton_Pressed;
			_backButton.ButtonColor = color;
			base.Controls.Add(_backButton);
		}

		private void _backButton_Pressed(object sender, EventArgs e)
		{
			PopMe();
		}

		public override void OnPushed()
		{
			if (_inGame && _game.IsOnlineGame && _game.IsGameHost)
			{
				if (!tabControl.Tabs.Contains(_hostOptionsTab))
				{
					tabControl.Tabs.Add(_hostOptionsTab);
				}
			}
			else if (tabControl.Tabs.Contains(_hostOptionsTab))
			{
				tabControl.Tabs.Remove(_hostOptionsTab);
			}
			tabControl.SelectedTab.OnSelected();
			base.OnPushed();
		}

		public override void OnPoped()
		{
			tabControl.SelectedTab.OnLostFocus();
			base.OnPoped();
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (prevScreenSize != Screen.Adjuster.ScreenRect)
			{
				prevScreenSize = Screen.Adjuster.ScreenRect;
				tabControl.LocalPosition = new Point(Screen.Adjuster.ScreenRect.Center.X - (int)(480f * Screen.Adjuster.ScaleFactor.Y), Screen.Adjuster.ScreenRect.Top + (int)(100f * Screen.Adjuster.ScaleFactor.Y));
				tabControl.Scale = Screen.Adjuster.ScaleFactor.Y;
				for (int i = 0; i < tabControl.Tabs.Count; i++)
				{
					tabControl.Tabs[i].LocalPosition = Point.Zero;
					tabControl.Tabs[i].Size = tabControl.Size;
				}
				_backButton.Scale = Screen.Adjuster.ScaleFactor.Y;
				_backButton.LocalPosition = new Point(tabControl.LocalPosition.X, Screen.Adjuster.ScreenRect.Bottom - (int)(40f * Screen.Adjuster.ScaleFactor.Y));
			}
			base.OnUpdate(game, gameTime);
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			if (inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				PopMe();
			}
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}
	}
}
