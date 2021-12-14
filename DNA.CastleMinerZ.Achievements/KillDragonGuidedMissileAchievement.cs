using DNA.CastleMinerZ.Globalization;

namespace DNA.CastleMinerZ.Achievements
{
	public class KillDragonGuidedMissileAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		private string lastString;

		private int _lastAmount = -1;

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.DragonsKilledWithGuidedMissile > 0;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				if (base.PlayerStats.DragonsKilledWithGuidedMissile > 0)
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
				int dragonsKilledWithGuidedMissile = base.PlayerStats.DragonsKilledWithGuidedMissile;
				if (_lastAmount != dragonsKilledWithGuidedMissile)
				{
					_lastAmount = dragonsKilledWithGuidedMissile;
					lastString = dragonsKilledWithGuidedMissile + " " + ((dragonsKilledWithGuidedMissile == 1) ? Strings.Dragon_Killed : Strings.Dragons_Killed);
				}
				return lastString;
			}
		}

		public KillDragonGuidedMissileAchievement(string apiName, CastleMinerZAchievementManager manager, string name)
			: base(apiName, (AchievementManager<CastleMinerZPlayerStats>)manager, name, Strings.Kill_A_Dragon_With_A_Guided_Missile)
		{
		}
	}
}
