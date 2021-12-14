using DNA.Audio;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.Drawing;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DNA.CastleMinerZ
{
	public class PickupManager : Entity
	{
		private static WeakReference<PickupManager> _instance = new WeakReference<PickupManager>(null);

		private int _nextPickupID;

		public List<PickupEntity> Pickups = new List<PickupEntity>();

		public List<PickupEntity> PendingPickupList = new List<PickupEntity>();

		public static PickupManager Instance
		{
			get
			{
				return _instance.Target;
			}
		}

		public PickupManager()
		{
			Visible = false;
			Collidee = false;
			Collider = false;
			_instance.Target = this;
		}

		public void HandleMessage(CastleMinerZMessage message)
		{
			if (message is CreatePickupMessage)
			{
				HandleCreatePickupMessage((CreatePickupMessage)message);
			}
			else if (message is ConsumePickupMessage)
			{
				HandleConsumePickupMessage((ConsumePickupMessage)message);
			}
			else if (message is RequestPickupMessage)
			{
				HandleRequestPickupMessage((RequestPickupMessage)message);
			}
		}

		public void CreateUpwardPickup(InventoryItem item, Vector3 location, float vel, bool displayOnPickup = false)
		{
			Vector3 vec = new Vector3(MathTools.RandomFloat(-0.5f, 0.501f), 0.1f, MathTools.RandomFloat(-0.5f, 0.501f));
			vec.Normalize();
			vec *= vel;
			CreatePickupMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, location, vec, _nextPickupID++, item, false, displayOnPickup);
		}

		public void CreatePickup(InventoryItem item, Vector3 location, bool dropped, bool displayOnPickup = false)
		{
			float num = 0f;
			if (dropped)
			{
				Player localPlayer = CastleMinerZGame.Instance.LocalPlayer;
				Matrix localToWorld = localPlayer.FPSCamera.LocalToWorld;
				Vector3 forward = localToWorld.Forward;
				forward.Y = 0f;
				forward.Normalize();
				forward.Y = 0.1f;
				forward += localToWorld.Left * (MathTools.RandomFloat() * 0.25f - 0.12f);
				num = 4f;
				forward.Normalize();
				forward *= num;
				CreatePickupMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, location, forward, _nextPickupID++, item, dropped, displayOnPickup);
			}
			else
			{
				CreateUpwardPickup(item, location, 1.5f, displayOnPickup);
			}
		}

		public void PlayerTouchedPickup(PickupEntity pickup)
		{
			RequestPickupMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, pickup.SpawnerID, pickup.PickupID);
		}

		private void HandleCreatePickupMessage(CreatePickupMessage msg)
		{
			int id = msg.Sender.Id;
			PickupEntity pickupEntity = new PickupEntity(msg.Item, msg.PickupID, id, msg.Dropped, msg.SpawnPosition);
			pickupEntity.Item.DisplayOnPickup = msg.DisplayOnPickup;
			pickupEntity.PlayerPhysics.LocalVelocity = msg.SpawnVector;
			pickupEntity.LocalPosition = msg.SpawnPosition + new Vector3(0.5f, 0.5f, 0.5f);
			Pickups.Add(pickupEntity);
			Scene scene = base.Scene;
			if (scene != null && scene.Children != null)
			{
				scene.Children.Add(pickupEntity);
			}
		}

		public void RemovePickup(PickupEntity pe)
		{
			Pickups.Remove(pe);
			pe.RemoveFromParent();
			if (CastleMinerZGame.Instance.IsGameHost)
			{
				PendingPickupList.Remove(pe);
			}
		}

		private void HandleRequestPickupMessage(RequestPickupMessage msg)
		{
			if (!CastleMinerZGame.Instance.IsGameHost)
			{
				return;
			}
			int num = 0;
			PickupEntity pickupEntity;
			while (true)
			{
				if (num >= Pickups.Count)
				{
					return;
				}
				if (Pickups[num].PickupID == msg.PickupID && Pickups[num].SpawnerID == msg.SpawnerID)
				{
					pickupEntity = Pickups[num];
					if (!PendingPickupList.Contains(pickupEntity))
					{
						break;
					}
				}
				num++;
			}
			PendingPickupList.Add(pickupEntity);
			ConsumePickupMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, msg.Sender.Id, pickupEntity.GetActualGraphicPos(), pickupEntity.SpawnerID, pickupEntity.PickupID, pickupEntity.Item, pickupEntity.Item.DisplayOnPickup);
		}

		private void HandleConsumePickupMessage(ConsumePickupMessage msg)
		{
			Vector3 zero = Vector3.Zero;
			PickupEntity pickupEntity = null;
			Player player = null;
			if (CastleMinerZGame.Instance.CurrentNetworkSession != null)
			{
				for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
				{
					NetworkGamer networkGamer = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
					if (networkGamer != null && networkGamer.Id == msg.PickerUpper)
					{
						Player player2 = (Player)networkGamer.Tag;
						if (player2 != null)
						{
							player = player2;
						}
					}
				}
			}
			for (int j = 0; j < Pickups.Count; j++)
			{
				if (Pickups[j].PickupID == msg.PickupID && Pickups[j].SpawnerID == msg.SpawnerID)
				{
					pickupEntity = Pickups[j];
					RemovePickup(pickupEntity);
				}
			}
			zero = ((pickupEntity == null) ? msg.PickupPosition : pickupEntity.GetActualGraphicPos());
			if (player != null)
			{
				if (player == CastleMinerZGame.Instance.LocalPlayer)
				{
					CastleMinerZGame.Instance.GameScreen.HUD.PlayerInventory.AddInventoryItem(msg.Item, msg.DisplayOnPickup);
					SoundManager.Instance.PlayInstance("pickupitem");
				}
				FlyingPickupEntity t = new FlyingPickupEntity(msg.Item, player, zero);
				Scene scene = base.Scene;
				if (scene != null && scene.Children != null)
				{
					scene.Children.Add(t);
				}
			}
		}
	}
}
