using DNA.CastleMinerZ.Globalization;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Achievements
{
	public class DepthTraveledAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		private float _depth;

		private string lastString;

		private int _lastAmount = -1;

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.MaxDepth <= _depth;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				return MathHelper.Clamp(base.PlayerStats.MaxDepth / _depth, 0f, 1f);
			}
		}

		public override string ProgressTowardsUnlockMessage
		{
			get
			{
				int num = (int)(0f - base.PlayerStats.MaxDepth);
				if (_lastAmount != num)
				{
					_lastAmount = num;
					lastString = "(" + num + "/" + (int)(0f - _depth) + ") " + Strings.Distance_Traveled;
				}
				return lastString;
			}
		}

		public DepthTraveledAchievement(string apiName, CastleMinerZAchievementManager manager, float depth, string name)
			: base(apiName, (AchievementManager<CastleMinerZPlayerStats>)manager, name, Strings.Travel_Down_At_Least + " " + (int)(0f - depth))
		{
			_depth = depth;
		}
	}
}
