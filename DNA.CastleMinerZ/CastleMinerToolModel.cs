using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ
{
	public class CastleMinerToolModel : ModelEntity
	{
		public Color ToolColor = Color.Gray;

		public Color ToolColor2 = Color.Gray;

		public Color EmissiveColor = Color.Black;

		public bool AttachedToLocalPlayer;

		public ItemUse Context = ItemUse.Hand;

		public Vector3[] DirectLightColor = new Vector3[2];

		public Vector3[] DirectLightDirection = new Vector3[2];

		public Vector3 AmbientLight = Color.Gray.ToVector3();

		public CastleMinerToolModel(Model model, ItemUse use, bool attachedToLocalPlayer)
			: base(model)
		{
			AttachedToLocalPlayer = false;
			Context = use;
			AttachedToLocalPlayer = attachedToLocalPlayer;
		}

		public void CalculateLighting()
		{
			if (Context != 0)
			{
				Vector3 position = base.WorldPosition;
				if (AttachedToLocalPlayer)
				{
					position = Vector3.Transform(new Vector3(0.1f, -0.3f, -0.25f), CastleMinerZGame.Instance.LocalPlayer.FPSCamera.LocalToWorld);
				}
				BlockTerrain.Instance.GetEnemyLighting(position, ref DirectLightDirection[0], ref DirectLightColor[0], ref DirectLightDirection[1], ref DirectLightColor[1], ref AmbientLight);
				if (Context == ItemUse.Pickup)
				{
					float num = (float)Math.IEEERemainder(CastleMinerZGame.Instance.CurrentGameTime.TotalGameTime.TotalSeconds, 1.0) * (float)Math.PI * 2f;
					num = 0.55f + 0.45f * (float)Math.Sin(num);
					AmbientLight = Vector3.Lerp(AmbientLight, Vector3.One, num);
				}
			}
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			CalculateLighting();
			base.OnUpdate(gameTime);
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect oeffect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (base.SetEffectParams(mesh, oeffect, gameTime, world, view, projection))
			{
				BasicEffect basicEffect = oeffect as BasicEffect;
				if (basicEffect != null)
				{
					if (mesh.Name.Contains("recolor_"))
					{
						if (ToolColor.A == 0)
						{
							return false;
						}
						basicEffect.DiffuseColor = ToolColor.ToVector3();
					}
					else if (mesh.Name.Contains("recolor2_"))
					{
						if (ToolColor2.A == 0)
						{
							return false;
						}
						basicEffect.DiffuseColor = ToolColor2.ToVector3();
					}
					else
					{
						basicEffect.DiffuseColor = Color.White.ToVector3();
					}
				}
				else
				{
					DNAEffect dNAEffect = oeffect as DNAEffect;
					if (dNAEffect != null)
					{
						dNAEffect.EmissiveColor = EmissiveColor;
						if (mesh.Name.Contains("recolor_"))
						{
							if (ToolColor.A == 0)
							{
								return false;
							}
							dNAEffect.DiffuseColor = ToolColor;
						}
						else if (mesh.Name.Contains("recolor2_"))
						{
							if (ToolColor2.A == 0)
							{
								return false;
							}
							dNAEffect.DiffuseColor = ToolColor2;
						}
						else
						{
							dNAEffect.DiffuseColor = Color.Gray;
						}
						if (dNAEffect.Parameters["LightDirection1"] != null)
						{
							dNAEffect.Parameters["LightDirection1"].SetValue(-DirectLightDirection[0]);
							dNAEffect.Parameters["LightColor1"].SetValue(DirectLightColor[0]);
							dNAEffect.Parameters["LightDirection2"].SetValue(-DirectLightDirection[1]);
							dNAEffect.Parameters["LightColor2"].SetValue(DirectLightColor[1]);
							dNAEffect.AmbientColor = ColorF.FromVector3(AmbientLight);
						}
					}
				}
				return true;
			}
			return false;
		}
	}
}
