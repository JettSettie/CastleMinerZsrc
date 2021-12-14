using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class PistolInventoryItemClass : GunInventoryItemClass
	{
		public PistolInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\Pistol\\Model"), name, description, TimeSpan.FromSeconds(0.10000000149011612), material, damage, durabilitydamage, ammotype, "GunShot4", "Reload")
		{
			_playerMode = PlayerMode.Pistol;
			ReloadTime = TimeSpan.FromSeconds(1.6299999952316284);
			Automatic = false;
			RoundsPerReload = (ClipCapacity = 8);
			ShoulderMagnification = 1f;
			ShoulderedMinAccuracy = Angle.FromDegrees(0.625);
			ShoulderedMaxAccuracy = Angle.FromDegrees(1.25);
			MinInnaccuracy = Angle.FromDegrees(1.25);
			MaxInnaccuracy = Angle.FromDegrees(2.5);
			Recoil = Angle.FromDegrees(3f);
			FlightTime = 1f;
			Velocity = 75f;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			ModelEntity modelEntity = (ModelEntity)base.CreateEntity(use, attachedToLocalPlayer);
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, -(float)Math.PI / 4f) * Quaternion.CreateFromYawPitchRoll(0f, -(float)Math.PI / 2f, 0f);
				Matrix localToParent = Matrix.Transform(Matrix.CreateScale(25.6f / modelEntity.GetLocalBoundingSphere().Radius), rotation);
				localToParent.Translation = new Vector3(4f, -12f, -16f);
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
