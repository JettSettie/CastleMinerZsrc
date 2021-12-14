using System;

namespace DNA.CastleMinerZ.AI
{
	[Flags]
	public enum DamageType
	{
		BLUNT = 0x1,
		PIERCING = 0x2,
		BLADE = 0x4,
		BULLET = 0x8,
		SHOTGUN = 0x10
	}
}
