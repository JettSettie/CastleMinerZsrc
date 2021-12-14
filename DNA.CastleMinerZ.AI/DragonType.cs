using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Terrain;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.AI
{
	public class DragonType
	{
		public enum TextureNameEnum
		{
			FIRE,
			FOREST,
			ICE,
			LIZARD,
			SKELETON,
			COUNT
		}

		public static readonly string[] _textureNames = new string[5]
		{
			"Enemies\\Dragon\\dragon-01_0",
			"Enemies\\Dragon\\dragon-09",
			"Enemies\\Dragon\\dragon-25",
			"Enemies\\Dragon\\dragon-27",
			"Enemies\\Dragon\\dragon-35"
		};

		public static DragonType[] Types = null;

		public static bool[,] BreakLookup;

		public Texture2D Texture;

		public DragonTypeEnum EType;

		public float StartingHealth;

		public float Speed;

		public float MaxAccel;

		public float Scale;

		public float YawRate;

		public float RollRate;

		public float MaxRoll;

		public float PitchRate;

		public float MaxPitch;

		public float SlowViewCheckInterval;

		public float FastViewCheckInterval;

		public float HoverViewCheckInterval;

		public float MaxViewDistance;

		public float HoverDistance;

		public float MaxAttackDistance;

		public float MinAttackDistance;

		public float BreakOffStrafeDistance;

		public float SpawnDistance;

		public float CruisingAltitude;

		public float LoiterAltitude;

		public float HuntingAltitude;

		public float StrafeFireRate;

		public float HoverFireRate;

		public float MinLoiterTime;

		public float MaxLoiterTime;

		public float LoiterDistance;

		public int MinHoverShots;

		public int MaxHoverShots;

		public float ChanceOfHoverAttack;

		public int ChancesToNotAttack;

		public float ShotHearingInterval;

		public float FireballVelocity;

		public float FireballDamage;

		public int MaxLoiters;

		public DragonDamageType DamageType;

		public static DragonType GetDragonType(DragonTypeEnum et)
		{
			return Types[(int)et];
		}

		public string GetDragonName()
		{
			switch (EType)
			{
			case DragonTypeEnum.FIRE:
				return Strings.Fire_Dragon;
			case DragonTypeEnum.FOREST:
				return Strings.Forest_Dragon;
			case DragonTypeEnum.LIZARD:
				return Strings.Sand_Dragon;
			case DragonTypeEnum.ICE:
				return Strings.Ice_Dragon;
			case DragonTypeEnum.SKELETON:
				return Strings.Undead_Dragon;
			default:
				return "";
			}
		}

		public static void Init()
		{
			if (Types != null)
			{
				return;
			}
			Types = new DragonType[5]
			{
				new DragonType(DragonTypeEnum.FIRE, TextureNameEnum.FIRE, 20f, 30f, 0.4f, DragonDamageType.DESTRUCTION),
				new DragonType(DragonTypeEnum.FOREST, TextureNameEnum.FOREST, 100f, 40f, 0.4f, DragonDamageType.DESTRUCTION),
				new DragonType(DragonTypeEnum.LIZARD, TextureNameEnum.LIZARD, 300f, 40f, 0.4f, DragonDamageType.DESTRUCTION),
				new DragonType(DragonTypeEnum.ICE, TextureNameEnum.ICE, 600f, 60f, 0.4f, DragonDamageType.ICE),
				new DragonType(DragonTypeEnum.SKELETON, TextureNameEnum.SKELETON, 1000f, 60f, 0.4f, DragonDamageType.DESTRUCTION)
			};
			BreakLookup = new bool[Types.Length, 95];
			BlockTypeEnum[][] array = new BlockTypeEnum[5][]
			{
				new BlockTypeEnum[17]
				{
					BlockTypeEnum.Rock,
					BlockTypeEnum.LanternFancy,
					BlockTypeEnum.Lantern,
					BlockTypeEnum.SpawnPointBasic,
					BlockTypeEnum.GlassBasic,
					BlockTypeEnum.GlassIron,
					BlockTypeEnum.GlassStrong,
					BlockTypeEnum.GlassMystery,
					BlockTypeEnum.GoldOre,
					BlockTypeEnum.IronOre,
					BlockTypeEnum.CopperOre,
					BlockTypeEnum.CoalOre,
					BlockTypeEnum.DiamondOre,
					BlockTypeEnum.IronWall,
					BlockTypeEnum.CopperWall,
					BlockTypeEnum.GoldenWall,
					BlockTypeEnum.DiamondWall
				},
				new BlockTypeEnum[11]
				{
					BlockTypeEnum.LanternFancy,
					BlockTypeEnum.Lantern,
					BlockTypeEnum.SpawnPointBasic,
					BlockTypeEnum.GlassBasic,
					BlockTypeEnum.GlassIron,
					BlockTypeEnum.GlassStrong,
					BlockTypeEnum.GlassMystery,
					BlockTypeEnum.IronWall,
					BlockTypeEnum.CopperWall,
					BlockTypeEnum.GoldenWall,
					BlockTypeEnum.DiamondWall
				},
				new BlockTypeEnum[9]
				{
					BlockTypeEnum.LanternFancy,
					BlockTypeEnum.Lantern,
					BlockTypeEnum.SpawnPointBasic,
					BlockTypeEnum.GlassIron,
					BlockTypeEnum.GlassStrong,
					BlockTypeEnum.GlassMystery,
					BlockTypeEnum.IronWall,
					BlockTypeEnum.GoldenWall,
					BlockTypeEnum.DiamondWall
				},
				new BlockTypeEnum[5]
				{
					BlockTypeEnum.SpawnPointBasic,
					BlockTypeEnum.GlassStrong,
					BlockTypeEnum.GlassMystery,
					BlockTypeEnum.GoldenWall,
					BlockTypeEnum.DiamondWall
				},
				new BlockTypeEnum[4]
				{
					BlockTypeEnum.SpawnPointBasic,
					BlockTypeEnum.GlassStrong,
					BlockTypeEnum.GlassMystery,
					BlockTypeEnum.DiamondWall
				}
			};
			for (int i = 0; i < Types.Length; i++)
			{
				BreakLookup[i, 0] = true;
				BreakLookup[i, 5] = true;
				BreakLookup[i, 14] = true;
				BreakLookup[i, 20] = true;
				BreakLookup[i, 21] = true;
				BreakLookup[i, 44] = true;
				BreakLookup[i, 43] = true;
				BreakLookup[i, 94] = true;
				for (int j = 0; j < array[i].Length; j++)
				{
					BreakLookup[i, (int)array[i][j]] = true;
				}
			}
		}

		public DragonType(DragonTypeEnum type, TextureNameEnum tname, float health, float fireballVelocity, float fireballDamage, DragonDamageType damageType)
		{
			Texture = CastleMinerZGame.Instance.Content.Load<Texture2D>(_textureNames[(int)tname]);
			EType = type;
			StartingHealth = health;
			Speed = 20f;
			MaxAccel = 10f;
			YawRate = 0.35f;
			RollRate = 1f;
			MaxRoll = 1f;
			PitchRate = 0.2f;
			MaxPitch = 0.4f;
			SlowViewCheckInterval = 2f;
			FastViewCheckInterval = 1f;
			HoverViewCheckInterval = 1f;
			MaxViewDistance = 300f;
			MaxAttackDistance = 200f;
			MinAttackDistance = 50f;
			BreakOffStrafeDistance = 40f;
			FireballDamage = fireballDamage;
			FireballVelocity = fireballVelocity;
			HoverDistance = 50f;
			LoiterDistance = 200f;
			SpawnDistance = 750f;
			CruisingAltitude = 120f;
			LoiterAltitude = 90f;
			HuntingAltitude = 70f;
			StrafeFireRate = 2f;
			HoverFireRate = 1f;
			MinLoiterTime = 2f;
			MaxLoiterTime = 6f;
			MinHoverShots = 4;
			MaxHoverShots = 8;
			ChanceOfHoverAttack = 0.5f;
			ChancesToNotAttack = 5;
			ShotHearingInterval = 5f;
			if (EType == DragonTypeEnum.SKELETON)
			{
				MaxLoiters = 100;
			}
			else
			{
				MaxLoiters = 3;
			}
			DamageType = damageType;
		}
	}
}
