using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class CompassInventoryItemClass : ModelInventoryItemClass
	{
		public override bool IsMeleeWeapon
		{
			get
			{
				return false;
			}
		}

		public CompassInventoryItemClass(InventoryItemIDs id, Model model)
			: base(id, model, Strings.Compass, Strings.Show_the_direction_to_or_away_from_the_start_point + ". " + Strings.In_endurance_mode_travel_in_the_direction_of_the_green_arrow, 1, TimeSpan.FromSeconds(0.30000001192092896), Color.White)
		{
			_playerMode = PlayerMode.Generic;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			CompassEntity compassEntity = null;
			compassEntity = new CompassEntity(_model, use, attachedToLocalPlayer);
			if (use != 0)
			{
				compassEntity.TrackPosition = false;
			}
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion rotation = Quaternion.CreateFromYawPitchRoll(0f, (float)Math.PI / 2f, 0f);
				Matrix localToParent = Matrix.Transform(Matrix.CreateScale(22.4f / compassEntity.GetLocalBoundingSphere().Radius), rotation);
				localToParent.Translation = new Vector3(0f, 0f, 0f);
				compassEntity.LocalToParent = localToParent;
				compassEntity.EnableDefaultLighting();
				break;
			}
			case ItemUse.Hand:
				compassEntity.TrackPosition = true;
				compassEntity.LocalRotation = new Quaternion(0.6469873f, 0.1643085f, 0.7078394f, -0.2310277f);
				compassEntity.LocalPosition = new Vector3(0f, 0.09360941f, 0f);
				break;
			}
			return compassEntity;
		}
	}
}
