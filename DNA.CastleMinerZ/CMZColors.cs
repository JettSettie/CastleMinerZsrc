using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ
{
	internal static class CMZColors
	{
		public static readonly Color Wood = Color.SaddleBrown;

		public static readonly Color Copper = new Color(184, 115, 51);

		public static readonly Color Coal = new Color(2, 2, 2);

		public static readonly Color Stone = Color.DarkGray;

		public static readonly Color Brass = new Color(234, 227, 150);

		public static readonly Color Iron = new Color(128, 128, 128);

		public static readonly Color Gold = new Color(255, 189, 0);

		public static readonly Color Diamond = new Color(26, 168, 177);

		public static readonly Color BloodStone = Color.DarkRed;

		public static readonly Color CopperOre = new Color(42, 156, 119);

		public static readonly Color IronOre = new Color(172, 85, 0);

		public static readonly Color MenuBlue = new Color(100, 158, 198);

		public static readonly Color MenuGreen = new Color(78, 177, 61);

		public static readonly Color MenuOrange = new Color(226, 106, 61);

		public static readonly Color MenuAqua = new Color(53, 170, 253);

		public static Color GetMaterialcColor(ToolMaterialTypes mat)
		{
			switch (mat)
			{
			case ToolMaterialTypes.Wood:
				return Wood;
			case ToolMaterialTypes.Stone:
				return Stone;
			case ToolMaterialTypes.Copper:
				return Copper;
			case ToolMaterialTypes.Iron:
				return Iron;
			case ToolMaterialTypes.Gold:
				return Gold;
			case ToolMaterialTypes.Diamond:
				return Diamond;
			case ToolMaterialTypes.BloodStone:
				return BloodStone;
			default:
				return Color.Gray;
			}
		}

		public static Color GetLaserMaterialcColor(ToolMaterialTypes mat)
		{
			switch (mat)
			{
			case ToolMaterialTypes.Copper:
				return Color.Lime;
			case ToolMaterialTypes.Iron:
				return Color.Red;
			case ToolMaterialTypes.Gold:
				return Color.Yellow;
			case ToolMaterialTypes.Diamond:
				return Color.Blue;
			case ToolMaterialTypes.BloodStone:
				return Color.GhostWhite;
			default:
				return Color.Gray;
			}
		}
	}
}
