using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class ClockEntity : CastleMinerToolModel
	{
		public bool TrackPosition = true;

		public ClockEntity(Model model, ItemUse use, bool attachedToLocalPlayer)
			: base(model, use, attachedToLocalPlayer)
		{
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (TrackPosition && mesh.Name.Contains("Needle") && CastleMinerZGame.Instance.GameScreen != null)
			{
				Quaternion quaternion = Quaternion.CreateFromAxisAngle(Vector3.Down, (float)Math.PI * 2f * CastleMinerZGame.Instance.GameScreen.TimeOfDay);
				float z = quaternion.Z;
				quaternion.Z = quaternion.Y;
				quaternion.Y = z;
				quaternion.Normalize();
				world = Matrix.CreateFromQuaternion(quaternion) * world;
			}
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}
	}
}
