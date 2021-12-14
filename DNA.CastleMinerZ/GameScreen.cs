using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.Drawing;
using DNA.Drawing.Particles;
using DNA.Drawing.UI;
using DNA.Net.GamerServices;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DNA.CastleMinerZ
{
	public class GameScreen : ScreenGroup
	{
		public struct LightColorPack
		{
			public Color fog;

			public Color direct;

			public Color ambient;

			public Color specular;

			public LightColorPack(Color f, Color a, Color d, Color s)
			{
				fog = f;
				direct = d;
				ambient = a;
				specular = s;
			}

			public LightColorPack(float lerp, ref LightColorPack fromColor, ref LightColorPack toColor)
			{
				fog = Color.Lerp(fromColor.fog, toColor.fog, lerp);
				direct = Color.Lerp(fromColor.direct, toColor.direct, lerp);
				ambient = Color.Lerp(fromColor.ambient, toColor.ambient, lerp);
				specular = Color.Lerp(fromColor.specular, toColor.specular, lerp);
			}
		}

		public const float MINUTES_PER_DAY = 16f;

		public BloomSettings DayBloomSettings = new BloomSettings(0.8f, 4f, 1.25f, 1f, 1f, 1f);

		public BloomSettings NightBloomSettings = new BloomSettings(0.25f, 4f, 1.25f, 1f, 1f, 1f);

		public BloomSettings InDoorBloomSettings = new BloomSettings(0.25f, 4f, 1.25f, 1f, 1f, 1f);

		public BloomSettings DeathBloomSettings = new BloomSettings(0.1f, 8f, 2f, 0.3f, 0.1f, 1f);

		public BloomSettings FlashBloomSettings = new BloomSettings(0f, 4f, 20f, 1f, 0.25f, 1f);

		public BloomSettings ConcussionBloomSettings = new BloomSettings(0.2f, 4f, 5f, 1f, 0.8f, 1f);

		public BloomSettings HUDBloomSettings = new BloomSettings(0f, 12f, 1f, 0f, 1f, 1f);

		public static readonly TimeSpan LengthOfDay = TimeSpan.FromMinutes(16.0);

		private OneShotTimer _sessionAliveTimer = new OneShotTimer(TimeSpan.FromMinutes(1.0));

		private CastleMinerZGame _game;

		private BlockTerrain _terrain;

		public CastleMinerSky _sky;

		private InGameMenu _inGameMenu;

		private OptionsScreen _optionsScreen;

		private TeleportMenu _teleportMenu;

		private InGameHUD _inGameUI;

		public ScreenGroup _uiGroup = new ScreenGroup(true);

		private CameraView mainView;

		private PostProcessView _postProcessView;

		private EnemyManager _enemyManager;

		private TracerManager _tracerManager;

		private PickupManager _pickupManager;

		private ItemBlockEntityManager _itemBlockManager;

		private GameMessageManager _gameMessageManager;

		public CraftingScreen _blockCraftingScreen;

		public Scene mainScene;

		public Selector SelectorEntity;

		public CrackBoxEntity CrackBox;

		public GPSMarkerEntity GPSMarker;

		public PerspectiveCamera FreeFlyCamera = new PerspectiveCamera();

		public bool FreeFlyCameraEnabled;

		private CameraView _fpsView;

		private List<ExplosiveFlashEntity> _explosiveFlashEntities = new List<ExplosiveFlashEntity>();

		private LightColorPack dawnColors = new LightColorPack(new Color(5, 10, 12), new Color(36, 39, 35), new Color(143, 74, 70), new Color(196, 158, 158));

		private LightColorPack duskColors = new LightColorPack(new Color(13, 15, 17), new Color(36, 39, 35), new Color(143, 74, 70), new Color(196, 158, 158));

		private LightColorPack dayColors = new LightColorPack(new Color(18, 26, 28), new Color(0.28f, 0.28f, 0.23f), new Color(0.82f, 0.7f, 0.5f), new Color(1f, 0.8f, 0.8f));

		private LightColorPack nightColors = new LightColorPack(new Color(0, 5, 11), new Color(4, 27, 52), new Color(10, 60, 106), new Color(128, 128, 220));

		private Player _localPlayer;

		private PCKeyboardInputScreen _serverNameScreen;

		private PCKeyboardInputScreen _serverPasswordScreen;

		private Size _lastScreenSize;

		public Scene _fpsScene;

		private SpriteBatch spriteBatch;

		public int exitCount;

		public InGameHUD HUD
		{
			get
			{
				return _inGameUI;
			}
		}

		public bool IsBlockPickerUp
		{
			get
			{
				return _uiGroup.Contains(_blockCraftingScreen);
			}
		}

		public float TimeOfDay
		{
			get
			{
				return _sky.TimeOfDay;
			}
		}

		public float DayNightBlender
		{
			get
			{
				float num = _sky.TimeOfDay * 24f;
				float num2 = (num - (float)(int)num) / 2f;
				int num3 = (int)num;
				switch (num3)
				{
				default:
					return 1f;
				case 9:
				case 10:
				case 11:
				case 12:
				case 13:
				case 14:
				case 15:
				case 16:
				case 17:
					return 0f;
				case 6:
				case 7:
				case 8:
				case 18:
				case 19:
				case 20:
					switch (num3)
					{
					case 7:
					case 19:
						return 0.5f;
					case 6:
						return 1f - num2;
					case 8:
						return 0.5f - num2;
					case 18:
						return num2;
					default:
						return 0.5f + num2;
					}
				}
			}
		}

		public float Day
		{
			get
			{
				return _sky.Day;
			}
			set
			{
				_sky.Day = value;
				float num = 1f;
				float num2 = 0f;
				float num3 = value + 1.625f;
				num3 -= (float)Math.Floor(num3);
				float num4 = num3 * ((float)Math.PI * 2f);
				float num5 = 0.2f + 0.8f * (float)Math.Abs(Math.Sin(num4));
				float num6 = (float)Math.Sqrt(1f - num5 * num5);
				num4 -= 0.236f;
				float x = (0f - (float)Math.Sin(num4)) * num6;
				float z = (float)Math.Cos(num4) * num6;
				Vector3 vectorToSun = new Vector3(x, num5, z);
				_terrain.VectorToSun = vectorToSun;
				float num7 = _sky.TimeOfDay * 24f;
				float num8 = num7 - (float)(int)num7;
				int num9 = (int)num7;
				float num10 = (_sky.TimeOfDay + 0.96f + 0.5f) % 1f * 2f;
				if (num10 > 1f)
				{
					num10 = 2f - num10;
				}
				LightColorPack lightColorPack;
				switch (num9)
				{
				default:
					num = 0f;
					num2 = 1f;
					lightColorPack = nightColors;
					break;
				case 9:
				case 10:
				case 11:
				case 12:
				case 13:
				case 14:
				case 15:
				case 16:
				case 17:
					num = 1f;
					num2 = 0f;
					lightColorPack = dayColors;
					break;
				case 6:
				case 7:
				case 8:
				case 18:
				case 19:
				case 20:
					switch (num9)
					{
					case 6:
						num = 0f;
						num2 = 1f - num8;
						lightColorPack = new LightColorPack(num8, ref nightColors, ref dawnColors);
						break;
					case 7:
						num = 0.5f;
						num2 = 0.5f;
						lightColorPack = dawnColors;
						break;
					case 8:
						num = num8;
						num2 = 0f;
						lightColorPack = new LightColorPack(num8, ref dawnColors, ref dayColors);
						break;
					case 18:
						num = 1f - num8;
						num2 = 0f;
						lightColorPack = new LightColorPack(num8, ref dayColors, ref duskColors);
						break;
					case 19:
						num = 0.5f;
						num2 = 0.5f;
						lightColorPack = duskColors;
						break;
					default:
						num = 0f;
						num2 = num8;
						_game.SetAudio(0f, num8, 0f, 0f);
						lightColorPack = new LightColorPack(num8, ref duskColors, ref nightColors);
						break;
					}
					break;
				}
				float num11 = (CastleMinerZGame.Instance.LocalPlayer.LocalPosition.Y + 32f) / 8f;
				if (num11 < 0f)
				{
					num11 = 0f;
				}
				if (num11 > 1f)
				{
					num11 = 1f;
				}
				num11 = 1f - num11;
				_terrain.FogColor = Color.Lerp(lightColorPack.fog, Color.Black, num11);
				_terrain.AmbientSunColor = lightColorPack.ambient;
				_terrain.SunlightColor = lightColorPack.direct;
				_terrain.SunSpecular = lightColorPack.specular;
				_terrain.PercentMidnight = num10;
				int num12 = _terrain.DepthUnderGround(_game.LocalPlayer.LocalPosition);
				float num13 = Math.Min(1f, (float)num12 / 15f);
				float num14 = 0f;
				if (_game.LocalPlayer.LocalPosition.Y <= -37f)
				{
					num14 = Math.Min(1f, (-37f - _game.LocalPlayer.LocalPosition.Y) / 10f);
				}
				_game.SetAudio(num * (1f - num13), num2 * (1f - num13), num13 * (1f - num14), num14);
			}
		}

		public void AddExplosiveFlashModel(IntVector3 position)
		{
			ExplosiveFlashEntity explosiveFlashEntity = new ExplosiveFlashEntity(position);
			if (!_explosiveFlashEntities.Contains(explosiveFlashEntity))
			{
				_explosiveFlashEntities.Add(explosiveFlashEntity);
				mainScene.Children.Add(explosiveFlashEntity);
			}
		}

		public void RemoveExplosiveFlashModel(IntVector3 position)
		{
			int num = 0;
			while (true)
			{
				if (num < _explosiveFlashEntities.Count)
				{
					if (_explosiveFlashEntities[num].BlockPosition == position)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			_explosiveFlashEntities[num].RemoveFromParent();
			_explosiveFlashEntities.RemoveAt(num);
		}

		public void ShowInGameMenu()
		{
			_uiGroup.PushScreen(_inGameMenu);
		}

		public void ShowBlockPicker()
		{
			_uiGroup.PushScreen(_blockCraftingScreen);
		}

		public GameScreen(CastleMinerZGame game, Player localPlayer)
			: base(false)
		{
			_game = game;
			_terrain = _game._terrain;
			_localPlayer = localPlayer;
		}

		public void Inialize()
		{
			_serverNameScreen = new PCKeyboardInputScreen(_game, Strings.Server_Message, Strings.Enter_a_server_message + ": ", _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_serverNameScreen.ClickSound = "Click";
			_serverNameScreen.OpenSound = "Popup";
			_serverPasswordScreen = new PCKeyboardInputScreen(_game, Strings.Server_Password, Strings.Enter_a_password_for_this_server + ": ", _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_serverPasswordScreen.ClickSound = "Click";
			_serverPasswordScreen.OpenSound = "Popup";
			_teleportMenu = new TeleportMenu(_game);
			_teleportMenu.MenuItemSelected += _teleportMenu_MenuItemSelected;
			_inGameUI = new InGameHUD(_game);
			_blockCraftingScreen = new CraftingScreen(_game, _inGameUI);
			_optionsScreen = new OptionsScreen(true, _uiGroup);
			_inGameMenu = new InGameMenu(_game);
			_inGameMenu.MenuItemSelected += _inGameMenu_MenuItemSelected;
			SceneScreen sceneScreen = new SceneScreen(false, false);
			PushScreen(sceneScreen);
			PushScreen(_uiGroup);
			_uiGroup.PushScreen(_inGameUI);
			sceneScreen.AfterDraw += gameScreen_AfterDraw;
			mainScene = new Scene();
			sceneScreen.Scenes.Add(mainScene);
			_sky = new CastleMinerSky();
			mainScene.Children.Add(_terrain);
			mainScene.Children.Add(_localPlayer);
			_localPlayer.Children.Add(_sky);
			SelectorEntity = new Selector();
			mainScene.Children.Add(SelectorEntity);
			CrackBox = new CrackBoxEntity();
			mainScene.Children.Add(CrackBox);
			GPSMarker = new GPSMarkerEntity();
			mainScene.Children.Add(GPSMarker);
			GPSMarker.Visible = false;
			PresentationParameters presentationParameter = _game.GraphicsDevice.PresentationParameters;
			_lastScreenSize = new Size(_game.GraphicsDevice.PresentationParameters.BackBufferWidth, _game.GraphicsDevice.PresentationParameters.BackBufferHeight);
			_gameMessageManager = new GameMessageManager();
			mainScene.Children.Add(_gameMessageManager);
			_enemyManager = new EnemyManager();
			mainScene.Children.Add(_enemyManager);
			_tracerManager = new TracerManager();
			mainScene.Children.Add(_tracerManager);
			_pickupManager = new PickupManager();
			mainScene.Children.Add(_pickupManager);
			_itemBlockManager = new ItemBlockEntityManager();
			mainScene.Children.Add(_itemBlockManager);
			_postProcessView = new PostProcessView(_game, _game.OffScreenBuffer);
			mainView = new CameraView(_game, _postProcessView.OffScreenTarget, _localPlayer.FPSCamera);
			mainView.BeforeDraw += PreDrawMain;
			sceneScreen.Views.Add(mainView);
			_fpsScene = new Scene();
			sceneScreen.Scenes.Add(_fpsScene);
			_localPlayer.FPSMode = true;
			_fpsScene.Children.Add(_localPlayer.FPSNode);
			_fpsView = new CameraView(_game, _postProcessView.OffScreenTarget, _localPlayer.GunEyePointCamera);
			sceneScreen.Views.Add(_fpsView);
			sceneScreen.Views.Add(_postProcessView);
		}

		public void PopToHUD()
		{
			while (_uiGroup.CurrentScreen != HUD)
			{
				_uiGroup.PopScreen();
			}
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			PresentationParameters presentationParameters = device.PresentationParameters;
			Size b = new Size(presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
			if (_lastScreenSize != b)
			{
				_postProcessView.SetDestinationTarget(_game.OffScreenBuffer);
				mainView.SetDestinationTarget(_postProcessView.OffScreenTarget);
				_fpsView.SetDestinationTarget(_postProcessView.OffScreenTarget);
			}
			base.OnDraw(device, spriteBatch, gameTime);
		}

		public void SwitchFreeFlyCameras()
		{
			if (_fpsView.Enabled)
			{
				_fpsView.Enabled = false;
				mainView.Camera = FreeFlyCamera;
				_localPlayer.FPSMode = false;
				FreeFlyCameraEnabled = true;
			}
			else
			{
				_fpsView.Enabled = true;
				mainView.Camera = _localPlayer.FPSCamera;
				_localPlayer.FPSMode = true;
				FreeFlyCameraEnabled = false;
			}
		}

		private void PreDrawReflection(object sender, DrawEventArgs args)
		{
			if (_terrain != null)
			{
				_terrain.DrawDistance = 0f;
			}
		}

		private void PreDrawMain(object sender, DrawEventArgs args)
		{
			if (_terrain != null)
			{
				_terrain.DrawDistance = (float)_game.PlayerStats.DrawDistance / 4f;
			}
		}

		public static bool FilterWorldGeo(Entity e)
		{
			if (e is CastleMinerToolModel || e is BaseZombie || e is TorchEntity || e is ParticleEmitter)
			{
				return false;
			}
			return CameraView.FilterDistortions(e);
		}

		private void gameScreen_AfterDraw(object sender, DrawEventArgs e)
		{
			if (spriteBatch == null)
			{
				spriteBatch = new SpriteBatch(e.Device);
			}
			if (_game.CurrentNetworkSession == null)
			{
				return;
			}
			Matrix view = mainView.Camera.View;
			Matrix projection = mainView.Camera.GetProjection(e.Device);
			Matrix matrix = view * projection;
			spriteBatch.Begin();
			for (int i = 0; i < _game.CurrentNetworkSession.AllGamers.Count; i++)
			{
				NetworkGamer networkGamer = _game.CurrentNetworkSession.AllGamers[i];
				if (networkGamer.Tag == null || networkGamer.IsLocal)
				{
					continue;
				}
				Player player = (Player)networkGamer.Tag;
				if (player.Visible)
				{
					Vector3 position = player.LocalPosition + new Vector3(0f, 2f, 0f);
					Vector4 vector = Vector4.Transform(position, matrix);
					if (vector.Z > 0f)
					{
						Vector3 vector2 = new Vector3(vector.X / vector.W, vector.Y / vector.W, vector.Z / vector.W);
						vector2 *= new Vector3(0.5f, -0.5f, 1f);
						vector2 += new Vector3(0.5f, 0.5f, 0f);
						vector2 *= new Vector3(Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height, 1f);
						Vector2 value = _game._nameTagFont.MeasureString(networkGamer.Gamertag);
						spriteBatch.DrawOutlinedText(_game._nameTagFont, networkGamer.Gamertag, new Vector2(vector2.X, vector2.Y) - value / 2f, Color.White, Color.Black, 1);
					}
				}
			}
			spriteBatch.End();
		}

		public void TeleportToLocation(Vector3 Location, bool spawnOnTop)
		{
			_game.LocalPlayer.LocalPosition = Location;
			EnemyManager.Instance.ResetFarthestDistance();
			_terrain.CenterOn(_game.LocalPlayer.LocalPosition, true);
			InGameWaitScreen.ShowScreen(_game, this, Strings.Please_Wait___, spawnOnTop, () => _terrain.MinimallyLoaded);
		}

		private void _teleportMenu_MenuItemSelected(object sender, SelectedMenuItemArgs e)
		{
			switch ((TeleportMenuItems)e.MenuItem.Tag)
			{
			case TeleportMenuItems.Quit:
				_uiGroup.PopScreen();
				_uiGroup.PopScreen();
				break;
			case TeleportMenuItems.Origin:
				_uiGroup.PopScreen();
				_uiGroup.PopScreen();
				TeleportToLocation(WorldInfo.DefaultStartLocation, true);
				break;
			case TeleportMenuItems.Player:
				SelectPlayerScreen.SelectPlayer(_game, _uiGroup, false, false, delegate(Player player)
				{
					if (player != null)
					{
						_game.LocalPlayer.LocalPosition = player.LocalPosition;
						_terrain.CenterOn(_game.LocalPlayer.LocalPosition, true);
					}
					_uiGroup.PopScreen();
					_uiGroup.PopScreen();
					InGameWaitScreen.ShowScreen(_game, this, Strings.Please_Wait___, false, () => _terrain.MinimallyLoaded);
				});
				break;
			case TeleportMenuItems.Surface:
				_game.MakeAboveGround(true);
				_uiGroup.PopScreen();
				_uiGroup.PopScreen();
				InGameWaitScreen.ShowScreen(_game, this, Strings.Please_Wait___, true, () => _terrain.MinimallyLoaded);
				break;
			}
		}

		private void _hostOptionsMenu_MenuItemSelected(object sender, SelectedMenuItemArgs e)
		{
			switch ((HostOptionItems)e.MenuItem.Tag)
			{
			case HostOptionItems.ServerMessage:
				_serverNameScreen.DefaultText = _game.ServerMessage;
				_uiGroup.ShowPCDialogScreen(_serverNameScreen, delegate
				{
					if (_serverNameScreen.OptionSelected != -1)
					{
						string textInput = _serverNameScreen.TextInput;
						if (!string.IsNullOrWhiteSpace(textInput))
						{
							_game.ServerMessage = textInput.Trim();
						}
					}
				});
				break;
			case HostOptionItems.Return:
				_uiGroup.PopScreen();
				break;
			case HostOptionItems.KickPlayer:
				SelectPlayerScreen.SelectPlayer(_game, _uiGroup, false, false, delegate(Player player)
				{
					if (player != null)
					{
						BroadcastTextMessage.Send(_game.MyNetworkGamer, player.Gamer.Gamertag + " " + Strings.has_been_kicked_by_the_host);
						KickMessage.Send(_game.MyNetworkGamer, player.Gamer, false);
					}
				});
				break;
			case HostOptionItems.BanPlayer:
				SelectPlayerScreen.SelectPlayer(_game, _uiGroup, false, false, delegate(Player player)
				{
					if (player != null)
					{
						BroadcastTextMessage.Send(_game.MyNetworkGamer, player.Gamer.Gamertag + " " + Strings.has_been_banned_by_the_host);
						KickMessage.Send(_game.MyNetworkGamer, player.Gamer, true);
						_game.PlayerStats.BanList[player.Gamer.AlternateAddress] = DateTime.UtcNow;
						_game.SaveData();
					}
				});
				break;
			case HostOptionItems.Restart:
				RestartLevelMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer);
				BroadcastTextMessage.Send(_game.MyNetworkGamer, _game.LocalPlayer.Gamer.Gamertag + " " + Strings.Has_Restarted_The_Game);
				break;
			case HostOptionItems.Password:
				_serverPasswordScreen.DefaultText = _game.CurrentWorld.ServerPassword;
				_uiGroup.ShowPCDialogScreen(_serverPasswordScreen, delegate
				{
					if (_serverPasswordScreen.OptionSelected != -1)
					{
						string textInput2 = _serverPasswordScreen.TextInput;
						if (string.IsNullOrWhiteSpace(textInput2))
						{
							if (_game.IsOnlineGame)
							{
								_game.CurrentNetworkSession.UpdateHostSession(null, false, null, null);
							}
							_game.CurrentWorld.ServerPassword = "";
							_game.CurrentNetworkSession.Password = null;
						}
						else
						{
							if (_game.IsOnlineGame)
							{
								_game.CurrentNetworkSession.UpdateHostSession(null, true, null, null);
							}
							_game.CurrentWorld.ServerPassword = textInput2;
							_game.CurrentNetworkSession.Password = textInput2;
						}
					}
				});
				break;
			case HostOptionItems.ChangeJoinPolicy:
			{
				int num = (int)(_game.JoinGamePolicy + 1);
				num %= 3;
				_game.JoinGamePolicy = (JoinGamePolicy)num;
				break;
			}
			case HostOptionItems.PVP:
			{
				_game.PVPState = (CastleMinerZGame.PVPEnum)((int)(_game.PVPState + 1) % 3);
				string message = "";
				switch (_game.PVPState)
				{
				case CastleMinerZGame.PVPEnum.Everyone:
					message = "PVP: " + Strings.Everyone;
					break;
				case CastleMinerZGame.PVPEnum.NotFriends:
					message = "PVP: " + Strings.Non_Friends_Only;
					break;
				case CastleMinerZGame.PVPEnum.Off:
					message = "PVP: " + Strings.Off;
					break;
				}
				BroadcastTextMessage.Send(_game.MyNetworkGamer, message);
				break;
			}
			case HostOptionItems.ClearBanList:
				_game.PlayerStats.BanList.Clear();
				_game.SaveData();
				break;
			}
		}

		public void AddPlayer(Player player)
		{
			mainScene.Children.Add(player);
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			if (exitCount > 0)
			{
				exitCount--;
				if (exitCount <= 0)
				{
					_game.EndGame(true);
					exitCount = 0;
					return;
				}
			}
			if (_localPlayer.Gamer.IsHost && _game.IsOnlineGame)
			{
				_sessionAliveTimer.Update(gameTime.ElapsedGameTime);
				if (_sessionAliveTimer.Expired)
				{
					_game.CurrentNetworkSession.ReportSessionAlive();
					_sessionAliveTimer.Reset();
				}
			}
			float day = Day;
			Day += (float)(gameTime.ElapsedGameTime.TotalSeconds / LengthOfDay.TotalSeconds);
			float simpleSunlightAtPoint = BlockTerrain.Instance.GetSimpleSunlightAtPoint(_localPlayer.WorldPosition + new Vector3(0f, 1.2f, 0f));
			BloomSettings set = BloomSettings.Lerp(DayBloomSettings, NightBloomSettings, DayNightBlender);
			BloomSettings bloomSettings = BloomSettings.Lerp(InDoorBloomSettings, set, simpleSunlightAtPoint);
			_postProcessView.BloomSettings = bloomSettings;
			if (_uiGroup.CurrentScreen == HUD || HUD.IsChatting)
			{
				_postProcessView.BloomSettings = BloomSettings.Lerp(DeathBloomSettings, bloomSettings, HUD.PlayerHealth);
			}
			else
			{
				_postProcessView.BloomSettings = HUDBloomSettings;
			}
			base.Update(game, gameTime);
		}

		private void _inGameMenu_MenuItemSelected(object sender, SelectedMenuItemArgs e)
		{
			if (exitCount != 0)
			{
				return;
			}
			switch ((InGameMenuItems)e.MenuItem.Tag)
			{
			case InGameMenuItems.Invite:
				break;
			case InGameMenuItems.Quit:
				if (_localPlayer.Gamer.IsHost)
				{
					_localPlayer.SaveInventory(_game.SaveDevice, _game.CurrentWorld.SavePath);
					_localPlayer.FinalSaveRegistered = true;
				}
				else
				{
					InventoryStoreOnServerMessage.Send((LocalNetworkGamer)_localPlayer.Gamer, _localPlayer.PlayerInventory, true);
				}
				exitCount = 60;
				break;
			case InGameMenuItems.Options:
				_uiGroup.PushScreen(_optionsScreen);
				break;
			case InGameMenuItems.MyBlocks:
				_uiGroup.PopScreen();
				if (!IsBlockPickerUp)
				{
					_uiGroup.PushScreen(_blockCraftingScreen);
				}
				break;
			case InGameMenuItems.Teleport:
				_uiGroup.PushScreen(_teleportMenu);
				break;
			case InGameMenuItems.Return:
				_uiGroup.PopScreen();
				if (IsBlockPickerUp)
				{
					_uiGroup.PopScreen();
				}
				break;
			}
		}
	}
}
