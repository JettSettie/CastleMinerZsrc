using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Inventory;

namespace DNA.CastleMinerZ.Achievements
{
	public class CraftTNTAcheivement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		private string lastString;

		private int _lastAmount = -1;

		protected override bool IsSastified
		{
			get
			{
				CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(InventoryItemIDs.TNT);
				return itemStats.Crafted > 0;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(InventoryItemIDs.TNT);
				if (itemStats.Crafted > 0)
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
				CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(InventoryItemIDs.TNT);
				int crafted = itemStats.Crafted;
				if (_lastAmount != crafted)
				{
					_lastAmount = crafted;
					lastString = crafted + " " + Strings.TNT_Crafted;
				}
				return lastString;
			}
		}

		public CraftTNTAcheivement(string apiName, CastleMinerZAchievementManager manager, string name)
			: base(apiName, (AchievementManager<CastleMinerZPlayerStats>)manager, name, Strings.Craft + " " + Strings.TNT)
		{
		}
	}
}
