using DNA.Audio;
using DNA.Avatars;
using DNA.CastleMinerZ.Achievements;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.GraphicsProfileSupport;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.CastleMinerZ.Utils.Threading;
using DNA.Distribution;
using DNA.Drawing;
using DNA.Drawing.Animation;
using DNA.Drawing.UI;
using DNA.IO.Storage;
using DNA.Net;
using DNA.Net.GamerServices;
using DNA.Net.MatchMaking;
using DNA.Profiling;
using DNA.Text;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace DNA.CastleMinerZ
{
	public class CastleMinerZGame : DNAGame
	{
		public enum NetworkProps
		{
			Version,
			JoinGame,
			GameMode,
			Difficulty,
			InfiniteResources,
			PVP,
			COUNT
		}

		public enum PVPEnum
		{
			Off,
			Everyone,
			NotFriends
		}

		public delegate void WorldInfoCallback(WorldInfo info);

		public delegate void GotSessionsCallback(AvailableNetworkSessionCollection sessions);

		private class SaveDataInfo
		{
			public WorldInfo Worldinfo;

			public PlayerInventory Inventory;

			public CastleMinerZPlayerStats PlayerStats;
		}

		private struct InventoryFromMessage
		{
			public PlayerInventory Inventory;

			public bool IsDefault;

			public InventoryFromMessage(PlayerInventory inventory, bool isDefault)
			{
				Inventory = inventory;
				IsDefault = isDefault;
			}
		}

		public const int NetworkVersion = 3;

		public const string NetworkGameName = "CastleMinerZSteam";

		private static Version GameVersion;

		public static CastleMinerZGame Instance;

		private Player _localPlayer;

		public AudioListener Listener = new AudioListener();

		public BlockTerrain _terrain;

		public SpriteFont _nameTagFont;

		public SpriteFont _largeFont;

		public SpriteFont _medFont;

		public SpriteFont _medLargeFont;

		public SpriteFont _smallFont;

		public SpriteFont _systemFont;

		public SpriteFont _consoleFont;

		public SpriteFont _myriadLarge;

		public SpriteFont _myriadMed;

		public SpriteFont _myriadSmall;

		public CastleMinerZPlayerStats PlayerStats = new CastleMinerZPlayerStats();

		public CastleMinerZAchievementManager AcheivmentManager;

		private AvatarDescription _myAvatarDescription;

		public SpriteManager _uiSprites;

		public ScalableFrame ButtonFrame;

		public CastleMinerZControllerMapping _controllerMapping = new CastleMinerZControllerMapping();

		public WorldInfo CurrentWorld;

		public FrontEndScreen FrontEnd;

		public GameScreen GameScreen;

		public Texture2D DialogScreenImage;

		public Texture2D MenuBackdrop;

		public byte TerrainServerID;

		public bool DrawingReflection;

		public LocalNetworkGamer MyNetworkGamer;

		public SaveDevice SaveDevice;

		public GameModeTypes GameMode;

		public bool InfiniteResourceMode;

		public GameDifficultyTypes Difficulty;

		public JoinGamePolicy _joinGamePolicy;

		public bool RequestEndGame;

		public static GlobalSettings GlobalSettings;

		public static bool TrialMode;

		public OneShotTimer TrialModeTimer = new OneShotTimer(TimeSpan.FromMinutes(8.0));

		public static string FacebookAccessToken;

		public AudioCategory MusicSounds;

		public AudioCategory DaySounds;

		public AudioCategory NightSounds;

		public AudioCategory CaveSounds;

		public AudioCategory HellSounds;

		public Cue MusicCue;

		public Cue DayCue;

		public Cue NightCue;

		public Cue CaveCue;

		public Cue HellCue;

		public Sprite Logo;

		private bool _waitToExit = true;

		private bool _exitRequested;

		public ScreenGroup mainScreenGroup = new ScreenGroup(false);

		public ScreenGroup overlayScreenGroup = new ScreenGroup(true);

		private ThreadStart _waitForTerrainCallback;

		private WorldInfoCallback _waitForWorldInfo;

		private bool _saving;

		private object saveLock = new object();

		public static readonly int[] SaveProcessorAffinity;

		private bool _savingTerrain;

		private int _currentFrameNumber;

		private OneShotTimer _worldUpdateTimer = new OneShotTimer(TimeSpan.FromSeconds(5.0));

		private bool _fadeMusic;

		private OneShotTimer musicFadeTimer = new OneShotTimer(TimeSpan.FromSeconds(3.0));

		private Entity holdingGround = new Entity();

		public PVPEnum PVPState
		{
			get
			{
				if (IsOnlineGame && base.CurrentNetworkSession.SessionProperties[5].HasValue)
				{
					return (PVPEnum)base.CurrentNetworkSession.SessionProperties[5].Value;
				}
				return PVPEnum.Off;
			}
			set
			{
				if (IsOnlineGame && base.CurrentNetworkSession.SessionProperties[5] != (int?)value)
				{
					base.CurrentNetworkSession.SessionProperties[5] = (int)value;
					if (IsOnlineGame)
					{
						base.CurrentNetworkSession.UpdateHostSession(null, null, null, base.CurrentNetworkSession.SessionProperties);
					}
				}
			}
		}

		public bool IsEnduranceMode
		{
			get
			{
				if (Instance.GameMode != 0)
				{
					return Instance.GameMode == GameModeTypes.DragonEndurance;
				}
				return true;
			}
		}

		public bool IsPublicGame
		{
			get
			{
				return _joinGamePolicy == JoinGamePolicy.Anyone;
			}
			set
			{
				if (value != IsPublicGame)
				{
					JoinGamePolicy = ((!value) ? JoinGamePolicy.FriendsOnly : JoinGamePolicy.Anyone);
				}
			}
		}

		public JoinGamePolicy JoinGamePolicy
		{
			get
			{
				return _joinGamePolicy;
			}
			set
			{
				if (value != _joinGamePolicy)
				{
					_joinGamePolicy = value;
					if (IsOnlineGame)
					{
						base.CurrentNetworkSession.UpdateHostSessionJoinPolicy(_joinGamePolicy);
					}
				}
			}
		}

		internal CastleMinerZArgs CommandLine
		{
			get
			{
				return CommandLineArgs.Get<CastleMinerZArgs>();
			}
		}

		public bool IsAvatarLoaded
		{
			get
			{
				return _myAvatarDescription != null;
			}
		}

		public float LoadProgress
		{
			get
			{
				return (float)_terrain.LoadingProgress / 100f;
			}
		}

		public bool IsOnlineGame
		{
			get
			{
				if (base.CurrentNetworkSession != null)
				{
					return base.CurrentNetworkSession.SessionType == NetworkSessionType.PlayerMatch;
				}
				return false;
			}
		}

		public bool IsGameHost
		{
			get
			{
				if (MyNetworkGamer != null)
				{
					return MyNetworkGamer.Id == TerrainServerID;
				}
				return false;
			}
		}

		public override string ServerMessage
		{
			get
			{
				string serverMessage = base.ServerMessage;
				if (serverMessage == null && CurrentWorld != null)
				{
					serverMessage = CurrentWorld.ServerMessage;
				}
				return serverMessage;
			}
			set
			{
				if (value != CurrentWorld.ServerMessage && IsOnlineGame)
				{
					base.CurrentNetworkSession.UpdateHostSession(value, null, null, null);
				}
				if (CurrentWorld != null)
				{
					CurrentWorld.ServerMessage = value;
				}
				base.ServerMessage = value;
			}
		}

		public Player LocalPlayer
		{
			get
			{
				return _localPlayer;
			}
		}

		public override OnlineServices LicenseServices
		{
			set
			{
				base.LicenseServices = value;
			}
		}

		public bool IsFullScreen
		{
			get
			{
				return Graphics.IsFullScreen;
			}
			set
			{
				if (Graphics.IsFullScreen != value)
				{
					Graphics.IsFullScreen = value;
					Graphics.ApplyChanges();
				}
			}
		}

		static CastleMinerZGame()
		{
			GameVersion = new Version(1, 7, 0);
			GlobalSettings = new GlobalSettings();
			TrialMode = true;
			SaveProcessorAffinity = new int[1]
			{
				5
			};
			GameVersion = Assembly.GetExecutingAssembly().GetName().Version;
		}

		public CastleMinerZGame()
			: base(GlobalSettings.ScreenSize, false, GameVersion)
		{
			Instance = this;
			_controllerMapping.SetToDefault();
			if (Debugger.IsAttached)
			{
				WantProfiling(false, true);
			}
			base.Content = new ProfiledContentManager(base.Services, "ReachContent", "HiDefContent", GlobalSettings.TextureQualityLevel);
			base.Content.RootDirectory = "Content";
			Graphics.PreparingDeviceSettings += GraphicsProfileManager.Instance.ExamineGraphicsDevices;
			Profiler.Profiling = false;
			Profiler.SetColor("Zombie Update", Color.Blue);
			Profiler.SetColor("Zombie Collision", Color.Red);
			Profiler.SetColor("Drawing Terrain", Color.Green);
			Graphics.SynchronizeWithVerticalRetrace = true;
			base.IsFixedTimeStep = false;
			PauseDuringGuide = false;
			StartGamerServices();
			TaskDispatcher.Create();
			base.Window.AllowUserResizing = false;
			IsFullScreen = GlobalSettings.FullScreen;
		}

		public bool IsLocalPlayerId(byte id)
		{
			if (LocalPlayer != null && LocalPlayer.Gamer != null)
			{
				return LocalPlayer.Gamer.Id == id;
			}
			return false;
		}

		protected override void SecondaryLoad()
		{
			SoundManager.ActiveListener = Listener;
			Texture2D texture2D = base.Content.Load<Texture2D>("UI\\Screens\\LoadScreen");
			LoadScreen loadScreen = new LoadScreen(texture2D, TimeSpan.FromSeconds(10.300000190734863));
			MainThreadMessageSender.Init();
			mainScreenGroup.PushScreen(loadScreen);
			base.ScreenManager.PushScreen(mainScreenGroup);
			base.ScreenManager.PushScreen(overlayScreenGroup);
			SoundManager.Instance.Load("Sounds");
			DaySounds = SoundManager.Instance.GetCatagory("AmbientDay");
			NightSounds = SoundManager.Instance.GetCatagory("AmbientNight");
			CaveSounds = SoundManager.Instance.GetCatagory("AmbientCave");
			MusicSounds = SoundManager.Instance.GetCatagory("Music");
			HellSounds = SoundManager.Instance.GetCatagory("AmbientHell");
			PlayMusic("Theme");
			SetAudio(1f, 0f, 0f, 0f);
			ControllerImages.Load(base.Content);
			MenuBackdrop = base.Content.Load<Texture2D>("UI\\Screens\\MenuBack");
			_terrain = new BlockTerrain(base.GraphicsDevice, base.Content);
			InventoryItem.Initalize(base.Content);
			BlockEntity.Initialize();
			TracerManager.Initialize();
			string str = "Fonts\\";
			_consoleFont = base.Content.LoadLocalized<SpriteFont>(str + "ConsoleFont");
			_largeFont = base.Content.LoadLocalized<SpriteFont>(str + "LargeFont");
			_medFont = base.Content.LoadLocalized<SpriteFont>(str + "MedFont");
			_medLargeFont = base.Content.LoadLocalized<SpriteFont>(str + "MedLargeFont");
			_smallFont = base.Content.LoadLocalized<SpriteFont>(str + "SmallFont");
			_systemFont = base.Content.LoadLocalized<SpriteFont>(str + "System");
			_nameTagFont = base.Content.LoadLocalized<SpriteFont>(str + "NameTagFont");
			_myriadLarge = base.Content.LoadLocalized<SpriteFont>(str + "MyriadLarge");
			_myriadMed = base.Content.LoadLocalized<SpriteFont>(str + "MyriadMedium");
			_myriadSmall = base.Content.LoadLocalized<SpriteFont>(str + "MyriadSmall");
			_uiSprites = base.Content.Load<SpriteManager>("UI\\SpriteSheet");
			DialogScreenImage = base.Content.Load<Texture2D>("UI\\Screens\\DialogBack");
			Logo = _uiSprites["Logo"];
			ButtonFrame = new ScalableFrame(_uiSprites, "CtrlFrame");
			PCDialogScreen.DefaultTitlePadding = new Vector2(55f, 15f);
			PCDialogScreen.DefaultDescriptionPadding = new Vector2(25f, 35f);
			PCDialogScreen.DefaultButtonsPadding = new Vector2(15f, 23f);
			PCDialogScreen.DefaultClickSound = "Click";
			PCDialogScreen.DefaultOpenSound = "Popup";
			ProfilerUtils.SystemFont = _systemFont;
			EnemyType.Init();
			DragonType.Init();
			FireballEntity.Init();
			DragonClientEntity.Init();
			RocketEntity.Init();
			BlasterShot.Init();
			GrenadeProjectile.Init();
			AvatarAnimationManager.Instance.RegisterAnimation("Swim", base.Content.Load<AnimationClip>("Character\\Animation\\Swim Underwater"), true);
			AvatarAnimationManager.Instance.RegisterAnimation("Wave", base.Content.Load<AnimationClip>("Character\\Animation\\Wave"), true);
			AvatarAnimationManager.Instance.RegisterAnimation("Run", base.Content.Load<AnimationClip>("Character\\Animation\\Run"), true);
			AvatarAnimationManager.Instance.RegisterAnimation("Walk", base.Content.Load<AnimationClip>("Character\\Animation\\Walk"), true);
			AvatarAnimationManager.Instance.RegisterAnimation("Die", base.Content.Load<AnimationClip>("Character\\Animation\\Faint"), false);
			AvatarAnimationManager.Instance.RegisterAnimation("RPGIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\RPG\\Animation\\Idle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("RPGWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\RPG\\Animation\\Walk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("RPGShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\RPG\\Animation\\Shoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("GunReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\AR\\Animation\\Reload"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("GunShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\AR\\Animation\\ShoulderIdle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("GunShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\AR\\Animation\\ShoulderWalk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("GunShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\AR\\Animation\\ShoulderShoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("GunShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\AR\\Animation\\Shoulder"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("GunShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\AR\\Animation\\Shoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("GunIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\AR\\Animation\\Idle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("GunRun", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\AR\\Animation\\Walk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserGunReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\AR\\Animation\\Reload"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserGunShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\AR\\Animation\\ShoulderIdle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserGunShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\AR\\Animation\\ShoulderWalk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserGunShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\AR\\Animation\\ShoulderShoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserGunShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\AR\\Animation\\Shoulder"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserGunShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\AR\\Animation\\Shoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserGunIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\AR\\Animation\\Idle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserGunRun", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\AR\\Animation\\Walk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserSMGReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\SMG\\Animation\\Reload"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserSMGShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\SMG\\Animation\\ShoulderIdle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserSMGShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\SMG\\Animation\\ShoulderWalk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserSMGShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\SMG\\Animation\\ShoulderShoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserSMGShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\SMG\\Animation\\Shoulder"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserSMGShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\SMG\\Animation\\Shoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserSMGIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\SMG\\Animation\\Idle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserSMGRun", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\SMG\\Animation\\Walk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserPistolReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Pistol\\Animation\\Reload"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserPistolShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Pistol\\Animation\\ShoulderIdle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserPistolShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Pistol\\Animation\\ShoulderWalk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserPistolShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Pistol\\Animation\\ShoulderShoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserPistolShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Pistol\\Animation\\Shoulder"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserPistolShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Pistol\\Animation\\Shoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserPistolIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Pistol\\Animation\\Idle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserPistolRun", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Pistol\\Animation\\Walk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserRifleReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Rifle\\Animation\\Reload"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserRifleShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Rifle\\Animation\\ShoulderIdle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserRifleShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Rifle\\Animation\\ShoulderWalk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserRifleShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Rifle\\Animation\\ShoulderShoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserRifleShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Rifle\\Animation\\Shoulder"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserRifleShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Rifle\\Animation\\Shoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserRifleIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Rifle\\Animation\\Idle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserRifleRun", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Rifle\\Animation\\Walk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserShotgunReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Shotgun\\Animation\\Reload"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserShotgunShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Shotgun\\Animation\\ShoulderShoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LaserShotgunShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Shotgun\\Animation\\Shoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LMGIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\LMG\\Animation\\Idle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LMGWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\LMG\\Animation\\Walk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LMGShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\LMG\\Animation\\Shoulder"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LMGShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\LMG\\Animation\\ShoulderWalk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LMGReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\LMG\\Animation\\Reload"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LMGShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\LMG\\Animation\\Shoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LMGShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\LMG\\Animation\\ShoulderIdle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("LMGShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\LMG\\Animation\\ShoulderShoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PistolIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Pistol\\Animation\\Idle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PistolWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Pistol\\Animation\\Walk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PistolShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Pistol\\Animation\\Shoulder"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PistolShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Pistol\\Animation\\ShoulderWalk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PistolReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Pistol\\Animation\\Reload"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PistolShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Pistol\\Animation\\Shoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PistolShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Pistol\\Animation\\ShoulderIdle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PistolShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Pistol\\Animation\\ShoulderShoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PumpShotgunIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Shotgun\\Animation\\Idle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PumpShotgunRun", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Shotgun\\Animation\\Walk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PumpShotgunShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Shotgun\\Animation\\Shoulder"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PumpShotgunShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Shotgun\\Animation\\ShoulderWalk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PumpShotgunReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Shotgun\\Animation\\Reload"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PumpShotgunShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Shotgun\\Animation\\Shoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PumpShotgunShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Shotgun\\Animation\\ShoulderIdle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PumpShotgunShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Shotgun\\Animation\\ShoulderShoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("RifleIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Rifle\\Animation\\Idle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("RifleWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Rifle\\Animation\\Walk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("RifleShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Rifle\\Animation\\Shoulder"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("RifleShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Rifle\\Animation\\ShoulderWalk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("RifleReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Rifle\\Animation\\Reload"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("RifleShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Rifle\\Animation\\Shoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("RifleShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Rifle\\Animation\\ShoulderIdle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("RifleShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Rifle\\Animation\\ShoulderShoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("SMGIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\SMG\\Animation\\Idle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("SMGWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\SMG\\Animation\\Walk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("SMGShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\SMG\\Animation\\Shoulder"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("SMGShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\SMG\\Animation\\ShoulderWalk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("SMGReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\SMG\\Animation\\Reload"), false, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("SMGShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\SMG\\Animation\\Shoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("SMGShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\SMG\\Animation\\ShoulderIdle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("SMGShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\SMG\\Animation\\ShoulderShoot"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("GenericIdle", base.Content.Load<AnimationClip>("Character\\Animation\\GenericIdle"), true, new AvatarBone[1]
			{
				AvatarBone.CollarRight
			});
			AvatarAnimationManager.Instance.RegisterAnimation("GenericUse", base.Content.Load<AnimationClip>("Character\\Animation\\GenericUse"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("GenericWalk", base.Content.Load<AnimationClip>("Character\\Animation\\GenericWalk"), true, new AvatarBone[1]
			{
				AvatarBone.CollarRight
			});
			AvatarAnimationManager.Instance.RegisterAnimation("FistIdle", base.Content.Load<AnimationClip>("Props\\Tools\\PickAxe\\Animation\\Idle"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("FistUse", base.Content.Load<AnimationClip>("Props\\Tools\\PickAxe\\Animation\\Use"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("FistWalk", base.Content.Load<AnimationClip>("Props\\Tools\\PickAxe\\Animation\\Walk"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PickIdle", base.Content.Load<AnimationClip>("Props\\Tools\\PickAxe\\Animation\\Idle"), true, new AvatarBone[1]
			{
				AvatarBone.CollarRight
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PickUse", base.Content.Load<AnimationClip>("Props\\Tools\\PickAxe\\Animation\\Use"), true, new AvatarBone[1]
			{
				AvatarBone.CollarRight
			});
			AvatarAnimationManager.Instance.RegisterAnimation("PickWalk", base.Content.Load<AnimationClip>("Props\\Tools\\PickAxe\\Animation\\Walk"), true, new AvatarBone[1]
			{
				AvatarBone.CollarRight
			});
			AvatarAnimationManager.Instance.RegisterAnimation("BlockIdle", base.Content.Load<AnimationClip>("Props\\Items\\Block\\Animation\\Idle"), true, new AvatarBone[1]
			{
				AvatarBone.CollarRight
			});
			AvatarAnimationManager.Instance.RegisterAnimation("BlockUse", base.Content.Load<AnimationClip>("Props\\Items\\Block\\Animation\\Use"), true, new AvatarBone[1]
			{
				AvatarBone.CollarRight
			});
			AvatarAnimationManager.Instance.RegisterAnimation("BlockWalk", base.Content.Load<AnimationClip>("Props\\Items\\Block\\Animation\\Walk"), true, new AvatarBone[1]
			{
				AvatarBone.CollarRight
			});
			AvatarAnimationManager.Instance.RegisterAnimation("Grenade_Reset", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Grenade\\Animation\\Release"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("Grenade_Throw", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Grenade\\Animation\\Throw"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("Grenade_Cook", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Grenade\\Animation\\Cook"), true, new AvatarBone[1]
			{
				AvatarBone.BackUpper
			});
			AvatarAnimationManager.Instance.RegisterAnimation("GrenadeIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Grenade\\Animation\\Idle"), true, new AvatarBone[1]
			{
				AvatarBone.CollarRight
			});
			AvatarAnimationManager.Instance.RegisterAnimation("GrenadeWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Grenade\\Animation\\Walk"), true, new AvatarBone[1]
			{
				AvatarBone.CollarRight
			});
			AvatarAnimationManager.Instance.RegisterAnimation("Stand", base.Content.Load<AnimationClip>("Character\\Animation\\Stand0"), true);
			AvatarAnimationManager.Instance.RegisterAnimation("IdleHead", base.Content.Load<AnimationClip>("Character\\Animation\\MaleIdleLookAround"), true, new AvatarBone[1]
			{
				AvatarBone.Neck
			});
			AvatarAnimationManager.Instance.RegisterAnimation("Tilt", base.Content.Load<AnimationClip>("Character\\Animation\\Tilt"), true, new AvatarBone[1]
			{
				AvatarBone.BackLower
			}, new AvatarBone[2]
			{
				AvatarBone.CollarRight,
				AvatarBone.CollarLeft
			});
			FrontEnd = new FrontEndScreen(this);
			BeginLoadTerrain(null, true);
			while (!loadScreen.Finished && !_exitRequested)
			{
				Thread.Sleep(50);
			}
			mainScreenGroup.PopScreen();
			mainScreenGroup.PushScreen(FrontEnd);
			texture2D.Dispose();
			NetworkSession.InviteAccepted += NetworkSession_InviteAccepted;
			base.SecondaryLoad();
			_waitToExit = false;
		}

		private void NetworkSession_InviteAccepted(object sender, InviteAcceptedEventArgs e)
		{
			DNA.Drawing.UI.Screen.SelectedPlayerIndex = e.Gamer.PlayerIndex;
			if (Guide.IsTrialMode)
			{
				ShowMarketPlace();
				return;
			}
			if (base.CurrentNetworkSession != null)
			{
				EndGame(true);
			}
			FrontEnd.PopToMainMenu(e.Gamer, delegate(bool success)
			{
				if (success)
				{
					WaitScreen.DoWait(FrontEnd._uiGroup, Strings.Loading_Player_Info___, delegate
					{
						FrontEnd.SetupNewGamer(e.Gamer, SaveDevice);
					}, delegate
					{
						TaskDispatcher.Instance.AddTaskForMainThread(delegate
						{
							FrontEnd.JoinInvitedGame(e.LobbyId);
						});
					});
				}
			});
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			_exitRequested = true;
			while (_waitToExit || _saving)
			{
				Thread.Sleep(50);
			}
			try
			{
				if (ChunkCache.Instance != null)
				{
					ChunkCache.Instance.Stop(false);
				}
				if (TaskDispatcher.Instance != null)
				{
					TaskDispatcher.Instance.Stop();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			base.OnExiting(sender, args);
		}

		public void BeginLoadTerrain(WorldInfo info, bool host)
		{
			if (info == null)
			{
				CurrentWorld = WorldInfo.CreateNewWorld(null);
			}
			else
			{
				CurrentWorld = info;
			}
			_terrain.AsyncInit(CurrentWorld, host, delegate
			{
				_savingTerrain = false;
			});
		}

		public void WaitForTerrainLoad(ThreadStart callback)
		{
			_waitForTerrainCallback = callback;
		}

		public void GetWorldInfo(WorldInfoCallback callback)
		{
			_waitForWorldInfo = callback;
			RequestWorldInfoMessage.Send(MyNetworkGamer);
		}

		public override void StartGamerServices()
		{
			base.Components.Add(new GamerServicesComponent(this, "CastleMinerZ"));
			HasGamerServices = true;
		}

		public void HostGame(bool local, SuccessCallback callback)
		{
			NetworkSessionProperties networkSessionProperties = new NetworkSessionProperties();
			networkSessionProperties[0] = 3;
			networkSessionProperties[2] = (int)GameMode;
			networkSessionProperties[1] = 0;
			networkSessionProperties[3] = (int)Difficulty;
			networkSessionProperties[5] = (int)PVPState;
			if (InfiniteResourceMode)
			{
				networkSessionProperties[4] = 1;
			}
			else
			{
				networkSessionProperties[4] = 0;
			}
			if (local)
			{
				HostGame(NetworkSessionType.Local, networkSessionProperties, new SignedInGamer[1]
				{
					DNA.Drawing.UI.Screen.CurrentGamer
				}, 2, false, true, callback, "CastleMinerZSteam", 3, CurrentWorld.ServerMessage, null);
			}
			else
			{
				HostGame(NetworkSessionType.PlayerMatch, networkSessionProperties, new SignedInGamer[1]
				{
					DNA.Drawing.UI.Screen.CurrentGamer
				}, 16, false, true, callback, "CastleMinerZSteam", 3, CurrentWorld.ServerMessage, string.IsNullOrWhiteSpace(CurrentWorld.ServerPassword) ? null : CurrentWorld.ServerPassword);
			}
		}

		public void GetNetworkSessions(GotSessionsCallback callback)
		{
			NetworkSessionProperties networkSessionProperties = new NetworkSessionProperties();
			for (int i = 0; i < networkSessionProperties.Count; i++)
			{
				networkSessionProperties[i] = null;
			}
			networkSessionProperties[0] = 3;
			networkSessionProperties[1] = 0;
			QuerySessionInfo querySessionInfo = new QuerySessionInfo();
			querySessionInfo._props = networkSessionProperties;
			NetworkSession.BeginFind(NetworkSessionType.PlayerMatch, new SignedInGamer[1]
			{
				DNA.Drawing.UI.Screen.CurrentGamer
			}, querySessionInfo, delegate(IAsyncResult result)
			{
				AvailableNetworkSessionCollection sessions = null;
				try
				{
					sessions = NetworkSession.EndFind(result);
				}
				catch
				{
				}
				try
				{
					GotSessionsCallback gotSessionsCallback = (GotSessionsCallback)result.AsyncState;
					if (gotSessionsCallback != null)
					{
						gotSessionsCallback(sessions);
					}
				}
				catch (Exception e)
				{
					CrashGame(e);
				}
			}, callback);
		}

		public void StartGame()
		{
			if (CurrentWorld != null)
			{
				ServerMessage = CurrentWorld.ServerMessage;
			}
			PlayerStats.GamesPlayed++;
			PlayerExistsMessage.Send(MyNetworkGamer, _myAvatarDescription, true);
			Difficulty = (GameDifficultyTypes)base.CurrentNetworkSession.SessionProperties[3].Value;
		}

		public void SaveData()
		{
			if (!_saving)
			{
				SaveDataInfo saveDataInfo = new SaveDataInfo();
				if (GameScreen != null && GameScreen.HUD != null)
				{
					saveDataInfo.Inventory = GameScreen.HUD.PlayerInventory;
					saveDataInfo.Worldinfo = CurrentWorld;
					saveDataInfo.PlayerStats = PlayerStats;
					TaskScheduler.QueueUserWorkItem(SaveDataInternal, saveDataInfo);
				}
			}
		}

		public void SavePlayerStats(CastleMinerZPlayerStats playerStats)
		{
			lock (saveLock)
			{
				if (DNA.Drawing.UI.Screen.CurrentGamer != null && !DNA.Drawing.UI.Screen.CurrentGamer.IsGuest)
				{
					SaveDevice.Save("stats.sav", true, true, delegate(Stream stream)
					{
						BinaryWriter binaryWriter = new BinaryWriter(stream);
						playerStats.Save(binaryWriter);
						binaryWriter.Flush();
					});
				}
			}
		}

		public void SaveDataInternal(object state)
		{
			SaveDataInfo saveDataInfo = (SaveDataInfo)state;
			lock (saveLock)
			{
				try
				{
					_saving = true;
					SavePlayerStats(saveDataInfo.PlayerStats);
					if (saveDataInfo.Worldinfo.OwnerGamerTag != null)
					{
						saveDataInfo.Worldinfo.LastPlayedDate = DateTime.Now;
						saveDataInfo.Worldinfo.LastPosition = LocalPlayer.LocalPosition;
						saveDataInfo.Worldinfo.SaveToStorage(DNA.Drawing.UI.Screen.CurrentGamer, SaveDevice);
					}
					if (!LocalPlayer.FinalSaveRegistered)
					{
						if (LocalPlayer.Gamer.IsHost)
						{
							LocalPlayer.SaveInventory(SaveDevice, saveDataInfo.Worldinfo.SavePath);
						}
						else if (base.CurrentNetworkSession == null)
						{
							LocalPlayer.SaveInventory(SaveDevice, saveDataInfo.Worldinfo.SavePath);
						}
						else
						{
							InventoryStoreOnServerMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, saveDataInfo.Inventory, false);
						}
					}
					if (GameMode != 0)
					{
						ChunkCache.Instance.Flush(true);
					}
					SaveDevice.Flush();
				}
				catch
				{
				}
				finally
				{
					_saving = false;
				}
			}
		}

		public void EndGame(bool saveData)
		{
			if (MyNetworkGamer.IsHost && IsOnlineGame)
			{
				base.CurrentNetworkSession.CloseNetworkSession();
			}
			if (LocalPlayer != null && LocalPlayer.UnderwaterCue != null && !LocalPlayer.UnderwaterCue.IsPaused)
			{
				LocalPlayer.UnderwaterCue.Pause();
			}
			if (GameScreen != null && GameScreen.HUD != null && GameScreen.HUD.ActiveInventoryItem != null)
			{
				GameScreen.HUD.ActiveInventoryItem.ItemClass.OnItemUnequipped();
			}
			LeaveGame();
			if (saveData && LocalPlayer != null)
			{
				SaveData();
			}
			if (mainScreenGroup.CurrentScreen == GameScreen)
			{
				mainScreenGroup.PopScreen();
			}
			GameScreen = null;
			if (_terrain.Parent != null)
			{
				_terrain.RemoveFromParent();
			}
			if (WaterPlane.Instance != null && WaterPlane.Instance.Parent != null)
			{
				WaterPlane.Instance.RemoveFromParent();
			}
			if (DNA.Drawing.UI.Screen.CurrentGamer == null)
			{
				FrontEnd.PopToStartScreen();
			}
			else
			{
				FrontEnd.PopToMainMenu(DNA.Drawing.UI.Screen.CurrentGamer, null);
			}
			if (GameMode == GameModeTypes.Endurance && FrontEnd.WorldManager != null)
			{
				FrontEnd.WorldManager.Delete(CurrentWorld);
				SaveDevice.Flush();
			}
			_waitForTerrainCallback = null;
			_savingTerrain = true;
			BeginLoadTerrain(null, true);
			WaitScreen.DoWait(FrontEnd._uiGroup, Strings.Please_Wait___, IsSavingProgress);
		}

		public override void OnSessionEnded(NetworkSessionEndReason reason)
		{
			EndGame(true);
			FrontEnd.ShowUIDialog(Strings.Session_Ended, Strings.You_have_been_disconnected_from_the_network_session_, false);
			base.OnSessionEnded(reason);
		}

		private bool IsSavingProgress()
		{
			return !_savingTerrain;
		}

		protected override void Update(GameTime gameTime)
		{
			if (TrialMode)
			{
				TrialModeTimer.Update(gameTime.ElapsedGameTime);
				if (TrialModeTimer.Expired)
				{
					Process.Start("http://www.digitaldnagames.com/upsell/castleminerz.aspx");
					Exit();
				}
			}
			if (CurrentWorld != null)
			{
				CurrentWorld.Update(gameTime);
			}
			UpdateMusic(gameTime);
			if (_terrain != null)
			{
				_terrain.GlobalUpdate(gameTime);
				if (_terrain.MinimallyLoaded && _waitForTerrainCallback != null)
				{
					_waitForTerrainCallback();
					_waitForTerrainCallback = null;
				}
			}
			if (PlayerStats != null)
			{
				PlayerStats.TimeInFull += gameTime.ElapsedGameTime;
				PlayerStats.TimeOfPurchase = DateTime.UtcNow;
				if (FrontEnd != null && mainScreenGroup.CurrentScreen == FrontEnd)
				{
					PlayerStats.TimeInMenu += gameTime.ElapsedGameTime;
				}
				if (base.CurrentNetworkSession != null && base.CurrentNetworkSession.SessionType == NetworkSessionType.PlayerMatch && Instance.GameMode == GameModeTypes.Endurance)
				{
					PlayerStats.TimeOnline += gameTime.ElapsedGameTime;
				}
			}
			if (RequestEndGame)
			{
				RequestEndGame = false;
				EndGame(true);
			}
			TaskDispatcher.Instance.RunMainThreadTasks();
			base.Update(gameTime);
		}

		public void CheaterFound()
		{
			SaveDevice.DeleteStorage();
			Exit();
		}

		protected override void SendNetworkUpdates(NetworkSession session, GameTime gameTime)
		{
			if (session == null || session.LocalGamers.Count == 0)
			{
				return;
			}
			if (session.LocalGamers[0].IsHost && GameScreen != null)
			{
				_worldUpdateTimer.Update(gameTime.ElapsedGameTime);
				if (_worldUpdateTimer.Expired)
				{
					TimeOfDayMessage.Send(session.LocalGamers[0], GameScreen.Day);
					_worldUpdateTimer.Reset();
				}
			}
			_currentFrameNumber++;
			if (_currentFrameNumber > session.AllGamers.Count)
			{
				_currentFrameNumber = 0;
			}
			if (_localPlayer != null)
			{
				for (int i = 0; i < session.AllGamers.Count; i++)
				{
					if (session.AllGamers[i].IsLocal && i != _currentFrameNumber && !_localPlayer.UsingTool && !_localPlayer.Reloading)
					{
						return;
					}
				}
				if (session.LocalGamers.Count > 0)
				{
					PlayerUpdateMessage.Send(session.LocalGamers[0], _localPlayer, _controllerMapping);
				}
			}
			MainThreadMessageSender.Instance.DrainQueue();
		}

		public void SetupNewGamer(SignedInGamer gamer)
		{
			PlayerStats = new CastleMinerZPlayerStats();
			PlayerStats.GamerTag = gamer.Gamertag;
			PlayerStats.InvertYAxis = gamer.GameDefaults.InvertYAxis;
			PlayerStats.SecondTrayFaded = false;
			LoadPlayerData();
			Brightness = PlayerStats.brightness;
			if (PlayerStats.musicMute)
			{
				MusicSounds.SetVolume(0f);
			}
			else
			{
				MusicSounds.SetVolume(PlayerStats.musicVolume);
			}
			AcheivmentManager = new CastleMinerZAchievementManager(this);
			_myAvatarDescription = new AvatarDescription(new byte[10]);
		}

		public void MakeAboveGround(bool spawnontop)
		{
			if (spawnontop)
			{
				_localPlayer.LocalPosition = _terrain.FindTopmostGroundLocation(_localPlayer.LocalPosition);
			}
			else
			{
				_localPlayer.LocalPosition = _terrain.FindSafeStartLocation(_localPlayer.LocalPosition);
			}
		}

		public void PlayMusic(string cueName)
		{
			_fadeMusic = false;
			if (MusicCue != null && MusicCue.IsPlaying && MusicCue.Name != cueName)
			{
				MusicCue.Stop(AudioStopOptions.Immediate);
				MusicCue = null;
			}
			if (MusicCue == null || !MusicCue.IsPlaying)
			{
				MusicCue = SoundManager.Instance.PlayInstance(cueName);
			}
			if (PlayerStats.musicMute)
			{
				MusicSounds.SetVolume(0f);
			}
			else
			{
				MusicSounds.SetVolume(PlayerStats.musicVolume);
			}
		}

		public void FadeMusic()
		{
			_fadeMusic = true;
			musicFadeTimer.Reset();
		}

		public void SetAudio(float day, float night, float cave, float hell)
		{
			if (DayCue == null)
			{
				DayCue = SoundManager.Instance.PlayInstance("Birds");
			}
			if (NightCue == null)
			{
				NightCue = SoundManager.Instance.PlayInstance("Crickets");
			}
			if (CaveCue == null)
			{
				CaveCue = SoundManager.Instance.PlayInstance("Drips");
			}
			if (HellCue == null)
			{
				HellCue = SoundManager.Instance.PlayInstance("lostSouls");
			}
			if (LocalPlayer != null && LocalPlayer.Underwater)
			{
				day = 0f;
				night = 0f;
				cave = 0f;
				hell = 0f;
			}
			DaySounds.SetVolume(day);
			NightSounds.SetVolume(night);
			CaveSounds.SetVolume(cave);
			HellSounds.SetVolume(hell);
			SoundManager.Instance.SetGlobalVarible("Outdoors", 1f - Math.Max(cave, hell));
		}

		protected override void AfterLoad()
		{
			InventoryItem.FinishInitialization(base.GraphicsDevice);
			base.AfterLoad();
		}

		public void UpdateMusic(GameTime time)
		{
			if (!_fadeMusic || !MusicCue.IsPlaying)
			{
				return;
			}
			musicFadeTimer.Update(time.ElapsedGameTime);
			if (musicFadeTimer.Expired)
			{
				if (MusicCue.IsPlaying)
				{
					MusicCue.Stop(AudioStopOptions.Immediate);
				}
				return;
			}
			float num = PlayerStats.musicVolume - musicFadeTimer.PercentComplete;
			if (num < 0f)
			{
				num = 0f;
			}
			if (PlayerStats.musicMute)
			{
				MusicSounds.SetVolume(0f);
			}
			else
			{
				MusicSounds.SetVolume(num);
			}
		}

		public NetworkGamer GetGamerFromID(byte id)
		{
			return base.CurrentNetworkSession.FindGamerById(id);
		}

		private void ProcessUpdateSpawnerMessage(DNA.Net.Message message)
		{
			UpdateSpawnerMessage updateSpawnerMessage = (UpdateSpawnerMessage)message;
			if (updateSpawnerMessage.IsStarted)
			{
				Instance.CurrentWorld.GetSpawner(IntVector3.FromVector3(updateSpawnerMessage.SpawnerPosition), true, BlockTypeEnum.Empty);
				return;
			}
			Spawner spawner = Instance.CurrentWorld.GetSpawner(IntVector3.FromVector3(updateSpawnerMessage.SpawnerPosition), false, BlockTypeEnum.Empty);
			spawner.HandleStopSpawningMessage();
		}

		private void ProcessDestroyCustomBlockMessage(DNA.Net.Message message)
		{
			DestroyCustomBlockMessage destroyCustomBlockMessage = (DestroyCustomBlockMessage)message;
			Door value;
			if (CurrentWorld.Doors.TryGetValue(destroyCustomBlockMessage.Location, out value))
			{
				value.Destroyed = true;
				CurrentWorld.Doors.Remove(destroyCustomBlockMessage.Location);
			}
		}

		private void ProcessMeleePlayerMessage(DNA.Net.Message message)
		{
			if (PVPState == PVPEnum.Everyone || (!MyNetworkGamer.IsHost && !MyNetworkGamer.SignedInGamer.IsFriend(base.CurrentNetworkSession.Host)))
			{
				MeleePlayerMessage meleePlayerMessage = (MeleePlayerMessage)message;
				float damageAmount = 0.21f;
				if (meleePlayerMessage.ItemID == InventoryItemIDs.IronLaserSword || meleePlayerMessage.ItemID == InventoryItemIDs.CopperLaserSword || meleePlayerMessage.ItemID == InventoryItemIDs.GoldLaserSword || meleePlayerMessage.ItemID == InventoryItemIDs.DiamondLaserSword || meleePlayerMessage.ItemID == InventoryItemIDs.BloodStoneLaserSword)
				{
					damageAmount = 1.1f;
				}
				GameScreen.HUD.ApplyDamage(damageAmount, meleePlayerMessage.DamageSource);
			}
		}

		private void _processAddExplosiveFlashMessage(DNA.Net.Message message)
		{
			AddExplosiveFlashMessage addExplosiveFlashMessage = (AddExplosiveFlashMessage)message;
			if (GameScreen != null)
			{
				GameScreen.AddExplosiveFlashModel(addExplosiveFlashMessage.Position);
			}
		}

		private void _processAddExplosionEffectsMessage(DNA.Net.Message message)
		{
			AddExplosionEffectsMessage addExplosionEffectsMessage = (AddExplosionEffectsMessage)message;
			Explosive.AddEffects(addExplosionEffectsMessage.Position, true);
		}

		private void _processKickMessage(DNA.Net.Message message, LocalNetworkGamer localGamer)
		{
			KickMessage kickMessage = (KickMessage)message;
			if (kickMessage.PlayerID == MyNetworkGamer.Id && localGamer.Gamertag != "DigitalDNA2" && localGamer.Gamertag != "DigitalDNA007")
			{
				EndGame(true);
				_waitForWorldInfo = null;
				string message2 = kickMessage.Banned ? Strings.You_have_been_banned_by_the_host_of_this_session_ : Strings.You_have_been_kicked_by_the_host_of_the_session_;
				FrontEnd.ShowUIDialog(Strings.Session_Ended, message2, false);
			}
		}

		private void _processRequestWorldInfoMessage(DNA.Net.Message message, LocalNetworkGamer localGamer, bool isEcho)
		{
			if (localGamer.IsHost && !isEcho)
			{
				if (PlayerStats.BanList.ContainsKey(message.Sender.AlternateAddress))
				{
					KickMessage.Send(localGamer, message.Sender, true);
				}
				else
				{
					WorldInfoMessage.Send(localGamer, CurrentWorld);
				}
			}
		}

		private void _processClientReadyForChunkMessage(DNA.Net.Message message, bool isEcho)
		{
			byte id = MyNetworkGamer.Id;
			if (id == TerrainServerID && !isEcho)
			{
				ChunkCache.Instance.SendRemoteChunkList(message.Sender.Id, false);
			}
		}

		private void _processProvideDeltaListMessage(DNA.Net.Message message)
		{
			ChunkCache.Instance.RemoteChunkListArrived(((ProvideDeltaListMessage)message).Delta);
		}

		private void _processAlterBlocksMessage(DNA.Net.Message message)
		{
			AlterBlockMessage alterBlockMessage = (AlterBlockMessage)message;
			_terrain.SetBlock(alterBlockMessage.BlockLocation, alterBlockMessage.BlockType);
		}

		private void _processRequestChunkMessage(DNA.Net.Message message)
		{
			RequestChunkMessage requestChunkMessage = (RequestChunkMessage)message;
			ChunkCache.Instance.RetrieveChunkForNetwork(requestChunkMessage.Sender.Id, requestChunkMessage.BlockLocation, requestChunkMessage.Priority, null);
		}

		private void _processProvideChunkMessage(DNA.Net.Message message)
		{
			ProvideChunkMessage provideChunkMessage = (ProvideChunkMessage)message;
			ChunkCache.Instance.ChunkDeltaArrived(provideChunkMessage.BlockLocation, provideChunkMessage.Delta, provideChunkMessage.Priority);
		}

		private void _processWorldInfoMessage(DNA.Net.Message message)
		{
			WorldInfoMessage worldInfoMessage = (WorldInfoMessage)message;
			WorldInfo worldInfo = worldInfoMessage.WorldInfo;
			if (_waitForWorldInfo != null)
			{
				_waitForWorldInfo(worldInfo);
				_waitForWorldInfo = null;
			}
		}

		private void _processTimeOfDayMessage(DNA.Net.Message message, bool isEcho)
		{
			if (!isEcho)
			{
				TimeOfDayMessage timeOfDayMessage = (TimeOfDayMessage)message;
				if (GameScreen != null)
				{
					GameScreen.Day = timeOfDayMessage.TimeOfDay;
				}
			}
		}

		private void _processBroadcastTextMessage(DNA.Net.Message message)
		{
			BroadcastTextMessage broadcastTextMessage = (BroadcastTextMessage)message;
			Console.WriteLine(broadcastTextMessage.Message);
		}

		private void _processItemCrateMessage(DNA.Net.Message message)
		{
			ItemCrateMessage itemCrateMessage = (ItemCrateMessage)message;
			itemCrateMessage.Apply(CurrentWorld);
		}

		private void _processDestroyCrateMessage(DNA.Net.Message message)
		{
			DestroyCrateMessage destroyCrateMessage = (DestroyCrateMessage)message;
			Crate value;
			if (CurrentWorld.Crates.TryGetValue(destroyCrateMessage.Location, out value))
			{
				value.Destroyed = true;
				CurrentWorld.Crates.Remove(destroyCrateMessage.Location);
			}
		}

		private void _processDoorOpenCloseMessage(DNA.Net.Message message)
		{
			DoorOpenCloseMessage doorOpenCloseMessage = (DoorOpenCloseMessage)message;
			AudioEmitter audioEmitter = new AudioEmitter();
			audioEmitter.Position = doorOpenCloseMessage.Location;
			if (doorOpenCloseMessage.Opened)
			{
				SoundManager.Instance.PlayInstance("DoorOpen", audioEmitter);
			}
			else
			{
				SoundManager.Instance.PlayInstance("DoorClose", audioEmitter);
			}
			Door value;
			if (CurrentWorld.Doors.TryGetValue(doorOpenCloseMessage.Location, out value))
			{
				value.Open = doorOpenCloseMessage.Opened;
			}
		}

		private void _processAppointServerMessage(DNA.Net.Message message)
		{
			byte id = MyNetworkGamer.Id;
			AppointServerMessage appointServerMessage = (AppointServerMessage)message;
			NetworkGamer gamerFromID = GetGamerFromID(appointServerMessage.PlayerID);
			if (appointServerMessage.PlayerID == id)
			{
				ChunkCache.Instance.MakeHost(null, true);
			}
			else if (TerrainServerID == id)
			{
				ChunkCache.Instance.MakeHost(null, false);
			}
			else if (appointServerMessage.PlayerID != TerrainServerID)
			{
				ChunkCache.Instance.HostChanged();
			}
			TerrainServerID = appointServerMessage.PlayerID;
		}

		private void _processRestartLevelMessage(DNA.Net.Message message)
		{
			if (GameScreen != null)
			{
				LocalPlayer.Dead = false;
				LocalPlayer.FPSMode = true;
				GameScreen.HUD.RefreshPlayer();
				GameScreen.TeleportToLocation(WorldInfo.DefaultStartLocation, true);
				if (MusicCue != null && MusicCue.IsPlaying)
				{
					MusicCue.Stop(AudioStopOptions.Immediate);
				}
				InGameHUD.Instance.Reset();
				Instance.GameScreen.Day = 0.4f;
				InGameHUD.Instance.maxDistanceTraveled = 0;
			}
		}

		private void _processInventoryStoreOnServerMessage(DNA.Net.Message message, bool isHost)
		{
			if (isHost)
			{
				InventoryStoreOnServerMessage inventoryStoreOnServerMessage = (InventoryStoreOnServerMessage)message;
				Player player = (Player)inventoryStoreOnServerMessage.Sender.Tag;
				if (player != _localPlayer)
				{
					player.PlayerInventory = inventoryStoreOnServerMessage.Inventory;
				}
				if (inventoryStoreOnServerMessage.FinalSave)
				{
					player.FinalSaveRegistered = true;
				}
				TaskScheduler.QueueUserWorkItem(delegate(object state)
				{
					Player player2 = (Player)state;
					player2.SaveInventory(SaveDevice, CurrentWorld.SavePath);
				}, player);
			}
		}

		private void _processInventoryRetrieveFromServerMessage(DNA.Net.Message message, bool isHost)
		{
			InventoryRetrieveFromServerMessage inventoryRetrieveFromServerMessage = (InventoryRetrieveFromServerMessage)message;
			NetworkGamer gamerFromID = GetGamerFromID(inventoryRetrieveFromServerMessage.playerID);
			if (gamerFromID != null && gamerFromID.Tag != null)
			{
				Player player = (Player)gamerFromID.Tag;
				player.PlayerInventory = inventoryRetrieveFromServerMessage.Inventory;
				player.PlayerInventory.Player = player;
			}
		}

		private void _processRequestInventoryMessage(DNA.Net.Message message, bool isHost)
		{
			if (isHost && message.Sender.Tag != null)
			{
				TaskScheduler.QueueUserWorkItem(delegate(object state)
				{
					Player player = (Player)state;
					bool isdefault = player.LoadInventory(SaveDevice, CurrentWorld.SavePath);
					InventoryRetrieveFromServerMessage.Send((LocalNetworkGamer)_localPlayer.Gamer, player, isdefault);
				}, message.Sender.Tag);
			}
		}

		private void _processPlayerExistsMessage(DNA.Net.Message message, bool isEcho, bool isHost)
		{
			PlayerExistsMessage playerExistsMessage = (PlayerExistsMessage)message;
			if (message.Sender.Tag != null)
			{
				return;
			}
			Player player = new Player(message.Sender, new AvatarDescription(playerExistsMessage.AvatarDescriptionData));
			if (isEcho)
			{
				_localPlayer = player;
				GameScreen = new GameScreen(this, player);
				GameScreen.Inialize();
				mainScreenGroup.PushScreen(GameScreen);
				_localPlayer.LocalPosition = CurrentWorld.LastPosition;
				CurrentWorld.InfiniteResourceMode = InfiniteResourceMode;
				RequestInventoryMessage.Send((LocalNetworkGamer)_localPlayer.Gamer);
				if (_localPlayer.LocalPosition == Vector3.Zero)
				{
					_localPlayer.LocalPosition = new Vector3(3f, 3f, 3f);
					MakeAboveGround(true);
				}
				else
				{
					MakeAboveGround(false);
				}
				FadeMusic();
				lock (holdingGround)
				{
					while (holdingGround.Children.Count > 0)
					{
						Entity entity = holdingGround.Children[0];
						entity.RemoveFromParent();
						GameScreen.AddPlayer((Player)entity);
					}
					holdingGround.Children.Clear();
				}
			}
			else if (GameScreen == null)
			{
				lock (holdingGround)
				{
					holdingGround.Children.Add(player);
				}
			}
			else
			{
				GameScreen.AddPlayer(player);
				if (playerExistsMessage.RequestResponse)
				{
					PlayerExistsMessage.Send(MyNetworkGamer, _myAvatarDescription, false);
					if (isHost)
					{
						TimeOfDayMessage.Send(MyNetworkGamer, GameScreen.Day);
					}
					TimeConnectedMessage.Send(MyNetworkGamer, LocalPlayer);
				}
				ChangeCarriedItemMessage.Send((LocalNetworkGamer)Instance.LocalPlayer.Gamer, GameScreen.HUD.ActiveInventoryItem.ItemClass.ID);
				CrateFocusMessage.Send((LocalNetworkGamer)Instance.LocalPlayer.Gamer, _localPlayer.FocusCrate, _localPlayer.FocusCrateItem);
			}
			if (EnemyManager.Instance != null)
			{
				EnemyManager.Instance.BroadcastExistingDragonMessage(message.Sender.Id);
			}
		}

		protected override void OnMessage(DNA.Net.Message message)
		{
			LocalNetworkGamer myNetworkGamer = MyNetworkGamer;
			bool isHost = myNetworkGamer.IsHost;
			bool isEcho = message.Sender == myNetworkGamer;
			if (3 != base.CurrentNetworkSession.SessionProperties[0].Value)
			{
				EndGame(false);
				FrontEnd.ShowUIDialog(Strings.Session_Ended, Strings.You_have_a_different_version_of_the_game_than_the_host_, false);
				return;
			}
			if (message is PlayerExistsMessage)
			{
				_processPlayerExistsMessage(message, isEcho, isHost);
			}
			else if (message is AddExplosiveFlashMessage)
			{
				_processAddExplosiveFlashMessage(message);
			}
			else if (message is AddExplosionEffectsMessage)
			{
				_processAddExplosionEffectsMessage(message);
			}
			else if (message is KickMessage)
			{
				_processKickMessage(message, myNetworkGamer);
			}
			else if (message is RequestWorldInfoMessage)
			{
				_processRequestWorldInfoMessage(message, myNetworkGamer, isEcho);
			}
			else if (message is ClientReadyForChunksMessage)
			{
				_processClientReadyForChunkMessage(message, isEcho);
			}
			else if (message is ProvideDeltaListMessage && !isHost)
			{
				_processProvideDeltaListMessage(message);
			}
			else if (message is AlterBlockMessage)
			{
				_processAlterBlocksMessage(message);
			}
			else if (message is RequestChunkMessage && isHost)
			{
				_processRequestChunkMessage(message);
			}
			else if (message is ProvideChunkMessage && !isHost)
			{
				_processProvideChunkMessage(message);
			}
			else if (message is WorldInfoMessage)
			{
				_processWorldInfoMessage(message);
			}
			else if (message is TimeOfDayMessage)
			{
				_processTimeOfDayMessage(message, isEcho);
			}
			else if (message is BroadcastTextMessage)
			{
				_processBroadcastTextMessage(message);
			}
			else if (message is ItemCrateMessage)
			{
				_processItemCrateMessage(message);
			}
			else if (message is DestroyCrateMessage)
			{
				_processDestroyCrateMessage(message);
			}
			else if (message is DoorOpenCloseMessage)
			{
				_processDoorOpenCloseMessage(message);
			}
			else if (message is AppointServerMessage)
			{
				_processAppointServerMessage(message);
			}
			else if (message is RestartLevelMessage)
			{
				_processRestartLevelMessage(message);
			}
			else if (message is InventoryStoreOnServerMessage)
			{
				_processInventoryStoreOnServerMessage(message, isHost);
			}
			else if (message is InventoryRetrieveFromServerMessage)
			{
				_processInventoryRetrieveFromServerMessage(message, isHost);
			}
			else if (message is RequestInventoryMessage)
			{
				_processRequestInventoryMessage(message, isHost);
			}
			else if (message is DetonateRocketMessage)
			{
				Explosive.HandleDetonateRocketMessage(message as DetonateRocketMessage);
			}
			else if (message is DetonateGrenadeMessage)
			{
				GrenadeProjectile.HandleDetonateGrenadeMessage(message as DetonateGrenadeMessage);
			}
			else if (message is DetonateExplosiveMessage)
			{
				Explosive.HandleDetonateExplosiveMessage((DetonateExplosiveMessage)message);
			}
			else if (message is RemoveBlocksMessage)
			{
				Explosive.HandleRemoveBlocksMessage((RemoveBlocksMessage)message);
			}
			else if (message is MeleePlayerMessage)
			{
				ProcessMeleePlayerMessage(message);
			}
			else if (message is DestroyCustomBlockMessage)
			{
				ProcessDestroyCustomBlockMessage(message);
			}
			if (message is CastleMinerZMessage)
			{
				CastleMinerZMessage castleMinerZMessage = (CastleMinerZMessage)message;
				switch (castleMinerZMessage.MessageType)
				{
				case CastleMinerZMessage.MessageTypes.Broadcast:
				{
					for (int i = 0; i < base.CurrentNetworkSession.AllGamers.Count; i++)
					{
						NetworkGamer networkGamer = base.CurrentNetworkSession.AllGamers[i];
						if (networkGamer.Tag != null)
						{
							Player player2 = (Player)networkGamer.Tag;
							player2.ProcessMessage(message);
						}
					}
					break;
				}
				case CastleMinerZMessage.MessageTypes.PlayerUpdate:
					if (message.Sender.Tag != null)
					{
						Player player = (Player)message.Sender.Tag;
						player.ProcessMessage(message);
					}
					break;
				case CastleMinerZMessage.MessageTypes.EnemyMessage:
					if (EnemyManager.Instance != null)
					{
						EnemyManager.Instance.HandleMessage(castleMinerZMessage);
					}
					break;
				case CastleMinerZMessage.MessageTypes.PickupMessage:
					if (PickupManager.Instance != null)
					{
						PickupManager.Instance.HandleMessage(castleMinerZMessage);
					}
					break;
				}
			}
			base.OnMessage(message);
		}

		protected override void OnGamerJoined(NetworkGamer gamer)
		{
			LocalNetworkGamer localNetworkGamer = base.CurrentNetworkSession.LocalGamers[0];
			Console.WriteLine(Strings.Player_Joined + ": " + gamer.Gamertag);
			if (gamer == localNetworkGamer)
			{
				MyNetworkGamer = localNetworkGamer;
				if (!localNetworkGamer.IsHost)
				{
					GameMode = (GameModeTypes)base.CurrentNetworkSession.SessionProperties[2].Value;
					if (base.CurrentNetworkSession.SessionProperties[4] == 1)
					{
						InfiniteResourceMode = true;
					}
					else
					{
						InfiniteResourceMode = false;
					}
				}
				else
				{
					base.CurrentNetworkSession.Password = CurrentWorld.ServerPassword;
				}
			}
			else if (localNetworkGamer.IsHost)
			{
				AppointServerMessage.Send(MyNetworkGamer, TerrainServerID);
				if (IsOnlineGame)
				{
					base.CurrentNetworkSession.ReportClientJoined(gamer.Gamertag);
				}
			}
			base.OnGamerJoined(gamer);
		}

		public override void OnHostChanged(NetworkGamer oldHost, NetworkGamer newHost)
		{
			if (newHost != null)
			{
				MyNetworkGamer = base.CurrentNetworkSession.LocalGamers[0];
				if (newHost == MyNetworkGamer)
				{
					AppointNewServer();
				}
			}
			base.OnHostChanged(oldHost, newHost);
		}

		private void AppointNewServer()
		{
			TimeSpan t = TimeSpan.Zero;
			byte b = 0;
			bool flag = false;
			foreach (NetworkGamer allGamer in base.CurrentNetworkSession.AllGamers)
			{
				if (allGamer.Tag != null)
				{
					Player player = (Player)allGamer.Tag;
					if (player.TimeConnected >= t)
					{
						t = player.TimeConnected;
						b = allGamer.Id;
						flag = true;
					}
				}
			}
			if (flag)
			{
				if (b != TerrainServerID)
				{
					AppointServerMessage.Send(MyNetworkGamer, b);
				}
			}
			else
			{
				base.CurrentNetworkSession.AllowHostMigration = false;
				base.CurrentNetworkSession.AllowJoinInProgress = false;
				EndGame(false);
				FrontEnd.ShowUIDialog(Strings.Session_Ended, Strings.You_have_been_disconnected_from_the_network_session_, false);
			}
		}

		protected override void OnGamerLeft(NetworkGamer gamer)
		{
			if (base.CurrentNetworkSession == null || base.CurrentNetworkSession.LocalGamers.Count == 0)
			{
				return;
			}
			NetworkGamer myNetworkGamer = MyNetworkGamer;
			Console.WriteLine(Strings.Player_Left + ": " + gamer.Gamertag);
			if (gamer != myNetworkGamer && myNetworkGamer.IsHost && TerrainServerID == gamer.Id)
			{
				AppointNewServer();
			}
			if (gamer != myNetworkGamer && myNetworkGamer.IsHost)
			{
				if (IsOnlineGame)
				{
					base.CurrentNetworkSession.ReportClientLeft(gamer.Gamertag);
				}
				if (gamer.Tag != null)
				{
					Player player = (Player)gamer.Tag;
					if (!player.FinalSaveRegistered)
					{
						TaskScheduler.QueueUserWorkItem(delegate
						{
							player.SaveInventory(SaveDevice, CurrentWorld.SavePath);
						}, player);
					}
				}
			}
			if (gamer.Tag != null)
			{
				Player player2 = (Player)gamer.Tag;
				player2.RemoveFromParent();
			}
			base.OnGamerLeft(gamer);
		}

		public void LoadPlayerData()
		{
			CastleMinerZPlayerStats stats = new CastleMinerZPlayerStats();
			stats.GamerTag = DNA.Drawing.UI.Screen.CurrentGamer.Gamertag;
			try
			{
				SaveDevice.Load("stats.sav", delegate(Stream stream)
				{
					stats.Load(new BinaryReader(stream));
				});
				if (stats.GamerTag != DNA.Drawing.UI.Screen.CurrentGamer.Gamertag)
				{
					throw new Exception("Stats Error");
				}
				PlayerStats = stats;
			}
			catch (Exception)
			{
				PlayerStats = new CastleMinerZPlayerStats();
				PlayerStats.GamerTag = DNA.Drawing.UI.Screen.CurrentGamer.Gamertag;
				if (GraphicsProfileManager.Instance.IsReach)
				{
					PlayerStats.DrawDistance = 0;
				}
			}
		}

		protected override void EndDraw()
		{
			if (_terrain != null)
			{
				_terrain.BuildPendingVertexBuffers();
			}
			base.EndDraw();
		}
	}
}
