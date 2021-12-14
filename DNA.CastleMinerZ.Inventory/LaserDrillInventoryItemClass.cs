using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserDrillInventoryItemClass : LaserGunInventoryItemClass
	{
		public LaserDrillInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Tools\\Drill\\Model"), name, description, TimeSpan.FromSeconds(0.05000000074505806), material, bulletdamage, durabilitydamage, ammotype, "LaserGun3", "AssaultReload")
		{
			_playerMode = PlayerMode.SpaceAssault;
			ReloadTime = TimeSpan.FromSeconds(3.0);
			Automatic = true;
			RoundsPerReload = (ClipCapacity = 200);
			ShoulderMagnification = 1.15f;
			ShoulderedMinAccuracy = Angle.FromDegrees(0.625f);
			ShoulderedMaxAccuracy = Angle.FromDegrees(1.4);
			MinInnaccuracy = Angle.FromDegrees(2f);
			MaxInnaccuracy = Angle.FromDegrees(4f);
			Recoil = Angle.FromDegrees(2f);
		}

		internal override bool IsHarvestWeapon()
		{
			return true;
		}
	}
}
