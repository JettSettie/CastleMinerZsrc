using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class GrenadeInventoryItemClass : ModelInventoryItemClass
	{
		public GrenadeTypeEnum GrenadeType;

		public override bool IsMeleeWeapon
		{
			get
			{
				return false;
			}
		}

		public GrenadeInventoryItemClass(InventoryItemIDs id, Model model, string name, string description, GrenadeTypeEnum grenadeType)
			: base(id, model, name, description, 10, TimeSpan.FromSeconds(1.0), Color.Gray)
		{
			_playerMode = PlayerMode.Grenade;
			GrenadeType = grenadeType;
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new GrenadeItem(this, stackCount);
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			CastleMinerToolModel castleMinerToolModel = new CastleMinerToolModel(_model, use, attachedToLocalPlayer);
			castleMinerToolModel.EnablePerPixelLighting();
			switch (use)
			{
			case ItemUse.UI:
			{
				float value = (GrenadeType != GrenadeTypeEnum.Sticky) ? (25.6f / castleMinerToolModel.GetLocalBoundingSphere().Radius) : (22.4f / castleMinerToolModel.GetLocalBoundingSphere().Radius);
				castleMinerToolModel.LocalScale = new Vector3(value);
				castleMinerToolModel.LocalRotation = Quaternion.Concatenate(Quaternion.CreateFromYawPitchRoll(-1.5f, -1.2f, -0.5f), Quaternion.CreateFromYawPitchRoll(0f, 0.2f, 0f));
				castleMinerToolModel.LocalPosition = new Vector3(-36f, -12f, 0f);
				castleMinerToolModel.EnableDefaultLighting();
				break;
			}
			case ItemUse.Hand:
				castleMinerToolModel.EnablePerPixelLighting();
				break;
			case ItemUse.Pickup:
				castleMinerToolModel.EnablePerPixelLighting();
				break;
			}
			return castleMinerToolModel;
		}
	}
}
