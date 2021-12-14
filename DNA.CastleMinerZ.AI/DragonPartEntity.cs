using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.AI
{
	public class DragonPartEntity : SkinnedModelEntity
	{
		public const float HEIGHT_OFFSET = -23.5f;

		public const float SUB_PART_SCALE = 0.5f;

		public Texture2D DragonTexture;

		public DragonPartEntity(DragonType type, Model model)
			: base(model)
		{
			base.LocalRotation = Quaternion.CreateFromYawPitchRoll((float)Math.PI, 0f, 0f);
			base.LocalPosition = new Vector3(0f, -23.5f, 4f) * 0.5f;
			DragonTexture = type.Texture;
			DrawPriority = (int)(520 + type.EType);
			Collider = false;
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (effect is DNAEffect)
			{
				DNAEffect dNAEffect = (DNAEffect)effect;
				if (dNAEffect.Parameters["LightDirection1"] != null)
				{
					dNAEffect.Parameters["LightDirection1"].SetValue(BlockTerrain.Instance.VectorToSun);
					dNAEffect.Parameters["LightColor1"].SetValue(BlockTerrain.Instance.SunlightColor.ToVector3());
					dNAEffect.AmbientColor = ColorF.FromVector3(BlockTerrain.Instance.AmbientSunColor.ToVector3() * 0.5f);
				}
				dNAEffect.DiffuseMap = DragonTexture;
			}
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}
	}
}
