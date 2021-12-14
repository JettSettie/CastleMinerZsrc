using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserGunInventoryItemClass : GunInventoryItemClass
	{
		public override bool NeedsDropCompensation
		{
			get
			{
				return false;
			}
		}

		public LaserGunInventoryItemClass(InventoryItemIDs id, Model model, string name, string description, TimeSpan fireRate, ToolMaterialTypes material, float bulletDamage, float itemSelfDamage, InventoryItem.InventoryItemClass ammoClass, string shootSound, string reloadSound)
			: base(id, model, name, description, fireRate, material, bulletDamage, itemSelfDamage, ammoClass, shootSound, reloadSound)
		{
			TracerColor = CMZColors.GetLaserMaterialcColor(Material).ToVector4();
			EmissiveColor = CMZColors.GetLaserMaterialcColor(Material);
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
				switch (ID)
				{
				case InventoryItemIDs.IronSpacePistol:
				case InventoryItemIDs.CopperSpacePistol:
				case InventoryItemIDs.GoldSpacePistol:
				case InventoryItemIDs.DiamondSpacePistol:
					localToParent.Translation = new Vector3(13f, -21f, -16f);
					break;
				case InventoryItemIDs.IronSpaceSMGGun:
				case InventoryItemIDs.CopperSpaceSMGGun:
				case InventoryItemIDs.GoldSpaceSMGGun:
				case InventoryItemIDs.DiamondSpaceSMGGun:
					localToParent.Translation = new Vector3(13f, -21f, -16f);
					break;
				default:
					localToParent.Translation = new Vector3(9f, -17f, -16f);
					break;
				}
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

		internal virtual bool IsHarvestWeapon()
		{
			return false;
		}
	}
}
