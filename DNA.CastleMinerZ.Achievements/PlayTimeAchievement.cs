using DNA.CastleMinerZ.Globalization;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Achievements
{
	public class PlayTimeAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		private int _hours;

		private string lastString;

		private int _lastAmount = -1;

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.TimeOnline.TotalHours >= (double)_hours;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				return MathHelper.Clamp((float)base.PlayerStats.TimeOnline.TotalHours / (float)_hours, 0f, 1f);
			}
		}

		public override string ProgressTowardsUnlockMessage
		{
			get
			{
				int num = (int)base.PlayerStats.TimeOnline.TotalHours;
				if (_lastAmount != num)
				{
					_lastAmount = num;
					lastString = "(" + num + "/" + _hours + ") " + Strings.hours_played;
				}
				return lastString;
			}
		}

		public PlayTimeAchievement(string apiName, CastleMinerZAchievementManager manager, int hours, string name)
			: base(apiName, (AchievementManager<CastleMinerZPlayerStats>)manager, name, Strings.Play_Online_For + " " + hours + " " + Strings.Hours)
		{
			_hours = hours;
		}
	}
}
