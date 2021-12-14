using DNA.Audio;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Drawing;
using DNA.Drawing.Animation;
using DNA.Drawing.Effects;
using DNA.Net.GamerServices;
using DNA.Profiling;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.AI
{
	public class BaseZombie : SkinnedModelEntity, IShootableEnemy
	{
		public StateMachine<BaseZombie> StateMachine;

		public Player Target;

		public Random Rnd;

		public AnimationPlayer CurrentPlayer;

		public bool IsLocalEnemy;

		private EnemyManager _mgr;

		public Spawner SpawnSource;

		public int SpawnValue;

		public int PlayerDistanceLimit = 25;

		public AABBTraceProbe MovementProbe = new AABBTraceProbe();

		public BoundingBox PlayerAABB = new BoundingBox(new Vector3(-0.35f, 0f, -0.35f), new Vector3(0.35f, 1.65f, 0.35f));

		public bool OnGround;

		public bool TouchingWall;

		public AudioEmitter SoundEmitter = new AudioEmitter();

		public SoundCue3D ZombieGrowlCue;

		public EnemyType.InitPackage InitPkg;

		public float TimeLeftTilFast;

		public float TimeLeftTilRunFast;

		public float CurrentSpeed;

		public float FrustrationCount = 15f;

		public float StateTimer;

		public int SwingCount;

		public int AnimationIndex;

		public int HitCount;

		public int MissCount;

		public float SoundUpdateTimer;

		public bool IsBlocking;

		public bool IsHittable;

		public bool IsMovingFast;

		public EnemyType EType;

		public int EnemyID;

		public float Health;

		public Vector3[] DirectLightColor = new Vector3[2];

		public Vector3[] DirectLightDirection = new Vector3[2];

		public Vector3 AmbientLight = Color.Gray.ToVector3();

		private ModelEntity _shadow;

		public static Random _rand;

		private static Model _shadowModel;

		private TraceProbe shadowProbe = new TraceProbe();

		public BasicPhysics PlayerPhysics
		{
			get
			{
				return (BasicPhysics)base.Physics;
			}
		}

		public bool IsNearAnimationEnd
		{
			get
			{
				return (CurrentPlayer.Duration - CurrentPlayer.CurrentTime).TotalSeconds < 0.25;
			}
		}

		public bool IsDead
		{
			get
			{
				return StateMachine._currentState == EType.GetDieState(this);
			}
		}

		public BaseZombie(EnemyManager mgr, EnemyTypeEnum et, Player target, Vector3 pos, int id, int seed, EnemyType.InitPackage initpkg)
			: base(CastleMinerZGame.Instance.Content.Load<Model>(EnemyType.GetEnemyType(et).ModelName))
		{
			Rnd = new Random(seed);
			Target = target;
			StateMachine = new StateMachine<BaseZombie>(this);
			_mgr = mgr;
			EType = EnemyType.GetEnemyType(et);
			CurrentPlayer = null;
			base.LocalRotation = Quaternion.Identity;
			base.LocalPosition = pos;
			base.LocalScale = new Vector3(EType.Scale);
			EnemyID = id;
			IsBlocking = false;
			IsHittable = false;
			Health = EType.StartingHealth;
			PlayerDistanceLimit = EType.StartingDistanceLimit;
			IsLocalEnemy = (target == CastleMinerZGame.Instance.LocalPlayer);
			DrawPriority = (int)(501 + EType.TextureIndex);
			Collider = true;
			base.Physics = new Player.NoMovePhysics(this);
			PlayerPhysics.WorldAcceleration = BasicPhysics.Gravity;
			SoundUpdateTimer = 0f;
			InitPkg = initpkg;
			CurrentSpeed = InitPkg.SlowSpeed;
			TimeLeftTilFast = InitPkg.NormalActivationTime;
			TimeLeftTilRunFast = InitPkg.RunActivationTime;
			IsMovingFast = false;
			StateMachine.ChangeState(EType.GetEmergeState(this));
			_shadow = new ModelEntity(_shadowModel);
			_shadow.LocalPosition = new Vector3(0f, 0.05f, 0f);
			_shadow.BlendState = BlendState.AlphaBlend;
			_shadow.DepthStencilState = DepthStencilState.DepthRead;
			_shadow.DrawPriority = 200;
			base.Children.Add(_shadow);
		}

		public void SetDistanceLimit()
		{
			int playerDistanceLimit = EType.StartingDistanceLimit;
			if (SpawnSource != null)
			{
				playerDistanceLimit = ((!SpawnSource.IsHellBlock()) ? 100 : 300);
			}
			PlayerDistanceLimit = playerDistanceLimit;
		}

		public void ResetFrustration()
		{
			FrustrationCount = 2.5f;
		}

		public void SpeedUp()
		{
			if (EType.HasRunFast)
			{
				IsMovingFast = true;
				CurrentSpeed = InitPkg.FastSpeed;
				((EnemyBaseState)StateMachine._currentState).HandleSpeedUp(this);
			}
		}

		public float TimeToIntercept()
		{
			Vector3 vector = base.WorldPosition - Target.WorldPosition;
			vector.Y = 0f;
			float num = vector.LengthSquared();
			if (num < 1f)
			{
				return 0f;
			}
			Vector3 worldVelocity = Target.PlayerPhysics.WorldVelocity;
			Vector3 vector2 = worldVelocity - PlayerPhysics.WorldVelocity;
			vector2.Y = 0f;
			if (Vector3.Dot(vector, vector2) < 0f)
			{
				return float.MaxValue;
			}
			float num2 = vector2.LengthSquared();
			if (num2 < 0.001f)
			{
				return float.MaxValue;
			}
			num2 = (float)Math.Sqrt(num2);
			vector2 *= 1f / num2;
			float num3 = (float)Math.Sqrt(num);
			vector *= 1f / num3;
			float num4 = Vector3.Dot(vector2, vector);
			if (num4 < 0.01f)
			{
				return float.MaxValue;
			}
			return num3 / num4 / num2;
		}

		public void Remove()
		{
			_mgr.RemoveZombie(this);
		}

		public void Kill()
		{
			if (!IsDead)
			{
				StateMachine.ChangeState(EType.GetDieState(this));
			}
		}

		public void GiveUp()
		{
			if (!IsDead && StateMachine._currentState != EType.GetGiveUpState(this))
			{
				StateMachine.ChangeState(EType.GetGiveUpState(this));
			}
		}

		public bool IsHeadshot(Vector3 hit)
		{
			return hit.Y - base.LocalPosition.Y > 1.5f;
		}

		public void TakeExplosiveDamage(float damageAmount, byte shooterID, InventoryItemIDs itemID)
		{
			if (!(Health > 0f))
			{
				return;
			}
			Health -= damageAmount;
			if (Health <= 0f)
			{
				if (itemID == InventoryItemIDs.TNT && CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance && shooterID == CastleMinerZGame.Instance.LocalPlayer.Gamer.Id)
				{
					CastleMinerZGame.Instance.PlayerStats.EnemiesKilledWithTNT++;
				}
				else if (itemID == InventoryItemIDs.Grenade && CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance && shooterID == CastleMinerZGame.Instance.LocalPlayer.Gamer.Id)
				{
					CastleMinerZGame.Instance.PlayerStats.EnemiesKilledWithGrenade++;
				}
				KillEnemyMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, EnemyID, Target.Gamer.Id, shooterID, itemID);
			}
			else if (StateMachine._currentState != EType.GetHitState(this))
			{
				StateMachine.ChangeState(EType.GetHitState(this));
			}
		}

		public void AttachProjectile(Entity projectile)
		{
			AdoptChild(projectile);
		}

		public void TakeDamage(Vector3 damagePosition, Vector3 damageDirection, InventoryItem.InventoryItemClass itemClass, byte shooterID)
		{
			DamageType enemyDamageType = itemClass.EnemyDamageType;
			float enemyDamage = itemClass.EnemyDamage;
			if (CastleMinerZGame.Instance.IsLocalPlayerId(shooterID))
			{
				CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(itemClass.ID);
				itemStats.Hits++;
			}
			if (!(Health > 0f))
			{
				return;
			}
			float damageTypeMultiplier = EType.GetDamageTypeMultiplier(enemyDamageType, IsHeadshot(damagePosition));
			enemyDamage *= damageTypeMultiplier;
			Health -= enemyDamage;
			if (Health <= 0f)
			{
				if (itemClass is LaserGunInventoryItemClass && CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance && shooterID == CastleMinerZGame.Instance.LocalPlayer.Gamer.Id)
				{
					CastleMinerZGame.Instance.PlayerStats.EnemiesKilledWithLaserWeapon++;
				}
				KillEnemyMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, EnemyID, Target.Gamer.Id, shooterID, itemClass.ID);
			}
			else if (StateMachine._currentState != EType.GetHitState(this))
			{
				StateMachine.ChangeState(EType.GetHitState(this));
			}
		}

		public void CreatePickup()
		{
			if (PickupManager.Instance == null)
			{
				return;
			}
			InventoryItem inventoryItem = null;
			float num = base.LocalPosition.Length();
			float min = (num / 5000f).Clamp(0f, 1f);
			float num2 = MathTools.RandomFloat(min, 1f);
			if (EType.FoundIn == EnemyType.FoundInEnum.HELL)
			{
				if ((double)num2 < 0.5)
				{
					inventoryItem = InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 1);
				}
				else if ((double)num2 < 0.8)
				{
					inventoryItem = InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 2);
				}
				else
				{
					inventoryItem = InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 3);
				}
			}
			if (EType.FoundIn == EnemyType.FoundInEnum.CRASHSITE)
			{
				inventoryItem = (((double)num2 < 0.5) ? InventoryItem.CreateItem(InventoryItemIDs.Copper, 1) : ((!((double)num2 < 0.8)) ? InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1) : InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)));
			}
			else
			{
				if ((double)num2 < 0.5)
				{
					return;
				}
				inventoryItem = ((base.LocalPosition.Y < -40f) ? (((double)num2 < 0.7) ? InventoryItem.CreateItem(InventoryItemIDs.CopperOre, 1) : (((double)num2 < 0.8) ? InventoryItem.CreateItem(InventoryItemIDs.Copper, 1) : (((double)num2 < 0.85) ? InventoryItem.CreateItem(InventoryItemIDs.Iron, 1) : ((!((double)num2 < 0.9)) ? InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1) : InventoryItem.CreateItem(InventoryItemIDs.GoldOre, 1))))) : (((double)num2 < 0.7) ? InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 1) : (((double)num2 < 0.8) ? InventoryItem.CreateItem(InventoryItemIDs.Coal, 1) : (((double)num2 < 0.85) ? InventoryItem.CreateItem(InventoryItemIDs.CopperOre, 1) : ((!((double)num2 < 0.9)) ? InventoryItem.CreateItem(InventoryItemIDs.IronOre, 1) : InventoryItem.CreateItem(InventoryItemIDs.Copper, 1))))));
			}
			if (inventoryItem != null)
			{
				PickupManager.Instance.CreatePickup(inventoryItem, base.LocalPosition + new Vector3(0f, 1f, 0f), false);
			}
		}

		public bool Touches(BoundingBox box)
		{
			BoundingBox playerAABB = PlayerAABB;
			Vector3 worldPosition = base.WorldPosition;
			playerAABB.Min += worldPosition;
			playerAABB.Max += worldPosition;
			return playerAABB.Intersects(box);
		}

		static BaseZombie()
		{
			_rand = new Random();
			_shadowModel = CastleMinerZGame.Instance.Content.Load<Model>("Shadow");
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			if (!BlockTerrain.Instance.RegionIsLoaded(base.LocalPosition))
			{
				Remove();
				return;
			}
			Vector3 vector = base.WorldPosition + new Vector3(0f, 1f, 0f);
			shadowProbe.Init(vector, base.WorldPosition + new Vector3(0f, -2.5f, 0f));
			shadowProbe.SkipEmbedded = true;
			BlockTerrain.Instance.Trace(shadowProbe);
			_shadow.Visible = shadowProbe._collides;
			if (_shadow.Visible)
			{
				Vector3 intersection = shadowProbe.GetIntersection();
				Vector3 value = intersection - base.WorldPosition;
				float num = Math.Abs(value.Y);
				_shadow.LocalPosition = value + new Vector3(0f, 0.05f, 0f);
				int num2 = 2;
				float num3 = num / (float)num2;
				_shadow.LocalScale = new Vector3(1f + 2f * num3, 1f, 1f + 2f * num3);
				_shadow.EntityColor = new Color(1f, 1f, 1f, Math.Max(0f, 0.5f * (1f - num3)));
			}
			BlockTerrain.Instance.GetEnemyLighting(vector, ref DirectLightDirection[0], ref DirectLightColor[0], ref DirectLightDirection[1], ref DirectLightColor[1], ref AmbientLight);
			using (Profiler.TimeSection("Zombie Update", ProfilerThreadEnum.MAIN))
			{
				if (!Target.ValidGamer || Target.Dead)
				{
					GiveUp();
				}
				StateMachine.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
				base.OnUpdate(gameTime);
			}
		}

		public override bool ResolveCollsion(Entity e, out Plane collsionPlane, GameTime dt)
		{
			using (Profiler.TimeSection("Zombie Collision", ProfilerThreadEnum.MAIN))
			{
				base.ResolveCollsion(e, out collsionPlane, dt);
				bool result = false;
				if (e == BlockTerrain.Instance)
				{
					float num = (float)dt.ElapsedGameTime.TotalSeconds;
					Vector3 worldPosition = base.WorldPosition;
					Vector3 vector = worldPosition;
					Vector3 vector2 = PlayerPhysics.WorldVelocity;
					OnGround = false;
					TouchingWall = false;
					MovementProbe.SkipEmbedded = true;
					int num2 = 0;
					do
					{
						Vector3 vector3 = vector;
						Vector3 vector4 = Vector3.Multiply(vector2, num);
						vector += vector4;
						MovementProbe.Init(vector3, vector, PlayerAABB);
						BlockTerrain.Instance.Trace(MovementProbe);
						if (MovementProbe._collides)
						{
							result = true;
							if (MovementProbe._inFace == BlockFace.POSY)
							{
								OnGround = true;
							}
							else
							{
								TouchingWall = true;
							}
							if (MovementProbe._startsIn)
							{
								TouchingWall = true;
								break;
							}
							float num3 = Math.Max(MovementProbe._inT - 0.001f, 0f);
							vector = vector3 + vector4 * num3;
							vector2 -= Vector3.Multiply(MovementProbe._inNormal, Vector3.Dot(MovementProbe._inNormal, vector2));
							num *= 1f - num3;
							if (num <= 1E-07f)
							{
								break;
							}
							if (vector2.LengthSquared() <= 1E-06f || Vector3.Dot(PlayerPhysics.WorldVelocity, vector2) <= 1E-06f)
							{
								vector2 = Vector3.Zero;
								break;
							}
						}
						num2++;
					}
					while (MovementProbe._collides && num2 < 4);
					if (num2 == 4)
					{
						vector2 = Vector3.Zero;
					}
					base.LocalPosition = vector;
					PlayerPhysics.WorldVelocity = vector2;
					SoundUpdateTimer -= (float)dt.ElapsedGameTime.TotalSeconds;
					if (SoundUpdateTimer < 0.1f)
					{
						SoundUpdateTimer += 0.1f;
						SoundEmitter.Position = vector;
						SoundEmitter.Forward = base.LocalToWorld.Forward;
						SoundEmitter.Up = Vector3.Up;
						SoundEmitter.Velocity = vector2;
					}
					vector2.Y = 0f;
					if (vector2.LengthSquared() < 0.25f)
					{
						FrustrationCount -= (float)dt.ElapsedGameTime.TotalSeconds;
					}
					else
					{
						ResetFrustration();
					}
					vector.Y += 1.2f;
				}
				return result;
			}
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (effect is DNAEffect)
			{
				DNAEffect dNAEffect = (DNAEffect)effect;
				if (dNAEffect.Parameters["LightDirection1"] != null)
				{
					dNAEffect.Parameters["LightDirection1"].SetValue(-DirectLightDirection[0]);
					dNAEffect.Parameters["LightColor1"].SetValue(DirectLightColor[0]);
					dNAEffect.Parameters["LightDirection2"].SetValue(-DirectLightDirection[1]);
					dNAEffect.Parameters["LightColor2"].SetValue(DirectLightColor[1]);
					dNAEffect.AmbientColor = ColorF.FromVector3(AmbientLight);
				}
				dNAEffect.DiffuseMap = EType.EnemyTexture;
			}
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}
	}
}
