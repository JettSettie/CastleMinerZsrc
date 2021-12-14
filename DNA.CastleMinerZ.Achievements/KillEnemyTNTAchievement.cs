using DNA.CastleMinerZ.Globalization;

namespace DNA.CastleMinerZ.Achievements
{
	public class KillEnemyTNTAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		private string lastString;

		private int _lastAmount = -1;

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.EnemiesKilledWithTNT > 0;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				if (base.PlayerStats.EnemiesKilledWithTNT > 0)
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
				int enemiesKilledWithTNT = base.PlayerStats.EnemiesKilledWithTNT;
				if (_lastAmount != enemiesKilledWithTNT)
				{
					_lastAmount = enemiesKilledWithTNT;
					lastString = enemiesKilledWithTNT + " " + ((enemiesKilledWithTNT == 1) ? Strings.Enemy_Killed : Strings.Enemies_Killed);
				}
				return lastString;
			}
		}

		public KillEnemyTNTAchievement(string apiName, CastleMinerZAchievementManager manager, string name)
			: base(apiName, (AchievementManager<CastleMinerZPlayerStats>)manager, name, Strings.Kill_An_Enemy_With_TNT)
		{
		}
	}
}
