using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.UI;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Inventory
{
	public class GrenadeLauncherBaseItem : GunInventoryItem
	{
		protected bool _deleteMe;

		public GrenadeLauncherBaseItem(GrenadeLauncherBaseInventoryItemClass cls, int stackCount)
			: base(cls, stackCount)
		{
		}

		public override bool InflictDamage()
		{
			ChangeCarriedItemMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, InventoryItemIDs.RocketLauncherShotFired);
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
