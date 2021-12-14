using DNA.Net.GamerServices;
using System.IO;

namespace DNA.CastleMinerZ.Inventory
{
	public class Door
	{
		private const int Columns = 8;

		public bool Destroyed;

		public bool Open;

		private DoorEntity.ModelNameEnum _modelName;

		private IntVector3 _location;

		public IntVector3 Location
		{
			get
			{
				return _location;
			}
		}

		public DoorEntity.ModelNameEnum ModelName
		{
			get
			{
				return _modelName;
			}
		}

		public Door(IntVector3 location, DoorEntity.ModelNameEnum modelName)
		{
			_location = location;
			_modelName = modelName;
		}

		public bool IsSlotLocked(int index)
		{
			foreach (NetworkGamer allGamer in CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers)
			{
				if (allGamer.Tag != null)
				{
					Player player = (Player)allGamer.Tag;
					int num = player.FocusCrateItem.X + player.FocusCrateItem.Y * 8;
					if (player.FocusCrate == Location && num == index)
					{
						return true;
					}
				}
			}
			return false;
		}

		public Door(BinaryReader reader)
		{
			Read(reader);
		}

		public void Write(BinaryWriter writer)
		{
			_location.Write(writer);
			writer.Write((byte)_modelName);
		}

		public void Read(BinaryReader reader)
		{
			_location = IntVector3.Read(reader);
			_modelName = (DoorEntity.ModelNameEnum)reader.ReadByte();
		}
	}
}
