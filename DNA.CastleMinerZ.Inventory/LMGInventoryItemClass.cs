using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class LMGInventoryItemClass : GunInventoryItemClass
	{
		public LMGInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\LMG\\Model"), name, description, TimeSpan.FromSeconds(0.1120000034570694), material, damage, durabilitydamage, ammotype, "GunShot2", "Reload")
		{
			_playerMode = PlayerMode.LMG;
			ReloadTime = TimeSpan.FromSeconds(9.6999998092651367);
			Automatic = true;
			RoundsPerReload = (ClipCapacity = 100);
			FlightTime = 1f;
			ShoulderMagnification = 1.3f;
			ShoulderedMinAccuracy = Angle.FromDegrees(1.25);
			ShoulderedMaxAccuracy = Angle.FromDegrees(3.4);
			MinInnaccuracy = Angle.FromDegrees(2.5);
			MaxInnaccuracy = Angle.FromDegrees(6.875);
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
				localToParent.Translation = new Vector3(6f, -13f, -16f);
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
