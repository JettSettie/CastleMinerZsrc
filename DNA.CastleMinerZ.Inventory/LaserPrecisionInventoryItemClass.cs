using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserPrecisionInventoryItemClass : LaserGunInventoryItemClass
	{
		public LaserPrecisionInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Tools\\Drill\\Model"), name, description, TimeSpan.FromSeconds(0.05000000074505806), material, bulletdamage, durabilitydamage, ammotype, "LaserGun3", "AssaultReload")
		{
			_playerMode = PlayerMode.SpaceAssault;
			ReloadTime = TimeSpan.FromSeconds(3.0);
			Automatic = false;
			RoundsPerReload = (ClipCapacity = 10);
			ShoulderMagnification = 3.45f;
			ShoulderedMinAccuracy = Angle.FromDegrees(0.1f);
			ShoulderedMaxAccuracy = Angle.FromDegrees(0.2f);
			MinInnaccuracy = Angle.FromDegrees(0.1);
			MaxInnaccuracy = Angle.FromDegrees(0.2);
			Recoil = Angle.FromDegrees(0.1);
		}

		internal override bool IsHarvestWeapon()
		{
			return true;
		}
	}
}
