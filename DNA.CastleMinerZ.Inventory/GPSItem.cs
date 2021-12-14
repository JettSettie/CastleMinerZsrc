using DNA.Audio;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.UI;
using DNA.Drawing.UI;
using DNA.IO;
using DNA.Text;
using Microsoft.Xna.Framework;
using System.IO;
using System.Text;

namespace DNA.CastleMinerZ.Inventory
{
	public class GPSItem : InventoryItem
	{
		private Vector3 _pointToLocation = Vector3.Zero;

		private string _GPSname = "Alpha";

		private new PCKeyboardInputScreen _keyboardInputScreen;

		public Vector3 PointToLocation
		{
			get
			{
				return _pointToLocation;
			}
		}

		public Color color
		{
			get
			{
				switch (GPSClass.ID)
				{
				case InventoryItemIDs.GPS:
					return CMZColors.GetMaterialcColor(ToolMaterialTypes.Gold);
				case InventoryItemIDs.TeleportGPS:
					return CMZColors.GetMaterialcColor(ToolMaterialTypes.BloodStone);
				default:
					return CMZColors.GetMaterialcColor(ToolMaterialTypes.Diamond);
				}
			}
		}

		public GPSItemClass GPSClass
		{
			get
			{
				return (GPSItemClass)base.ItemClass;
			}
		}

		public override void GetDisplayText(StringBuilder builder)
		{
			base.GetDisplayText(builder);
			Vector3 localPosition = CastleMinerZGame.Instance.LocalPlayer.LocalPosition;
			builder.Append(": ");
			builder.Append(_GPSname);
			builder.Append(" - ");
			builder.Append(Strings.Distance);
			builder.Append(": ");
			builder.Concat((int)Vector3.Distance(localPosition, PointToLocation));
		}

		public GPSItem(GPSItemClass classType, int stackCount)
			: base(classType, stackCount)
		{
			_keyboardInputScreen = new PCKeyboardInputScreen(CastleMinerZGame.Instance, Strings.Name, Strings.Enter_A_Name_For_This_Locator, CastleMinerZGame.Instance.DialogScreenImage, CastleMinerZGame.Instance._myriadMed, true, CastleMinerZGame.Instance.ButtonFrame);
			_keyboardInputScreen.ClickSound = "Click";
			_keyboardInputScreen.OpenSound = "Popup";
		}

		protected override void Read(BinaryReader reader)
		{
			base.Read(reader);
			_pointToLocation = reader.ReadVector3();
			_GPSname = reader.ReadString();
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(_pointToLocation);
			writer.Write(_GPSname);
		}

		private void ShowKeyboard()
		{
			_keyboardInputScreen.DefaultText = _GPSname;
			CastleMinerZGame.Instance.GameScreen._uiGroup.ShowPCDialogScreen(_keyboardInputScreen, delegate
			{
				if (_keyboardInputScreen.OptionSelected != -1)
				{
					string textInput = _keyboardInputScreen.TextInput;
					if (textInput != null)
					{
						if (textInput.Length > 10)
						{
							_GPSname = textInput.Substring(0, 10);
						}
						else
						{
							_GPSname = textInput;
						}
					}
				}
			});
		}

		public void PlaceLocator(InGameHUD hud)
		{
			SoundManager.Instance.PlayInstance("locator");
			_pointToLocation = hud.ConstructionProbe._worldIndex + Vector3.Zero;
		}

		public override void ProcessInput(InGameHUD hud, CastleMinerZControllerMapping controller)
		{
			if (controller.Reload.Pressed)
			{
				ShowKeyboard();
				return;
			}
			switch (GPSClass.ID)
			{
			case InventoryItemIDs.GPS:
				if (controller.Use.Pressed)
				{
					PlaceLocator(hud);
					if (InflictDamage())
					{
						hud.PlayerInventory.Remove(this);
					}
					else
					{
						ShowKeyboard();
					}
				}
				break;
			case InventoryItemIDs.SpawnBasic:
				if (controller.Use.Pressed)
				{
					PlaceLocator(hud);
					hud.PlayerInventory.Remove(this);
				}
				else if (controller.Shoulder.Pressed)
				{
					if (_pointToLocation == Vector3.Zero)
					{
						SoundManager.Instance.PlayInstance("Error");
						break;
					}
					SoundManager.Instance.PlayInstance("Teleport");
					string message2 = CastleMinerZGame.Instance.MyNetworkGamer.Gamertag + " " + Strings.Teleported_To + " " + _GPSname;
					BroadcastTextMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, message2);
					CastleMinerZGame.Instance.GameScreen.TeleportToLocation(_pointToLocation, false);
				}
				break;
			case InventoryItemIDs.TeleportGPS:
				if (controller.Use.Pressed)
				{
					PlaceLocator(hud);
					ShowKeyboard();
				}
				else
				{
					if (!controller.Shoulder.Pressed)
					{
						break;
					}
					if (_pointToLocation == Vector3.Zero)
					{
						SoundManager.Instance.PlayInstance("Error");
						break;
					}
					SoundManager.Instance.PlayInstance("Teleport");
					string message = CastleMinerZGame.Instance.MyNetworkGamer.Gamertag + " " + Strings.Teleported_To + " " + _GPSname;
					BroadcastTextMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, message);
					CastleMinerZGame.Instance.GameScreen.TeleportToLocation(_pointToLocation, false);
					if (InflictDamage())
					{
						hud.PlayerInventory.Remove(this);
					}
				}
				break;
			}
		}
	}
}
