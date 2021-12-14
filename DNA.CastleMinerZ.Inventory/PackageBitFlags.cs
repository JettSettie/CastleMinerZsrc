using System;

namespace DNA.CastleMinerZ.Inventory
{
	[Flags]
	public enum PackageBitFlags
	{
		None = 0x0,
		Common = 0x1,
		Normal = 0x2,
		Rare = 0x4,
		Epic = 0x8,
		Legendary = 0x10,
		Alien = 0x20,
		Hell = 0x40,
		Desert = 0x80,
		Forest = 0x100,
		Moutain = 0x200,
		Volcano = 0x400,
		Underground = 0x800,
		SkyIsland = 0x1000,
		Dragon = 0x2000,
		Champion = 0x4000,
		Boss = 0x8000,
		UndeadDragon = 0x10000,
		CurrentBiome = 0x20000
	}
}
