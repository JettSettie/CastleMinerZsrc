using DNA.CastleMinerZ.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Terrain
{
	public class VertexBufferKeeper : ILinkedListNode, IReleaseable
	{
		public VertexBuffer Buffer;

		public int NumVertexesUsed;

		private static ObjectCache<VertexBufferKeeper> _smallBufferCache = new ObjectCache<VertexBufferKeeper>();

		private static ObjectCache<VertexBufferKeeper> _largeBufferCache = new ObjectCache<VertexBufferKeeper>();

		private ILinkedListNode _nextNode;

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

		public static VertexBufferKeeper Alloc(int numNeeded)
		{
			if (numNeeded < 2048)
			{
				return _smallBufferCache.Get();
			}
			return _largeBufferCache.Get();
		}

		public void Release()
		{
			if (Buffer == null || Buffer.VertexCount < 2048)
			{
				_smallBufferCache.Put(this);
			}
			else
			{
				_largeBufferCache.Put(this);
			}
		}
	}
}
