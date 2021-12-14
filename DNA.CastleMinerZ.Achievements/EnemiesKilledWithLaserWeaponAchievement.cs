using DNA.CastleMinerZ.Globalization;

namespace DNA.CastleMinerZ.Achievements
{
	public class EnemiesKilledWithLaserWeaponAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		private string lastString;

		private int _lastAmount = -1;

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.EnemiesKilledWithLaserWeapon > 0;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				if (base.PlayerStats.EnemiesKilledWithLaserWeapon > 0)
				{
					return 1f;
				}
				return 0f;
			}
		}

		public override string ProgressTowardsUnlockMessage
		{
			get
			{
				int enemiesKilledWithLaserWeapon = base.PlayerStats.EnemiesKilledWithLaserWeapon;
				if (_lastAmount != enemiesKilledWithLaserWeapon)
				{
					_lastAmount = enemiesKilledWithLaserWeapon;
					lastString = enemiesKilledWithLaserWeapon + " " + ((enemiesKilledWithLaserWeapon == 1) ? Strings.Enemy_Killed : Strings.Enemies_Killed);
				}
				return lastString;
			}
		}

		public EnemiesKilledWithLaserWeaponAchievement(string apiName, CastleMinerZAchievementManager manager, string name)
			: base(apiName, (AchievementManager<CastleMinerZPlayerStats>)manager, name, Strings.Kill_An_Enemy_With_A_Laser_Weapon)
		{
		}
	}
}
