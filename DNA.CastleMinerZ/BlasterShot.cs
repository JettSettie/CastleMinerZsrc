using DNA.Audio;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.Drawing;
using DNA.Drawing.Particles;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DNA.CastleMinerZ
{
	public class BlasterShot : Entity
	{
		private static List<BlasterShot> _garbage = new List<BlasterShot>();

		private static ParticleEffect _spashEffect;

		private static ParticleEffect _sparkEffect;

		private static ParticleEffect _smokeEffect;

		private static Model _tracerModel;

		private static TracerManager.TracerProbe tp = new TracerManager.TracerProbe();

		private bool _headshot;

		private bool _firstUpdate = true;

		private bool _noCollideFrame;

		private Vector3 _lastPosition;

		private byte _shooter;

		private int _enemyID = -1;

		private Color _color;

		public AudioEmitter Emitter = new AudioEmitter();

		public int CollisionsRemaining;

		private bool ReflectedShot;

		private static readonly TimeSpan TotalLifeTime = TimeSpan.FromSeconds(3.0);

		private TimeSpan _lifeTime = TotalLifeTime;

		private ModelEntity _tracer;

		private InventoryItemIDs _weaponUsed;

		private LaserGunInventoryItemClass _weaponClassUsed;

		private Vector3 _velocity;

		private ExplosiveTypes _explosiveType = ExplosiveTypes.Laser;

		public static void Init()
		{
			_tracerModel = CastleMinerZGame.Instance.Content.Load<Model>("Props\\Projectiles\\Laser\\Tracer_Bolt");
			_smokeEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\SmokeEffect");
			_sparkEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\SparksEffect");
			_spashEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\BlasterFlash");
		}

		public static BlasterShot Create(Vector3 position, Vector3 velocity, int enemyId, InventoryItemIDs item)
		{
			BlasterShot blasterShot = null;
			LaserGunInventoryItemClass laserGunInventoryItemClass = InventoryItem.GetClass(item) as LaserGunInventoryItemClass;
			if (laserGunInventoryItemClass != null)
			{
				for (int i = 0; i < _garbage.Count; i++)
				{
					if (_garbage[i].Parent == null)
					{
						blasterShot = _garbage[i];
						break;
					}
				}
				if (blasterShot == null)
				{
					blasterShot = new BlasterShot(0);
				}
				blasterShot._lifeTime = TotalLifeTime;
				blasterShot._color = new Color(laserGunInventoryItemClass.TracerColor);
				blasterShot._enemyID = enemyId;
				blasterShot._tracer.EntityColor = blasterShot._color;
				blasterShot.CollisionsRemaining = 3;
				blasterShot.ReflectedShot = false;
				blasterShot._velocity = velocity * 200f;
				blasterShot.LocalToParent = MathTools.CreateWorld(position, velocity);
				blasterShot._firstUpdate = true;
				blasterShot._noCollideFrame = false;
				blasterShot._weaponClassUsed = laserGunInventoryItemClass;
				blasterShot._weaponUsed = item;
				blasterShot._lastPosition = position;
				blasterShot._explosiveType = (laserGunInventoryItemClass.IsHarvestWeapon() ? ExplosiveTypes.Harvest : ExplosiveTypes.Laser);
				_garbage.Add(blasterShot);
			}
			return blasterShot;
		}

		public static BlasterShot Create(Vector3 position, Vector3 velocity, InventoryItemIDs item, byte shooterID)
		{
			BlasterShot blasterShot = null;
			LaserGunInventoryItemClass laserGunInventoryItemClass = InventoryItem.GetClass(item) as LaserGunInventoryItemClass;
			if (laserGunInventoryItemClass != null)
			{
				for (int i = 0; i < _garbage.Count; i++)
				{
					if (_garbage[i].Parent == null)
					{
						blasterShot = _garbage[i];
						break;
					}
				}
				if (blasterShot == null)
				{
					blasterShot = new BlasterShot(shooterID);
				}
				blasterShot._lifeTime = TotalLifeTime;
				blasterShot._color = new Color(laserGunInventoryItemClass.TracerColor);
				blasterShot._shooter = shooterID;
				blasterShot._enemyID = -1;
				blasterShot._tracer.EntityColor = blasterShot._color;
				blasterShot.CollisionsRemaining = 30;
				blasterShot.ReflectedShot = false;
				blasterShot._velocity = velocity * 200f;
				blasterShot.LocalToParent = MathTools.CreateWorld(position, velocity);
				blasterShot._firstUpdate = true;
				blasterShot._noCollideFrame = false;
				blasterShot._weaponClassUsed = laserGunInventoryItemClass;
				blasterShot._weaponUsed = item;
				blasterShot._lastPosition = position;
				blasterShot._explosiveType = (laserGunInventoryItemClass.IsHarvestWeapon() ? ExplosiveTypes.Harvest : ExplosiveTypes.Laser);
				_garbage.Add(blasterShot);
			}
			return blasterShot;
		}

		public BlasterShot()
		{
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			bool flag = false;
			_lifeTime -= gameTime.ElapsedGameTime;
			if (_lifeTime <= TimeSpan.Zero)
			{
				flag = true;
			}
			else
			{
				_lastPosition = base.WorldPosition;
				base.LocalPosition = _lastPosition + _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
				if ((CastleMinerZGame.Instance.CurrentNetworkSession != null && CastleMinerZGame.Instance.PVPState != 0) || _enemyID >= 0)
				{
					bool flag2 = false;
					bool flag3 = false;
					float num = 2.14748365E+09f;
					Vector3 vector = Vector3.Zero;
					Player player = null;
					for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
					{
						NetworkGamer networkGamer = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
						if (networkGamer.Tag == null)
						{
							continue;
						}
						Player player2 = (Player)networkGamer.Tag;
						if (player2.ValidLivingGamer)
						{
							Vector3 worldPosition = player2.WorldPosition;
							BoundingBox playerAABB = player2.PlayerAABB;
							playerAABB.Min += worldPosition;
							playerAABB.Max += worldPosition;
							tp.Reset();
							tp.TestBoundBox(playerAABB);
							if (tp._collides && tp._inT < num)
							{
								player = player2;
								vector = tp.GetIntersection();
								flag2 = true;
								flag3 = true;
								num = tp._inT;
							}
						}
					}
					tp.Reset();
					BlockTerrain.Instance.Trace(tp);
					if (tp._collides && tp._inT < num)
					{
						flag2 = false;
						flag3 = true;
						vector = tp.GetIntersection();
					}
					if (flag3)
					{
						Vector3 damageSource = vector;
						if (_enemyID < 0)
						{
							flag = true;
						}
						if (flag2)
						{
							if (player.IsLocal && (_enemyID > 0 || _shooter != player.Gamer.Id))
							{
								LocalNetworkGamer localNetworkGamer = (LocalNetworkGamer)player.Gamer;
								if (CastleMinerZGame.Instance.PVPState == CastleMinerZGame.PVPEnum.Everyone || (!localNetworkGamer.IsHost && !localNetworkGamer.SignedInGamer.IsFriend(CastleMinerZGame.Instance.CurrentNetworkSession.Host)))
								{
									InGameHUD.Instance.ApplyDamage(0.4f, damageSource);
								}
							}
							SoundManager.Instance.PlayInstance("BulletHitHuman", player.SoundEmitter);
						}
					}
				}
				IShootableEnemy shootableEnemy = null;
				tp.Init(_lastPosition, base.WorldPosition);
				if (_enemyID < 0)
				{
					shootableEnemy = EnemyManager.Instance.Trace(tp, false);
				}
				if (tp._collides)
				{
					Vector3 intersection = tp.GetIntersection();
					bool bounce = false;
					bool destroyBlock = false;
					IntVector3 blockToDestroy = IntVector3.Zero;
					if (shootableEnemy != null)
					{
						shootableEnemy.TakeDamage(intersection, Vector3.Normalize(_velocity), _weaponClassUsed, _shooter);
						if (!(shootableEnemy is BaseZombie))
						{
						}
					}
					else
					{
						BlockType type = BlockType.GetType(BlockTerrain.Instance.GetBlockWithChanges(tp._worldIndex));
						bounce = type.BouncesLasers;
						destroyBlock = type.CanBeDug;
						blockToDestroy = tp._worldIndex;
					}
					if (shootableEnemy is DragonClientEntity)
					{
						ParticleEmitter particleEmitter = TracerManager._dragonFlashEffect.CreateEmitter(CastleMinerZGame.Instance);
						particleEmitter.Reset();
						particleEmitter.Emitting = true;
						TracerManager.Instance.Scene.Children.Add(particleEmitter);
						particleEmitter.LocalPosition = intersection;
						particleEmitter.DrawPriority = 900;
					}
					new Plane(tp._inNormal, Vector3.Dot(tp._inNormal, intersection));
					HandleCollision(tp._inNormal, intersection, bounce, destroyBlock, blockToDestroy);
				}
			}
			if (flag)
			{
				RemoveFromParent();
			}
		}

		private BlasterShot(byte shooter)
		{
			_tracer = new ModelEntity(_tracerModel);
			_tracer.DrawPriority = 900;
			_tracer.BlendState = BlendState.Additive;
			_tracer.DepthStencilState = DepthStencilState.DepthRead;
			_tracer.RasterizerState = RasterizerState.CullNone;
			_tracer.LocalScale = new Vector3(1.5f, 1.5f, 8f);
			_tracer.LocalPosition = new Vector3(0f, 0f, -12f);
			base.Children.Add(_tracer);
			_shooter = shooter;
			Collider = false;
			Collidee = false;
		}

		private void HandleCollision(Vector3 collisionNormal, Vector3 collisionLocation, bool bounce, bool destroyBlock, IntVector3 blockToDestroy)
		{
			Scene scene = null;
			if (TracerManager.Instance != null)
			{
				scene = TracerManager.Instance.Scene;
			}
			Matrix localToParent = MathTools.CreateWorld(collisionLocation, -collisionNormal);
			if (scene != null)
			{
				ParticleEmitter particleEmitter = _spashEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter.LocalScale = new Vector3(0.01f);
				particleEmitter.Reset();
				particleEmitter.Emitting = true;
				particleEmitter.LocalToParent = localToParent;
				scene.Children.Add(particleEmitter);
				particleEmitter.DrawPriority = 900;
				ParticleEmitter particleEmitter2 = _sparkEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter2.Reset();
				particleEmitter2.Emitting = true;
				particleEmitter2.LocalToParent = localToParent;
				scene.Children.Add(particleEmitter2);
				particleEmitter2.DrawPriority = 900;
				ParticleEmitter particleEmitter3 = _smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter3.Reset();
				particleEmitter3.Emitting = true;
				particleEmitter3.LocalToParent = localToParent;
				scene.Children.Add(particleEmitter3);
				particleEmitter3.DrawPriority = 900;
				Emitter.Velocity = new Vector3(0f, 0f, 0f);
				Emitter.Position = localToParent.Translation;
				Emitter.Up = new Vector3(0f, 1f, 0f);
				Emitter.Forward = new Vector3(0f, 0f, 1f);
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = true;
			if (CollisionsRemaining > 0)
			{
				if (bounce)
				{
					CollisionsRemaining--;
					flag = true;
					ReflectedShot = true;
				}
				else if (flag3)
				{
					CollisionsRemaining--;
					flag2 = true;
					ReflectedShot = false;
				}
			}
			if (!flag2)
			{
				if (flag)
				{
					Vector3 vector = base.WorldPosition - collisionLocation;
					Vector3 position = Vector3.Reflect(vector, collisionNormal) + collisionLocation;
					_lastPosition = collisionLocation;
					_velocity = Vector3.Reflect(_velocity, collisionNormal);
					base.LocalToParent = MathTools.CreateWorld(position, _velocity);
					_noCollideFrame = true;
				}
				else
				{
					_velocity = new Vector3(0f, 0f, 0f);
					RemoveFromParent();
				}
			}
			if (CastleMinerZGame.Instance.IsLocalPlayerId(_shooter) && destroyBlock)
			{
				Explosive.FindBlocksToRemove(blockToDestroy, _explosiveType, true);
			}
		}
	}
}
