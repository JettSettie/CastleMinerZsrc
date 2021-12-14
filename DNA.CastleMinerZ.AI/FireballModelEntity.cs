using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.AI
{
	public class FireballModelEntity : ModelEntity
	{
		public FireballModelEntity(Model fireballModel)
			: base(fireballModel)
		{
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			effect.CurrentTechnique = effect.Techniques[0];
			if (effect is BasicEffect)
			{
				BasicEffect basicEffect = (BasicEffect)effect;
				basicEffect.World = world;
				basicEffect.View = view;
				basicEffect.Projection = projection;
				basicEffect.EmissiveColor = Vector3.Zero;
				basicEffect.DiffuseColor = Vector3.One;
				basicEffect.AmbientLightColor = Vector3.One;
				basicEffect.DirectionalLight0.Enabled = false;
				basicEffect.DirectionalLight1.Enabled = false;
				basicEffect.DirectionalLight2.Enabled = false;
			}
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}
	}
}
