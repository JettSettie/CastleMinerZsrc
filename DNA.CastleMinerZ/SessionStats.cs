using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Net;
using System.Collections.Generic;

namespace DNA.CastleMinerZ
{
	public class SessionStats
	{
		public enum StatType
		{
			ZombieDefeated,
			SkeletonDefeated,
			SKELETONARCHER,
			SKELETONAXES,
			SKELETONSWORD,
			FelguardDefeated,
			AlienDefeated,
			DragonDefeated,
			HellMinionDefeated,
			LootBlockOpened,
			LuckyLootBlockOpened,
			PlayerDefeated
		}

		public enum StatAction
		{
			HasDefeated,
			HasOpened,
			HasFallen
		}

		public struct SessionStatsData
		{
			public int Count;

			public int DisplayIncrement;

			public string StatSingular;

			public string StatPlural;

			public StatAction Action;

			public SessionStatsData(int count, int displayIncrement, StatAction action, string statSingular, string statPlural)
			{
				Count = count;
				DisplayIncrement = displayIncrement;
				Action = action;
				StatSingular = statSingular;
				StatPlural = statPlural;
			}
		}

		private bool _broadcastStatsAllowed = true;

		private Dictionary<StatType, SessionStatsData> statMap = new Dictionary<StatType, SessionStatsData>();

		public SessionStats()
		{
			statMap.Add(StatType.ZombieDefeated, new SessionStatsData(0, 10, StatAction.HasDefeated, Strings.Zombies, Strings.Zombies));
			statMap.Add(StatType.HellMinionDefeated, new SessionStatsData(0, 10, StatAction.HasDefeated, Strings.Skeletons, Strings.Skeletons));
			statMap.Add(StatType.SkeletonDefeated, new SessionStatsData(0, 10, StatAction.HasDefeated, Strings.Skeletons, Strings.Skeletons));
			statMap.Add(StatType.DragonDefeated, new SessionStatsData(0, 1, StatAction.HasDefeated, Strings.Dragon, Strings.Dragons));
			statMap.Add(StatType.AlienDefeated, new SessionStatsData(0, 5, StatAction.HasDefeated, Strings.Alien, Strings.Aliens));
			statMap.Add(StatType.FelguardDefeated, new SessionStatsData(0, 1, StatAction.HasDefeated, Strings.Underlord, Strings.Underlords));
			statMap.Add(StatType.PlayerDefeated, new SessionStatsData(0, 10, StatAction.HasFallen, Strings.Times, Strings.Times));
			statMap.Add(StatType.LootBlockOpened, new SessionStatsData(0, 10, StatAction.HasOpened, Strings.Loot_Block, Strings.Loot_Blocks));
			statMap.Add(StatType.LuckyLootBlockOpened, new SessionStatsData(0, 5, StatAction.HasOpened, Strings.Lucky_Loot_Block, Strings.Lucky_Loot_Blocks));
		}

		internal void AddStat(StatType category)
		{
			if (statMap.ContainsKey(category))
			{
				SessionStatsData sessionStatsData = statMap[category];
				sessionStatsData.Count++;
				statMap[category] = sessionStatsData;
				CheckProgress(sessionStatsData, category);
			}
		}

		private string GetActionAsString(StatAction action)
		{
			string result = "has";
			switch (action)
			{
			case StatAction.HasDefeated:
				result = Strings.Has_Defeated;
				break;
			case StatAction.HasFallen:
				result = Strings.Has_Fallen;
				break;
			case StatAction.HasOpened:
				result = Strings.Has_Opened;
				break;
			}
			return result;
		}

		private string GetBroadcastString(SessionStatsData statData)
		{
			string text = statData.Count + " " + ((statData.Count == 1) ? statData.StatSingular : statData.StatPlural);
			return CastleMinerZGame.Instance.LocalPlayer.Gamer.Gamertag + " " + GetActionAsString(statData.Action) + " " + text;
		}

		private void CheckProgress(SessionStatsData statData, StatType category)
		{
			int displayIncrement = statData.DisplayIncrement;
			int count = statData.Count;
			if (count % displayIncrement == 0 && _broadcastStatsAllowed)
			{
				BroadcastTextMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, GetBroadcastString(statData));
			}
		}
	}
}
