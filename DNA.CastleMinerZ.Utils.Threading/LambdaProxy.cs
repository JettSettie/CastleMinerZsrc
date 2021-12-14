using System.Threading;

namespace DNA.CastleMinerZ.Utils.Threading
{
	public class LambdaProxy : ILinkedListNode, IReleaseable
	{
		private WaitCallback _oneArgumentCallback;

		private ThreadStart _zeroArgumentCallback;

		private static ObjectCache<LambdaProxy> _cache = new ObjectCache<LambdaProxy>();

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

		public void Execute(BaseTask t, object c)
		{
			if (_oneArgumentCallback != null)
			{
				_oneArgumentCallback(c);
			}
			else
			{
				_zeroArgumentCallback();
			}
			Release();
		}

		public static LambdaProxy Alloc(WaitCallback callback)
		{
			LambdaProxy lambdaProxy = _cache.Get();
			lambdaProxy._oneArgumentCallback = callback;
			return lambdaProxy;
		}

		public static LambdaProxy Alloc(ThreadStart callback)
		{
			LambdaProxy lambdaProxy = _cache.Get();
			lambdaProxy._zeroArgumentCallback = callback;
			return lambdaProxy;
		}

		public void Release()
		{
			_oneArgumentCallback = null;
			_zeroArgumentCallback = null;
			_cache.Put(this);
		}
	}
}
