using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class SMGInventoryItemClass : GunInventoryItemClass
	{
		public SMGInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\SMG\\Model"), name, description, TimeSpan.FromMinutes(0.0010672358591248667), material, damage, durabilitydamage, ammotype, "GunShot2", "Reload")
		{
			_playerMode = PlayerMode.SMG;
			ReloadTime = TimeSpan.FromSeconds(2.0);
			Automatic = true;
			RoundsPerReload = (ClipCapacity = 20);
			FlightTime = 1f;
			ShoulderMagnification = 1.3f;
			ShoulderedMinAccuracy = Angle.FromDegrees(0.375);
			ShoulderedMaxAccuracy = Angle.FromDegrees(0.91);
			MinInnaccuracy = Angle.FromDegrees(0.875);
			MaxInnaccuracy = Angle.FromDegrees(2.125);
			Recoil = Angle.FromDegrees(3f);
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			ModelEntity modelEntity = (ModelEntity)base.CreateEntity(use, attachedToLocalPlayer);
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, -(float)Math.PI / 4f) * Quaternion.CreateFromYawPitchRoll(0f, -(float)Math.PI / 2f, 0f);
				Matrix localToParent = Matrix.Transform(Matrix.CreateScale(32f / modelEntity.GetLocalBoundingSphere().Radius), rotation);
				localToParent.Translation = new Vector3(12f, -19f, -16f);
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
