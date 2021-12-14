using DNA.CastleMinerZ.Utils;
using DNA.Drawing;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Threading;

namespace DNA.CastleMinerZ.Terrain
{
	public class BlockBuildData : IReleaseable, ILinkedListNode
	{
		private const int NUM_VERTS = 7000;

		public float[] _sun = new float[9];

		public float[] _torch = new float[9];

		public int[] _vxsun = new int[4];

		public int[] _vxtorch = new int[4];

		public IntVector3 _min = default(IntVector3);

		public IntVector3 _max = default(IntVector3);

		public BlockVertex[] _vxList = new BlockVertex[7000];

		private int _vxBufferSize = 7000;

		private int _vxCount;

		public BlockVertex[] _fancyVXList = new BlockVertex[7000];

		private int _fancyVXBufferSize = 7000;

		private int _fancyVXCount;

		private static ObjectCache<BlockBuildData> _cache = new ObjectCache<BlockBuildData>();

		private ILinkedListNode _nextNode;

		public bool HasVertexes
		{
			get
			{
				if (_vxCount == 0)
				{
					return _fancyVXCount != 0;
				}
				return true;
			}
		}

		public ILinkedListNode NextNode
		{
			get
			{
				return _nextNode;
			}
			set
			{
				_nextNode = value;
			}
		}

		public void AddVertex(BlockVertex bv, bool fancy)
		{
			if (fancy)
			{
				if (_fancyVXCount == _fancyVXBufferSize)
				{
					BlockVertex[] array = new BlockVertex[_fancyVXBufferSize + 100];
					_fancyVXList.CopyTo(array, 0);
					_fancyVXList = array;
					_fancyVXBufferSize += 100;
				}
				_fancyVXList[_fancyVXCount++] = bv;
			}
			else
			{
				if (_vxCount == _vxBufferSize)
				{
					BlockVertex[] array2 = new BlockVertex[_vxBufferSize + 100];
					_vxList.CopyTo(array2, 0);
					_vxList = array2;
					_vxBufferSize += 100;
				}
				_vxList[_vxCount++] = bv;
			}
		}

		public void BuildVBs(GraphicsDevice gd, ref List<VertexBufferKeeper> vbs, ref List<VertexBufferKeeper> fancy)
		{
			if (_vxCount > 0)
			{
				int num = 0;
				int num2 = _vxCount;
				int num3 = 0;
				while (num2 != 0)
				{
					int num4 = (num2 > 16384) ? 16384 : num2;
					num2 -= num4;
					if (gd.IsDisposed)
					{
						return;
					}
					VertexBufferKeeper vertexBufferKeeper = VertexBufferKeeper.Alloc(num4);
					if (vertexBufferKeeper.Buffer != null && vertexBufferKeeper.Buffer.VertexCount < num4)
					{
						vertexBufferKeeper.Buffer.Dispose();
						vertexBufferKeeper.Buffer = null;
					}
					bool flag = false;
					do
					{
						if (GraphicsDeviceLocker.Instance.TryLockDevice())
						{
							try
							{
								if (vertexBufferKeeper.Buffer == null)
								{
									vertexBufferKeeper.Buffer = new VertexBuffer(gd, typeof(BlockVertex), num4, BufferUsage.WriteOnly);
								}
								VertexBuffer buffer = vertexBufferKeeper.Buffer;
								buffer.SetData(_vxList, num, num4);
							}
							finally
							{
								GraphicsDeviceLocker.Instance.UnlockDevice();
							}
							flag = true;
						}
						if (!flag)
						{
							Thread.Sleep(10);
						}
					}
					while (!flag);
					vertexBufferKeeper.NumVertexesUsed = num4;
					num += num4;
					vbs.Add(vertexBufferKeeper);
					num3++;
				}
			}
			if (_fancyVXCount <= 0)
			{
				return;
			}
			int startIndex = 0;
			int num5 = _fancyVXCount;
			int num6 = 0;
			while (num5 != 0)
			{
				int num7 = (num5 > 16384) ? 16384 : num5;
				num5 -= num7;
				if (gd.IsDisposed)
				{
					break;
				}
				VertexBufferKeeper vertexBufferKeeper2 = VertexBufferKeeper.Alloc(num7);
				if (vertexBufferKeeper2.Buffer != null && vertexBufferKeeper2.Buffer.VertexCount < num7)
				{
					vertexBufferKeeper2.Buffer.Dispose();
					vertexBufferKeeper2.Buffer = null;
				}
				bool flag2 = false;
				do
				{
					if (GraphicsDeviceLocker.Instance.TryLockDevice())
					{
						try
						{
							if (vertexBufferKeeper2.Buffer == null)
							{
								vertexBufferKeeper2.Buffer = new VertexBuffer(gd, typeof(BlockVertex), num7, BufferUsage.WriteOnly);
							}
							VertexBuffer buffer2 = vertexBufferKeeper2.Buffer;
							buffer2.SetData(_fancyVXList, startIndex, num7);
						}
						finally
						{
							GraphicsDeviceLocker.Instance.UnlockDevice();
						}
						flag2 = true;
					}
					if (!flag2)
					{
						Thread.Sleep(10);
					}
				}
				while (!flag2);
				vertexBufferKeeper2.NumVertexesUsed = num7;
				fancy.Add(vertexBufferKeeper2);
				num6++;
			}
		}

		public static BlockBuildData Alloc()
		{
			BlockBuildData blockBuildData = _cache.Get();
			blockBuildData._min.SetValues(int.MaxValue, int.MaxValue, int.MaxValue);
			blockBuildData._max.SetValues(int.MinValue, int.MinValue, int.MinValue);
			blockBuildData._vxCount = 0;
			blockBuildData._fancyVXCount = 0;
			return blockBuildData;
		}

		public void Release()
		{
			_cache.Put(this);
		}
	}
}
