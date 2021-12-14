using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DNA.CastleMinerZ
{
	public class DoorEntity : Entity
	{
		public enum ModelNameEnum
		{
			None,
			Wood,
			Iron,
			Diamond,
			Tech
		}

		private class DoorModelEntity : ModelEntity
		{
			public Vector3[] DirectLightColor = new Vector3[2];

			public Vector3[] DirectLightDirection = new Vector3[2];

			public Vector3 AmbientLight = Color.Gray.ToVector3();

			public DoorModelEntity(ModelNameEnum modelName)
				: base(GetDoorModel(modelName))
			{
			}

			public void CalculateLighting()
			{
				Vector3 worldPosition = base.WorldPosition;
				BlockTerrain.Instance.GetEnemyLighting(worldPosition, ref DirectLightDirection[0], ref DirectLightColor[0], ref DirectLightDirection[1], ref DirectLightColor[1], ref AmbientLight);
			}

			protected override void OnUpdate(GameTime gameTime)
			{
				CalculateLighting();
				base.OnUpdate(gameTime);
			}

			protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
			{
				DNAEffect dNAEffect = effect as DNAEffect;
				if (dNAEffect != null && dNAEffect.Parameters["LightDirection1"] != null)
				{
					dNAEffect.Parameters["LightDirection1"].SetValue(-DirectLightDirection[0]);
					dNAEffect.Parameters["LightColor1"].SetValue(DirectLightColor[0]);
					dNAEffect.Parameters["LightDirection2"].SetValue(-DirectLightDirection[1]);
					dNAEffect.Parameters["LightColor2"].SetValue(DirectLightColor[1]);
					dNAEffect.AmbientColor = ColorF.FromVector3(AmbientLight);
				}
				return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
			}
		}

		public static readonly string[] _modelPaths;

		private static List<Model> _doorModels;

		public static Model _doorModel;

		private bool _xAxis;

		private DoorModelEntity _modelEnt;

		private bool _doorOpen;

		public bool DoorOpen
		{
			get
			{
				return _doorOpen;
			}
		}

		public static ModelNameEnum GetModelNameFromInventoryId(InventoryItemIDs invID)
		{
			ModelNameEnum result = ModelNameEnum.Wood;
			switch (invID)
			{
			case InventoryItemIDs.Door:
				result = ModelNameEnum.Wood;
				break;
			case InventoryItemIDs.IronDoor:
				result = ModelNameEnum.Iron;
				break;
			case InventoryItemIDs.DiamondDoor:
				result = ModelNameEnum.Diamond;
				break;
			case InventoryItemIDs.TechDoor:
				result = ModelNameEnum.Tech;
				break;
			}
			return result;
		}

		public static Model GetDoorModel(ModelNameEnum modelName)
		{
			return _doorModels[(int)modelName];
		}

		static DoorEntity()
		{
			_modelPaths = new string[5]
			{
				"Props\\Items\\Door\\Model",
				"Props\\Items\\Door\\Model",
				"Props\\Items\\IronDoor\\Model",
				"Props\\Items\\DiamondDoor\\Model",
				"Props\\Items\\TechDoor\\Model"
			};
			_doorModels = new List<Model>();
			_doorModel = CastleMinerZGame.Instance.Content.Load<Model>("Props\\Items\\Door\\Model");
			if (_doorModels.Count == 0)
			{
				for (int i = 0; i < _modelPaths.Length; i++)
				{
					_doorModels.Add(CastleMinerZGame.Instance.Content.Load<Model>(_modelPaths[i]));
				}
			}
		}

		public DoorEntity(ModelNameEnum modelName, BlockTypeEnum blockType)
		{
			_modelEnt = new DoorModelEntity(modelName);
			base.Children.Add(_modelEnt);
			SetPosition(DoorInventoryitem.GetDoorPiece(BlockTypeEnum.NormalLowerDoorClosedX, blockType));
		}

		public void SetPosition(BlockTypeEnum doorType)
		{
			switch (doorType)
			{
			case BlockTypeEnum.NormalLowerDoorClosedX:
				_xAxis = true;
				_doorOpen = false;
				break;
			case BlockTypeEnum.NormalLowerDoorClosedZ:
				_xAxis = false;
				_doorOpen = false;
				break;
			case BlockTypeEnum.NormalLowerDoorOpenX:
				_xAxis = true;
				_doorOpen = true;
				break;
			case BlockTypeEnum.NormalLowerDoorOpenZ:
				_xAxis = false;
				_doorOpen = true;
				break;
			case BlockTypeEnum.StrongLowerDoorClosedX:
				_xAxis = true;
				_doorOpen = false;
				break;
			case BlockTypeEnum.StrongLowerDoorClosedZ:
				_xAxis = false;
				_doorOpen = false;
				break;
			case BlockTypeEnum.StrongLowerDoorOpenX:
				_xAxis = true;
				_doorOpen = true;
				break;
			case BlockTypeEnum.StrongLowerDoorOpenZ:
				_xAxis = false;
				_doorOpen = true;
				break;
			}
			if (_xAxis)
			{
				if (_doorOpen)
				{
					_modelEnt.LocalPosition = new Vector3(-0.5f, -0.5f, 0f);
					_modelEnt.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI / 2f);
				}
				else
				{
					_modelEnt.LocalPosition = new Vector3(-0.5f, -0.5f, 0f);
					_modelEnt.LocalRotation = Quaternion.Identity;
				}
			}
			else if (_doorOpen)
			{
				_modelEnt.LocalPosition = new Vector3(0f, -0.5f, -0.5f);
				_modelEnt.LocalRotation = Quaternion.Identity;
			}
			else
			{
				_modelEnt.LocalPosition = new Vector3(0f, -0.5f, -0.5f);
				_modelEnt.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -(float)Math.PI / 2f);
			}
		}
	}
}
