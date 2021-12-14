using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class DoorInventoryItemClass : ItemBlockInventoryItemClass
	{
		public DoorEntity.ModelNameEnum ModelName = DoorEntity.ModelNameEnum.Wood;

		public DoorInventoryItemClass(InventoryItemIDs itemID, BlockTypeEnum blockType, DoorEntity.ModelNameEnum modelName, string longDescription)
			: base(itemID, blockType, longDescription)
		{
			_playerMode = PlayerMode.Block;
			ModelName = modelName;
			ModelNameIndex = (int)DoorEntity.GetModelNameFromInventoryId(itemID);
			BlockInventoryItemClass.DoorClasses[modelName] = this;
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new DoorInventoryitem(this, stackCount);
		}

		public override Entity CreateWorldEntity(bool attachedToLocalPlayer, BlockTypeEnum blockType, DoorEntity.ModelNameEnum modelName)
		{
			DoorEntity doorEntity = new DoorEntity(modelName, blockType);
			doorEntity.SetPosition(blockType);
			return doorEntity;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			Entity entity = null;
			switch (use)
			{
			case ItemUse.Hand:
			{
				ModelName = DoorEntity.GetModelNameFromInventoryId(ID);
				DoorEntity doorEntity2 = new DoorEntity(ModelName, BlockType._type);
				entity = doorEntity2;
				doorEntity2.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 2f) * Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI / 4f);
				doorEntity2.LocalScale = new Vector3(0.1f, 0.1f, 0.1f);
				doorEntity2.LocalPosition = new Vector3(0f, 0.11126215f, 0f);
				break;
			}
			case ItemUse.Pickup:
			{
				Door door = CastleMinerZGame.Instance.CurrentWorld.GetDoor(Location);
				if (door != null)
				{
					ModelName = door.ModelName;
				}
				DoorEntity doorEntity = new DoorEntity(ModelName, BlockType._type);
				entity = doorEntity;
				break;
			}
			case ItemUse.UI:
			{
				ModelEntity modelEntity = new ModelEntity(DoorEntity.GetDoorModel(ModelName));
				modelEntity.EnableDefaultLighting();
				entity = modelEntity;
				Quaternion rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, 0f);
				float scale = 31.36f / modelEntity.GetLocalBoundingSphere().Radius;
				Matrix localToParent = Matrix.Transform(Matrix.CreateScale(scale), rotation);
				localToParent.Translation = new Vector3(-14f, -28f, 0f);
				modelEntity.LocalToParent = localToParent;
				break;
			}
			}
			if (entity != null)
			{
				entity.EntityColor = Color.Gray;
			}
			return entity;
		}
	}
}
