using DNA.CastleMinerZ.Inventory;
using DNA.Drawing;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ
{
	public class GunEntity : CastleMinerToolModel
	{
		private static Model _muzzleFlashModel;

		public Vector3 BarrelTipLocation;

		private ModelEntity _muzzleFlash;

		private Random rand = new Random();

		public Color DiffuseColor = Color.Gray;

		public void ShowMuzzleFlash()
		{
			_muzzleFlash.Visible = true;
		}

		static GunEntity()
		{
			_muzzleFlashModel = CastleMinerZGame.Instance.Content.Load<Model>("MuzzleFlash");
		}

		public GunEntity(Model gunModel, ItemUse use, bool attachedToLocalPlayer)
			: base(gunModel, use, attachedToLocalPlayer)
		{
			_muzzleFlash = new ModelEntity(_muzzleFlashModel);
			_muzzleFlash.BlendState = BlendState.Additive;
			_muzzleFlash.DepthStencilState = DepthStencilState.DepthRead;
			ModelBone modelBone = base.Model.Bones["BarrelTip"];
			if (modelBone != null)
			{
				BarrelTipLocation = Vector3.Transform(Vector3.Zero, modelBone.Transform);
			}
			else
			{
				BarrelTipLocation = new Vector3(0f, 0f, -0.5f);
			}
			_muzzleFlash.Visible = false;
			base.Children.Add(_muzzleFlash);
			EnableDefaultLighting();
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			bool result = base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
			DNAEffect dNAEffect = effect as DNAEffect;
			if (dNAEffect != null)
			{
				dNAEffect.DiffuseColor = DiffuseColor;
			}
			return result;
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			CalculateLighting();
			base.Draw(device, gameTime, view, projection);
			_muzzleFlash.LocalToParent = Matrix.CreateScale(0.75f + (float)rand.NextDouble() / 2f) * Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)rand.NextDouble() * ((float)Math.PI * 2f))) * base.Skeleton["BarrelTip"].Transform;
			_muzzleFlash.Visible = false;
		}
	}
}
