using DNA.Audio;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.UI;
using DNA.Drawing.UI;
using DNA.IO.Storage;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace DNA.CastleMinerZ.Inventory
{
	public class PlayerInventory
	{
		private enum PlayerInventoryVersion
		{
			RespawnBeacon = 1,
			TeleportStations,
			MultiTray,
			CurrentVersion
		}

		public const int MaxTrayItems = 8;

		public const string Ident = "PINV";

		public InventoryItem[] Inventory = new InventoryItem[32];

		public InventoryTrayManager TrayManager = new InventoryTrayManager();

		public List<Recipe> DiscoveredRecipies = new List<Recipe>();

		public BlockInventoryItem InventorySpawnPointTeleport;

		public List<BlockInventoryItem> TeleportStationObjects = new List<BlockInventoryItem>();

		public int SelectedInventoryIndex;

		private PCListSelectScreen _teleportSelectionScreen;

		private Player _player;

		private InventoryItem _bareHands = InventoryItem.CreateItem(InventoryItemIDs.BareHands, 1);

		public int Version
		{
			get
			{
				return 4;
			}
		}

		public GameScreen GameScreen
		{
			get
			{
				return CastleMinerZGame.Instance.GameScreen;
			}
		}

		public Player Player
		{
			set
			{
				_player = value;
			}
		}

		public InventoryItem ActiveInventoryItem
		{
			get
			{
				InventoryItem itemFromCurrentTray = TrayManager.GetItemFromCurrentTray(SelectedInventoryIndex);
				if (itemFromCurrentTray == null)
				{
					return _bareHands;
				}
				return itemFromCurrentTray;
			}
		}

		public void SaveToStorage(SaveDevice saveDevice, string path)
		{
			try
			{
				string directoryName = Path.GetDirectoryName(path);
				if (!saveDevice.DirectoryExists(directoryName))
				{
					saveDevice.CreateDirectory(directoryName);
				}
				saveDevice.Save(path, true, true, delegate(Stream stream)
				{
					BinaryWriter binaryWriter = new BinaryWriter(stream);
					Save(binaryWriter);
					binaryWriter.Flush();
				});
			}
			catch
			{
			}
		}

		public void LoadFromStorage(SaveDevice saveDevice, string path)
		{
			saveDevice.Load(path, delegate(Stream stream)
			{
				BinaryReader reader = new BinaryReader(stream);
				Load(reader);
			});
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write("PINV");
			writer.Write(Version);
			for (int i = 0; i < Inventory.Length; i++)
			{
				if (Inventory[i] == null)
				{
					writer.Write(false);
					continue;
				}
				writer.Write(true);
				Inventory[i].Write(writer);
			}
			TrayManager.Write(writer);
			writer.Write(TeleportStationObjects.Count);
			for (int j = 0; j < TeleportStationObjects.Count; j++)
			{
				if (TeleportStationObjects[j] == null)
				{
					writer.Write(false);
					continue;
				}
				writer.Write(true);
				TeleportStationObjects[j].Write(writer);
			}
			if (InventorySpawnPointTeleport != null)
			{
				writer.Write(true);
				InventorySpawnPointTeleport.Write(writer);
			}
			else
			{
				writer.Write(false);
			}
		}

		private PlayerInventory()
		{
		}

		public void Load(BinaryReader reader)
		{
			if (reader.ReadString() != "PINV")
			{
				throw new Exception("Invalid Inv File");
			}
			int num = reader.ReadInt32();
			PlayerInventoryVersion playerInventoryVersion = (PlayerInventoryVersion)num;
			if (num < 0 || playerInventoryVersion > PlayerInventoryVersion.CurrentVersion)
			{
				return;
			}
			for (int i = 0; i < Inventory.Length; i++)
			{
				if (reader.ReadBoolean())
				{
					Inventory[i] = InventoryItem.Create(reader);
					if (Inventory[i] != null && !Inventory[i].IsValid())
					{
						Inventory[i] = null;
					}
				}
				else
				{
					Inventory[i] = null;
				}
			}
			if (playerInventoryVersion > PlayerInventoryVersion.MultiTray)
			{
				TrayManager.Read(reader);
			}
			else
			{
				TrayManager.ReadLegacy(reader);
			}
			if (playerInventoryVersion > PlayerInventoryVersion.TeleportStations)
			{
				int num2 = reader.ReadInt32();
				for (int j = 0; j < num2; j++)
				{
					if (reader.ReadBoolean())
					{
						BlockInventoryItem blockInventoryItem = InventoryItem.Create(reader) as BlockInventoryItem;
						TeleportStationObjects.Add(blockInventoryItem);
						if (TeleportStationObjects[j] != null && !TeleportStationObjects[j].IsValid())
						{
							TeleportStationObjects[j] = null;
						}
						else
						{
							AlterBlockMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, IntVector3.FromVector3(blockInventoryItem.PointToLocation), blockInventoryItem.BlockTypeID);
						}
					}
				}
			}
			if (playerInventoryVersion > PlayerInventoryVersion.RespawnBeacon)
			{
				if (reader.ReadBoolean())
				{
					InventorySpawnPointTeleport = (InventoryItem.Create(reader) as BlockInventoryItem);
					if (InventorySpawnPointTeleport != null && !InventorySpawnPointTeleport.IsValid())
					{
						InventorySpawnPointTeleport = null;
					}
				}
				else
				{
					InventorySpawnPointTeleport = null;
				}
			}
			DiscoverRecipies();
		}

		public void DiscoverRecipies()
		{
			DiscoveredRecipies.Clear();
			LinkedList<Recipe> linkedList = new LinkedList<Recipe>();
			Dictionary<Recipe, bool> dictionary = new Dictionary<Recipe, bool>();
			foreach (Recipe item in Recipe.CookBook)
			{
				if (Discovered(item) && CanCraft(item))
				{
					DiscoveredRecipies.Add(item);
					linkedList.AddLast(item);
					dictionary[item] = true;
				}
			}
			foreach (Recipe item2 in Recipe.CookBook)
			{
				if (Discovered(item2) && !CanCraft(item2))
				{
					DiscoveredRecipies.Add(item2);
					linkedList.AddLast(item2);
					dictionary[item2] = true;
				}
			}
			for (LinkedListNode<Recipe> linkedListNode = linkedList.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				foreach (InventoryItem ingredient in linkedListNode.Value.Ingredients)
				{
					foreach (Recipe item3 in Recipe.CookBook)
					{
						if (item3.Result.ItemClass == ingredient.ItemClass && !dictionary.ContainsKey(item3))
						{
							dictionary[item3] = true;
							linkedList.AddLast(item3);
							DiscoveredRecipies.Add(item3);
						}
					}
				}
			}
			if (DiscoveredRecipies.Count == 0)
			{
				DiscoveredRecipies.Add(Recipe.CookBook[0]);
			}
		}

		public bool Discovered(Recipe recipe)
		{
			bool flag = false;
			for (int i = 0; i < Inventory.Length; i++)
			{
				InventoryItem item = Inventory[i];
				if (DoesItemUnlockRecipe(item, recipe))
				{
					return true;
				}
			}
			int upperBound = TrayManager.Trays.GetUpperBound(0);
			int upperBound2 = TrayManager.Trays.GetUpperBound(1);
			for (int j = 0; j <= upperBound; j++)
			{
				for (int k = 0; k <= upperBound2; k++)
				{
					InventoryItem item2 = TrayManager.Trays[j, k];
					if (DoesItemUnlockRecipe(item2, recipe))
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool DoesItemUnlockRecipe(InventoryItem item, Recipe recipe)
		{
			if (item != null)
			{
				if (recipe.Result.ItemClass == item.ItemClass)
				{
					return true;
				}
				if (item.ItemClass is GunInventoryItemClass)
				{
					GunInventoryItemClass gunInventoryItemClass = (GunInventoryItemClass)item.ItemClass;
					if (recipe.Result.ItemClass == gunInventoryItemClass.AmmoType)
					{
						return true;
					}
				}
				for (int i = 0; i < recipe.Ingredients.Count; i++)
				{
					if (recipe.Ingredients[i].ItemClass == item.ItemClass)
					{
						return true;
					}
				}
			}
			return false;
		}

		public int CountItems(InventoryItem.InventoryItemClass itemClass)
		{
			int num = 0;
			for (int i = 0; i < Inventory.Length; i++)
			{
				if (Inventory[i] != null && itemClass == Inventory[i].ItemClass)
				{
					num += Inventory[i].StackCount;
				}
			}
			return num + TrayManager.GetItemClassCount(itemClass);
		}

		public bool CanCraft(Recipe recipe)
		{
			if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Creative)
			{
				return true;
			}
			for (int i = 0; i < recipe.Ingredients.Count; i++)
			{
				int num = CountItems(recipe.Ingredients[i].ItemClass);
				if (num < recipe.Ingredients[i].StackCount)
				{
					return false;
				}
			}
			return true;
		}

		public void Craft(Recipe recipe)
		{
			if (CastleMinerZGame.Instance.InfiniteResourceMode)
			{
				InventoryItem item = recipe.Result.ItemClass.CreateItem(recipe.Result.StackCount);
				AddInventoryItem(item);
				return;
			}
			for (int i = 0; i < recipe.Ingredients.Count; i++)
			{
				InventoryItem inventoryItem = recipe.Ingredients[i];
				int required = inventoryItem.StackCount;
				for (int j = 0; j < Inventory.Length; j++)
				{
					InventoryItem item2 = Inventory[j];
					DeductFromItemStack(ref item2, ref required, inventoryItem.ItemClass);
					if (item2 == null)
					{
						Inventory[j] = null;
					}
				}
				int upperBound = TrayManager.Trays.GetUpperBound(0);
				int upperBound2 = TrayManager.Trays.GetUpperBound(1);
				for (int k = 0; k <= upperBound; k++)
				{
					for (int l = 0; l <= upperBound2; l++)
					{
						InventoryItem item2 = TrayManager.Trays[k, l];
						DeductFromItemStack(ref item2, ref required, inventoryItem.ItemClass);
						if (item2 == null)
						{
							TrayManager.Trays[k, l] = null;
						}
					}
				}
			}
			InventoryItem item3 = recipe.Result.ItemClass.CreateItem(recipe.Result.StackCount);
			AddInventoryItem(item3);
		}

		private void DeductFromItemStack(ref InventoryItem item, ref int required, InventoryItem.InventoryItemClass itemClass)
		{
			if (item != null && item.ItemClass == itemClass)
			{
				if (required < item.StackCount)
				{
					item.StackCount -= required;
					required = 0;
				}
				else
				{
					required -= item.StackCount;
					item = null;
				}
			}
		}

		public void Remove(InventoryItem item)
		{
			if (TrayManager.RemoveItem(item))
			{
				return;
			}
			for (int i = 0; i < Inventory.Length; i++)
			{
				if (Inventory[i] == item)
				{
					Inventory[i] = null;
				}
			}
		}

		public bool CanConsume(InventoryItem.InventoryItemClass itemType, int amount)
		{
			if (itemType == null)
			{
				return true;
			}
			if (TrayManager.CanConsume(itemType, amount))
			{
				return true;
			}
			for (int i = 0; i < Inventory.Length; i++)
			{
				if (Inventory[i] != null && Inventory[i].CanConsume(itemType, amount))
				{
					return true;
				}
			}
			return false;
		}

		public bool Consume(InventoryItem item, int amount, bool ignoreInfiniteResources = false)
		{
			if (CastleMinerZGame.Instance.InfiniteResourceMode && !ignoreInfiniteResources)
			{
				return true;
			}
			if (item.StackCount >= amount)
			{
				item.StackCount -= amount;
				RemoveEmptyItems();
				return true;
			}
			return false;
		}

		public bool Consume(InventoryItem.InventoryItemClass itemClass, int amount)
		{
			if (CastleMinerZGame.Instance.InfiniteResourceMode)
			{
				return true;
			}
			if (itemClass == null)
			{
				return true;
			}
			InventoryItem item = null;
			int num = 0;
			while (num < int.MaxValue && amount > 0)
			{
				num = int.MaxValue;
				int upperBound = TrayManager.Trays.GetUpperBound(0);
				int upperBound2 = TrayManager.Trays.GetUpperBound(1);
				bool flag = false;
				for (int i = 0; i <= upperBound; i++)
				{
					for (int j = 0; j <= upperBound2; j++)
					{
						item = TrayManager.Trays[i, j];
						if (item != null && item.ItemClass == itemClass && item.StackCount < num)
						{
							num = item.StackCount;
							flag = true;
							break;
						}
						item = null;
					}
					if (flag)
					{
						break;
					}
				}
				DeductFromItemStack(ref item, ref amount, itemClass);
			}
			num = 0;
			while (num < int.MaxValue && amount > 0)
			{
				num = int.MaxValue;
				for (int k = 0; k < Inventory.Length; k++)
				{
					item = Inventory[k];
					if (item != null && item.ItemClass == itemClass && item.StackCount < num)
					{
						num = item.StackCount;
						break;
					}
					item = null;
				}
				DeductFromItemStack(ref item, ref amount, itemClass);
			}
			return amount <= 0;
		}

		public void RemoveEmptyItems()
		{
			TrayManager.RemoveAllItems(true);
			for (int i = 0; i < Inventory.Length; i++)
			{
				if (Inventory[i] != null && Inventory[i].StackCount <= 0)
				{
					Inventory[i] = null;
				}
			}
		}

		public bool CanAdd(InventoryItem item)
		{
			if (TrayManager.CanAdd(item))
			{
				return true;
			}
			for (int i = 0; i < Inventory.Length; i++)
			{
				if (Inventory[i] == null || Inventory[i].CanStack(item))
				{
					return true;
				}
			}
			return false;
		}

		public int AddItemToTray(InventoryItem item)
		{
			return TrayManager.AddItemToTray(item);
		}

		public int AddItemToInventory(InventoryItem item)
		{
			for (int i = 0; i < Inventory.Length; i++)
			{
				if (Inventory[i] != null)
				{
					Inventory[i].Stack(item);
				}
			}
			if (item.StackCount <= 0)
			{
				return 0;
			}
			for (int j = 0; j < Inventory.Length; j++)
			{
				if (Inventory[j] == null)
				{
					Inventory[j] = item;
					return 0;
				}
			}
			return item.StackCount;
		}

		public void AddInventoryItem(InventoryItem item, bool displayOnPickup = false)
		{
			if (displayOnPickup)
			{
				if (item.StackCount > 1)
				{
					Console.WriteLine("You looted: " + item.Name + " (" + item.StackCount + ")");
				}
				else
				{
					Console.WriteLine("You looted: " + item.Name);
				}
			}
			TrayManager.AddItemToExisting(item);
			for (int i = 0; i < Inventory.Length; i++)
			{
				if (Inventory[i] != null)
				{
					Inventory[i].Stack(item);
				}
			}
			if (item.StackCount <= 0)
			{
				DiscoverRecipies();
				return;
			}
			if (TrayManager.AddItemToEmpty(item))
			{
				DiscoverRecipies();
				return;
			}
			for (int j = 0; j < Inventory.Length; j++)
			{
				if (Inventory[j] == null)
				{
					Inventory[j] = item;
					DiscoverRecipies();
					return;
				}
			}
			if (item.StackCount > 0)
			{
				CreateAndPlacePickup(item, _player.LocalPosition);
			}
		}

		protected void InitSelectionScreen(BlockInventoryItem teleportStation)
		{
			_teleportSelectionScreen = new PCListSelectScreen(CastleMinerZGame.Instance, "Teleport Station: " + teleportStation.CustomBlockName, "Select a destination teleport:", CastleMinerZGame.Instance.DialogScreenImage, CastleMinerZGame.Instance._myriadMed, true, CastleMinerZGame.Instance.ButtonFrame);
			_teleportSelectionScreen.ClickSound = "Click";
			_teleportSelectionScreen.OpenSound = "Popup";
		}

		public void ShowTeleportStationMenu(Vector3 worldIndex)
		{
			BlockInventoryItem teleportAtWorldIndex = GetTeleportAtWorldIndex(worldIndex);
			BlockInventoryItem blockInventoryItem = null;
			List<string> list = new List<string>();
			if (teleportAtWorldIndex == null)
			{
				return;
			}
			int sourceIndex = -1;
			for (int i = 0; i < TeleportStationObjects.Count; i++)
			{
				blockInventoryItem = TeleportStationObjects[i];
				if (blockInventoryItem == teleportAtWorldIndex)
				{
					sourceIndex = i;
				}
				list.Add(blockInventoryItem.CustomBlockName);
			}
			InitSelectionScreen(teleportAtWorldIndex);
			_teleportSelectionScreen.Init(sourceIndex, list);
			CastleMinerZGame.Instance.GameScreen._uiGroup.ShowPCDialogScreen(_teleportSelectionScreen, delegate
			{
				if (_teleportSelectionScreen.OptionSelected != -1)
				{
					SoundManager.Instance.PlayInstance("Teleport");
					CastleMinerZGame.Instance.GameScreen.TeleportToLocation(TeleportStationObjects[_teleportSelectionScreen.OptionSelected].PointToLocation, false);
				}
			});
		}

		public BlockInventoryItem GetTeleportAtWorldIndex(Vector3 worldIndex)
		{
			BlockInventoryItem blockInventoryItem = null;
			for (int i = 0; i < TeleportStationObjects.Count; i++)
			{
				blockInventoryItem = TeleportStationObjects[i];
				if (blockInventoryItem.PointToLocation == worldIndex)
				{
					return blockInventoryItem;
				}
			}
			return null;
		}

		public void DropOneSelectedTrayItem()
		{
			InventoryItem itemFromCurrentTray = TrayManager.GetItemFromCurrentTray(SelectedInventoryIndex);
			if (itemFromCurrentTray != null)
			{
				SoundManager.Instance.PlayInstance("dropitem");
				if (itemFromCurrentTray.StackCount == 1)
				{
					CreateAndPlacePickup(itemFromCurrentTray, _player.LocalPosition);
					TrayManager.RemoveItem(itemFromCurrentTray);
				}
				else
				{
					InventoryItem item = itemFromCurrentTray.PopOneItem();
					CreateAndPlacePickup(item, _player.LocalPosition);
				}
			}
		}

		private void CreateAndPlacePickup(InventoryItem item, Vector3 location)
		{
			location.Y += 1f;
			PickupManager.Instance.CreatePickup(item, location, true);
		}

		public void DropAll(bool dropTray)
		{
			Vector3 localPosition = _player.LocalPosition;
			localPosition.Y += 1f;
			if (dropTray)
			{
				int upperBound = TrayManager.Trays.GetUpperBound(0);
				int upperBound2 = TrayManager.Trays.GetUpperBound(1);
				for (int i = 0; i <= upperBound; i++)
				{
					for (int j = 0; j <= upperBound2; j++)
					{
						InventoryItem inventoryItem = TrayManager.Trays[i, j];
						if (inventoryItem != null)
						{
							PickupManager.Instance.CreatePickup(inventoryItem, localPosition, true);
							TrayManager.Trays[i, j] = null;
						}
					}
				}
			}
			for (int k = 0; k < Inventory.Length; k++)
			{
				if (Inventory[k] != null)
				{
					PickupManager.Instance.CreatePickup(Inventory[k], localPosition, true);
					Inventory[k] = null;
				}
			}
			DiscoverRecipies();
		}

		public void DropItem(InventoryItem item)
		{
			if (TrayManager.RemoveItem(item))
			{
				CreateAndPlacePickup(item, _player.LocalPosition);
				SoundManager.Instance.PlayInstance("dropitem");
				DiscoverRecipies();
				return;
			}
			int num = 0;
			while (true)
			{
				if (num < Inventory.Length)
				{
					if (Inventory[num] == item)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			Inventory[num] = null;
			CreateAndPlacePickup(item, _player.LocalPosition);
			SoundManager.Instance.PlayInstance("dropitem");
			DiscoverRecipies();
		}

		public PlayerInventory(Player player, bool setDefault)
		{
			_player = player;
			if (setDefault)
			{
				SetDefaultInventory();
			}
		}

		public void SetDefaultInventory()
		{
			for (int i = 0; i < Inventory.Length; i++)
			{
				Inventory[i] = null;
			}
			TrayManager.RemoveAllItems();
			if (CastleMinerZGame.Instance.Difficulty != GameDifficultyTypes.HARDCORE)
			{
				AddInventoryItem(InventoryItem.CreateItem(InventoryItemIDs.StonePickAxe, 1));
				AddInventoryItem(InventoryItem.CreateItem(InventoryItemIDs.Compass, 1));
				AddInventoryItem(InventoryItem.CreateItem(InventoryItemIDs.Pistol, 1));
				AddInventoryItem(InventoryItem.CreateItem(InventoryItemIDs.Knife, 1));
				AddInventoryItem(InventoryItem.CreateItem(InventoryItemIDs.Bullets, 200));
				AddInventoryItem(InventoryItem.CreateItem(InventoryItemIDs.Torch, 16));
			}
			DiscoverRecipies();
		}

		public void Update(GameTime gameTime)
		{
			_bareHands.Update(gameTime);
			for (int i = 0; i < Inventory.Length; i++)
			{
				if (Inventory[i] != null)
				{
					Inventory[i].Update(gameTime);
				}
			}
			TrayManager.Update(gameTime);
		}

		internal void SwitchCurrentTray()
		{
			TrayManager.SwitchCurrentTray();
		}
	}
}
