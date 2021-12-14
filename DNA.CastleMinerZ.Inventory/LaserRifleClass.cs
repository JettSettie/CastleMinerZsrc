using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserRifleClass : LaserGunInventoryItemClass
	{
		public LaserRifleClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Space\\Rifle\\Model"), name, description, TimeSpan.FromSeconds(0.25999999046325684), material, bulletdamage, durabilitydamage, ammotype, "LaserGun1", "AssaultReload")
		{
			_playerMode = PlayerMode.SpaceBoltRifle;
			ReloadTime = TimeSpan.FromSeconds(2.9500000476837158);
			Automatic = false;
			RoundsPerReload = (ClipCapacity = 10);
			ShoulderMagnification = 4.3f;
			ShoulderedMinAccuracy = Angle.FromDegrees(0.25f);
			ShoulderedMaxAccuracy = Angle.FromDegrees(0.36);
			MinInnaccuracy = Angle.FromDegrees(6.875);
			MaxInnaccuracy = Angle.FromDegrees(10f);
			Recoil = Angle.FromDegrees(12f);
			Scoped = true;
		}
	}
}
