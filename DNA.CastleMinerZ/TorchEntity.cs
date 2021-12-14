using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using DNA.Drawing.Effects;
using DNA.Drawing.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ
{
	public class TorchEntity : Entity
	{
		private class TorchModelEntity : ModelEntity
		{
			public Vector3[] DirectLightColor = new Vector3[2];

			public Vector3[] DirectLightDirection = new Vector3[2];

			public Vector3 AmbientLight = Color.Gray.ToVector3();

			public TorchModelEntity()
				: base(_torchModel)
			{
			}

			protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
			{
				DNAEffect dNAEffect = effect as DNAEffect;
				if (dNAEffect != null)
				{
					dNAEffect.EmissiveColor = Color.Black;
					if (dNAEffect.Parameters["LightDirection1"] != null)
					{
						dNAEffect.Parameters["LightDirection1"].SetValue(-DirectLightDirection[0]);
						dNAEffect.Parameters["LightColor1"].SetValue(DirectLightColor[0]);
						dNAEffect.Parameters["LightDirection2"].SetValue(-DirectLightDirection[1]);
						dNAEffect.Parameters["LightColor2"].SetValue(DirectLightColor[1]);
						dNAEffect.AmbientColor = ColorF.FromVector3(AmbientLight);
					}
				}
				return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
			}

			protected override void OnUpdate(GameTime gameTime)
			{
				Vector3 position = Vector3.Transform(new Vector3(0.1f, -0.3f, -0.25f), CastleMinerZGame.Instance.LocalPlayer.FPSCamera.LocalToWorld);
				BlockTerrain.Instance.GetEnemyLighting(position, ref DirectLightDirection[0], ref DirectLightColor[0], ref DirectLightDirection[1], ref DirectLightColor[1], ref AmbientLight);
				base.OnUpdate(gameTime);
			}
		}

		public static Model _torchModel;

		private static ParticleEffect _smokeEffect;

		private static ParticleEffect _fireEffect;

		private BlockFace AttachedFace = BlockFace.NUM_FACES;

		private ModelEntity _modelEnt;

		private ParticleEmitter _smokeEmitter;

		private ParticleEmitter _fireEmitter;

		private bool _hasFlame;

		public bool HasFlame
		{
			get
			{
				return _hasFlame;
			}
			set
			{
				if (value)
				{
					AddFlame();
				}
				else
				{
					RemoveFlame();
				}
			}
		}

		static TorchEntity()
		{
			_torchModel = CastleMinerZGame.Instance.Content.Load<Model>("Props\\Items\\Torch\\Model");
			_smokeEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\TorchSmoke");
			_fireEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\TorchFire");
		}

		public TorchEntity(bool hasParticles)
		{
			_modelEnt = new TorchModelEntity();
			base.Children.Add(_modelEnt);
			HasFlame = hasParticles;
			SetPosition(AttachedFace);
		}

		protected override void OnParentChanged(Entity oldParent, Entity newParent)
		{
			if (newParent == null)
			{
				RemoveFlame();
			}
			base.OnParentChanged(oldParent, newParent);
		}

		public void RemoveFlame()
		{
			if (_hasFlame)
			{
				_hasFlame = false;
				if (_smokeEmitter != null)
				{
					_smokeEmitter.RemoveFromParent();
				}
				if (_fireEmitter != null)
				{
					_fireEmitter.RemoveFromParent();
				}
			}
		}

		public void AddFlame()
		{
			if (!_hasFlame)
			{
				_hasFlame = true;
				_smokeEmitter = _smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
				_smokeEmitter.Emitting = true;
				_smokeEmitter.DrawPriority = 900;
				_modelEnt.Children.Add(_smokeEmitter);
				_fireEmitter = _fireEffect.CreateEmitter(CastleMinerZGame.Instance);
				_fireEmitter.Emitting = true;
				_fireEmitter.DrawPriority = 900;
				_modelEnt.Children.Add(_fireEmitter);
				Matrix transform = _modelEnt.Skeleton["Flame"].Transform;
				_smokeEmitter.LocalToParent = transform;
				_fireEmitter.LocalToParent = transform;
			}
		}

		public void SetPosition(BlockFace face)
		{
			AttachedFace = face;
			switch (AttachedFace)
			{
			case BlockFace.NUM_FACES:
				_modelEnt.LocalPosition = Vector3.Zero;
				_modelEnt.LocalRotation = Quaternion.Identity;
				break;
			case BlockFace.POSY:
				_modelEnt.LocalPosition = new Vector3(0f, -0.5f, 0f);
				_modelEnt.LocalRotation = Quaternion.Identity;
				break;
			case BlockFace.NEGX:
				_modelEnt.LocalPosition = new Vector3(0.5f, -0.25f, 0f);
				_modelEnt.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Math.PI / 4f);
				break;
			case BlockFace.NEGZ:
				_modelEnt.LocalPosition = new Vector3(0f, -0.25f, 0.5f);
				_modelEnt.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -(float)Math.PI / 4f);
				break;
			case BlockFace.POSX:
				_modelEnt.LocalPosition = new Vector3(-0.5f, -0.25f, 0f);
				_modelEnt.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -(float)Math.PI / 4f);
				break;
			case BlockFace.POSZ:
				_modelEnt.LocalPosition = new Vector3(0f, -0.25f, -0.5f);
				_modelEnt.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 4f);
				break;
			case BlockFace.NEGY:
				_modelEnt.LocalPosition = new Vector3(0f, 0.5f, 0f);
				_modelEnt.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Math.PI);
				break;
			}
		}
	}
}
