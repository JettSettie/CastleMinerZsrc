namespace DNA.CastleMinerZ.Utils.Threading
{
	public sealed class Task : BaseTask
	{
		private static ObjectCache<Task> _cache = new ObjectCache<Task>();

		public override void DoWork(TaskThread thread)
		{
			base.DoWork(thread);
			Release();
		}

		public static Task Alloc()
		{
			return _cache.Get();
		}

		public override void Release()
		{
			base.Release();
			_cache.Put(this);
		}
	}
}
