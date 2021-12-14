using DNA.Drawing;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class PlayerModelEntity : SkinnedModelEntity
	{
		public Vector3[] DirectLightColor = new Vector3[2];

		public Vector3[] DirectLightDirection = new Vector3[2];

		public Vector3 AmbientLight = Color.Gray.ToVector3();

		public PlayerModelEntity(Model model)
			: base(model)
		{
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			DNAEffect dNAEffect = effect as DNAEffect;
			if (dNAEffect != null)
			{
				effect.Parameters["LightDirection1"].SetValue(-DirectLightDirection[0]);
				effect.Parameters["LightColor1"].SetValue(DirectLightColor[0]);
				effect.Parameters["LightDirection2"].SetValue(-DirectLightDirection[1]);
				effect.Parameters["LightColor2"].SetValue(DirectLightColor[1]);
				dNAEffect.AmbientColor = ColorF.FromVector3(AmbientLight);
				dNAEffect.EmissiveColor = Color.Black;
			}
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}
	}
}
