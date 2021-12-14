using DNA.Audio;
using DNA.Drawing;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class SaberInventoryItemClass : PickInventoryItemClass
	{
		public class SaberModelEntity : CastleMinerToolModel
		{
			public Color BeamColor;

			public SaberModelEntity(Model model, ItemUse use, bool attachedToLocalPlayer)
				: base(model, use, attachedToLocalPlayer)
			{
			}

			public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
			{
				CalculateLighting();
				int count = base.Model.Meshes.Count;
				for (int i = 0; i < count; i++)
				{
					ModelMesh modelMesh = base.Model.Meshes[i];
					Matrix world = _worldBoneTransforms[modelMesh.ParentBone.Index];
					int count2 = modelMesh.Effects.Count;
					int num = 0;
					while (true)
					{
						if (num < count2)
						{
							if (!SetEffectParams(modelMesh, modelMesh.Effects[num], gameTime, world, view, projection))
							{
								break;
							}
							num++;
							continue;
						}
						if (modelMesh.Name.Contains("Beam"))
						{
							BlendState blendState = device.BlendState;
							device.BlendState = BlendState.Additive;
							modelMesh.Draw();
							device.BlendState = blendState;
						}
						else
						{
							modelMesh.Draw();
						}
						break;
					}
				}
			}

			protected override bool SetEffectParams(ModelMesh mesh, Effect oeffect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
			{
				bool result = base.SetEffectParams(mesh, oeffect, gameTime, world, view, projection);
				DNAEffect dNAEffect = oeffect as DNAEffect;
				if (dNAEffect != null && mesh.Name.Contains("Beam"))
				{
					if (ToolColor.A == 0)
					{
						return false;
					}
					dNAEffect.DiffuseColor = BeamColor;
				}
				return result;
			}
		}

		private Color BeamColor;

		private Cue _activeSoundCue;

		private string _activeSound;

		public SaberInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, Model model, string name, string description, float meleeDamage)
			: base(id, material, model, name, description, 1f)
		{
			BeamColor = CMZColors.GetLaserMaterialcColor(Material);
			ToolColor = Color.Gray;
			_activeSound = "LightSaber";
			ItemSelfDamagePerUse = 0.005f;
		}

		public override void OnItemEquipped()
		{
			if (_activeSound != null)
			{
				_activeSoundCue = SoundManager.Instance.PlayInstance(_activeSound);
			}
			base.OnItemEquipped();
		}

		public override void OnItemUnequipped()
		{
			if (_activeSoundCue != null && _activeSoundCue.IsPlaying)
			{
				_activeSoundCue.Stop(AudioStopOptions.Immediate);
			}
			base.OnItemUnequipped();
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			SaberModelEntity saberModelEntity = new SaberModelEntity(_model, use, attachedToLocalPlayer);
			saberModelEntity.BeamColor = BeamColor;
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, (float)Math.PI * -111f / 400f) * Quaternion.CreateFromYawPitchRoll((float)Math.PI * -5f / 16f, 0f, 0f);
				Matrix localToParent = Matrix.Transform(Matrix.CreateScale(89.6f / saberModelEntity.GetLocalBoundingSphere().Radius), rotation);
				localToParent.Translation += localToParent.Translation + new Vector3(11f, -22f, 0f);
				saberModelEntity.LocalToParent = localToParent;
				saberModelEntity.EnableDefaultLighting();
				break;
			}
			}
			return saberModelEntity;
		}
	}
}
