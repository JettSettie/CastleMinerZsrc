using DNA.Input;

namespace DNA.CastleMinerZ
{
	public class CastleMinerZControllerMapping : FPSControllerMapping
	{
		public enum CMZControllerFunctions
		{
			Use,
			Jump,
			MoveForward,
			MoveBackward,
			StrafeLeft,
			StrafeRight,
			Sprint,
			Shoulder_Activate,
			Shoulder,
			Activate,
			Reload,
			NextItem,
			PreviousItem,
			BlockUI,
			PlayersScreen,
			FlyMode,
			TextChat,
			DropQuickBarItem,
			SwitchTray,
			Tray1,
			Tray2,
			UseItem1,
			UseItem2,
			UseItem3,
			UseItem4,
			UseItem5,
			UseItem6,
			UseItem7,
			UseItem8,
			AltUseItem1,
			AltUseItem2,
			AltUseItem3,
			AltUseItem4,
			AltUseItem5,
			AltUseItem6,
			AltUseItem7,
			AltUseItem8,
			Count
		}

		public Trigger Shoulder;

		public Trigger Use;

		public Trigger Activate;

		public new Trigger Sprint;

		public Trigger Reload;

		public Trigger NextItem;

		public Trigger PrevoiusItem;

		public Trigger BlockUI;

		public Trigger PlayersScreen;

		public Trigger FlyMode;

		public Trigger TextChat;

		public Trigger DropQuickbarItem;

		public Trigger SwitchTray;

		public Trigger Slot1;

		public Trigger Slot2;

		public Trigger Slot3;

		public Trigger Slot4;

		public Trigger Slot5;

		public Trigger Slot6;

		public Trigger Slot7;

		public Trigger Slot8;

		public CastleMinerZControllerMapping()
		{
			base.Binding.AvoidShiftTab = true;
		}

		public override void SetToDefault()
		{
			base.SetToDefault();
			base.Binding.Bind(7, InputBinding.Bindable.MouseButtonRight, InputBinding.Bindable.None, InputBinding.Bindable.ButtonLTrigger);
			base.Binding.Bind(9, InputBinding.Bindable.KeyEnter, InputBinding.Bindable.None, InputBinding.Bindable.ButtonB);
			base.Binding.Bind(10, InputBinding.Bindable.KeyR, InputBinding.Bindable.None, InputBinding.Bindable.ButtonX);
			base.Binding.Bind(11, InputBinding.Bindable.MouseWheelDown, InputBinding.Bindable.None, InputBinding.Bindable.ButtonRShldr);
			base.Binding.Bind(12, InputBinding.Bindable.MouseWheelUp, InputBinding.Bindable.None, InputBinding.Bindable.ButtonLShldr);
			base.Binding.Bind(13, InputBinding.Bindable.KeyE, InputBinding.Bindable.None, InputBinding.Bindable.ButtonY);
			base.Binding.Bind(14, InputBinding.Bindable.KeyTab, InputBinding.Bindable.None, InputBinding.Bindable.ButtonBack);
			base.Binding.Bind(15, InputBinding.Bindable.KeyF, InputBinding.Bindable.None, InputBinding.Bindable.ButtonRStick);
			base.Binding.Bind(16, InputBinding.Slot.KeyMouse1, InputBinding.Bindable.KeyT);
			base.Binding.Bind(17, InputBinding.Bindable.KeyQ, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadDown);
			base.Binding.Bind(18, InputBinding.Bindable.KeyG, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			base.Binding.Bind(21, InputBinding.Bindable.KeyD1, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			base.Binding.Bind(22, InputBinding.Bindable.KeyD2, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			base.Binding.Bind(23, InputBinding.Bindable.KeyD3, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			base.Binding.Bind(24, InputBinding.Bindable.KeyD4, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			base.Binding.Bind(25, InputBinding.Bindable.KeyD5, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			base.Binding.Bind(26, InputBinding.Bindable.KeyD6, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			base.Binding.Bind(27, InputBinding.Bindable.KeyD7, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			base.Binding.Bind(28, InputBinding.Bindable.KeyD8, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			SetTrayDefaultKeys();
		}

		public void SetTrayDefaultKeys()
		{
			base.Binding.Bind(18, InputBinding.Bindable.KeyG, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			base.Binding.Bind(21, InputBinding.Bindable.KeyD1, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			base.Binding.Bind(22, InputBinding.Bindable.KeyD2, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			base.Binding.Bind(23, InputBinding.Bindable.KeyD3, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			base.Binding.Bind(24, InputBinding.Bindable.KeyD4, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			base.Binding.Bind(25, InputBinding.Bindable.KeyD5, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			base.Binding.Bind(26, InputBinding.Bindable.KeyD6, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			base.Binding.Bind(27, InputBinding.Bindable.KeyD7, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
			base.Binding.Bind(28, InputBinding.Bindable.KeyD8, InputBinding.Bindable.None, InputBinding.Bindable.ButtonDPadUp);
		}

		public override void ClearAllControls()
		{
			Trigger[] array = new Trigger[20]
			{
				PrevoiusItem,
				NextItem,
				DropQuickbarItem,
				BlockUI,
				Activate,
				Shoulder,
				Use,
				Reload,
				PlayersScreen,
				FlyMode,
				TextChat,
				SwitchTray,
				Slot1,
				Slot2,
				Slot3,
				Slot4,
				Slot5,
				Slot6,
				Slot7,
				Slot8
			};
			for (int i = 0; i < array.Length; i++)
			{
				Trigger trigger = array[i];
			}
			base.ClearAllControls();
		}

		public override void ProcessInput(KeyboardInput keyboard, MouseInput mouse, GameController controller)
		{
			if (!base.Binding.Initialized)
			{
				SetToDefault();
			}
			PrevoiusItem = base.Binding.GetFunction(12, keyboard, mouse, controller);
			NextItem = base.Binding.GetFunction(11, keyboard, mouse, controller);
			BlockUI = base.Binding.GetFunction(13, keyboard, mouse, controller);
			Activate = (base.Binding.GetFunction(9, keyboard, mouse, controller) | base.Binding.GetFunction(7, keyboard, mouse, controller));
			Shoulder = (base.Binding.GetFunction(8, keyboard, mouse, controller) | base.Binding.GetFunction(7, keyboard, mouse, controller));
			Reload = base.Binding.GetFunction(10, keyboard, mouse, controller);
			PlayersScreen = base.Binding.GetFunction(14, keyboard, mouse, controller);
			FlyMode = base.Binding.GetFunction(15, keyboard, mouse, controller);
			Use = base.Binding.GetFunction(0, keyboard, mouse, controller);
			TextChat = base.Binding.GetFunction(16, keyboard, mouse, controller);
			DropQuickbarItem = base.Binding.GetFunction(17, keyboard, mouse, controller);
			SwitchTray = base.Binding.GetFunction(18, keyboard, mouse, controller);
			Slot1 = base.Binding.GetFunction(21, keyboard, mouse, controller);
			Slot2 = base.Binding.GetFunction(22, keyboard, mouse, controller);
			Slot3 = base.Binding.GetFunction(23, keyboard, mouse, controller);
			Slot4 = base.Binding.GetFunction(24, keyboard, mouse, controller);
			Slot5 = base.Binding.GetFunction(25, keyboard, mouse, controller);
			Slot6 = base.Binding.GetFunction(26, keyboard, mouse, controller);
			Slot7 = base.Binding.GetFunction(27, keyboard, mouse, controller);
			Slot8 = base.Binding.GetFunction(28, keyboard, mouse, controller);
			base.ProcessInput(keyboard, mouse, controller);
		}
	}
}
