using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Drawing;
using DNA.Profiling;
using Microsoft.Xna.Framework;
using System;

namespace DNA.CastleMinerZ
{
	public class PickupEntity : Entity
	{
		private const float PickupRadSqu = 4f;

		private Entity _displayEntity;

		public InventoryItem Item;

		public int PickupID;

		public int SpawnerID;

		private float _timeLeft;

		private float _bouncePhase;

		private bool _pickedUp;

		private bool _readyForPickup;

		public AABBTraceProbe MovementProbe = new AABBTraceProbe();

		public BoundingBox CollisionAABB = new BoundingBox(new Vector3(-0.3f, -0.3f, -0.3f), new Vector3(0.3f, 0.3f, 0.3f));

		public bool OnGround;

		public BasicPhysics PlayerPhysics
		{
			get
			{
				return (BasicPhysics)base.Physics;
			}
		}

		public PickupEntity(InventoryItem item, int pid, int sid, bool dropped, Vector3 pickupLoc)
		{
			Item = item;
			PickupID = pid;
			SpawnerID = sid;
			item.SetLocation(IntVector3.FromVector3(pickupLoc));
			int modelIndex = GetModelIndex(item, IntVector3.FromVector3(pickupLoc));
			if (modelIndex > 0)
			{
				item.SetModelNameIndex(modelIndex);
			}
			_displayEntity = item.CreateEntity(ItemUse.Pickup, false);
			base.Children.Add(_displayEntity);
			_pickedUp = false;
			if (CastleMinerZGame.Instance.LocalPlayer.ValidLivingGamer)
			{
				_readyForPickup = (!dropped || sid != CastleMinerZGame.Instance.LocalPlayer.Gamer.Id);
			}
			else
			{
				_readyForPickup = true;
			}
			_bouncePhase = MathTools.RandomFloat(0f, (float)Math.PI);
			_timeLeft = item.ItemClass.PickupTimeoutLength;
			Collider = true;
			base.Physics = new Player.NoMovePhysics(this);
			PlayerPhysics.WorldAcceleration = BasicPhysics.Gravity * 0.25f;
		}

		private int GetModelIndex(InventoryItem item, IntVector3 location)
		{
			int val = 0;
			if (item is DoorInventoryitem)
			{
				Door door = CastleMinerZGame.Instance.CurrentWorld.GetDoor(location);
				if (door != null)
				{
					val = (int)door.ModelName;
				}
			}
			return Math.Max(0, val);
		}

		public Vector3 GetActualGraphicPos()
		{
			return base.LocalPosition + _displayEntity.LocalPosition;
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			_displayEntity.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.Down, (float)gameTime.ElapsedGameTime.TotalSeconds) * _displayEntity.LocalRotation;
			Vector3 vector = _displayEntity.LocalPosition;
			_bouncePhase += (float)gameTime.ElapsedGameTime.TotalSeconds * 1.5f;
			vector.Y = (float)Math.Sin(_bouncePhase) * 0.1f + 0.1f;
			_displayEntity.LocalPosition = vector;
			vector = PlayerPhysics.LocalVelocity;
			if (OnGround)
			{
				vector.X *= 0.9f;
				vector.Z *= 0.9f;
			}
			else
			{
				vector.X *= 0.99f;
				vector.Z *= 0.99f;
			}
			if (Math.Abs(vector.X) < 0.1f)
			{
				vector.X = 0f;
			}
			if (Math.Abs(vector.Z) < 0.1f)
			{
				vector.Z = 0f;
			}
			PlayerPhysics.LocalVelocity = vector;
			_timeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (_timeLeft <= 0f)
			{
				PickupManager.Instance.RemovePickup(this);
				return;
			}
			if (!BlockTerrain.Instance.RegionIsLoaded(base.LocalPosition))
			{
				PlayerPhysics.LocalVelocity = Vector3.Zero;
				PlayerPhysics.WorldVelocity = Vector3.Zero;
				Visible = false;
			}
			else if (_timeLeft < 5f)
			{
				Visible = (((int)Math.Floor(_timeLeft * 8f) & 1) == 0);
			}
			else
			{
				Visible = true;
			}
			if (!CastleMinerZGame.Instance.LocalPlayer.ValidLivingGamer)
			{
				_readyForPickup = true;
			}
			else if (!_pickedUp)
			{
				Player localPlayer = CastleMinerZGame.Instance.LocalPlayer;
				Vector3 localPosition = localPlayer.LocalPosition;
				localPosition.Y += 1f;
				if (!_readyForPickup && Vector3.DistanceSquared(localPosition, base.LocalPosition) > 4f)
				{
					_readyForPickup = true;
				}
				if (_readyForPickup && Vector3.DistanceSquared(localPosition, base.LocalPosition) < 4f)
				{
					_pickedUp = true;
					PickupManager.Instance.PlayerTouchedPickup(this);
				}
			}
			base.OnUpdate(gameTime);
		}

		public override bool ResolveCollsion(Entity e, out Plane collsionPlane, GameTime dt)
		{
			using (Profiler.TimeSection("Pickup Collision", ProfilerThreadEnum.MAIN))
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
					MovementProbe.SkipEmbedded = true;
					int num2 = 0;
					do
					{
						Vector3 vector3 = vector;
						Vector3 vector4 = Vector3.Multiply(vector2, num);
						vector += vector4;
						MovementProbe.Init(vector3, vector, CollisionAABB);
						BlockTerrain.Instance.Trace(MovementProbe);
						if (MovementProbe._collides)
						{
							result = true;
							if (MovementProbe._inFace == BlockFace.POSY)
							{
								OnGround = true;
							}
							if (MovementProbe._startsIn)
							{
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
					vector.Y += 1.2f;
				}
				return result;
			}
		}
	}
}
