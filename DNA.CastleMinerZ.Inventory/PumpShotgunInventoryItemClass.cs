using DNA.CastleMinerZ.AI;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class PumpShotgunInventoryItemClass : GunInventoryItemClass
	{
		public PumpShotgunInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\Shotgun\\Model"), name, description, TimeSpan.FromMinutes(0.016666666666666666), material, damage, durabilitydamage, ammotype, "Shotgun", "ShotGunReload")
		{
			_playerMode = PlayerMode.PumpShotgun;
			ReloadTime = TimeSpan.FromSeconds(0.567);
			Automatic = false;
			ClipCapacity = 6;
			RoundsPerReload = 1;
			FlightTime = 0.4f;
			ShoulderMagnification = 1.3f;
			ShoulderedMinAccuracy = Angle.FromDegrees(3.375);
			ShoulderedMaxAccuracy = Angle.FromDegrees(5.375);
			MinInnaccuracy = Angle.FromDegrees(3.375);
			MaxInnaccuracy = Angle.FromDegrees(5.375);
			Recoil = Angle.FromDegrees(10f);
			Velocity = 50f;
			EnemyDamageType = DamageType.SHOTGUN;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			ModelEntity modelEntity = (ModelEntity)base.CreateEntity(use, attachedToLocalPlayer);
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, -213f / 226f) * Quaternion.CreateFromYawPitchRoll(0f, -(float)Math.PI / 2f, 0f);
				Matrix localToParent = Matrix.Transform(Matrix.CreateScale(28.8f / modelEntity.GetLocalBoundingSphere().Radius), rotation);
				localToParent.Translation = new Vector3(8f, -14f, -16f);
				modelEntity.LocalToParent = localToParent;
				break;
			}
			case ItemUse.Pickup:
			{
				Matrix matrix2 = modelEntity.LocalToParent = Matrix.CreateFromYawPitchRoll(0f, -(float)Math.PI / 2f, 0f);
				break;
			}
			}
			return modelEntity;
		}
	}
}
