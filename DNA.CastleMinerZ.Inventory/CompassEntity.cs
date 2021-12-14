using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class CompassEntity : CastleMinerToolModel
	{
		public bool TrackPosition = true;

		public CompassEntity(Model model, ItemUse use, bool attachedToLocalPlayer)
			: base(model, use, attachedToLocalPlayer)
		{
		}

		public Player GetPlayer()
		{
			for (Entity parent = base.Parent; parent != null; parent = parent.Parent)
			{
				if (parent is Player)
				{
					return (Player)parent;
				}
			}
			return null;
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (TrackPosition && mesh.Name.Contains("Needle"))
			{
				Player player = GetPlayer();
				if (player == null)
				{
					player = CastleMinerZGame.Instance.LocalPlayer;
				}
				if (player != null)
				{
					Vector3 v = -player.WorldPosition;
					Vector3 v2 = Vector3.TransformNormal(Vector3.Forward, player.LocalToWorld);
					v.Y = 0f;
					v2.Y = 0f;
					Quaternion quaternion = v2.RotationBetween(v);
					float z = quaternion.Z;
					quaternion.Z = quaternion.Y;
					quaternion.Y = z;
					quaternion.Normalize();
					world = Matrix.CreateFromQuaternion(quaternion) * world;
				}
			}
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}
	}
}
