using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserPistolClass : LaserGunInventoryItemClass
	{
		public LaserPistolClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Space\\Pistol\\Model"), name, description, TimeSpan.FromSeconds(0.059999998658895493), material, bulletdamage, durabilitydamage, ammotype, "LaserGun4", "Reload")
		{
			_playerMode = PlayerMode.SpacePistol;
			ReloadTime = TimeSpan.FromSeconds(1.25);
			Automatic = false;
			RoundsPerReload = (ClipCapacity = 7);
			ShoulderMagnification = 1f;
			ShoulderedMinAccuracy = Angle.FromDegrees(0.625);
			ShoulderedMaxAccuracy = Angle.FromDegrees(1.25);
			MinInnaccuracy = Angle.FromDegrees(1.25);
			MaxInnaccuracy = Angle.FromDegrees(2.5);
			Recoil = Angle.FromDegrees(3f);
		}
	}
}
