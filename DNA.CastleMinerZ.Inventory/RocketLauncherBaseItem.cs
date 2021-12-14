using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.UI;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Inventory
{
	public class RocketLauncherBaseItem : GunInventoryItem
	{
		protected bool _deleteMe;

		public RocketLauncherBaseItem(RocketLauncherBaseInventoryItemClass cls, int stackCount)
			: base(cls, stackCount)
		{
		}

		public override bool InflictDamage()
		{
			if (base.StackCount <= 1)
			{
				_deleteMe = true;
				ChangeCarriedItemMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, InventoryItemIDs.RocketLauncherShotFired);
			}
			else
			{
				CastleMinerZGame.Instance.LocalPlayer.PlayerInventory.Consume(this, 1, true);
				base.RoundsInClip++;
			}
			return false;
		}

		public override void ProcessInput(InGameHUD hud, CastleMinerZControllerMapping controller)
		{
			if (_deleteMe && !hud.LocalPlayer.UsingAnimationPlaying)
			{
				hud.PlayerInventory.Remove(this);
			}
			else
			{
				base.ProcessInput(hud, controller);
			}
		}
	}
}
