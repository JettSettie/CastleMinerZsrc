using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserSMGClass : LaserGunInventoryItemClass
	{
		public LaserSMGClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Space\\SMG\\Model"), name, description, TimeSpan.FromSeconds(0.059999998658895493), material, bulletdamage, durabilitydamage, ammotype, "LaserGun2", "Reload")
		{
			_playerMode = PlayerMode.SpaceSMG;
			ReloadTime = TimeSpan.FromSeconds(2.05);
			Automatic = true;
			RoundsPerReload = (ClipCapacity = 20);
			ShoulderMagnification = 1.3f;
			ShoulderedMinAccuracy = Angle.FromDegrees(0.375);
			ShoulderedMaxAccuracy = Angle.FromDegrees(0.91);
			MinInnaccuracy = Angle.FromDegrees(0.875);
			MaxInnaccuracy = Angle.FromDegrees(2.125);
			Recoil = Angle.FromDegrees(3f);
		}
	}
}
