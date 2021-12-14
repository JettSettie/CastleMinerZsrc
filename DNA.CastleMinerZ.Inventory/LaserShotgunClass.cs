using DNA.CastleMinerZ.AI;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserShotgunClass : LaserGunInventoryItemClass
	{
		public LaserShotgunClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Space\\Shotgun\\Model"), name, description, TimeSpan.FromSeconds(0.18999999761581421), material, bulletdamage, durabilitydamage, ammotype, "LaserGun5", "ShotGunReload")
		{
			_playerMode = PlayerMode.SpacePumpShotgun;
			ReloadTime = TimeSpan.FromSeconds(0.56999999284744263);
			Automatic = false;
			RoundsPerReload = 1;
			ClipCapacity = 8;
			EnemyDamageType = DamageType.SHOTGUN;
			ShoulderMagnification = 1.3f;
			ShoulderedMinAccuracy = Angle.FromDegrees(3.375);
			ShoulderedMaxAccuracy = Angle.FromDegrees(5.375);
			MinInnaccuracy = Angle.FromDegrees(3.375);
			MaxInnaccuracy = Angle.FromDegrees(5.375);
			Recoil = Angle.FromDegrees(10f);
		}
	}
}
