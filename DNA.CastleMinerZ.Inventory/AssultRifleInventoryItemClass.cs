using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class AssultRifleInventoryItemClass : GunInventoryItemClass
	{
		public AssultRifleInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\AR\\Model"), name, description, TimeSpan.FromSeconds(0.10000000149011612), material, bulletdamage, durabilitydamage, ammotype, "GunShot3", "AssaultReload")
		{
			_playerMode = PlayerMode.Assault;
			ReloadTime = TimeSpan.FromSeconds(3.0);
			Automatic = true;
			RoundsPerReload = (ClipCapacity = 30);
			ShoulderMagnification = 1.35f;
			ShoulderedMinAccuracy = Angle.FromDegrees(0.625f);
			ShoulderedMaxAccuracy = Angle.FromDegrees(1.4);
			MinInnaccuracy = Angle.FromDegrees(2f);
			MaxInnaccuracy = Angle.FromDegrees(4.625);
			Recoil = Angle.FromDegrees(3f);
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			ModelEntity modelEntity = (ModelEntity)base.CreateEntity(use, attachedToLocalPlayer);
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, (float)Math.PI * -11f / 40f) * Quaternion.CreateFromYawPitchRoll(0f, -(float)Math.PI / 2f, 0f);
				Matrix localToParent = Matrix.Transform(Matrix.CreateScale(32f / modelEntity.GetLocalBoundingSphere().Radius), rotation);
				localToParent.Translation = new Vector3(6f, -12f, -16f);
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
