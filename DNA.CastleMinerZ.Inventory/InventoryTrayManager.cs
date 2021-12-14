using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace DNA.CastleMinerZ.Inventory
{
	public class InventoryTrayManager
	{
		private enum InventoryTrayVersion
		{
			CurrentVersion
		}

		public const int MaxTrayItems = 8;

		public const int MaxTrayCount = 2;

		public InventoryItem[,] Trays = new InventoryItem[2, 8];

		public InventoryItem[] InventoryTray = new InventoryItem[8];

		public InventoryItem[] InventoryTray2 = new InventoryItem[8];

		private int _currentTrayIndex;

		public int Version
		{
			get
			{
				return 0;
			}
		}

		public int CurrentTrayLength
		{
			get
			{
				return 8;
			}
		}

		private bool IsOutOfBounds(int trayIndex, int itemIndex)
		{
			int upperBound = Trays.GetUpperBound(0);
			int upperBound2 = Trays.GetUpperBound(1);
			if (trayIndex <= upperBound)
			{
				return itemIndex > upperBound2;
			}
			return true;
		}

		public InventoryItem GetItemFromCurrentTray(int itemIndex)
		{
			return Trays[_currentTrayIndex, itemIndex];
		}

		public InventoryItem GetItemFromNextTray(int itemIndex)
		{
			int nextTrayIndex = GetNextTrayIndex(_currentTrayIndex);
			return Trays[nextTrayIndex, itemIndex];
		}

		public InventoryItem GetTrayItem(int trayIndex, int itemIndex)
		{
			if (IsOutOfBounds(trayIndex, itemIndex))
			{
				return null;
			}
			return Trays[trayIndex, itemIndex];
		}

		public void SetTrayItem(int trayIndex, int itemIndex, InventoryItem item)
		{
			if (!IsOutOfBounds(trayIndex, itemIndex))
			{
				Trays[trayIndex, itemIndex] = item;
			}
		}

		public void SetItem(int itemIndex, InventoryItem item)
		{
			Trays[_currentTrayIndex, itemIndex] = item;
		}

		public bool RemoveItem(InventoryItem targetItem)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					InventoryItem inventoryItem = Trays[i, j];
					if (Trays[i, j] == targetItem)
					{
						Trays[i, j] = null;
						return true;
					}
				}
			}
			return false;
		}

		public bool CanConsume(InventoryItem.InventoryItemClass itemClass, int amount)
		{
			int num = ForAllItems((InventoryItem trayItem) => (trayItem != null && trayItem.CanConsume(itemClass, amount)) ? 1 : 0);
			return num == 1;
		}

		internal void Update(GameTime gameTime)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					InventoryItem inventoryItem = Trays[i, j];
					if (inventoryItem != null)
					{
						inventoryItem.Update(gameTime);
					}
				}
			}
		}

		internal int ForAllItems(TrayItemAction itemAction)
		{
			int num = 0;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					InventoryItem inventoryItem = Trays[i, j];
					if (inventoryItem != null)
					{
						num = itemAction(inventoryItem);
						if (num != 0)
						{
							return num;
						}
					}
				}
			}
			return 0;
		}

		internal void ActOnEachItem(Action<InventoryItem> itemAction)
		{
			InventoryItem[,] trays = Trays;
			foreach (InventoryItem inventoryItem in trays)
			{
				if (inventoryItem != null)
				{
					itemAction(inventoryItem);
				}
			}
		}

		private int GetNextTrayIndex(int currentIndex)
		{
			int num = currentIndex + 1;
			if (num >= 2)
			{
				num = 0;
			}
			return num;
		}

		internal int GetItemClassCount(InventoryItem.InventoryItemClass itemClass)
		{
			int num = 0;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					InventoryItem inventoryItem = Trays[i, j];
					if (inventoryItem != null && itemClass == inventoryItem.ItemClass)
					{
						num += inventoryItem.StackCount;
					}
				}
			}
			return num;
		}

		internal void RemoveAllItems(bool onlyRemoveEmptyItems = false)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					if (Trays[i, j] != null && (!onlyRemoveEmptyItems || Trays[i, j].StackCount <= 0))
					{
						Trays[i, j] = null;
					}
				}
			}
		}

		internal bool CanAdd(InventoryItem item)
		{
			int num = ForAllItems((InventoryItem trayItem) => (trayItem != null && trayItem.CanStack(item)) ? 1 : 0);
			return num == 1;
		}

		internal void AddItemToExisting(InventoryItem item)
		{
			ActOnEachItem(delegate(InventoryItem trayItem)
			{
				if (trayItem != null)
				{
					trayItem.Stack(item);
				}
			});
		}

		internal bool AddItemToEmpty(InventoryItem item)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					if (Trays[i, j] == null)
					{
						Trays[i, j] = item;
						return true;
					}
				}
			}
			return false;
		}

		internal int AddItemToTray(InventoryItem item)
		{
			AddItemToExisting(item);
			if (item.StackCount <= 0)
			{
				return 0;
			}
			if (AddItemToEmpty(item))
			{
				return 0;
			}
			return item.StackCount;
		}

		internal void Write(BinaryWriter writer)
		{
			writer.Write(2);
			for (int i = 0; i < 2; i++)
			{
				writer.Write(8);
				for (int j = 0; j < 8; j++)
				{
					InventoryItem inventoryItem = Trays[i, j];
					if (inventoryItem == null)
					{
						writer.Write(false);
						continue;
					}
					writer.Write(true);
					inventoryItem.Write(writer);
				}
			}
		}

		internal void Read(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int num2 = reader.ReadInt32();
				for (int j = 0; j < num2; j++)
				{
					InventoryItem inventoryItem2 = Trays[i, j];
					Trays[i, j] = null;
					if (reader.ReadBoolean())
					{
						InventoryItem inventoryItem = InventoryItem.Create(reader);
						if (inventoryItem != null && inventoryItem.IsValid())
						{
							Trays[i, j] = inventoryItem;
						}
					}
				}
			}
		}

		internal void ReadLegacy(BinaryReader reader)
		{
			for (int i = 0; i < 8; i++)
			{
				if (reader.ReadBoolean())
				{
					Trays[0, i] = InventoryItem.Create(reader);
					if (Trays[0, i] != null && !Trays[0, i].IsValid())
					{
						Trays[0, i] = null;
					}
				}
				else
				{
					Trays[0, i] = null;
				}
			}
		}

		internal InventoryItem Stack(InventoryItem sourceItem)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					InventoryItem inventoryItem = Trays[i, j];
					if (inventoryItem != null)
					{
						InventoryItem inventoryItem2 = sourceItem;
						inventoryItem.Stack(inventoryItem2);
						sourceItem = inventoryItem2;
					}
				}
			}
			return sourceItem;
		}

		internal bool PlaceInEmptySlot(InventoryItem SelectedItem)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					if (Trays[i, j] == null)
					{
						Trays[i, j] = SelectedItem;
						return true;
					}
				}
			}
			return false;
		}

		internal bool Contains(InventoryItem sourceItem)
		{
			InventoryItem[,] trays = Trays;
			foreach (InventoryItem inventoryItem in trays)
			{
				if (inventoryItem == sourceItem)
				{
					return true;
				}
			}
			return false;
		}

		internal void SwitchCurrentTray()
		{
			_currentTrayIndex++;
			if (_currentTrayIndex >= 2)
			{
				_currentTrayIndex = 0;
			}
		}
	}
}
