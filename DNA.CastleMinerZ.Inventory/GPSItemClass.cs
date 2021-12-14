using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class GPSItemClass : ModelInventoryItemClass
	{
		public override bool IsMeleeWeapon
		{
			get
			{
				return false;
			}
		}

		public GPSItemClass(InventoryItemIDs id, Model model, string name, string description)
			: base(id, model, name, description, 1, TimeSpan.FromSeconds(0.30000001192092896), Color.Gray)
		{
			_playerMode = PlayerMode.Generic;
			switch (id)
			{
			case InventoryItemIDs.GPS:
				ItemSelfDamagePerUse = 0.1f;
				break;
			case InventoryItemIDs.TeleportGPS:
			case InventoryItemIDs.SpawnBasic:
				ItemSelfDamagePerUse = 1f;
				break;
			}
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			GPSEntity gPSEntity = new GPSEntity(_model, use, attachedToLocalPlayer);
			if (use != 0)
			{
				gPSEntity.TrackPosition = false;
			}
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion rotation = Quaternion.CreateFromYawPitchRoll(0f, (float)Math.PI / 2f, 0f);
				Matrix localToParent = Matrix.Transform(Matrix.CreateScale(22.4f / gPSEntity.GetLocalBoundingSphere().Radius), rotation);
				localToParent.Translation = new Vector3(0f, 0f, 0f);
				gPSEntity.LocalToParent = localToParent;
				gPSEntity.EnableDefaultLighting();
				break;
			}
			case ItemUse.Hand:
				gPSEntity.TrackPosition = true;
				gPSEntity.LocalRotation = new Quaternion(0.6469873f, 0.1643085f, 0.7078394f, -0.2310277f);
				gPSEntity.LocalPosition = new Vector3(0f, 0.09360941f, 0f);
				break;
			}
			return gPSEntity;
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new GPSItem(this, stackCount);
		}
	}
}
