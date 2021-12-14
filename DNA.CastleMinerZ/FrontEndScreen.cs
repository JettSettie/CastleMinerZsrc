using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.UI;
using DNA.CastleMinerZ.Utils.Threading;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Input;
using DNA.IO.Storage;
using DNA.Net.GamerServices;
using DNA.Net.MatchMaking;
using DNA.Profiling;
using DNA.Security.Cryptography;
using DNA.Text;
using DNA.Timers;
using Facebook;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DNA.CastleMinerZ
{
	public class FrontEndScreen : ScreenGroup
	{
		public CastleMinerZGame _game;

		public ScreenGroup _uiGroup = new ScreenGroup(true);

		private ChooseSavedWorldScreen _chooseSavedWorldScreen;

		private Screen _connectingScreen = new Screen(true, false);

		private Screen _loadingScreen = new Screen(true, false);

		private MainMenu _mainMenu;

		private OptionsScreen _optionsScreen;

		private AchievementScreen<CastleMinerZPlayerStats> _achievementScreen;

		private GameModeMenu _gameModeMenu;

		private PCDialogScreen _undeadNotKilledDialog;

		private PCDialogScreen _modeNotUnlockedDialog;

		private PCDialogScreen _onlinePlayNotPurchasedDialog;

		private DifficultyLevelScreen _difficultyLevelScreen;

		private SinglePlayerStartScreen _startScreen = new SinglePlayerStartScreen(false);

		private Screen _chooseAnotherGameScreen = new Screen(true, false);

		private ReleaseNotesScreen _releaseNotesScreen;

		private SpriteBatch SpriteBatch;

		public SpriteFont _largeFont;

		private PromoCode.PromoCodeManager _promoManager;

		private CheatCode.CheatCodeManager _cheatcodeManager;

		public WorldManager WorldManager;

		public ChooseOnlineGameScreen _chooseOnlineGameScreen;

		public PCDialogScreen _optimizeStorageDialog;

		private PCKeyboardInputScreen _playerNameInput;

		private PCKeyboardInputScreen _serverPasswordScreen;

		private PCDialogScreen _quitDialog;

		private WaitScreen optimizeStorageWaitScreen;

		private int CurrentWorldsCount;

		private bool Cancel;

		private int OriginalWorldsCount;

		private bool _localGame;

		private bool _hostGame;

		private bool _draw2DInventoryAtlas;

		private ProfilerPrimitiveBatch _primitiveBatch;

		private float _draw2DInventoryBackgroundBrightness;

		private string _versionString;

		private bool _flashDir;

		private OneShotTimer _flashTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private bool _scrollWheelSaved;

		private int _scrollWheelValue;

		private OneShotTimer textFlashTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5), true);

		public FrontEndScreen(CastleMinerZGame game)
			: base(false)
		{
			_versionString = Strings.Version + " " + game.Version.ToString();
			_releaseNotesScreen = new ReleaseNotesScreen(game, _versionString);
			_optionsScreen = new OptionsScreen(false, _uiGroup);
			_game = game;
			_largeFont = game._largeFont;
			SpriteBatch = new SpriteBatch(game.GraphicsDevice);
			MenuBackdropScreen screen = new MenuBackdropScreen(game);
			PushScreen(screen);
			PushScreen(_uiGroup);
			_uiGroup.PushScreen(_startScreen);
			_startScreen.ClickSound = "Click";
			_startScreen.OnStartPressed += _startScreen_OnStartPressed;
			_startScreen.AfterDraw += _startScreen_AfterDraw;
			_startScreen.OnBackPressed += _startScreen_OnBackPressed;
			_mainMenu = new MainMenu(game);
			_mainMenu.MenuItemSelected += _mainMenu_MenuItemSelected;
			_gameModeMenu = new GameModeMenu(game);
			_gameModeMenu.MenuItemSelected += _gameModeMenu_MenuItemSelected;
			_quitDialog = new PCDialogScreen(Strings.Quit_Game, Strings.Are_you_sure_you_want_to_quit, null, true, _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_quitDialog.UseDefaultValues();
			_undeadNotKilledDialog = new PCDialogScreen(Strings.Kill_The_Undead_Dragon, Strings.Unlock_this_game_mode_by_killing_the_Undead_Dragon_in_Endurance_Mode, null, false, _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_undeadNotKilledDialog.UseDefaultValues();
			_modeNotUnlockedDialog = new PCDialogScreen(Strings.Trial_Mode, Strings.You_must_purchase_the_game_before_you_can_play_in_this_game_mode_, null, true, _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_modeNotUnlockedDialog.UseDefaultValues();
			_onlinePlayNotPurchasedDialog = new PCDialogScreen(Strings.Trial_Mode, Strings.You_must_purchase_the_game_before_you_can_play_online_, null, true, _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_onlinePlayNotPurchasedDialog.UseDefaultValues();
			_optimizeStorageDialog = new PCDialogScreen(Strings.Optimize_Storage, Strings.To_decrease_load_time_it_is_recommended_that_you_optimize_your_storage__Would_you_like_to_do_this_now___this_may_take_several_minutes_, null, true, _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_optimizeStorageDialog.UseDefaultValues();
			_playerNameInput = new PCKeyboardInputScreen(_game, "  ", Strings.Enter_Your_Name + ":", _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_playerNameInput.ClickSound = "Click";
			_playerNameInput.OpenSound = "Popup";
			_serverPasswordScreen = new PCKeyboardInputScreen(_game, Strings.Server_Password, Strings.Enter_a_password_for_this_server + ": ", _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_serverPasswordScreen.ClickSound = "Click";
			_serverPasswordScreen.OpenSound = "Popup";
			_difficultyLevelScreen = new DifficultyLevelScreen(game);
			_difficultyLevelScreen.MenuItemSelected += _difficultyLevelScreen_MenuItemSelected;
			_connectingScreen.BeforeDraw += _connectingScreen_BeforeDraw;
			_chooseOnlineGameScreen = new ChooseOnlineGameScreen();
			_chooseOnlineGameScreen.Clicked += _chooseOnlineGameScreen_Clicked;
			_chooseSavedWorldScreen = new ChooseSavedWorldScreen();
			_chooseSavedWorldScreen.Clicked += _chooseSavedWorldScreen_Clicked;
			_chooseAnotherGameScreen.BeforeDraw += _chooseAnotherGameScreen_BeforeDraw;
			_chooseAnotherGameScreen.ProcessingPlayerInput += _chooseAnotherGameScreen_ProcessingPlayerInput;
			_loadingScreen.BeforeDraw += _loadingScreen_BeforeDraw;
			optimizeStorageWaitScreen = new WaitScreen(Strings.Optimizing_Storage___, true, DeleteWorlds, null);
			optimizeStorageWaitScreen.Updating += optimizeStorageWaitScreen_Updating;
			optimizeStorageWaitScreen.ProcessingPlayerInput += optimizeStorageWaitScreen_ProcessingPlayerInput;
			optimizeStorageWaitScreen.AfterDraw += optimizeStorageWaitScreen_AfterDraw;
		}

		private void _backButton_Pressed(object sender, EventArgs e)
		{
			_uiGroup.PopScreen();
		}

		private void _chooseSavedWorldScreen_Clicked(object sender, EventArgs e)
		{
			ChooseSavedWorldScreen.SavedWorldItem info = (ChooseSavedWorldScreen.SavedWorldItem)_chooseSavedWorldScreen.SelectedItem;
			if (info.World.OwnerGamerTag != Screen.CurrentGamer.Gamertag)
			{
				_uiGroup.ShowPCDialogScreen(_chooseSavedWorldScreen._takeOverTerrain, delegate
				{
					if (_chooseSavedWorldScreen._takeOverTerrain.OptionSelected != -1)
					{
						WorldManager.TakeOwnership(info.World);
						_game.BeginLoadTerrain(info.World, true);
						HostGame(_localGame);
					}
				});
				return;
			}
			if (info.World.InfiniteResourceMode != _game.InfiniteResourceMode)
			{
				_uiGroup.ShowPCDialogScreen(_chooseSavedWorldScreen._infiniteModeConversion, delegate
				{
					if (_chooseSavedWorldScreen._infiniteModeConversion.OptionSelected != -1)
					{
						WorldManager.TakeOwnership(info.World);
						_game.BeginLoadTerrain(info.World, true);
						HostGame(_localGame);
					}
				});
				return;
			}
			_game.BeginLoadTerrain(info.World, true);
			HostGame(_localGame);
		}

		public override void OnPushed()
		{
			base.OnPushed();
		}

		private void _chooseOnlineGameScreen_Clicked(object sender, EventArgs e)
		{
			ChooseOnlineGameScreen.OnlineGameMenuItem onlineGameMenuItem = (ChooseOnlineGameScreen.OnlineGameMenuItem)_chooseOnlineGameScreen.SelectedItem;
			_chooseOnlineGameScreen.ShutdownHostDiscovery();
			JoinGame(onlineGameMenuItem.NetworkSession, onlineGameMenuItem.Password);
		}

		private void _startScreen_OnBackPressed(object sender, EventArgs e)
		{
			_game.Exit();
		}

		public void PushReleaseNotesScreen()
		{
			_uiGroup.PushScreen(_releaseNotesScreen);
		}

		private void _difficultyLevelScreen_MenuItemSelected(object sender, SelectedMenuItemArgs e)
		{
			if (e.MenuItem.Tag == null)
			{
				_uiGroup.PopScreen();
				return;
			}
			GameDifficultyTypes difficulty = (GameDifficultyTypes)e.MenuItem.Tag;
			_game.Difficulty = difficulty;
			_uiGroup.PushScreen(_chooseSavedWorldScreen);
		}

		private void _gameModeMenu_MenuItemSelected(object sender, SelectedMenuItemArgs e)
		{
			if (e.MenuItem.Tag == null)
			{
				_uiGroup.PopScreen();
				return;
			}
			GameModeTypes gameModeTypes = (GameModeTypes)e.MenuItem.Tag;
			_game.GameMode = gameModeTypes;
			_game.InfiniteResourceMode = false;
			_game.Difficulty = GameDifficultyTypes.EASY;
			_game.JoinGamePolicy = JoinGamePolicy.Anyone;
			if (_localGame)
			{
				switch (gameModeTypes)
				{
				case GameModeTypes.Endurance:
					startWorld();
					break;
				case GameModeTypes.DragonEndurance:
					if (CastleMinerZGame.TrialMode)
					{
						_uiGroup.ShowPCDialogScreen(_modeNotUnlockedDialog, delegate
						{
							if (_modeNotUnlockedDialog.OptionSelected != -1)
							{
								Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
							}
						});
					}
					else if (_game.PlayerStats.UndeadDragonKills > 0 || _game.LicenseServices.GetAddOn(AddOnIDs.DragonEndurance).HasValue)
					{
						_uiGroup.PushScreen(_chooseSavedWorldScreen);
					}
					else
					{
						_uiGroup.ShowPCDialogScreen(_undeadNotKilledDialog, delegate
						{
						});
					}
					break;
				case GameModeTypes.Survival:
					if (CastleMinerZGame.TrialMode)
					{
						_uiGroup.ShowPCDialogScreen(_modeNotUnlockedDialog, delegate
						{
							if (_modeNotUnlockedDialog.OptionSelected != -1)
							{
								Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
							}
						});
						break;
					}
					_game.GameMode = GameModeTypes.Survival;
					_uiGroup.PushScreen(_difficultyLevelScreen);
					break;
				case GameModeTypes.Scavenger:
					if (CastleMinerZGame.TrialMode)
					{
						_uiGroup.ShowPCDialogScreen(_modeNotUnlockedDialog, delegate
						{
							if (_modeNotUnlockedDialog.OptionSelected != -1)
							{
								Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
							}
						});
						break;
					}
					_game.GameMode = GameModeTypes.Scavenger;
					_uiGroup.PushScreen(_difficultyLevelScreen);
					_game.BeginLoadTerrain(null, true);
					break;
				case GameModeTypes.Creative:
					if (CastleMinerZGame.TrialMode)
					{
						_uiGroup.ShowPCDialogScreen(_modeNotUnlockedDialog, delegate
						{
							if (_modeNotUnlockedDialog.OptionSelected != -1)
							{
								Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
							}
						});
						break;
					}
					_game.GameMode = GameModeTypes.Creative;
					_game.InfiniteResourceMode = true;
					_game.Difficulty = GameDifficultyTypes.EASY;
					_uiGroup.PushScreen(_difficultyLevelScreen);
					break;
				case GameModeTypes.Exploration:
					if (CastleMinerZGame.TrialMode)
					{
						_uiGroup.ShowPCDialogScreen(_modeNotUnlockedDialog, delegate
						{
							if (_modeNotUnlockedDialog.OptionSelected != -1)
							{
								Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
							}
						});
						break;
					}
					_game.GameMode = GameModeTypes.Exploration;
					_game.InfiniteResourceMode = true;
					_game.Difficulty = GameDifficultyTypes.EASY;
					_uiGroup.PushScreen(_difficultyLevelScreen);
					break;
				}
				return;
			}
			switch (gameModeTypes)
			{
			case GameModeTypes.Creative:
				_game.GameMode = GameModeTypes.Creative;
				_game.InfiniteResourceMode = true;
				_game.Difficulty = GameDifficultyTypes.EASY;
				_uiGroup.PushScreen(_difficultyLevelScreen);
				break;
			case GameModeTypes.Exploration:
				_game.GameMode = GameModeTypes.Exploration;
				_game.InfiniteResourceMode = true;
				_game.Difficulty = GameDifficultyTypes.EASY;
				_uiGroup.PushScreen(_difficultyLevelScreen);
				break;
			case GameModeTypes.DragonEndurance:
				if (_game.PlayerStats.UndeadDragonKills > 0 || _game.LicenseServices.GetAddOn(AddOnIDs.DragonEndurance).HasValue)
				{
					_uiGroup.PushScreen(_chooseSavedWorldScreen);
				}
				else
				{
					_uiGroup.ShowPCDialogScreen(_undeadNotKilledDialog, delegate
					{
					});
				}
				break;
			case GameModeTypes.Endurance:
				startWorld();
				break;
			case GameModeTypes.Survival:
				_game.GameMode = GameModeTypes.Survival;
				_game.Difficulty = GameDifficultyTypes.EASY;
				_uiGroup.PushScreen(_difficultyLevelScreen);
				break;
			case GameModeTypes.Scavenger:
				_game.GameMode = GameModeTypes.Scavenger;
				_game.Difficulty = GameDifficultyTypes.EASY;
				_uiGroup.PushScreen(_difficultyLevelScreen);
				_game.BeginLoadTerrain(null, true);
				break;
			}
		}

		public void startWorld()
		{
			WorldTypeIDs terrainVersion = _game.CurrentWorld._terrainVersion;
			WorldManager.TakeOwnership(_game.CurrentWorld);
			_game.CurrentWorld._terrainVersion = WorldTypeIDs.CastleMinerZ;
			if (terrainVersion != _game.CurrentWorld._terrainVersion)
			{
				_game.BeginLoadTerrain(_game.CurrentWorld, true);
			}
			HostGame(_localGame);
		}

		public void ShowUIDialog(string title, string message, bool drawbehind)
		{
			PCDialogScreen pCDialogScreen = new PCDialogScreen(title, message, null, false, _game.DialogScreenImage, _game._myriadMed, drawbehind, _game.ButtonFrame);
			pCDialogScreen.UseDefaultValues();
			_uiGroup.ShowPCDialogScreen(pCDialogScreen, null);
		}

		private void JoinCallback(bool success, string message)
		{
			if (success)
			{
				_game.GetWorldInfo(delegate(WorldInfo worldInfo)
				{
					_uiGroup.PopScreen();
					WorldManager.RegisterNetworkWorld(worldInfo);
					_game.BeginLoadTerrain(worldInfo, false);
					_uiGroup.PushScreen(_loadingScreen);
					_game.WaitForTerrainLoad(delegate
					{
						_uiGroup.PopScreen();
						_game.StartGame();
					});
				});
				return;
			}
			PopToMainMenu(Screen.CurrentGamer, null);
			if (message == "Connection failed: GamerAlreadyConnected")
			{
				ShowUIDialog(Strings.Connection_Error, Strings.A_gamer_logged_in_with_these_credentials_is_already_playing_in_this_session_, false);
			}
			else if (message == null)
			{
				ShowUIDialog(Strings.Connection_Error, Strings.There_was_an_unspecified_error_connecting_, false);
			}
			else
			{
				ShowUIDialog(Strings.Connection_Error, message, false);
			}
		}

		private void JoinGame(AvailableNetworkSession session, string password)
		{
			_uiGroup.PushScreen(_connectingScreen);
			_game.JoinGame(session, new SignedInGamer[1]
			{
				Screen.CurrentGamer
			}, JoinCallback, "CastleMinerZSteam", 3, password);
		}

		private void GetPasswordForInvitedGameCallback(ClientSessionInfo sessionInfo, object context, SetPasswordForInvitedGameCallback callback)
		{
			int? num = sessionInfo.SessionProperties[1];
			if (num.GetValueOrDefault() != 0 || !num.HasValue)
			{
				TaskDispatcher.Instance.AddTaskForMainThread(delegate
				{
					callback(true, "", Strings.Game_can_no_longer_be_joined, context);
				});
			}
			else if (sessionInfo.PasswordProtected)
			{
				_serverPasswordScreen.DefaultText = "";
				_game.FrontEnd.ShowPCDialogScreen(_serverPasswordScreen, delegate
				{
					string password;
					bool cancelled;
					string errorString;
					if (_serverPasswordScreen.OptionSelected != -1)
					{
						password = _serverPasswordScreen.TextInput;
						cancelled = false;
						errorString = null;
					}
					else
					{
						password = "";
						cancelled = true;
						errorString = Strings.Action_was_cancelled;
					}
					callback(cancelled, password, errorString, context);
				});
			}
			else
			{
				TaskDispatcher.Instance.AddTaskForMainThread(delegate
				{
					callback(false, "", null, context);
				});
			}
		}

		public void JoinInvitedGame(ulong lobbyId)
		{
			_uiGroup.PushScreen(_connectingScreen);
			_game.JoinInvitedGame(lobbyId, 3, "CastleMinerZSteam", new SignedInGamer[1]
			{
				Screen.CurrentGamer
			}, JoinCallback, GetPasswordForInvitedGameCallback);
		}

		private void _startScreen_AfterDraw(object sender, DrawEventArgs e)
		{
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			_flashTimer.Update(e.GameTime.ElapsedGameTime);
			if (_flashTimer.Expired)
			{
				_flashTimer.Reset();
				_flashDir = !_flashDir;
			}
			float amount = _flashDir ? _flashTimer.PercentComplete : (1f - _flashTimer.PercentComplete);
			Color textColor = Color.Lerp(CMZColors.MenuGreen, Color.White, amount);
			Rectangle destinationRectangle = new Rectangle(screenRect.Center.X - (int)((float)_game.Logo.Width * Screen.Adjuster.ScaleFactor.Y / 2f), screenRect.Center.Y - (int)((float)_game.Logo.Height * Screen.Adjuster.ScaleFactor.Y / 2f), (int)((float)_game.Logo.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)_game.Logo.Height * Screen.Adjuster.ScaleFactor.Y));
			SpriteBatch.Begin();
			_game.Logo.Draw(SpriteBatch, destinationRectangle, Color.White);
			string text = "www.CastleMinerZ.com";
			Vector2 vector = _game._medFont.MeasureString(text);
			SpriteBatch.DrawOutlinedText(_game._medFont, text, new Vector2((float)Screen.Adjuster.ScreenRect.Center.X - vector.X / 2f, (float)Screen.Adjuster.ScreenRect.Bottom - vector.Y), Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			vector = _largeFont.MeasureString(Strings.Start_Game);
			SpriteBatch.DrawOutlinedText(_largeFont, Strings.Start_Game, new Vector2((float)Screen.Adjuster.ScreenRect.Center.X - vector.X / 2f, destinationRectangle.Bottom), textColor, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			SpriteBatch.DrawOutlinedText(_game._consoleFont, _versionString, new Vector2(0f, 0f), Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			SpriteBatch.End();
		}

		private void _startScreen_OnStartPressed(object sender, EventArgs e)
		{
			SetupSaveDevice(delegate(bool success)
			{
				if (success)
				{
					WaitScreen.DoWait(_uiGroup, Strings.Loading_Player_Info___, delegate
					{
						DateTime now = DateTime.Now;
						if (Screen.CurrentGamer != null)
						{
							SetupNewGamer(Screen.CurrentGamer, _game.SaveDevice);
							TimeSpan t = DateTime.Now - now;
							if (Screen.CurrentGamer != null)
							{
								_uiGroup.PushScreen(_mainMenu);
								if (t > TimeSpan.FromSeconds(20.0))
								{
									_uiGroup.ShowPCDialogScreen(_optimizeStorageDialog, delegate
									{
										if (_optimizeStorageDialog.OptionSelected != -1)
										{
											OptimizeStorage();
										}
									});
								}
							}
							_game.CurrentWorld.ServerMessage = Screen.CurrentGamer.Gamertag + "'s " + Strings.Server;
						}
					}, delegate
					{
						FrontEndScreen frontEndScreen = this;
						ulong lobbyId = CommandLineArgs.Get<CastleMinerZArgs>().InvitedLobbyID;
						if (lobbyId != 0)
						{
							TaskDispatcher.Instance.AddTaskForMainThread(delegate
							{
								CommandLineArgs.Get<CastleMinerZArgs>().InvitedLobbyID = 0uL;
								frontEndScreen.JoinInvitedGame(lobbyId);
							});
						}
					});
				}
			});
		}

		public void OptimizeStorage()
		{
			WaitScreen.DoWait(_uiGroup, Strings.Optimizing_Storage___, delegate
			{
				Cancel = false;
				WorldInfo[] worlds = WorldManager.GetWorlds();
				OriginalWorldsCount = 0;
				for (int i = 0; i < worlds.Length; i++)
				{
					string gamertag = Screen.CurrentGamer.Gamertag;
					if (worlds[i].OwnerGamerTag != gamertag)
					{
						OriginalWorldsCount++;
					}
				}
				OriginalWorldsCount += WorldInfo.CorruptWorlds.Count;
				CurrentWorldsCount = OriginalWorldsCount;
				optimizeStorageWaitScreen.Progress = 0;
				optimizeStorageWaitScreen.Start(_uiGroup);
			}, null);
			PopToStartScreen();
		}

		private void DeleteWorlds()
		{
			WorldManager worldManager = WorldManager;
			if (worldManager == null)
			{
				return;
			}
			WorldInfo[] worlds = worldManager.GetWorlds();
			for (int i = 0; i < worlds.Length; i++)
			{
				if (Screen.CurrentGamer == null)
				{
					return;
				}
				string gamertag = Screen.CurrentGamer.Gamertag;
				if (worlds[i].OwnerGamerTag != gamertag)
				{
					worldManager.Delete(worlds[i]);
					CurrentWorldsCount--;
				}
				if (Cancel)
				{
					break;
				}
			}
			int index = 0;
			while (WorldInfo.CorruptWorlds.Count > 0)
			{
				try
				{
					_game.SaveDevice.DeleteDirectory(WorldInfo.CorruptWorlds[index]);
				}
				catch
				{
				}
				WorldInfo.CorruptWorlds.RemoveAt(index);
				CurrentWorldsCount--;
				if (Cancel)
				{
					break;
				}
			}
			_game.SaveDevice.Flush();
		}

		private void optimizeStorageWaitScreen_AfterDraw(object sender, DrawEventArgs e)
		{
			Vector2 vector = _game._medFont.MeasureString(Strings.Press_Esc_to_Cancel);
			int num = (int)((float)Screen.Adjuster.ScreenRect.Height - vector.Y);
			int num2 = (int)((float)Screen.Adjuster.ScreenRect.Width - vector.X);
			SpriteBatch.Begin();
			SpriteBatch.DrawOutlinedText(_game._medFont, Strings.Press_Esc_to_Cancel, new Vector2(num2, num), Color.White, Color.Black, 1);
			SpriteBatch.End();
		}

		private void optimizeStorageWaitScreen_ProcessingPlayerInput(object sender, ControllerInputEventArgs e)
		{
			if (e.Controller.PressedButtons.B || e.Controller.PressedButtons.Back || e.Keyboard.WasKeyPressed(Keys.Escape))
			{
				Cancel = true;
				optimizeStorageWaitScreen.Message = Strings.Canceling___;
				optimizeStorageWaitScreen._drawProgress = false;
			}
		}

		private void optimizeStorageWaitScreen_Updating(object sender, UpdateEventArgs e)
		{
			float num = (OriginalWorldsCount <= 0) ? 1f : (1f - (float)CurrentWorldsCount / (float)OriginalWorldsCount);
			optimizeStorageWaitScreen.Progress = (int)(100f * num);
		}

		private void CloseSaveDevice()
		{
			if (_game.SaveDevice != null)
			{
				_game.SaveDevice.Dispose();
				_game.SaveDevice = null;
			}
		}

		private static void GetFiles(string path, List<string> returnedFiles)
		{
			string[] directories = Directory.GetDirectories(path);
			string[] array = directories;
			foreach (string path2 in array)
			{
				GetFiles(path2, returnedFiles);
			}
			string[] files = Directory.GetFiles(path);
			string[] array2 = files;
			foreach (string item in array2)
			{
				returnedFiles.Add(item);
			}
		}

		private void UpdateSaves(string path, byte[] newKey)
		{
			string path2 = Path.Combine(path, "save.version");
			if (!File.Exists(path2))
			{
				MD5HashProvider mD5HashProvider = new MD5HashProvider();
				byte[] data = mD5HashProvider.Compute(Encoding.UTF8.GetBytes(Screen.CurrentGamer.Gamertag + "CMZ778")).Data;
				SaveDevice saveDevice = new FileSystemSaveDevice(path, data);
				SaveDevice saveDevice2 = new FileSystemSaveDevice(path, newKey);
				List<string> list = new List<string>();
				GetFiles(path, list);
				foreach (string item in list)
				{
					byte[] dataToSave;
					try
					{
						dataToSave = saveDevice.LoadData(item);
					}
					catch
					{
						continue;
					}
					saveDevice2.Save(item, dataToSave, true, true);
				}
				using (FileStream output = File.Open(path2, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					BinaryWriter binaryWriter = new BinaryWriter(output);
					binaryWriter.Write("VER");
					binaryWriter.Write(1);
				}
			}
		}

		private void SetupSaveDevice(SuccessCallback callback)
		{
			WaitScreen waitScreen = new WaitScreen(Strings.Opening_Storage_Device);
			_uiGroup.PushScreen(waitScreen);
			CloseSaveDevice();
			ulong steamUserID = CastleMinerZGame.Instance.LicenseServices.SteamUserID;
			MD5HashProvider mD5HashProvider = new MD5HashProvider();
			byte[] data = mD5HashProvider.Compute(Encoding.UTF8.GetBytes(steamUserID + "CMZ778")).Data;
			string appDataDirectory = GlobalSettings.GetAppDataDirectory();
			string text = Path.Combine(appDataDirectory, steamUserID.ToString());
			try
			{
				string text2 = Path.Combine(appDataDirectory, Screen.CurrentGamer.Gamertag);
				if (Directory.Exists(text2))
				{
					Directory.Move(text2, text);
				}
			}
			catch (Exception)
			{
			}
			UpdateSaves(text, data);
			_game.SaveDevice = new FileSystemSaveDevice(text, data);
			callback(true);
			waitScreen.PopMe();
		}

		public void BeginSetupNewGamer(SignedInGamer gamer)
		{
			WorldManager = null;
			_game.SetupNewGamer(gamer);
		}

		public void EndSetupNewGamer(SignedInGamer gamer, SaveDevice saveDevice)
		{
			WorldManager = new WorldManager(gamer, saveDevice);
		}

		public void SetupNewGamer(SignedInGamer gamer, SaveDevice saveDevice)
		{
			BeginSetupNewGamer(gamer);
			EndSetupNewGamer(gamer, saveDevice);
		}

		private bool _allowConnectionCallbackAlt(PlayerID playerID, ulong id)
		{
			if (_game.PlayerStats.BanList.ContainsKey(id))
			{
				return false;
			}
			return true;
		}

		private void HostGame(bool local)
		{
			_uiGroup.PushScreen(_loadingScreen);
			_game.WaitForTerrainLoad(delegate
			{
				_uiGroup.PopScreen();
				_uiGroup.PushScreen(_connectingScreen);
				_game.HostGame(local, delegate(bool result)
				{
					if (result)
					{
						_game.TerrainServerID = _game.MyNetworkGamer.Id;
						_game.StartGame();
						_game.CurrentNetworkSession.AllowConnectionCallbackAlt = _allowConnectionCallbackAlt;
						if (CastleMinerZGame.Instance.CurrentNetworkSession.ExternalIPString != null && _game.PlayerStats.PostOnHost)
						{
							CastleMinerZGame.Instance.TaskScheduler.QueueUserWorkItem(delegate
							{
								//IL_0005: Unknown result type (might be due to invalid IL or missing references)
								try
								{
									new FacebookClient(CastleMinerZGame.FacebookAccessToken);
									PostToWall postToWall = new PostToWall();
									postToWall.Message = Strings.Hosting_at_internet_address + ": " + CastleMinerZGame.Instance.CurrentNetworkSession.ExternalIPString + " #CMZServer";
									postToWall.Link = "http://castleminerz.com/";
									postToWall.Description = Strings.Travel_with_your_friends_in_a_huge__ever_changing_world_and_craft_modern_weapons_to_defend_yourself_from_dragons_and_the_zombie_horde_;
									postToWall.ActionName = Strings.Download_Now;
									postToWall.ActionURL = "http://castleminerz.com/Download.html";
									postToWall.ImageURL = "http://digitaldnagames.com/Images/CastleMinerZBox.jpg";
									postToWall.AccessToken = CastleMinerZGame.FacebookAccessToken;
									postToWall.Post();
								}
								catch
								{
								}
							});
						}
					}
					else
					{
						_uiGroup.PopScreen();
						ShowUIDialog(Strings.Hosting_Error, Strings.There_was_an_error_hosting_the_game_, false);
					}
				});
			});
		}

		private void _mainMenu_MenuItemSelected(object sender, SelectedMenuItemArgs e)
		{
			if (WorldManager == null)
			{
				return;
			}
			switch ((MainMenuItems)e.MenuItem.Tag)
			{
			case MainMenuItems.HostOnline:
				if (CastleMinerZGame.TrialMode)
				{
					_uiGroup.ShowPCDialogScreen(_onlinePlayNotPurchasedDialog, delegate
					{
						if (_onlinePlayNotPurchasedDialog.OptionSelected != -1)
						{
							Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
						}
					});
					break;
				}
				_localGame = false;
				_hostGame = true;
				_uiGroup.PushScreen(_gameModeMenu);
				break;
			case MainMenuItems.JoinOnline:
				if (CastleMinerZGame.TrialMode)
				{
					_uiGroup.ShowPCDialogScreen(_onlinePlayNotPurchasedDialog, delegate
					{
						if (_onlinePlayNotPurchasedDialog.OptionSelected != -1)
						{
							Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
						}
					});
					break;
				}
				_localGame = false;
				_hostGame = false;
				_game.GetNetworkSessions(delegate(AvailableNetworkSessionCollection result)
				{
					_chooseOnlineGameScreen.Populate(result);
					_uiGroup.PopScreen();
					_uiGroup.PushScreen(_chooseOnlineGameScreen);
				});
				_uiGroup.PushScreen(_connectingScreen);
				break;
			case MainMenuItems.PlayOffline:
				_localGame = true;
				_uiGroup.PushScreen(_gameModeMenu);
				break;
			case MainMenuItems.Purchase:
				Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
				break;
			case MainMenuItems.Options:
				_uiGroup.PushScreen(_optionsScreen);
				break;
			case MainMenuItems.Quit:
				ConfirmExit();
				break;
			}
		}

		public void ConfirmExit()
		{
			_uiGroup.ShowPCDialogScreen(_quitDialog, delegate
			{
				if (_quitDialog.OptionSelected != -1)
				{
					_game.Exit();
				}
			});
		}

		private void _loadingScreen_BeforeDraw(object sender, DrawEventArgs e)
		{
			float loadProgress = _game.LoadProgress;
			string text = Strings.Loading_The_World____ + Strings.Please_Wait___;
			float num = (float)Screen.Adjuster.ScreenRect.Width * 0.8f;
			float num2 = (float)Screen.Adjuster.ScreenRect.Left + ((float)Screen.Adjuster.ScreenRect.Width - num) / 2f;
			Sprite sprite = _game._uiSprites["Bar"];
			Vector2 vector = _largeFont.MeasureString(text);
			Vector2 location = new Vector2(num2, (float)(Screen.Adjuster.ScreenRect.Height / 2) + vector.Y);
			float num3 = location.Y + (float)_largeFont.LineSpacing + 10f * Screen.Adjuster.ScaleFactor.Y;
			Rectangle rectangle = new Rectangle((int)num2, (int)num3, (int)num, _largeFont.LineSpacing);
			int left = rectangle.Left;
			int top = rectangle.Top;
			float num6 = (float)rectangle.Width / (float)sprite.Width;
			SpriteBatch.Begin();
			int num4 = (int)((float)_game.Logo.Width * Screen.Adjuster.ScaleFactor.X);
			int height = (int)((float)_game.Logo.Height * Screen.Adjuster.ScaleFactor.Y);
			Rectangle destinationRectangle = new Rectangle(Screen.Adjuster.ScreenRect.Center.X - num4 / 2, 0, num4, height);
			_game.Logo.Draw(SpriteBatch, destinationRectangle, Color.White);
			SpriteBatch.DrawOutlinedText(_largeFont, text, location, Color.White, Color.Black, 1);
			SpriteBatch.Draw(_game.DummyTexture, new Rectangle(left - 2, top - 2, rectangle.Width + 4, rectangle.Height + 4), Color.White);
			SpriteBatch.Draw(_game.DummyTexture, new Rectangle(left, top, rectangle.Width, rectangle.Height), Color.Black);
			int num5 = (int)((float)sprite.Width * loadProgress);
			sprite.Draw(SpriteBatch, new Rectangle(left, top, (int)((float)rectangle.Width * loadProgress), rectangle.Height), new Rectangle(sprite.Width - num5, 0, num5, sprite.Height), Color.White);
			textFlashTimer.Update(e.GameTime.ElapsedGameTime);
			Color.Lerp(Color.Green, Color.White, textFlashTimer.PercentComplete);
			if (textFlashTimer.Expired)
			{
				textFlashTimer.Reset();
			}
			SpriteBatch.End();
		}

		private void _connectingScreen_BeforeDraw(object sender, DrawEventArgs e)
		{
			string text = Strings.Connecting____ + Strings.Please_Wait___;
			Vector2 vector = _largeFont.MeasureString(text);
			Vector2 location = new Vector2((float)(Screen.Adjuster.ScreenRect.Width / 2) - vector.X / 2f, (float)(Screen.Adjuster.ScreenRect.Height / 2) + vector.Y);
			textFlashTimer.Update(e.GameTime.ElapsedGameTime);
			Color textColor = Color.Lerp(Color.Green, Color.White, textFlashTimer.PercentComplete);
			if (textFlashTimer.Expired)
			{
				textFlashTimer.Reset();
			}
			SpriteBatch.Begin();
			int num = (int)((float)_game.Logo.Width * Screen.Adjuster.ScaleFactor.X);
			int height = (int)((float)_game.Logo.Height * Screen.Adjuster.ScaleFactor.Y);
			Rectangle destinationRectangle = new Rectangle(Screen.Adjuster.ScreenRect.Center.X - num / 2, 0, num, height);
			_game.Logo.Draw(SpriteBatch, destinationRectangle, Color.White);
			SpriteBatch.DrawOutlinedText(_largeFont, text, location, textColor, Color.Black, 1);
			SpriteBatch.End();
		}

		private void _chooseAnotherGameScreen_ProcessingPlayerInput(object sender, ControllerInputEventArgs e)
		{
			if (e.Controller.PressedButtons.A || e.Controller.PressedButtons.B || e.Controller.PressedButtons.Back || e.Keyboard.WasKeyPressed(Keys.Escape) || e.Keyboard.WasKeyPressed(Keys.Enter) || e.Mouse.LeftButtonPressed)
			{
				_uiGroup.PopScreen();
			}
		}

		private void _chooseAnotherGameScreen_BeforeDraw(object sender, DrawEventArgs e)
		{
			SpriteBatch.Begin();
			string session_Ended = Strings.Session_Ended;
			Vector2 vector = _largeFont.MeasureString(session_Ended);
			int lineSpacing = _largeFont.LineSpacing;
			SpriteBatch.DrawOutlinedText(_largeFont, session_Ended, new Vector2(640f - vector.X / 2f, 360f - vector.Y / 2f), Color.White, Color.Black, 2);
			SpriteBatch.End();
		}

		public void PopToStartScreen()
		{
			while (_uiGroup.CurrentScreen != _startScreen && _uiGroup.CurrentScreen != null)
			{
				_uiGroup.PopScreen();
			}
			if (_uiGroup.CurrentScreen == null)
			{
				_uiGroup.PushScreen(_startScreen);
			}
			_game.SetAudio(1f, 0f, 0f, 0f);
			_game.PlayMusic("Theme");
		}

		public void PopToMainMenu(SignedInGamer gamer, SuccessCallback callback)
		{
			while (_uiGroup.CurrentScreen != _mainMenu && _uiGroup.CurrentScreen != null)
			{
				_uiGroup.PopScreen();
			}
			Screen.SelectedPlayerIndex = gamer.PlayerIndex;
			if (_uiGroup.CurrentScreen == null && _game.SaveDevice != null)
			{
				CloseSaveDevice();
			}
			_game.SetAudio(1f, 0f, 0f, 0f);
			_game.PlayMusic("Theme");
			if (_uiGroup.CurrentScreen == null)
			{
				_uiGroup.PushScreen(_startScreen);
			}
			if (_game.SaveDevice == null)
			{
				SetupSaveDevice(delegate(bool success)
				{
					_uiGroup.PushScreen(_mainMenu);
					callback(success);
				});
			}
			else if (callback != null)
			{
				callback(true);
			}
		}
	}
}
