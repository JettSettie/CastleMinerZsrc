using DNA.CastleMinerZ.AI;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DNA.CastleMinerZ.Inventory
{
	public class KnifeInventoryItemClass : ModelInventoryItemClass
	{
		public ToolMaterialTypes Material;

		public KnifeInventoryItemClass(InventoryItemIDs id, Model model, ToolMaterialTypes material, string name, string description, float meleeDamage, float itemselfDamage, TimeSpan coolDown)
			: base(id, model, name, description, 1, coolDown, Color.Gray)
		{
			_playerMode = PlayerMode.Pick;
			ItemSelfDamagePerUse = itemselfDamage;
			EnemyDamage = meleeDamage;
			EnemyDamageType = DamageType.BLADE;
			Material = material;
			ToolColor = CMZColors.GetMaterialcColor(Material);
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new KnifeInventoryItem(this, stackCount);
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			CastleMinerToolModel castleMinerToolModel = new CastleMinerToolModel(_model, use, attachedToLocalPlayer);
			castleMinerToolModel.EnablePerPixelLighting();
			castleMinerToolModel.ToolColor = ToolColor;
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, (float)Math.PI / 10f) * Quaternion.CreateFromYawPitchRoll(0f, -(float)Math.PI / 2f, 0f);
				Matrix localToParent = Matrix.Transform(Matrix.CreateScale(35.2f / castleMinerToolModel.GetLocalBoundingSphere().Radius), rotation);
				localToParent.Translation = new Vector3(16f, -19f, -16f);
				castleMinerToolModel.LocalToParent = localToParent;
				castleMinerToolModel.EnableDefaultLighting();
				break;
			}
			}
			return castleMinerToolModel;
		}
	}
}
