using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class BoltRifleInventoryItemClass : GunInventoryItemClass
	{
		public BoltRifleInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\Rifle\\Model"), name, description, TimeSpan.FromSeconds(0.0031746031746031746), material, damage, durabilitydamage, ammotype, "GunShot1", "AssaultReload")
		{
			_playerMode = PlayerMode.BoltRifle;
			ReloadTime = TimeSpan.FromSeconds(2.4);
			Automatic = false;
			RoundsPerReload = (ClipCapacity = 8);
			ShoulderMagnification = 1.3f;
			ShoulderedMinAccuracy = Angle.FromDegrees(3.375);
			ShoulderedMaxAccuracy = Angle.FromDegrees(5.375);
			MinInnaccuracy = Angle.FromDegrees(3.375);
			MaxInnaccuracy = Angle.FromDegrees(5.375);
			Recoil = Angle.FromDegrees(10f);
			Velocity = 150f;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			ModelEntity modelEntity = (ModelEntity)base.CreateEntity(use, attachedToLocalPlayer);
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, (float)Math.PI * -11f / 100f) * Quaternion.CreateFromYawPitchRoll(0f, -(float)Math.PI / 2f, 0f);
				Matrix localToParent = Matrix.Transform(Matrix.CreateScale(35.2f / modelEntity.GetLocalBoundingSphere().Radius), rotation);
				localToParent.Translation = new Vector3(12f, -16f, -16f);
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
