using DNA.Audio;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using DNA.Drawing.Particles;
using DNA.Drawing.UI;
using DNA.Input;
using DNA.Net.GamerServices;
using DNA.Text;
using DNA.Timers;
using DNA.Triggers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DNA.CastleMinerZ.UI
{
	public class InGameHUD : Screen
	{
		public class DamageIndicator
		{
			public Vector3 DamageSource;

			public OneShotTimer drawTimer;

			public float fadeAmount
			{
				get
				{
					if ((double)drawTimer.PercentComplete < 0.67)
					{
						return 1f;
					}
					return (1f - drawTimer.PercentComplete) * 3f;
				}
			}

			public DamageIndicator(Vector3 source)
			{
				DamageSource = source;
				drawTimer = new OneShotTimer(TimeSpan.FromSeconds(3.0));
			}
		}

		public const float MaxHealth = 1f;

		public const float MaxStamina = 1f;

		public const float MaxOxygen = 1f;

		private const float PROBE_LENGTH = 5f;

		public static InGameHUD Instance;

		public int maxDistanceTraveled;

		public float PlayerHealth = 1f;

		public float HealthRecoverRate = 0.75f;

		public OneShotTimer HealthRecoverTimer = new OneShotTimer(TimeSpan.FromSeconds(3.0));

		public float PlayerStamina = 1f;

		public float StaminaRecoverRate = 0.25f;

		public float StaminaDamagedRecoveryModifier;

		public OneShotTimer StaminaRecoverTimer = new OneShotTimer(TimeSpan.FromSeconds(1.0));

		public OneShotTimer StaminaBlockTimer = new OneShotTimer(TimeSpan.FromSeconds(2.0));

		public float PlayerOxygen = 1f;

		public float OxygenDecayRate = 0.1f;

		public float OxygenHealthPenaltyRate = 0.2f;

		private float maxGunCameraShift = 0.04f;

		private Vector2 GunEyePointCameraLocation = Vector2.Zero;

		private int lightningFlashCount;

		private OneShotTimer timeToLightning;

		private OneShotTimer timeToThunder;

		private OneShotTimer timeToRespawn = new OneShotTimer(TimeSpan.FromSeconds(20.0));

		private OneShotTimer timeToShowRespawnText = new OneShotTimer(TimeSpan.FromSeconds(3.0));

		private OneShotTimer fadeInGameStart = new OneShotTimer(TimeSpan.FromSeconds(1.0));

		private Random rand = new Random();

		private List<Explosive> _tntWaitingToExplode = new List<Explosive>();

		public float InnaccuracyMultiplier;

		private List<DNA.Triggers.Trigger> _triggers = new List<DNA.Triggers.Trigger>();

		private bool gameBegun;

		private Angle compassRotation;

		private Vector3 trialMaxPosition;

		private PCDialogScreen _travelMaxDialog;

		private CrateScreen _crateScreen;

		public OneShotTimer drawDayTimer = new OneShotTimer(TimeSpan.FromSeconds(9.0));

		public int currentDay;

		private List<DamageIndicator> _damageIndicator = new List<DamageIndicator>();

		private Sprite _damageArrow;

		private Sprite _crosshairTick;

		private float _crosshairTickDrawLocation;

		private float _secondTrayAlpha = 0.4f;

		public ConstructionProbeClass ConstructionProbe = new ConstructionProbeClass();

		public ConsoleElement console;

		public ConsoleElement lootConsole;

		private CastleMinerZGame _game;

		private Sprite _gridSprite;

		private Sprite _selectorSprite;

		private Sprite _crosshair;

		private Sprite _emptyStaminaBar;

		private Sprite _emptyHealthBar;

		private Sprite _fullHealthBar;

		private Sprite _bubbleBar;

		private Sprite _sniperScope;

		private Sprite _missileLocking;

		private Sprite _missileLock;

		private Queue<AchievementManager<CastleMinerZPlayerStats>.Achievement> AcheivementsToDraw = new Queue<AchievementManager<CastleMinerZPlayerStats>.Achievement>();

		private AchievementManager<CastleMinerZPlayerStats>.Achievement displayedAcheivement;

		private OneShotTimer _resetCraterFoundTriggerTimer = new OneShotTimer(TimeSpan.FromMinutes(1.0));

		private DNA.Triggers.Trigger _craterFoundTrigger;

		private OneShotTimer acheivementDisplayTimer = new OneShotTimer(TimeSpan.FromSeconds(10.0));

		private Vector2 acheimentDisplayLocation = new Vector2(453f, 439f);

		private string _achievementText1 = "";

		private string _achievementText2 = "";

		private int currentDistanceTraveled;

		private Vector2 _achievementLocation;

		private InventoryItem lastItem;

		private StringBuilder sbuilder = new StringBuilder();

		private StringBuilder distanceBuilder = new StringBuilder();

		private Rectangle _prevTitleSafe = Rectangle.Empty;

		private EulerAngle freeFlyCameraRotation = default(EulerAngle);

		private OneShotTimer lavaDamageTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		private bool lavaSoundPlayed;

		private Vector3 lastPosition = Vector3.Zero;

		private OneShotTimer _periodicSaveTimer = new OneShotTimer(TimeSpan.FromSeconds(10.0));

		private float lastTOD = -1f;

		private StringBuilder _builder = new StringBuilder();

		private bool _hideUI;

		private bool showPlayers;

		private PlainChatInputScreen _chatScreen;

		public PlayerInventory PlayerInventory
		{
			get
			{
				return LocalPlayer.PlayerInventory;
			}
		}

		private bool WaitToRespawn
		{
			get
			{
				if (_game.GameMode == GameModeTypes.Endurance && !timeToRespawn.Expired && LocalPlayer.Dead)
				{
					return _game.IsOnlineGame;
				}
				return false;
			}
		}

		public bool IsChatting
		{
			get
			{
				if (_game != null)
				{
					return _game.GameScreen._uiGroup.CurrentScreen == _chatScreen;
				}
				return false;
			}
		}

		public InventoryItem ActiveInventoryItem
		{
			get
			{
				return PlayerInventory.ActiveInventoryItem;
			}
		}

		public Player LocalPlayer
		{
			get
			{
				return _game.LocalPlayer;
			}
		}

		private BlockTerrain Terrain
		{
			get
			{
				return _game._terrain;
			}
		}

		public void ApplyDamage(float damageAmount, Vector3 damageSource)
		{
			if (!LocalPlayer.Dead)
			{
				InnaccuracyMultiplier = 1f;
				LocalPlayer.ApplyRecoil(Angle.FromDegrees(5f));
				_damageIndicator.Add(new DamageIndicator(damageSource));
				SoundManager.Instance.PlayInstance("Hit");
				HealthRecoverTimer.Reset();
				StaminaRecoverTimer.Reset();
				PlayerHealth -= damageAmount;
				if (PlayerHealth <= 0f)
				{
					PlayerHealth = 0f;
					KillPlayer();
				}
			}
		}

		public void UseStamina(float amount)
		{
			PlayerStamina -= amount;
			StaminaRecoverTimer.Reset();
			if ((double)PlayerStamina <= 0.01)
			{
				StaminaBlockTimer.Reset();
			}
		}

		public bool CanUseStamina(float amount)
		{
			if (!CastleMinerZGame.Instance.IsEnduranceMode && StaminaBlockTimer.Expired)
			{
				return amount <= PlayerStamina;
			}
			return false;
		}

		public bool CheckAndUseStamina(float amount)
		{
			if (amount <= 0f)
			{
				return true;
			}
			if (CanUseStamina(amount))
			{
				UseStamina(amount);
				return true;
			}
			return false;
		}

		public void KillPlayer()
		{
			LocalPlayer.PlayGrenadeAnim = false;
			LocalPlayer.ReadyToThrowGrenade = false;
			CrateFocusMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, IntVector3.Zero, Point.Zero);
			SoundManager.Instance.PlayInstance("Fall");
			LocalPlayer.Dead = true;
			LocalPlayer.FPSMode = false;
			LocalPlayer.Avatar.HideHead = true;
			if (CastleMinerZGame.Instance.GameMode != GameModeTypes.Creative)
			{
				if (_game.Difficulty == GameDifficultyTypes.HARDCORE)
				{
					PlayerInventory.DropAll(true);
					PlayerInventory.SetDefaultInventory();
				}
				else
				{
					PlayerInventory.DropAll(false);
				}
			}
			timeToRespawn = new OneShotTimer(TimeSpan.FromSeconds(20.0));
			timeToShowRespawnText = new OneShotTimer(TimeSpan.FromSeconds(3.0));
			if (_game.IsOnlineGame)
			{
				BroadcastTextMessage.Send(_game.MyNetworkGamer, LocalPlayer.Gamer.Gamertag + " " + Strings.Has_Fallen);
			}
		}

		public void RespawnPlayer()
		{
			Player player = null;
			float num = float.MaxValue;
			foreach (NetworkGamer remoteGamer in _game.CurrentNetworkSession.RemoteGamers)
			{
				if (remoteGamer.Tag != null)
				{
					Player player2 = (Player)remoteGamer.Tag;
					if (player2 != null && !player2.Dead)
					{
						float num2 = player2.LocalPosition.LengthSquared();
						if (num2 < num)
						{
							player = player2;
							num = num2;
						}
					}
				}
			}
			if (_game.GameMode != 0)
			{
				RefreshPlayer();
				_game.GameScreen.TeleportToLocation(LocalPlayer.GetSpawnPoint(), false);
				if (_game.IsOnlineGame)
				{
					BroadcastTextMessage.Send(_game.MyNetworkGamer, LocalPlayer.Gamer.Gamertag + " " + Strings.Has_Respawned);
				}
			}
			else if (player == null)
			{
				if (_game.LocalPlayer.Gamer.IsHost && !_game.IsOnlineGame)
				{
					RestartLevelMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer);
				}
			}
			else
			{
				RefreshPlayer();
				_game.GameScreen.TeleportToLocation(player.LocalPosition, false);
				if (_game.IsOnlineGame)
				{
					BroadcastTextMessage.Send(_game.MyNetworkGamer, LocalPlayer.Gamer.Gamertag + " " + Strings.Has_Respawned);
				}
			}
		}

		public bool AllPlayersDead()
		{
			Player player = null;
			foreach (NetworkGamer remoteGamer in _game.CurrentNetworkSession.RemoteGamers)
			{
				if (remoteGamer.Tag != null)
				{
					Player player2 = (Player)remoteGamer.Tag;
					if (player2 != null && !player2.Dead)
					{
						player = player2;
						break;
					}
				}
			}
			if (player == null)
			{
				return true;
			}
			return false;
		}

		public void RefreshPlayer()
		{
			LocalPlayer.Dead = false;
			LocalPlayer.FPSMode = true;
			PlayerHealth = 1f;
		}

		public static BlockTypeEnum GetBlock(IntVector3 worldPosition)
		{
			return BlockTerrain.Instance.GetBlockWithChanges(worldPosition);
		}

		public InGameHUD(CastleMinerZGame game)
			: base(true, false)
		{
			CaptureMouse = true;
			ShowMouseCursor = false;
			_triggers.Add(new TransitionMusicTrigger("Song6", 3400f));
			_triggers.Add(new TransitionMusicTrigger("Song5", 3000f));
			_triggers.Add(new TransitionMusicTrigger("Song4", 2300f));
			_triggers.Add(new TransitionMusicTrigger("Song3", 1600f));
			_triggers.Add(new TransitionMusicTrigger("Song2", 900f));
			_triggers.Add(new TransitionMusicTrigger("Song1", 200f));
			_craterFoundTrigger = new CraterFoundTransitionMusicTrigger("SpaceTheme", 1f);
			Instance = this;
			_game = game;
			_damageArrow = _game._uiSprites["DamageArrow"];
			_gridSprite = _game._uiSprites["HudGrid"];
			_selectorSprite = _game._uiSprites["Selector"];
			_crosshair = _game._uiSprites["CrossHair"];
			_crosshairTick = _game._uiSprites["CrossHairTick"];
			_emptyStaminaBar = _game._uiSprites["StaminaBarEmpty"];
			_emptyHealthBar = _game._uiSprites["HealthBarEmpty"];
			_fullHealthBar = _game._uiSprites["HealthBarFull"];
			_bubbleBar = _game._uiSprites["BubbleBar"];
			_sniperScope = _game._uiSprites["SniperScope"];
			_missileLocking = _game._uiSprites["MissleLocking"];
			_missileLock = _game._uiSprites["MissleLock"];
			console = new ConsoleElement(_game._consoleFont);
			console.GrabConsole();
			console.Location = Vector2.Zero;
			console.Size = new Vector2((float)_game.GraphicsDevice.PresentationParameters.BackBufferWidth * 0.3f, (float)_game.GraphicsDevice.PresentationParameters.BackBufferHeight * 0.25f);
			_chatScreen = new PlainChatInputScreen(console.Bounds.Bottom + 25f);
			timeToLightning = new OneShotTimer(TimeSpan.FromSeconds(rand.Next(5, 10)));
			timeToThunder = new OneShotTimer(TimeSpan.FromSeconds((float)rand.NextDouble() * 2f));
			lightningFlashCount = rand.Next(0, 4);
			_travelMaxDialog = new PCDialogScreen(Strings.Purchase_Game, Strings.You_must_purchase_the_game_to_travel_further, null, true, _game.DialogScreenImage, _game._myriadMed, true, _game.ButtonFrame);
			_travelMaxDialog.UseDefaultValues();
			_crateScreen = new CrateScreen(game, this);
		}

		public void DisplayAcheivement(AchievementManager<CastleMinerZPlayerStats>.Achievement acheivement)
		{
			AcheivementsToDraw.Enqueue(acheivement);
		}

		private void DrawAcheivement(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			Sprite sprite = _game._uiSprites["AwardEnd"];
			Sprite sprite2 = _game._uiSprites["AwardCenter"];
			Sprite sprite3 = _game._uiSprites["AwardCircle"];
			float num = sprite.Width;
			Vector2 value = new Vector2(79f, 10f);
			Vector2 value2 = new Vector2(79f, 37f);
			float num2 = value.X - num;
			Vector2 vector = _game._systemFont.MeasureString(_achievementText1);
			Vector2 vector2 = _game._systemFont.MeasureString(_achievementText2);
			float num3 = Math.Max(vector.X, vector2.X) + num2;
			float num4 = num3 + num * 2f;
			float num5 = (float)screenRect.Center.X - num4 / 2f;
			int num6 = (int)acheimentDisplayLocation.Y;
			_achievementLocation = new Vector2(num5, num6);
			sprite.Draw(spriteBatch, new Vector2(num5, num6), Color.White);
			sprite.Draw(spriteBatch, new Vector2(num5 + num3 + num, num6), 1f, Color.White, SpriteEffects.FlipHorizontally);
			sprite2.Draw(spriteBatch, new Rectangle((int)(num5 + num) - 1, num6, (int)(num3 + 2f), sprite2.Height), Color.White);
			sprite3.Draw(spriteBatch, new Vector2(num5, num6), Color.White);
			spriteBatch.DrawString(_game._systemFont, _achievementText1, value + _achievementLocation, new Color(219, 219, 219));
			spriteBatch.DrawString(_game._systemFont, _achievementText2, value2 + _achievementLocation, new Color(219, 219, 219));
		}

		private void EquipActiveItem()
		{
			if (lastItem != ActiveInventoryItem)
			{
				if (lastItem != null)
				{
					lastItem.ItemClass.OnItemUnequipped();
				}
				ActiveInventoryItem.ItemClass.OnItemEquipped();
				lastItem = ActiveInventoryItem;
				LocalPlayer.Equip(ActiveInventoryItem);
				if (ActiveInventoryItem is GunInventoryItem)
				{
					GunInventoryItem gunInventoryItem = (GunInventoryItem)ActiveInventoryItem;
					LocalPlayer.ReloadSound = gunInventoryItem.GunClass.ReloadSound;
				}
			}
		}

		private void UpdateAcheivements(GameTime gameTime)
		{
			_game.AcheivmentManager.Update();
			if (displayedAcheivement == null)
			{
				if (AcheivementsToDraw.Count > 0)
				{
					SoundManager.Instance.PlayInstance("Award");
					acheivementDisplayTimer.Reset();
					displayedAcheivement = AcheivementsToDraw.Dequeue();
					BroadcastTextMessage.Send(_game.MyNetworkGamer, LocalPlayer.Gamer.Gamertag + " " + Strings.Has_earned + " '" + displayedAcheivement.Name + "'");
					_achievementText2 = displayedAcheivement.HowToUnlock;
					_achievementText1 = displayedAcheivement.Name;
				}
			}
			else
			{
				acheivementDisplayTimer.Update(gameTime.ElapsedGameTime);
				if (acheivementDisplayTimer.Expired)
				{
					displayedAcheivement = null;
				}
			}
		}

		private void DrawAcheivements(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (displayedAcheivement != null)
			{
				DrawAcheivement(device, spriteBatch);
			}
		}

		private float GetTrayAlphaSetting()
		{
			if (CastleMinerZGame.Instance.PlayerStats.SecondTrayFaded)
			{
				return _secondTrayAlpha;
			}
			return 1f;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (_hideUI)
			{
				return;
			}
			Rectangle rectangle = new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height);
			spriteBatch.Begin();
			if (!LocalPlayer.Dead && LocalPlayer.ShoulderedAnimState)
			{
				GunInventoryItemClass gunInventoryItemClass = ActiveInventoryItem.ItemClass as GunInventoryItemClass;
				if (gunInventoryItemClass != null && gunInventoryItemClass.Scoped)
				{
					LocalPlayer.Avatar.Visible = false;
					if (gunInventoryItemClass is RocketLauncherGuidedInventoryItemClass)
					{
						RocketLauncherGuidedInventoryItemClass rocketLauncherGuidedInventoryItemClass = (RocketLauncherGuidedInventoryItemClass)gunInventoryItemClass;
						if (rocketLauncherGuidedInventoryItemClass.LockedOnToDragon)
						{
							spriteBatch.Draw(_missileLock, rocketLauncherGuidedInventoryItemClass.LockedOnSpriteLocation, Color.Red);
						}
						else
						{
							spriteBatch.Draw(_missileLocking, rocketLauncherGuidedInventoryItemClass.LockedOnSpriteLocation, Color.Lime);
						}
					}
					Size size = new Size((int)((float)_sniperScope.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)_sniperScope.Height * Screen.Adjuster.ScaleFactor.Y));
					Vector2 vector = new Vector2(rectangle.Center.X - size.Width / 2, rectangle.Center.Y - size.Height / 2);
					spriteBatch.Draw(_sniperScope, new Rectangle((int)vector.X, (int)vector.Y, size.Width, size.Height), Color.White);
					spriteBatch.Draw(_game.DummyTexture, new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, (int)vector.Y), Color.Black);
					spriteBatch.Draw(_game.DummyTexture, new Rectangle(0, (int)vector.Y + size.Height, Screen.Adjuster.ScreenRect.Width, (int)vector.Y), Color.Black);
					spriteBatch.Draw(_game.DummyTexture, new Rectangle(0, (int)vector.Y, (int)vector.X, size.Height), Color.Black);
					spriteBatch.Draw(_game.DummyTexture, new Rectangle(Screen.Adjuster.ScreenRect.Width - (int)vector.X, (int)vector.Y, (int)vector.X, size.Height), Color.Black);
				}
				else
				{
					LocalPlayer.Avatar.Visible = true;
				}
			}
			else
			{
				LocalPlayer.Avatar.Visible = true;
			}
			spriteBatch.End();
			if (rectangle != _prevTitleSafe)
			{
				console.Location = new Vector2(rectangle.Left, rectangle.Top);
				_chatScreen.YLoc = console.Bounds.Bottom + 25f;
			}
			console.Draw(device, spriteBatch, gameTime, false);
			if (showPlayers)
			{
				DrawPlayerList(device, spriteBatch, gameTime);
			}
			else
			{
				spriteBatch.Begin();
				int num = (int)((1f - PlayerHealth) * 120f);
				int r = (int)((1f - PlayerHealth) * 102f);
				if (num > 0)
				{
					spriteBatch.Draw(_game.DummyTexture, new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height), new Color(r, 0, 0, num));
				}
				if (LocalPlayer.PercentSubmergedLava > 0f)
				{
					spriteBatch.Draw(_game.DummyTexture, new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height), Color.Lerp(Color.Transparent, Color.Red, LocalPlayer.PercentSubmergedLava));
				}
				if (LocalPlayer.UnderLava)
				{
					spriteBatch.Draw(_game.DummyTexture, new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height), Color.Red);
				}
				Rectangle sourceRectangle = new Rectangle(0, 0, _damageArrow.Width, _damageArrow.Height);
				Vector2 origin = new Vector2(_damageArrow.Width / 2, _damageArrow.Height + 150);
				Vector2 position = new Vector2(rectangle.Center.X, rectangle.Center.Y);
				int num2 = 0;
				while (num2 < _damageIndicator.Count)
				{
					_damageIndicator[num2].drawTimer.Update(gameTime.ElapsedGameTime);
					if (_damageIndicator[num2].drawTimer.Expired)
					{
						_damageIndicator.RemoveAt(num2);
						continue;
					}
					Vector3 normal = _damageIndicator[num2].DamageSource - LocalPlayer.LocalPosition;
					normal = Vector3.TransformNormal(normal, LocalPlayer.WorldToLocal);
					Angle rotation = Angle.ATan2(normal.X, 0f - normal.Z);
					_damageArrow.Draw(spriteBatch, position, sourceRectangle, new Color((int)(139f * _damageIndicator[num2].fadeAmount), 0, 0, (int)(255f * _damageIndicator[num2].fadeAmount)), rotation, origin, 0.75f, SpriteEffects.None, 0f);
					num2++;
				}
				if (LocalPlayer.Dead && timeToShowRespawnText.Expired && _game.GameScreen._uiGroup.CurrentScreen == this)
				{
					string text = "";
					string text2 = Strings.Click_To_Respawn;
					if (WaitToRespawn)
					{
						text = Strings.Respawn_In + ": ";
						text2 = "";
					}
					if (_game.IsOnlineGame && AllPlayersDead() && _game.GameMode == GameModeTypes.Endurance)
					{
						if (_game.CurrentNetworkSession.IsHost)
						{
							text = "";
							text2 = Strings.Click_To_Restart;
						}
						else
						{
							text = Strings.Waiting_For_Host_To_Restart;
							text2 = "";
						}
					}
					Vector2 vector2 = _game._medLargeFont.MeasureString(text + text2);
					Vector2 vector3 = new Vector2((float)rectangle.Center.X - vector2.X / 2f, (float)rectangle.Center.Y - vector2.Y / 2f);
					if (_game.GameMode == GameModeTypes.Endurance)
					{
						sbuilder.Length = 0;
						sbuilder.Append(Strings.Distance_Traveled);
						sbuilder.Append(": ");
						sbuilder.Concat(maxDistanceTraveled);
						Vector2 vector4 = _game._medLargeFont.MeasureString(sbuilder);
						vector3 = new Vector2((float)rectangle.Center.X - vector4.X / 2f, (float)rectangle.Center.Y - vector4.Y - vector4.Y / 2f);
						spriteBatch.DrawOutlinedText(_game._medLargeFont, sbuilder, new Vector2(vector3.X, vector3.Y), Color.White, Color.Black, 2);
						sbuilder.Length = 0;
						sbuilder.Append(" " + Strings.In + " ");
						sbuilder.Concat(currentDay);
						sbuilder.Append((currentDay != 1) ? (" " + Strings.Days) : (" " + Strings.Day));
						vector4 = _game._medLargeFont.MeasureString(sbuilder);
						vector3 = new Vector2((float)rectangle.Center.X - vector4.X / 2f, (float)rectangle.Center.Y - vector4.Y / 2f);
						spriteBatch.DrawOutlinedText(_game._medLargeFont, sbuilder, new Vector2(vector3.X, vector3.Y), Color.White, Color.Black, 2);
						vector3 = new Vector2((float)rectangle.Center.X - vector2.X / 2f, (float)rectangle.Center.Y - vector2.Y / 2f + vector4.Y);
					}
					spriteBatch.DrawOutlinedText(_game._medLargeFont, text, new Vector2(vector3.X, vector3.Y), Color.White, Color.Black, 2);
					vector3.X += _game._medLargeFont.MeasureString(text).X;
					if (!WaitToRespawn || (AllPlayersDead() && _game.CurrentNetworkSession.IsHost && _game.GameMode == GameModeTypes.Endurance))
					{
						if (_game.GameMode != 0 || !AllPlayersDead() || _game.CurrentNetworkSession.IsHost)
						{
							spriteBatch.DrawOutlinedText(_game._medLargeFont, text2, new Vector2(vector3.X, vector3.Y), Color.White, Color.Black, 2);
						}
					}
					else if (_game.IsOnlineGame && !AllPlayersDead() && _game.GameMode == GameModeTypes.Endurance)
					{
						sbuilder.Length = 0;
						sbuilder.Concat((int)(21.0 - timeToRespawn.ElaspedTime.TotalSeconds));
						spriteBatch.DrawOutlinedText(_game._medLargeFont, sbuilder, new Vector2(vector3.X, vector3.Y), Color.White, Color.Black, 2);
					}
				}
				DrawDistanceStr(spriteBatch);
				if (ConstructionProbe.AbleToBuild && PlayerInventory.ActiveInventoryItem != null)
				{
					BlockTypeEnum block = GetBlock(ConstructionProbe._worldIndex);
					BlockType type = BlockType.GetType(block);
					spriteBatch.DrawString(_game._medFont, type.Name, new Vector2((float)rectangle.Right - (_game._medFont.MeasureString(type.Name).X + 10f) * Screen.Adjuster.ScaleFactor.Y, (float)(_game._medFont.LineSpacing * 4) * Screen.Adjuster.ScaleFactor.Y), CMZColors.MenuAqua * 0.75f, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
				}
				Size size2 = new Size((int)((float)_gridSprite.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)_gridSprite.Height * Screen.Adjuster.ScaleFactor.Y));
				Rectangle rectangle2 = new Rectangle(rectangle.Center.X - size2.Width / 2, rectangle.Bottom - size2.Height - (int)(5f * Screen.Adjuster.ScaleFactor.Y), size2.Width, size2.Height);
				_gridSprite.Draw(spriteBatch, rectangle2, new Color(1f, 1f, 1f, GetTrayAlphaSetting()));
				Vector2 vector5 = new Vector2(-35f * Screen.Adjuster.ScaleFactor.Y, -68f * Screen.Adjuster.ScaleFactor.Y);
				Rectangle rectangle3 = rectangle2;
				rectangle2.X += (int)vector5.X;
				rectangle2.Y += (int)vector5.Y;
				_gridSprite.Draw(spriteBatch, rectangle2, Color.White);
				DrawPlayerStats(spriteBatch);
				if (LocalPlayer.Underwater)
				{
					float num3 = PlayerOxygen / 1f;
					Vector2 vector6 = new Vector2(rectangle2.Center.X, rectangle2.Top - 30);
					_bubbleBar.Draw(spriteBatch, new Rectangle((int)(vector6.X + (float)_bubbleBar.Width * (1f - num3)), (int)vector6.Y, (int)((float)_bubbleBar.Width * num3), _bubbleBar.Height), new Rectangle((int)((float)_bubbleBar.Width * (1f - num3)), 0, (int)((float)_bubbleBar.Width * num3), _bubbleBar.Height), Color.White);
				}
				int num4 = (int)(64f * Screen.Adjuster.ScaleFactor.Y);
				for (int i = 0; i < PlayerInventory.TrayManager.CurrentTrayLength; i++)
				{
					InventoryItem itemFromCurrentTray = PlayerInventory.TrayManager.GetItemFromCurrentTray(i);
					if (itemFromCurrentTray != null)
					{
						int x = (int)(59f * Screen.Adjuster.ScaleFactor.Y * (float)i + (float)rectangle2.Left + 2f * Screen.Adjuster.ScaleFactor.Y);
						itemFromCurrentTray.Draw2D(spriteBatch, new Rectangle(x, rectangle2.Top + (int)(2f * Screen.Adjuster.ScaleFactor.Y), num4, num4));
					}
				}
				for (int j = 0; j < PlayerInventory.TrayManager.CurrentTrayLength; j++)
				{
					InventoryItem itemFromNextTray = PlayerInventory.TrayManager.GetItemFromNextTray(j);
					if (itemFromNextTray != null)
					{
						int x2 = (int)(59f * Screen.Adjuster.ScaleFactor.Y * (float)j + (float)rectangle3.Left + 2f * Screen.Adjuster.ScaleFactor.Y);
						itemFromNextTray.Draw2D(spriteBatch, new Rectangle(x2, rectangle3.Top + (int)(2f * Screen.Adjuster.ScaleFactor.Y), num4, num4), new Color(1f, 1f, 1f, GetTrayAlphaSetting()), true);
					}
				}
				Rectangle destinationRectangle = new Rectangle(rectangle2.Left + (int)(7f * Screen.Adjuster.ScaleFactor.Y + 59f * Screen.Adjuster.ScaleFactor.Y * (float)PlayerInventory.SelectedInventoryIndex), (int)((float)rectangle2.Top + 7f * Screen.Adjuster.ScaleFactor.Y), num4, num4);
				_selectorSprite.Draw(spriteBatch, destinationRectangle, Color.White);
				sbuilder.Length = 0;
				if (ActiveInventoryItem != null)
				{
					Vector2 vector7 = _game._medFont.MeasureString(ActiveInventoryItem.Name);
					ActiveInventoryItem.GetDisplayText(sbuilder);
					spriteBatch.DrawString(_game._medFont, sbuilder, new Vector2(rectangle2.Left, (float)rectangle2.Y - vector7.Y * Screen.Adjuster.ScaleFactor.Y), CMZColors.MenuAqua * 0.75f, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
				}
				if (!LocalPlayer.Dead && !LocalPlayer.Shouldering)
				{
					GunInventoryItem gunInventoryItem = ActiveInventoryItem as GunInventoryItem;
					Angle fieldOfView = LocalPlayer.FPSCamera.FieldOfView;
					Angle a = Angle.FromDegrees(0.5f);
					if (gunInventoryItem != null)
					{
						if (LocalPlayer.Shouldering && !LocalPlayer.Reloading)
						{
							Angle angle = Angle.Lerp(gunInventoryItem.GunClass.MinInnaccuracy, gunInventoryItem.GunClass.ShoulderedMinAccuracy, LocalPlayer.Avatar.Animations[2].Progress);
							a = angle + InnaccuracyMultiplier * (gunInventoryItem.GunClass.ShoulderedMaxAccuracy - angle);
						}
						else
						{
							a = gunInventoryItem.GunClass.MinInnaccuracy + InnaccuracyMultiplier * (gunInventoryItem.GunClass.MaxInnaccuracy - gunInventoryItem.GunClass.MinInnaccuracy);
						}
					}
					_crosshairTickDrawLocation = a / fieldOfView * (float)Screen.Adjuster.ScreenRect.Width;
					float num5 = Math.Max(1f, Screen.Adjuster.ScaleFactor.Y);
					Color color = new Color((1f - InnaccuracyMultiplier) / 2f + 0.5f, (1f - InnaccuracyMultiplier) / 2f + 0.5f, (1f - InnaccuracyMultiplier) / 2f + 0.5f, (1f - InnaccuracyMultiplier) / 2f + 0.5f);
					spriteBatch.Draw(_crosshairTick, new Vector2((int)((float)rectangle.Center.X + _crosshairTickDrawLocation), (int)((float)rectangle.Center.Y - 1f * num5)), num5, color);
					spriteBatch.Draw(_crosshairTick, new Vector2((int)((float)rectangle.Center.X - (9f * num5 + _crosshairTickDrawLocation)), (int)((float)rectangle.Center.Y - 1f * num5)), num5, color);
					spriteBatch.Draw(_crosshairTick, new Vector2((int)((float)rectangle.Center.X + 1f * num5), (int)((float)rectangle.Center.Y - 8f * num5 - _crosshairTickDrawLocation)), color, Angle.FromDegrees(90f), Vector2.Zero, num5, SpriteEffects.None, 0f);
					spriteBatch.Draw(_crosshairTick, new Vector2((int)((float)rectangle.Center.X + 1f * num5), (int)((float)rectangle.Center.Y + _crosshairTickDrawLocation + 1f * num5)), color, Angle.FromDegrees(90f), Vector2.Zero, num5, SpriteEffects.None, 0f);
				}
				if (!fadeInGameStart.Expired)
				{
					float num6 = (float)fadeInGameStart.ElaspedTime.TotalSeconds;
					num6 = num6 * 1f - num6;
					spriteBatch.Draw(_game.DummyTexture, new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height), new Color(num6, num6, num6, 1f - (float)fadeInGameStart.ElaspedTime.TotalSeconds));
				}
				if (!drawDayTimer.Expired && !LocalPlayer.Dead)
				{
					float scale = 1f;
					float num7 = 0.333333343f;
					if (drawDayTimer.ElaspedTime < TimeSpan.FromSeconds(3.0))
					{
						scale = drawDayTimer.PercentComplete / num7;
					}
					else if (drawDayTimer.ElaspedTime > TimeSpan.FromSeconds(6.0))
					{
						scale = 1f - (drawDayTimer.PercentComplete - num7 * 2f) / num7;
					}
					sbuilder.Length = 0;
					sbuilder.Append(Strings.Day + " ");
					sbuilder.Concat(currentDay);
					spriteBatch.DrawString(_game._largeFont, sbuilder, new Vector2(rectangle.Left, (float)rectangle.Bottom - _game._largeFont.MeasureString(sbuilder).Y), CMZColors.MenuAqua * 0.75f * scale);
				}
				DrawAcheivements(device, spriteBatch, gameTime);
				spriteBatch.End();
			}
			_prevTitleSafe = rectangle;
			base.OnDraw(device, spriteBatch, gameTime);
		}

		public void DrawDistanceStr(SpriteBatch spriteBatch)
		{
			distanceBuilder.Length = 0;
			distanceBuilder.Concat(currentDistanceTraveled);
			distanceBuilder.Append("-");
			distanceBuilder.Concat(maxDistanceTraveled);
			distanceBuilder.Append(" ");
			distanceBuilder.Append(Strings.Max);
			spriteBatch.DrawString(_game._medFont, Strings.Distance, new Vector2((float)Screen.Adjuster.ScreenRect.Right - (_game._medFont.MeasureString(Strings.Distance).X + 10f) * Screen.Adjuster.ScaleFactor.Y, Screen.Adjuster.ScreenRect.Top), CMZColors.MenuAqua * 0.75f, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
			spriteBatch.DrawString(_game._medFont, distanceBuilder, new Vector2((float)Screen.Adjuster.ScreenRect.Right - (_game._medFont.MeasureString(distanceBuilder).X + 10f) * Screen.Adjuster.ScaleFactor.Y, (float)Screen.Adjuster.ScreenRect.Top + (float)_game._medFont.LineSpacing * Screen.Adjuster.ScaleFactor.Y), CMZColors.MenuAqua * 0.75f, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
		}

		public void DrawPlayerStats(SpriteBatch spriteBatch)
		{
			if (CastleMinerZGame.Instance.IsEnduranceMode)
			{
				DrawHealthBar(spriteBatch, true);
				return;
			}
			DrawHealthBar(spriteBatch, false);
			DrawStaminaBar(spriteBatch);
		}

		public void DrawHealthBar(SpriteBatch spriteBatch, bool isCenterScreen)
		{
			Color dodgerBlue = Color.DodgerBlue;
			Rectangle barLocation = isCenterScreen ? new Rectangle((int)((float)Screen.Adjuster.ScreenRect.Center.X - (float)_emptyHealthBar.Width * Screen.Adjuster.ScaleFactor.Y / 2f), (int)((float)Screen.Adjuster.ScreenRect.Top + 20f * Screen.Adjuster.ScaleFactor.Y), (int)((float)_emptyHealthBar.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)_emptyHealthBar.Height * Screen.Adjuster.ScaleFactor.Y)) : new Rectangle((int)((float)Screen.Adjuster.ScreenRect.Center.X - (float)_emptyHealthBar.Width * 2.2f * Screen.Adjuster.ScaleFactor.Y / 2f), (int)((float)Screen.Adjuster.ScreenRect.Top + 20f * Screen.Adjuster.ScaleFactor.Y), (int)((float)_emptyHealthBar.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)_emptyHealthBar.Height * Screen.Adjuster.ScaleFactor.Y));
			float ratio = PlayerHealth / 1f;
			DrawResourceBar(spriteBatch, _emptyHealthBar, dodgerBlue, ratio, barLocation);
		}

		public void DrawStaminaBar(SpriteBatch spriteBatch)
		{
			Color barColor = Color.Yellow;
			Rectangle barLocation = new Rectangle((int)((float)Screen.Adjuster.ScreenRect.Center.X + (float)_emptyStaminaBar.Width * 0.2f * Screen.Adjuster.ScaleFactor.Y / 2f), (int)((float)Screen.Adjuster.ScreenRect.Top + 20f * Screen.Adjuster.ScaleFactor.Y), (int)((float)_emptyStaminaBar.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)_emptyStaminaBar.Height * Screen.Adjuster.ScaleFactor.Y));
			float ratio = PlayerStamina / 1f;
			if (!StaminaBlockTimer.Expired)
			{
				barColor = Color.OrangeRed;
			}
			DrawResourceBar(spriteBatch, _emptyStaminaBar, barColor, ratio, barLocation);
		}

		public void DrawResourceBar(SpriteBatch spriteBatch, Sprite resourceIcon, Color barColor, float ratio, Rectangle barLocation)
		{
			resourceIcon.Draw(spriteBatch, barLocation, barColor);
			barLocation = new Rectangle(barLocation.X + (int)(56f * Screen.Adjuster.ScaleFactor.Y), barLocation.Y, (int)((float)_fullHealthBar.Width * Screen.Adjuster.ScaleFactor.Y * ratio), (int)((float)_fullHealthBar.Height * Screen.Adjuster.ScaleFactor.Y));
			spriteBatch.Draw(_fullHealthBar, new Vector2(barLocation.X, barLocation.Y), new Rectangle(0, 0, (int)((float)_fullHealthBar.Width * ratio), _fullHealthBar.Height), barColor, Angle.Zero, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
			sbuilder.Clear();
			sbuilder.Append(Math.Truncate(ratio * 100f));
			sbuilder.Append("%");
			spriteBatch.DrawString(_game._medFont, sbuilder, new Vector2(barLocation.X, (float)barLocation.Y - 20f * Screen.Adjuster.ScaleFactor.Y), barColor, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
		}

		public bool DrawAbleToBuild()
		{
			IntVector3 neighborIndex = BlockTerrain.Instance.GetNeighborIndex(ConstructionProbe._worldIndex, ConstructionProbe._inFace);
			CastleMinerZGame.Instance.LocalPlayer.MovementProbe.SkipEmbedded = false;
			Vector3 worldPosition = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
			worldPosition.Y += 0.05f;
			CastleMinerZGame.Instance.LocalPlayer.MovementProbe.Init(worldPosition, worldPosition, CastleMinerZGame.Instance.LocalPlayer.PlayerAABB);
			if (BlockTerrain.Instance.ProbeTouchesBlock(CastleMinerZGame.Instance.LocalPlayer.MovementProbe, neighborIndex))
			{
				return false;
			}
			return true;
		}

		protected void DoConstructionModeUpdate()
		{
			if (PlayerInventory.ActiveInventoryItem == null)
			{
				ConstructionProbe.Init(new Vector3(0f), new Vector3(1f), false);
			}
			else
			{
				Matrix localToWorld = LocalPlayer.FPSCamera.LocalToWorld;
				Vector3 translation = localToWorld.Translation;
				ConstructionProbe.Init(translation, Vector3.Add(translation, Vector3.Multiply(localToWorld.Forward, 5f)), PlayerInventory.ActiveInventoryItem.ItemClass.IsMeleeWeapon);
				ConstructionProbe.SkipEmbedded = true;
				ConstructionProbe.Trace();
			}
			if (ConstructionProbe.AbleToBuild)
			{
				if (PlayerInventory.ActiveInventoryItem.ItemClass is BlockInventoryItemClass && !DrawAbleToBuild())
				{
					_game.GameScreen.SelectorEntity.Visible = false;
					return;
				}
				IntVector3 worldIndex = ConstructionProbe._worldIndex;
				Vector3 value = worldIndex + new Vector3(0.5f, 0.5f, 0.5f);
				Vector3 vector = -ConstructionProbe._inNormal;
				Matrix localToParent = Matrix.Identity;
				float scaleFactor = 0.51f;
				switch (ConstructionProbe._inFace)
				{
				case BlockFace.POSX:
					localToParent = Matrix.CreateWorld(value + new Vector3(1f, 0f, 0f) * scaleFactor, -Vector3.UnitY, Vector3.UnitX);
					break;
				case BlockFace.POSY:
					localToParent = Matrix.CreateWorld(value + new Vector3(0f, 1f, 0f) * scaleFactor, Vector3.UnitX, Vector3.UnitY);
					break;
				case BlockFace.POSZ:
					localToParent = Matrix.CreateWorld(value + new Vector3(0f, 0f, 1f) * scaleFactor, Vector3.UnitX, Vector3.UnitZ);
					break;
				case BlockFace.NEGX:
					localToParent = Matrix.CreateWorld(value + new Vector3(-1f, 0f, 0f) * scaleFactor, Vector3.UnitY, -Vector3.UnitX);
					break;
				case BlockFace.NEGY:
					localToParent = Matrix.CreateWorld(value + new Vector3(0f, -1f, 0f) * scaleFactor, -Vector3.UnitX, -Vector3.UnitY);
					break;
				case BlockFace.NEGZ:
					localToParent = Matrix.CreateWorld(value + new Vector3(0f, 0f, -1f) * scaleFactor, -Vector3.UnitX, -Vector3.UnitZ);
					break;
				}
				_game.GameScreen.CrackBox.LocalPosition = worldIndex + new Vector3(0.5f, -0.002f, 0.5f);
				_game.GameScreen.SelectorEntity.LocalToParent = localToParent;
				_game.GameScreen.SelectorEntity.Visible = true;
			}
			else
			{
				_game.GameScreen.SelectorEntity.Visible = false;
			}
		}

		public void Reset()
		{
			lastTOD = -1f;
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (!CastleMinerZGame.Instance.GameScreen.DoUpdate)
			{
				return;
			}
			LocalPlayer.UpdateGunEyePointCamera(GunEyePointCameraLocation);
			if (ActiveInventoryItem is GPSItem)
			{
				GPSItem gPSItem = (GPSItem)ActiveInventoryItem;
				_game.GameScreen.GPSMarker.Visible = true;
				_game.GameScreen.GPSMarker.LocalPosition = gPSItem.PointToLocation + new Vector3(0.5f, 1f, 0.5f);
				_game.GameScreen.GPSMarker.color = gPSItem.color;
			}
			else
			{
				_game.GameScreen.GPSMarker.Visible = false;
			}
			if (ActiveInventoryItem.ItemClass is RocketLauncherGuidedInventoryItemClass)
			{
				RocketLauncherGuidedInventoryItemClass rocketLauncherGuidedInventoryItemClass = (RocketLauncherGuidedInventoryItemClass)ActiveInventoryItem.ItemClass;
				rocketLauncherGuidedInventoryItemClass.CheckIfLocked(gameTime.ElapsedGameTime);
			}
			for (int i = 0; i < _tntWaitingToExplode.Count; i++)
			{
				_tntWaitingToExplode[i].Update(gameTime.ElapsedGameTime);
				if (_tntWaitingToExplode[i].Timer.Expired)
				{
					_tntWaitingToExplode.RemoveAt(i);
					i--;
				}
			}
			CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(ActiveInventoryItem.ItemClass.ID);
			itemStats.TimeHeld += gameTime.ElapsedGameTime;
			if (!fadeInGameStart.Expired)
			{
				fadeInGameStart.Update(gameTime.ElapsedGameTime);
			}
			drawDayTimer.Update(gameTime.ElapsedGameTime);
			if (lastTOD < 0.4f && _game.GameScreen.TimeOfDay > 0.4f)
			{
				currentDay = (int)_game.GameScreen.Day + 1;
				if (_game.GameMode == GameModeTypes.Endurance && currentDay > 1)
				{
					CastleMinerZGame.Instance.PlayerStats.MaxDaysSurvived++;
				}
				SoundManager.Instance.PlayInstance("HorrorStinger");
				drawDayTimer.Reset();
			}
			lastTOD = _game.GameScreen.TimeOfDay;
			for (int j = 0; j < _triggers.Count; j++)
			{
				_triggers[j].Update();
			}
			_craterFoundTrigger.Update();
			if (_craterFoundTrigger.Triggered)
			{
				_resetCraterFoundTriggerTimer.Update(gameTime.ElapsedGameTime);
				if (_resetCraterFoundTriggerTimer.Expired)
				{
					_resetCraterFoundTriggerTimer.Reset();
					if (BlockTerrain.Instance.DepthUnderSpaceRock(LocalPlayer.LocalPosition) == 0)
					{
						_craterFoundTrigger.Reset();
					}
				}
			}
			if (LocalPlayer.Dead && !timeToShowRespawnText.Expired)
			{
				timeToShowRespawnText.Update(TimeSpan.FromSeconds(gameTime.ElapsedGameTime.TotalSeconds));
			}
			else if (WaitToRespawn)
			{
				timeToRespawn.Update(TimeSpan.FromSeconds(gameTime.ElapsedGameTime.TotalSeconds));
			}
			EquipActiveItem();
			Vector2 value = new Vector2(0f, 0f);
			Vector2 value2 = new Vector2(_game.LocalPlayer.LocalPosition.X, _game.LocalPlayer.LocalPosition.Z);
			currentDistanceTraveled = (int)Vector2.Distance(value, value2);
			if (CastleMinerZGame.TrialMode)
			{
				if (currentDistanceTraveled <= 300)
				{
					trialMaxPosition = LocalPlayer.LocalPosition;
				}
				else if (currentDistanceTraveled > 301)
				{
					LocalPlayer.LocalPosition = trialMaxPosition;
					_game.GameScreen._uiGroup.ShowPCDialogScreen(_travelMaxDialog, delegate
					{
						if (_travelMaxDialog.OptionSelected != -1)
						{
							Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
						}
					});
				}
			}
			if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance)
			{
				CastleMinerZGame.Instance.PlayerStats.MaxDistanceTraveled = Math.Max(CastleMinerZGame.Instance.PlayerStats.MaxDistanceTraveled, currentDistanceTraveled);
				CastleMinerZGame.Instance.PlayerStats.MaxDepth = Math.Min(CastleMinerZGame.Instance.PlayerStats.MaxDepth, _game.LocalPlayer.LocalPosition.Y);
			}
			if (!gameBegun && _game.GameMode == GameModeTypes.Endurance && _game.IsOnlineGame)
			{
				if (_game.CurrentNetworkSession.IsHost)
				{
					if (maxDistanceTraveled >= 100 || currentDay > 1)
					{
						_game.CurrentNetworkSession.SessionProperties[1] = 1;
						if (_game.IsOnlineGame)
						{
							_game.CurrentNetworkSession.UpdateHostSession(null, null, null, _game.CurrentNetworkSession.SessionProperties);
						}
						gameBegun = true;
						Console.WriteLine(Strings.The_Game_Has_Begun___No_Other_Players_Can_Join);
					}
				}
				else if (_game.CurrentNetworkSession.SessionProperties[1] == 1)
				{
					gameBegun = true;
					Console.WriteLine(Strings.The_Game_Has_Begun___No_Other_Players_Can_Join);
				}
			}
			if (currentDistanceTraveled > maxDistanceTraveled)
			{
				maxDistanceTraveled = currentDistanceTraveled;
			}
			Vector3 v = new Vector3(0f, 0f, 1f);
			Vector3 v2 = new Vector3(value2.X, 0f, value2.Y);
			compassRotation = v.AngleBetween(v2);
			if (LocalPlayer.InLava)
			{
				if (!lavaSoundPlayed)
				{
					lavaSoundPlayed = true;
					SoundManager.Instance.PlayInstance("Douse");
				}
				lavaDamageTimer.Update(gameTime.ElapsedGameTime);
				if (lavaDamageTimer.Expired)
				{
					ApplyDamage(0.25f, LocalPlayer.WorldPosition - new Vector3(0f, 10f, 0f));
					lavaDamageTimer.Reset();
					lavaSoundPlayed = false;
				}
			}
			else
			{
				lavaDamageTimer.Reset();
			}
			if (LocalPlayer.Underwater)
			{
				PlayerOxygen -= OxygenDecayRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (PlayerOxygen < 0f)
				{
					PlayerOxygen = 0f;
					PlayerHealth -= OxygenHealthPenaltyRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
					HealthRecoverTimer.Reset();
				}
			}
			else
			{
				PlayerOxygen = 1f;
			}
			if (!LocalPlayer.Dead)
			{
				HealthRecoverTimer.Update(gameTime.ElapsedGameTime);
				if (PlayerHealth < 1f && HealthRecoverTimer.Expired)
				{
					PlayerHealth += HealthRecoverRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
					if (PlayerHealth > 1f)
					{
						PlayerHealth = 1f;
					}
				}
				StaminaBlockTimer.Update(gameTime.ElapsedGameTime);
				StaminaRecoverTimer.Update(gameTime.ElapsedGameTime);
				if (PlayerStamina < 1f)
				{
					float num = StaminaRecoverTimer.Expired ? StaminaRecoverRate : (StaminaRecoverRate * StaminaDamagedRecoveryModifier);
					PlayerStamina += num * (float)gameTime.ElapsedGameTime.TotalSeconds;
					if (PlayerStamina > 1f)
					{
						PlayerStamina = 1f;
					}
				}
			}
			_periodicSaveTimer.Update(gameTime.ElapsedGameTime);
			if (_periodicSaveTimer.Expired)
			{
				_periodicSaveTimer.Reset();
				_game.SaveData();
			}
			int num2 = _game._terrain.DepthUnderGround(_game.LocalPlayer.LocalPosition);
			if (lightningFlashCount <= 0 || _game.LocalPlayer.LocalPosition.Y <= -32f)
			{
				CastleMinerZGame.Instance.GameScreen._sky.drawLightning = false;
			}
			if (timeToLightning.Expired)
			{
				if (lightningFlashCount > 0 && !CastleMinerZGame.Instance.GameScreen._sky.drawLightning)
				{
					CastleMinerZGame.Instance.GameScreen._sky.drawLightning = true;
					lightningFlashCount--;
					timeToLightning = new OneShotTimer(TimeSpan.FromSeconds(rand.NextDouble() / 4.0 + 0.10000000149011612));
				}
				else if (lightningFlashCount > 0 && CastleMinerZGame.Instance.GameScreen._sky.drawLightning)
				{
					CastleMinerZGame.Instance.GameScreen._sky.drawLightning = false;
				}
				else if (timeToThunder.Expired)
				{
					if (num2 < 4)
					{
						if (lightningFlashCount < 3)
						{
							SoundManager.Instance.PlayInstance("thunderLow");
						}
						else
						{
							SoundManager.Instance.PlayInstance("thunderHigh");
						}
					}
					timeToThunder = new OneShotTimer(TimeSpan.FromSeconds((float)rand.NextDouble() * 2f));
					timeToLightning = new OneShotTimer(TimeSpan.FromSeconds(rand.Next(10, 40)));
					lightningFlashCount = rand.Next(0, 4);
				}
				else
				{
					timeToThunder.Update(TimeSpan.FromSeconds(gameTime.ElapsedGameTime.TotalSeconds));
				}
			}
			else if (_game.LocalPlayer.LocalPosition.Y > -32f)
			{
				timeToLightning.Update(TimeSpan.FromSeconds(gameTime.ElapsedGameTime.TotalSeconds));
				CastleMinerZGame.Instance.GameScreen._sky.drawLightning = false;
			}
			DoConstructionModeUpdate();
			UpdateAcheivements(gameTime);
			lastPosition = LocalPlayer.LocalPosition;
			base.OnUpdate(game, gameTime);
		}

		public void Shoot(GunInventoryItemClass gun)
		{
			Matrix localToWorld = LocalPlayer.FPSCamera.LocalToWorld;
			GameMessageManager.Instance.Send(GameMessageType.LocalPlayerFiredGun, null, gun);
			if (gun is RocketLauncherBaseInventoryItemClass)
			{
				RocketLauncherBaseInventoryItemClass rocketLauncherBaseInventoryItemClass = gun as RocketLauncherBaseInventoryItemClass;
				FireRocketMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, localToWorld, gun.ID, rocketLauncherBaseInventoryItemClass.IsGuided());
			}
			else if (gun is PumpShotgunInventoryItemClass || gun is LaserShotgunClass)
			{
				ShotgunShotMessage.Send(innacuracy: (!LocalPlayer.Shouldering) ? (gun.MinInnaccuracy + InnaccuracyMultiplier * (gun.MaxInnaccuracy - gun.MinInnaccuracy)) : (gun.ShoulderedMinAccuracy + InnaccuracyMultiplier * (gun.ShoulderedMaxAccuracy - gun.ShoulderedMinAccuracy)), from: (LocalNetworkGamer)LocalPlayer.Gamer, m: localToWorld, item: gun.ID, addDropCompensation: gun.NeedsDropCompensation);
			}
			else
			{
				GunshotMessage.Send(innacuracy: (!LocalPlayer.Shouldering) ? (gun.MinInnaccuracy + InnaccuracyMultiplier * (gun.MaxInnaccuracy - gun.MinInnaccuracy)) : (gun.ShoulderedMinAccuracy + InnaccuracyMultiplier * (gun.ShoulderedMaxAccuracy - gun.ShoulderedMinAccuracy)), from: (LocalNetworkGamer)LocalPlayer.Gamer, m: localToWorld, item: gun.ID, addDropCompensation: gun.NeedsDropCompensation);
			}
		}

		public void MeleePlayer(InventoryItem tool, Player player)
		{
			MeleePlayerMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, player.Gamer, ActiveInventoryItem.ItemClass.ID, ConstructionProbe.GetIntersection());
			ParticleEmitter particleEmitter = TracerManager._smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
			particleEmitter.Reset();
			particleEmitter.Emitting = true;
			TracerManager.Instance.Scene.Children.Add(particleEmitter);
			particleEmitter.LocalPosition = ConstructionProbe.GetIntersection();
			particleEmitter.DrawPriority = 900;
			if (tool.InflictDamage())
			{
				PlayerInventory.Remove(tool);
			}
		}

		public void Melee(InventoryItem tool)
		{
			byte shooterID = (_game.LocalPlayer == null || !_game.LocalPlayer.ValidGamer) ? byte.MaxValue : _game.LocalPlayer.Gamer.Id;
			ConstructionProbe.EnemyHit.TakeDamage(ConstructionProbe.GetIntersection(), Vector3.Normalize(ConstructionProbe._end - ConstructionProbe._start), tool.ItemClass, shooterID);
			ParticleEmitter particleEmitter = TracerManager._smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
			particleEmitter.Reset();
			particleEmitter.Emitting = true;
			TracerManager.Instance.Scene.Children.Add(particleEmitter);
			particleEmitter.LocalPosition = ConstructionProbe.GetIntersection();
			particleEmitter.DrawPriority = 900;
			if (tool.InflictDamage())
			{
				PlayerInventory.Remove(tool);
			}
		}

		private bool IsValidDigTarget(BlockTypeEnum blockType, IntVector3 worldPos)
		{
			if (blockType == BlockTypeEnum.TeleportStation)
			{
				if (LocalPlayer.PlayerInventory.GetTeleportAtWorldIndex(worldPos * Vector3.One) != null)
				{
					return true;
				}
				return false;
			}
			return true;
		}

		public void Dig(InventoryItem tool, bool effective)
		{
			if (!ConstructionProbe._collides || !BlockTerrain.Instance.OkayToBuildHere(ConstructionProbe._worldIndex))
			{
				return;
			}
			BlockTypeEnum block = GetBlock(ConstructionProbe._worldIndex);
			if (!BlockType.GetType(block).CanBeDug || !IsValidDigTarget(block, ConstructionProbe._worldIndex))
			{
				return;
			}
			DigMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, false, ConstructionProbe.GetIntersection(), ConstructionProbe._inNormal, block);
			if (effective)
			{
				if (BlockType.IsContainer(block))
				{
					DestroyCrateMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex);
					Crate value;
					if (CastleMinerZGame.Instance.CurrentWorld.Crates.TryGetValue(ConstructionProbe._worldIndex, out value))
					{
						value.EjectContents();
					}
				}
				if (block == BlockTypeEnum.TeleportStation)
				{
					DestroyCustomBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex, block);
					LocalPlayer.RemoveTeleportStationObject(ConstructionProbe._worldIndex + Vector3.Zero);
				}
				CastleMinerZGame.Instance.PlayerStats.DugBlock(block);
				GameMessageManager.Instance.Send(GameMessageType.LocalPlayerMinedBlock, block, tool);
				if (BlockType.ShouldDropLoot(block))
				{
					PossibleLootType.ProcessLootBlockOutput(block, ConstructionProbe._worldIndex);
				}
				else
				{
					IntVector3 worldIndex = ConstructionProbe._worldIndex;
					if (BlockType.IsUpperDoor(block))
					{
						worldIndex += new IntVector3(0, -1, 0);
					}
					InventoryItem inventoryItem = tool.CreatesWhenDug(block, worldIndex);
					if (inventoryItem != null)
					{
						PickupManager.Instance.CreatePickup(inventoryItem, worldIndex + Vector3.Zero, false);
					}
				}
				AlterBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex, BlockTypeEnum.Empty);
				for (BlockFace blockFace = BlockFace.POSX; blockFace < BlockFace.NUM_FACES; blockFace++)
				{
					IntVector3 neighborIndex = BlockTerrain.Instance.GetNeighborIndex(ConstructionProbe._worldIndex, blockFace);
					BlockTypeEnum blockWithChanges = BlockTerrain.Instance.GetBlockWithChanges(neighborIndex);
					if (BlockType.GetType(blockWithChanges).Facing == blockFace)
					{
						InventoryItem item = BlockInventoryItemClass.CreateBlockItem(BlockType.GetType(blockWithChanges).ParentBlockType, 1, neighborIndex);
						PickupManager.Instance.CreatePickup(item, IntVector3.ToVector3(neighborIndex) + new Vector3(0.5f, 0.5f, 0.5f), false);
						AlterBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, neighborIndex, BlockTypeEnum.Empty);
						CheckToRemoveDoorConnections(BlockType.GetType(blockWithChanges).ParentBlockType, neighborIndex);
					}
				}
				CheckToRemoveDoorConnections(block, ConstructionProbe._worldIndex);
			}
			else
			{
				GameMessageManager.Instance.Send(GameMessageType.LocalPlayerPickedAtBlock, block, tool);
			}
			if (tool.InflictDamage())
			{
				PlayerInventory.Remove(tool);
			}
		}

		private void CheckToRemoveDoorConnections(BlockTypeEnum removedBlockType, IntVector3 location)
		{
			IntVector3 intVector = IntVector3.Zero;
			if (BlockType.IsLowerDoor(removedBlockType))
			{
				IntVector3 blockLocaion = location + new IntVector3(0, 1, 0);
				intVector = location;
				AlterBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, blockLocaion, BlockTypeEnum.Empty);
			}
			if (BlockType.IsUpperDoor(removedBlockType))
			{
				intVector = location + new IntVector3(0, -1, 0);
				AlterBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, intVector, BlockTypeEnum.Empty);
			}
			if (intVector != IntVector3.Zero)
			{
				DestroyCustomBlockMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, intVector, removedBlockType);
			}
		}

		public IntVector3 Build(BlockInventoryItem blockItem, bool validateOnly = false)
		{
			IntVector3 zero = IntVector3.Zero;
			if (!ConstructionProbe._collides)
			{
				return zero;
			}
			IntVector3 neighborIndex = BlockTerrain.Instance.GetNeighborIndex(ConstructionProbe._worldIndex, ConstructionProbe._inFace);
			BlockTypeEnum block = GetBlock(ConstructionProbe._worldIndex);
			if (!BlockTerrain.Instance.OkayToBuildHere(neighborIndex))
			{
				return zero;
			}
			BlockType type = BlockType.GetType(block);
			if (!type.CanBuildOn)
			{
				return zero;
			}
			if (!blockItem.CanPlaceHere(neighborIndex, ConstructionProbe._inFace))
			{
				return zero;
			}
			bool flag = true;
			if (CastleMinerZGame.Instance.CurrentNetworkSession != null)
			{
				for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
				{
					NetworkGamer networkGamer = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
					if (networkGamer == null)
					{
						continue;
					}
					Player player = (Player)networkGamer.Tag;
					if (player != null)
					{
						player.MovementProbe.SkipEmbedded = false;
						Vector3 worldPosition = player.WorldPosition;
						worldPosition.Y += 0.05f;
						player.MovementProbe.Init(worldPosition, worldPosition, player.PlayerAABB);
						if (BlockTerrain.Instance.ProbeTouchesBlock(player.MovementProbe, neighborIndex))
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					if (validateOnly)
					{
						return neighborIndex;
					}
					BoundingBox box = default(BoundingBox);
					box.Min = IntVector3.ToVector3(neighborIndex) + new Vector3(0.01f, 0.01f, 0.01f);
					box.Max = box.Min + new Vector3(0.98f, 0.98f, 0.98f);
					if (!EnemyManager.Instance.TouchesZombies(box))
					{
						BlockTypeEnum block2 = GetBlock(ConstructionProbe._worldIndex);
						DigMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, true, ConstructionProbe.GetIntersection(), ConstructionProbe._inNormal, block2);
						blockItem.AlterBlock(LocalPlayer, neighborIndex, ConstructionProbe._inFace);
						return neighborIndex;
					}
				}
			}
			return zero;
		}

		public override void OnLostFocus()
		{
			new GameTime(TimeSpan.FromSeconds(0.001), TimeSpan.FromSeconds(0.001));
			CastleMinerZGame.Instance._controllerMapping.ClearAllControls();
			base.OnLostFocus();
		}

		protected void DrawPlayerList(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			SpriteFont medFont = _game._medFont;
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			spriteBatch.Begin();
			int count = _game.CurrentNetworkSession.AllGamers.Count;
			int maxGamers = _game.CurrentNetworkSession.MaxGamers;
			_builder.Length = 0;
			_builder.Append(Strings.Players + " ").Concat(count).Append("/")
				.Concat(maxGamers);
			Vector2 vector = medFont.MeasureString(_builder);
			spriteBatch.DrawOutlinedText(medFont, _builder, new Vector2((float)screenRect.Right - vector.X, (float)screenRect.Bottom - vector.Y), Color.White, Color.Black, 2);
			float[] array = new float[1];
			float num = 0f;
			num += (array[0] = medFont.MeasureString("XXXXXXXXXXXXXXXXXXX ").X);
			float num2 = ((float)Screen.Adjuster.ScreenRect.Width - num) / 2f;
			float num3 = screenRect.Top;
			spriteBatch.DrawOutlinedText(medFont, Strings.Player, new Vector2(num2, num3), Color.Orange, Color.Black, 2);
			num3 += (float)medFont.LineSpacing;
			for (int i = 0; i < _game.CurrentNetworkSession.AllGamers.Count; i++)
			{
				NetworkGamer networkGamer = _game.CurrentNetworkSession.AllGamers[i];
				if (networkGamer.Tag == null)
				{
					continue;
				}
				Player player = (Player)networkGamer.Tag;
				spriteBatch.DrawOutlinedText(medFont, player.Gamer.Gamertag, new Vector2(num2, num3), player.Gamer.IsLocal ? Color.Red : Color.White, Color.Black, 2);
				if (player.Profile != null)
				{
					float num4 = (float)medFont.LineSpacing * 0.9f;
					float num5 = (float)medFont.LineSpacing - num4;
					if (player.GamerPicture != null)
					{
						spriteBatch.Draw(player.GamerPicture, new Rectangle((int)(num2 - (float)medFont.LineSpacing), (int)(num3 + num5), (int)num4, (int)num4), Color.White);
					}
				}
				num3 += (float)medFont.LineSpacing;
			}
			spriteBatch.End();
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			CastleMinerZGame.Instance.PlayerStats.GetItemStats(ActiveInventoryItem.ItemClass.ID);
			CastleMinerZControllerMapping controllerMapping = _game._controllerMapping;
			controllerMapping.Sensitivity = _game.PlayerStats.controllerSensitivity;
			controllerMapping.InvertY = _game.PlayerStats.InvertYAxis;
			controllerMapping.ProcessInput(inputManager.Keyboard, inputManager.Mouse, controller);
			float scaleFactor = 5f;
			Vector2 value = new Vector2(maxGunCameraShift, maxGunCameraShift);
			if (LocalPlayer.Shouldering)
			{
				value /= 2f;
			}
			Vector2 value2 = controllerMapping.Aiming * value;
			GunEyePointCameraLocation += (value2 - GunEyePointCameraLocation) * scaleFactor * (float)gameTime.ElapsedGameTime.TotalSeconds;
			GunInventoryItem gunInventoryItem = ActiveInventoryItem as GunInventoryItem;
			if (gunInventoryItem != null)
			{
				if (!LocalPlayer.InContact)
				{
					if (InnaccuracyMultiplier < 1f)
					{
						InnaccuracyMultiplier += gunInventoryItem.GunClass.InnaccuracySpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
					}
					if (InnaccuracyMultiplier > 1f)
					{
						InnaccuracyMultiplier = 1f;
					}
				}
				else if ((double)controllerMapping.Movement.X < -0.1 || (double)controllerMapping.Movement.X > 0.1 || (double)controllerMapping.Movement.Y < -0.1 || (double)controllerMapping.Movement.Y > 0.1)
				{
					if (InnaccuracyMultiplier < 1f)
					{
						float num = MathHelper.Max(Math.Abs(controllerMapping.Movement.X), Math.Abs(controllerMapping.Movement.Y)) * gunInventoryItem.GunClass.InnaccuracySpeed;
						InnaccuracyMultiplier += num * (float)gameTime.ElapsedGameTime.TotalSeconds;
					}
					if (InnaccuracyMultiplier > 1f)
					{
						InnaccuracyMultiplier = 1f;
					}
				}
				else
				{
					if (InnaccuracyMultiplier > 0f)
					{
						InnaccuracyMultiplier -= gunInventoryItem.GunClass.InnaccuracySpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
					}
					if (InnaccuracyMultiplier < 0f)
					{
						InnaccuracyMultiplier = 0f;
					}
				}
			}
			if (controller.PressedButtons.Start || inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				SoundManager.Instance.PlayInstance("Click");
				_game.GameScreen.ShowInGameMenu();
			}
			else if (_game.IsOnlineGame && !IsChatting && controllerMapping.TextChat.Released)
			{
				_game.GameScreen._uiGroup.PushScreen(_chatScreen);
			}
			else if (!LocalPlayer.Dead && controllerMapping.BlockUI.Pressed)
			{
				_game.GameScreen.ShowBlockPicker();
				SoundManager.Instance.PlayInstance("Click");
			}
			LocalPlayer.ProcessInput(_game._controllerMapping, gameTime);
			if (_game.IsOnlineGame)
			{
				if (controllerMapping.PlayersScreen.Pressed)
				{
					showPlayers = true;
				}
				else if (!controllerMapping.PlayersScreen.Held)
				{
					showPlayers = false;
				}
			}
			else
			{
				showPlayers = false;
			}
			_game.ShowTitleSafeArea = !_hideUI;
			PlayerInventory.Update(gameTime);
			_game.GameScreen.CrackBox.CrackAmount = 0f;
			if (LocalPlayer.Dead)
			{
				LocalPlayer.UsingTool = false;
				if ((controllerMapping.Jump.Pressed || inputManager.Keyboard.WasKeyPressed(Keys.Enter) || inputManager.Mouse.LeftButtonPressed) && timeToShowRespawnText.Expired)
				{
					if (_game.IsOnlineGame && AllPlayersDead() && _game.GameMode == GameModeTypes.Endurance)
					{
						if (_game.CurrentNetworkSession.IsHost)
						{
							RestartLevelMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer);
							BroadcastTextMessage.Send(_game.MyNetworkGamer, LocalPlayer.Gamer.Gamertag + " " + Strings.Has_Restarted_The_Game);
						}
					}
					else if (!WaitToRespawn)
					{
						RespawnPlayer();
					}
				}
			}
			else
			{
				if (controllerMapping.NextItem.Pressed && !LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					PlayerInventory.SelectedInventoryIndex++;
					PlayerInventory.SelectedInventoryIndex %= 8;
					LocalPlayer.Shouldering = false;
					PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.PrevoiusItem.Pressed && !LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					PlayerInventory.SelectedInventoryIndex--;
					if (PlayerInventory.SelectedInventoryIndex < 0)
					{
						PlayerInventory.SelectedInventoryIndex = 8 + PlayerInventory.SelectedInventoryIndex;
					}
					LocalPlayer.Shouldering = false;
					PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.Slot1.Pressed && !LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					PlayerInventory.SelectedInventoryIndex = 0;
					LocalPlayer.Shouldering = false;
					PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.Slot2.Pressed && !LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					PlayerInventory.SelectedInventoryIndex = 1;
					LocalPlayer.Shouldering = false;
					PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.Slot3.Pressed && !LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					PlayerInventory.SelectedInventoryIndex = 2;
					LocalPlayer.Shouldering = false;
					PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.Slot4.Pressed && !LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					PlayerInventory.SelectedInventoryIndex = 3;
					LocalPlayer.Shouldering = false;
					PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.Slot5.Pressed && !LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					PlayerInventory.SelectedInventoryIndex = 4;
					LocalPlayer.Shouldering = false;
					PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.Slot6.Pressed && !LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					PlayerInventory.SelectedInventoryIndex = 5;
					LocalPlayer.Shouldering = false;
					PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.Slot7.Pressed && !LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					PlayerInventory.SelectedInventoryIndex = 6;
					LocalPlayer.Shouldering = false;
					PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.Slot8.Pressed && !LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					PlayerInventory.SelectedInventoryIndex = 7;
					LocalPlayer.Shouldering = false;
					PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				if (controllerMapping.DropQuickbarItem.Pressed)
				{
					PlayerInventory.DropOneSelectedTrayItem();
				}
				if (controllerMapping.SwitchTray.Pressed)
				{
					PlayerInventory.SwitchCurrentTray();
				}
				if (controllerMapping.Activate.Pressed && ConstructionProbe._collides)
				{
					BlockTypeEnum block = GetBlock(ConstructionProbe._worldIndex);
					if (BlockType.IsContainer(block))
					{
						Crate crate = CastleMinerZGame.Instance.CurrentWorld.GetCrate(ConstructionProbe._worldIndex, true);
						_crateScreen.CurrentCrate = crate;
						_game.GameScreen._uiGroup.PushScreen(_crateScreen);
						SoundManager.Instance.PlayInstance("Click");
					}
					else if (BlockType.IsSpawnerClickable(block))
					{
						Spawner spawner = CastleMinerZGame.Instance.CurrentWorld.GetSpawner(ConstructionProbe._worldIndex, true, block);
						if (_game.IsOnlineGame)
						{
							BroadcastTextMessage.Send(_game.MyNetworkGamer, LocalPlayer.Gamer.Gamertag + " " + Strings.Has_triggered_a_monster_spawner);
						}
						spawner.StartSpawner(block);
						SoundManager.Instance.PlayInstance("Click");
					}
					else
					{
						switch (block)
						{
						case BlockTypeEnum.TeleportStation:
							PlayerInventory.ShowTeleportStationMenu(ConstructionProbe._worldIndex + Vector3.Zero);
							break;
						case BlockTypeEnum.TNT:
							SetFuseForExplosive(ConstructionProbe._worldIndex, ExplosiveTypes.TNT);
							break;
						case BlockTypeEnum.C4:
							SetFuseForExplosive(ConstructionProbe._worldIndex, ExplosiveTypes.C4);
							break;
						case BlockTypeEnum.StrongUpperDoorOpen:
						{
							DoorOpenCloseMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex, false);
							AlterBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex, BlockTypeEnum.StrongUpperDoorClosed);
							BlockTypeEnum block5 = GetBlock(ConstructionProbe._worldIndex + new IntVector3(0, -1, 0));
							if (block5 == BlockTypeEnum.StrongLowerDoorOpenX)
							{
								AlterBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.StrongLowerDoorClosedX);
							}
							else
							{
								AlterBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.StrongLowerDoorClosedZ);
							}
							SoundManager.Instance.PlayInstance("Click");
							break;
						}
						case BlockTypeEnum.StrongUpperDoorClosed:
						{
							DoorOpenCloseMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex, true);
							AlterBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex, BlockTypeEnum.StrongUpperDoorOpen);
							BlockTypeEnum block3 = GetBlock(ConstructionProbe._worldIndex + new IntVector3(0, -1, 0));
							if (block3 == BlockTypeEnum.StrongLowerDoorClosedX)
							{
								AlterBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.StrongLowerDoorOpenX);
							}
							else
							{
								AlterBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.StrongLowerDoorOpenZ);
							}
							SoundManager.Instance.PlayInstance("Click");
							break;
						}
						case BlockTypeEnum.StrongLowerDoorClosedX:
							UseDoor(BlockTypeEnum.StrongLowerDoorOpenX, BlockTypeEnum.StrongUpperDoorOpen);
							break;
						case BlockTypeEnum.StrongLowerDoorClosedZ:
							UseDoor(BlockTypeEnum.StrongLowerDoorOpenZ, BlockTypeEnum.StrongUpperDoorOpen);
							break;
						case BlockTypeEnum.StrongLowerDoorOpenX:
							UseDoor(BlockTypeEnum.StrongLowerDoorClosedX, BlockTypeEnum.StrongUpperDoorClosed);
							break;
						case BlockTypeEnum.StrongLowerDoorOpenZ:
							UseDoor(BlockTypeEnum.StrongLowerDoorClosedZ, BlockTypeEnum.StrongUpperDoorClosed);
							break;
						case BlockTypeEnum.NormalUpperDoorOpen:
						{
							DoorOpenCloseMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex, false);
							AlterBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex, BlockTypeEnum.NormalUpperDoorClosed);
							BlockTypeEnum block4 = GetBlock(ConstructionProbe._worldIndex + new IntVector3(0, -1, 0));
							if (block4 == BlockTypeEnum.NormalLowerDoorOpenX)
							{
								AlterBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.NormalLowerDoorClosedX);
							}
							else
							{
								AlterBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.NormalLowerDoorClosedZ);
							}
							SoundManager.Instance.PlayInstance("Click");
							break;
						}
						case BlockTypeEnum.NormalUpperDoorClosed:
						{
							DoorOpenCloseMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex, true);
							AlterBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex, BlockTypeEnum.NormalUpperDoorOpen);
							BlockTypeEnum block2 = GetBlock(ConstructionProbe._worldIndex + new IntVector3(0, -1, 0));
							if (block2 == BlockTypeEnum.NormalLowerDoorClosedX)
							{
								AlterBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.NormalLowerDoorOpenX);
							}
							else
							{
								AlterBlockMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.NormalLowerDoorOpenZ);
							}
							SoundManager.Instance.PlayInstance("Click");
							break;
						}
						case BlockTypeEnum.NormalLowerDoorClosedX:
							UseDoor(BlockTypeEnum.NormalLowerDoorOpenX, BlockTypeEnum.NormalUpperDoorOpen);
							break;
						case BlockTypeEnum.NormalLowerDoorClosedZ:
							UseDoor(BlockTypeEnum.NormalLowerDoorOpenZ, BlockTypeEnum.NormalUpperDoorOpen);
							break;
						case BlockTypeEnum.NormalLowerDoorOpenX:
							UseDoor(BlockTypeEnum.NormalLowerDoorClosedX, BlockTypeEnum.NormalUpperDoorClosed);
							break;
						case BlockTypeEnum.NormalLowerDoorOpenZ:
							UseDoor(BlockTypeEnum.NormalLowerDoorClosedZ, BlockTypeEnum.NormalUpperDoorClosed);
							break;
						default:
							SoundManager.Instance.PlayInstance("Error");
							break;
						case BlockTypeEnum.EnemySpawnAltar:
							break;
						}
					}
				}
				if (ActiveInventoryItem == null)
				{
					LocalPlayer.UsingTool = false;
				}
				else
				{
					ActiveInventoryItem.ProcessInput(this, controllerMapping);
					PlayerInventory.RemoveEmptyItems();
				}
			}
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		private void UseDoor(BlockTypeEnum bottomDoorPiece, BlockTypeEnum topDoorPiece)
		{
			LocalNetworkGamer from = (LocalNetworkGamer)LocalPlayer.Gamer;
			DoorOpenCloseMessage.Send(from, ConstructionProbe._worldIndex, true);
			AlterBlockMessage.Send(from, ConstructionProbe._worldIndex, bottomDoorPiece);
			AlterBlockMessage.Send(from, ConstructionProbe._worldIndex + new IntVector3(0, 1, 0), topDoorPiece);
			SoundManager.Instance.PlayInstance("Click");
		}

		public void SetFuseForExplosive(IntVector3 location, ExplosiveTypes explosiveType)
		{
			Explosive item = new Explosive(location, explosiveType);
			if (!_tntWaitingToExplode.Contains(item))
			{
				_tntWaitingToExplode.Add(item);
				AddExplosiveFlashMessage.Send((LocalNetworkGamer)LocalPlayer.Gamer, location);
			}
		}
	}
}
