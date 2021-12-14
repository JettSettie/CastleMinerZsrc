using DNA.CastleMinerZ.Globalization;
using Facebook;

namespace DNA.CastleMinerZ.Achievements
{
	public class CastleMinerZAchievementManager : AchievementManager<CastleMinerZPlayerStats>
	{
		private CastleMinerZGame _game;

		public Achievement[] Achievements = new Achievement[31];

		public CastleMinerZAchievementManager(CastleMinerZGame game)
			: base(game.PlayerStats)
		{
			_game = game;
		}

		public override void CreateAcheivements()
		{
			AddAcheivement(Achievements[0] = new PlayTimeAchievement("ACH_TIME_PLAYED_1", this, 1, Strings.Short_Timer));
			AddAcheivement(Achievements[1] = new PlayTimeAchievement("ACH_TIME_PLAYED_10", this, 10, Strings.Veteren_MinerZ));
			AddAcheivement(Achievements[2] = new PlayTimeAchievement("ACH_TIME_PLAYED_100", this, 100, Strings.MinerZ_Potato));
			AddAcheivement(Achievements[3] = new DistaceTraveledAchievement("ACH_DISTANCE_50", this, 50, Strings.First_Contact));
			AddAcheivement(Achievements[4] = new DistaceTraveledAchievement("ACH_DISTANCE_200", this, 200, Strings.Leaving_Home));
			AddAcheivement(Achievements[5] = new DistaceTraveledAchievement("ACH_DISTANCE_1000", this, 1000, Strings.Desert_Crawler));
			AddAcheivement(Achievements[6] = new DistaceTraveledAchievement("ACH_DISTANCE_2300", this, 2300, Strings.Mountain_Man));
			AddAcheivement(Achievements[7] = new DistaceTraveledAchievement("ACH_DISTANCE_3000", this, 3000, Strings.Deep_Freeze));
			AddAcheivement(Achievements[8] = new DistaceTraveledAchievement("ACH_DISTANCE_3600", this, 3600, Strings.Hell_On_Earth));
			AddAcheivement(Achievements[9] = new DistaceTraveledAchievement("ACH_DISTANCE_5000", this, 5000, Strings.Around_the_World));
			AddAcheivement(Achievements[10] = new DepthTraveledAchievement("ACH_DEPTH_20", this, -20f, Strings.Deep_Digger));
			AddAcheivement(Achievements[11] = new DepthTraveledAchievement("ACH_DEPTH_40", this, -40f, Strings.Welcome_To_Hell));
			AddAcheivement(Achievements[12] = new DaysPastAchievement("ACH_DAYS_1", this, 1, Strings.Survived_The_Night));
			AddAcheivement(Achievements[13] = new DaysPastAchievement("ACH_DAYS_7", this, 7, Strings.A_Week_Later));
			AddAcheivement(Achievements[14] = new DaysPastAchievement("ACH_DAYS_28", this, 28, Strings._28_Days_Later));
			AddAcheivement(Achievements[15] = new DaysPastAchievement("ACH_DAYS_100", this, 100, Strings.Survivor));
			AddAcheivement(Achievements[16] = new DaysPastAchievement("ACH_DAYS_196", this, 196, Strings._28_Weeks_Later));
			AddAcheivement(Achievements[17] = new DaysPastAchievement("ACH_DAYS_365", this, 365, Strings.Anniversary));
			AddAcheivement(Achievements[18] = new TotalCraftedAchievement("ACH_CRAFTED_1", this, 1, Strings.Tinkerer));
			AddAcheivement(Achievements[19] = new TotalCraftedAchievement("ACH_CRAFTED_100", this, 100, Strings.Crafter));
			AddAcheivement(Achievements[20] = new TotalCraftedAchievement("ACH_CRAFTED_1000", this, 1000, Strings.Master_Craftsman));
			AddAcheivement(Achievements[21] = new TotalKillsAchievement("ACH_TOTAL_KILLS_1", this, 1, Strings.Self_Defense));
			AddAcheivement(Achievements[22] = new TotalKillsAchievement("ACH_TOTAL_KILLS_100", this, 100, Strings.No_Fear));
			AddAcheivement(Achievements[23] = new TotalKillsAchievement("ACH_TOTAL_KILLS_1000", this, 1000, Strings.Zombie_Slayer));
			AddAcheivement(Achievements[24] = new UndeadKilledAchievement("ACH_UNDEAD_DRAGON_KILLED", this, Strings.Dragon_Slayer));
			AddAcheivement(Achievements[25] = new AlienEncounterAchievement("ACH_ALIEN_ENCOUNTER", this, Strings.Alien_Encounter));
			AddAcheivement(Achievements[26] = new EnemiesKilledWithLaserWeaponAchievement("ACH_LASER_KILLS", this, Strings.Alien_Technology));
			AddAcheivement(Achievements[27] = new CraftTNTAcheivement("ACH_CRAFT_TNT", this, Strings.Demolition_Expert));
			AddAcheivement(Achievements[28] = new KillDragonGuidedMissileAchievement("ACH_GUIDED_MISSILE_KILL", this, Strings.Air_Defense));
			AddAcheivement(Achievements[29] = new KillEnemyTNTAchievement("ACH_TNT_KILL", this, Strings.Fire_In_The_Hole));
			AddAcheivement(Achievements[30] = new KillEnemyGrenadeAchievement("ACH_GRENADE_KILL", this, Strings.Boom));
		}

		public override void OnAchieved(Achievement acheivement)
		{
			string aPIName = acheivement.APIName;
			if (aPIName != null)
			{
				base.PlayerStats.SteamAPI.SetAchievement(aPIName, true);
			}
			if (_game.PlayerStats.PostOnAchievement)
			{
				CastleMinerZGame.Instance.TaskScheduler.QueueUserWorkItem(delegate
				{
					//IL_0005: Unknown result type (might be due to invalid IL or missing references)
					try
					{
						new FacebookClient(CastleMinerZGame.FacebookAccessToken);
						PostToWall postToWall = new PostToWall();
						postToWall.Message = Strings.Has_earned + " " + acheivement.Name + " " + Strings.playing + " CastleMiner Z";
						postToWall.Link = "http://castleminerz.com/";
						postToWall.Description = Strings.Travel_with_your_friends_in_a_huge__ever_changing_world_and_craft_modern_weapons_to_defend_yourself_from_dragons_and_the_zombie_horde_;
						postToWall.ActionName = Strings.Download_Now;
						postToWall.ActionURL = "http://castleminerz.com/Download.html";
						postToWall.ImageURL = "http://digitaldnagames.com/Images/CastleMinerZBox.jpg";
						postToWall.AccessToken = CastleMinerZGame.FacebookAccessToken;
						postToWall.Post();
					}
					catch
					{
					}
				});
			}
			base.OnAchieved(acheivement);
		}
	}
}
