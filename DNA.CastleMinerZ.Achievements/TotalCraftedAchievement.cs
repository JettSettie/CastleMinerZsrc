using DNA.CastleMinerZ.Globalization;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Achievements
{
	public class TotalCraftedAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		private int _items;

		private string lastString;

		private int _lastAmount = -1;

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.TotalItemsCrafted >= _items;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				return MathHelper.Clamp((float)base.PlayerStats.TotalItemsCrafted / (float)_items, 0f, 1f);
			}
		}

		public override string ProgressTowardsUnlockMessage
		{
			get
			{
				int totalItemsCrafted = base.PlayerStats.TotalItemsCrafted;
				if (_lastAmount != totalItemsCrafted)
				{
					_lastAmount = totalItemsCrafted;
					lastString = "(" + totalItemsCrafted + "/" + _items + ") " + Strings.Items_Crafted;
				}
				return lastString;
			}
		}

		public TotalCraftedAchievement(string apiName, CastleMinerZAchievementManager manager, int items, string name)
			: base(apiName, (AchievementManager<CastleMinerZPlayerStats>)manager, name, Strings.Craft + " " + items + " " + Strings.items)
		{
			_items = items;
		}
	}
}
