using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Net;
using DNA.Drawing;
using DNA.Drawing.Effects;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class GrenadeLauncherBaseInventoryItemClass : GunInventoryItemClass
	{
		public class GrenadeLauncherGrenadeModel : ModelEntity
		{
			private bool _recolor;

			public GrenadeLauncherGrenadeModel(Model model, bool recolor)
				: base(model)
			{
				_recolor = recolor;
			}

			protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
			{
				DNAEffect dNAEffect = (DNAEffect)effect;
				if (_recolor)
				{
					dNAEffect.DiffuseColor = ColorF.FromRGB(1.5f, 0f, 0f);
				}
				else
				{
					dNAEffect.DiffuseColor = Color.Gray;
				}
				return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
			}
		}

		public class GrenadeLauncherModel : ModelEntity
		{
			public bool Darken;

			public GrenadeLauncherModel(Model model)
				: base(model)
			{
			}

			protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
			{
				DNAEffect dNAEffect = (DNAEffect)effect;
				if (Darken)
				{
					dNAEffect.DiffuseColor = Color.Gray;
				}
				else
				{
					dNAEffect.DiffuseColor = Color.White;
				}
				return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
			}
		}

		private static Model WeaponModel;

		private static readonly Vector3 cBarrelTipOffset;

		private static readonly Vector3 cBarrelTipScale;

		static GrenadeLauncherBaseInventoryItemClass()
		{
			cBarrelTipOffset = new Vector3(0.01f, -0.005f, 0.01f);
			cBarrelTipScale = new Vector3(0.65f);
			WeaponModel = CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\RPG\\RPGGrenade");
		}

		public GrenadeLauncherBaseInventoryItemClass(InventoryItemIDs id, Model model, string name, string description, TimeSpan reloadTime, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype, string shootSound, string reloadSound)
			: base(id, model, name, description, reloadTime, ToolMaterialTypes.BloodStone, damage, durabilitydamage, ammotype, shootSound, reloadSound)
		{
			_playerMode = PlayerMode.RPG;
			ReloadTime = TimeSpan.FromSeconds(0.567);
			Automatic = false;
			ClipCapacity = 1;
			RoundsPerReload = 1;
			FlightTime = 0.4f;
			ShoulderedMinAccuracy = Angle.FromDegrees(0.1f);
			ShoulderedMaxAccuracy = Angle.FromDegrees(0.25);
			MinInnaccuracy = Angle.FromDegrees(6.875);
			MaxInnaccuracy = Angle.FromDegrees(10f);
			Recoil = Angle.FromDegrees(12f);
			Velocity = 50f;
			EnemyDamageType = DamageType.SHOTGUN;
			Scoped = false;
		}

		public virtual bool IsGuided()
		{
			return false;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			GunEntity gunEntity = (GunEntity)base.CreateEntity(use, attachedToLocalPlayer);
			if (ID == InventoryItemIDs.RocketLauncherGuidedShotFired || ID == InventoryItemIDs.RocketLauncherGuided)
			{
				gunEntity.DiffuseColor = Color.Black;
			}
			else
			{
				gunEntity.DiffuseColor = Color.Gray;
			}
			switch (use)
			{
			case ItemUse.UI:
				gunEntity.LocalPosition += new Vector3(-2f, -4f, 0f);
				break;
			case ItemUse.Hand:
				if (ID == InventoryItemIDs.RocketLauncherGuided || ID == InventoryItemIDs.RocketLauncher)
				{
					ModelBone modelBone = _model.Bones["BarrelTip"];
					Vector3 position;
					Vector3 fwd;
					if (modelBone != null)
					{
						position = Vector3.Transform(cBarrelTipOffset, modelBone.Transform);
						fwd = Vector3.Normalize(modelBone.Transform.Left);
					}
					else
					{
						position = new Vector3(0f, 0f, -0.5f);
						fwd = Vector3.Forward;
					}
					GrenadeLauncherGrenadeModel grenadeLauncherGrenadeModel = (ID != InventoryItemIDs.RocketLauncherGuided) ? new GrenadeLauncherGrenadeModel(WeaponModel, false) : new GrenadeLauncherGrenadeModel(WeaponModel, true);
					gunEntity.Children.Add(grenadeLauncherGrenadeModel);
					grenadeLauncherGrenadeModel.LocalToParent = MathTools.CreateWorld(position, fwd);
					grenadeLauncherGrenadeModel.LocalScale = cBarrelTipScale;
					Matrix localToWorld = CastleMinerZGame.Instance.LocalPlayer.FPSCamera.LocalToWorld;
					GrenadeMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, localToWorld, GrenadeTypeEnum.Sticky, 3f);
				}
				break;
			}
			return gunEntity;
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new GrenadeLauncherBaseItem(this, stackCount);
		}
	}
}
