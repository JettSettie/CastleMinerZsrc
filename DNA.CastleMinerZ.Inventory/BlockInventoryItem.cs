using DNA.Audio;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.Input;
using DNA.IO;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using System.IO;

namespace DNA.CastleMinerZ.Inventory
{
	public class BlockInventoryItem : InventoryItem
	{
		private Vector3 _pointToLocation = Vector3.Zero;

		private string _customBlockName;

		private InGameHUD _hudReference;

		public Vector3 PointToLocation
		{
			get
			{
				return _pointToLocation;
			}
		}

		public string CustomBlockName
		{
			get
			{
				return _customBlockName;
			}
		}

		public BlockTypeEnum BlockTypeID
		{
			get
			{
				return ((BlockInventoryItemClass)base.ItemClass).BlockType._type;
			}
		}

		public BlockInventoryItem(BlockInventoryItemClass classtype, int stackCount)
			: base(classtype, stackCount)
		{
		}

		public virtual BlockTypeEnum GetConstructedBlockType(BlockFace face, IntVector3 position)
		{
			return BlockTypeID;
		}

		public virtual void AlterBlock(Player player, IntVector3 addSpot, BlockFace inFace)
		{
			AlterBlockMessage.Send((LocalNetworkGamer)player.Gamer, addSpot, GetConstructedBlockType(inFace, addSpot));
		}

		public virtual bool CanPlaceHere(IntVector3 addSpot, BlockFace inFace)
		{
			return true;
		}

		public override void ProcessInput(InGameHUD hud, CastleMinerZControllerMapping controller)
		{
			Trigger use = controller.Use;
			if (base.CoolDownTimer.Expired && controller.Use.Pressed && base.StackCount > 0)
			{
				base.CoolDownTimer.Reset();
				IntVector3 intVector = hud.Build(this, true);
				if (intVector != IntVector3.Zero)
				{
					bool flag = true;
					if (BlockType.IsDoor(BlockTypeID))
					{
						CastleMinerZGame.Instance.CurrentWorld.SetDoor(intVector, DoorEntity.GetModelNameFromInventoryId(base.ItemClass.ID));
					}
					if (BlockTypeID == BlockTypeEnum.SpawnPointBasic)
					{
						PlaceLocator(intVector);
						CastleMinerZGame.Instance.LocalPlayer.SetSpawnPoint(this);
						hud.PlayerInventory.Consume(this, 1);
					}
					else if (BlockTypeID == BlockTypeEnum.TeleportStation)
					{
						_hudReference = hud;
						ShowKeyboard();
						flag = false;
					}
					else
					{
						hud.PlayerInventory.Consume(this, 1);
					}
					if (flag)
					{
						hud.Build(this);
					}
				}
				hud.LocalPlayer.UsingTool = true;
				CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(base.ItemClass.ID);
				itemStats.Used++;
			}
			else
			{
				hud.LocalPlayer.UsingTool = false;
			}
		}

		public void PlaceLocator(InGameHUD hud)
		{
			PlaceLocator(hud.ConstructionProbe._worldIndex + Vector3.Zero);
		}

		public void PlaceLocator(Vector3 location)
		{
			SoundManager.Instance.PlayInstance("locator");
			_pointToLocation = location;
		}

		protected override void Read(BinaryReader reader)
		{
			base.Read(reader);
			if (BlockTypeID == BlockTypeEnum.SpawnPointBasic || BlockTypeID == BlockTypeEnum.TeleportStation)
			{
				_pointToLocation = reader.ReadVector3();
			}
			if (BlockTypeID == BlockTypeEnum.TeleportStation)
			{
				_customBlockName = reader.ReadString();
			}
			BlockTypeEnum inActiveSpawnBlockType = SpawnBlockView.GetInActiveSpawnBlockType(BlockTypeID);
			BlockTypeEnum blockTypeID = BlockTypeID;
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			if (BlockTypeID == BlockTypeEnum.SpawnPointBasic || BlockTypeID == BlockTypeEnum.TeleportStation)
			{
				writer.Write(_pointToLocation);
			}
			if (BlockTypeID == BlockTypeEnum.TeleportStation)
			{
				writer.Write(string.IsNullOrEmpty(_customBlockName) ? "Delta" : _customBlockName);
			}
		}

		protected override void OnKeyboardSubmit()
		{
			string textInput = _keyboardInputScreen.TextInput;
			if (textInput != null)
			{
				if (textInput.Length > 10)
				{
					_customBlockName = textInput.Substring(0, 10);
				}
				else
				{
					_customBlockName = textInput;
				}
			}
			IntVector3 vect = _hudReference.Build(this);
			PlaceLocator(vect);
			CastleMinerZGame.Instance.LocalPlayer.AddTeleportStationObject(this);
			CastleMinerZGame.Instance.LocalPlayer.PlayerInventory.Consume(this, 1, true);
			_hudReference = null;
		}

		protected override void OnKeyboardCancel()
		{
			_hudReference = null;
		}
	}
}
