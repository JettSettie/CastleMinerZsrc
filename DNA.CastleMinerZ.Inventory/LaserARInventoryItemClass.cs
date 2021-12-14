using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserARInventoryItemClass : LaserGunInventoryItemClass
	{
		public LaserARInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Space\\AR\\Model"), name, description, TimeSpan.FromSeconds(0.059999998658895493), material, bulletdamage, durabilitydamage, ammotype, "LaserGun3", "AssaultReload")
		{
			_playerMode = PlayerMode.SpaceAssault;
			ReloadTime = TimeSpan.FromSeconds(2.5);
			Automatic = true;
			RoundsPerReload = (ClipCapacity = 30);
			ShoulderMagnification = 1.35f;
			ShoulderedMinAccuracy = Angle.FromDegrees(0.625f);
			ShoulderedMaxAccuracy = Angle.FromDegrees(1.4);
			MinInnaccuracy = Angle.FromDegrees(2f);
			MaxInnaccuracy = Angle.FromDegrees(4.625);
			Recoil = Angle.FromDegrees(3f);
		}
	}
}
