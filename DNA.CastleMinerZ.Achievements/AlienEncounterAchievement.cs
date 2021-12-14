using DNA.CastleMinerZ.Globalization;

namespace DNA.CastleMinerZ.Achievements
{
	public class AlienEncounterAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		private string lastString;

		private int _lastAmount = -1;

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.AlienEncounters > 0;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				if (base.PlayerStats.AlienEncounters > 0)
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
				int alienEncounters = base.PlayerStats.AlienEncounters;
				if (_lastAmount != alienEncounters)
				{
					_lastAmount = alienEncounters;
					lastString = alienEncounters + " " + ((alienEncounters == 1) ? Strings.Alien_Encounter : Strings.Alien_Encounters);
				}
				return lastString;
			}
		}

		public AlienEncounterAchievement(string apiName, CastleMinerZAchievementManager manager, string name)
			: base(apiName, (AchievementManager<CastleMinerZPlayerStats>)manager, name, Strings.Find_An_Alien)
		{
		}
	}
}
