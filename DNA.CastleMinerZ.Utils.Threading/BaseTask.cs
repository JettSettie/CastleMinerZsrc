using System;

namespace DNA.CastleMinerZ.Utils.Threading
{
	public class BaseTask : IReleaseable, ILinkedListNode
	{
		public TaskDelegate _workDelegate;

		public GatherTask _parent;

		public TaskThread _thread;

		public object _context;

		private volatile bool _interrupted;

		public ILinkedListNode _nextNode;

		public virtual bool Interrupted
		{
			get
			{
				return _interrupted;
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

		public virtual void Init(TaskDelegate work, object context, GatherTask parent)
		{
			Init(work, context);
			_parent = parent;
			_thread = null;
		}

		public virtual void Init(TaskDelegate work, object context)
		{
			if (work == null)
			{
				throw new ArgumentNullException("work", "Task initialized with null work delegate");
			}
			_parent = null;
			_workDelegate = work;
			_context = context;
			_thread = null;
		}

		public virtual void DoWork(TaskThread thread)
		{
			_thread = thread;
			_workDelegate(this, _context);
			if (_parent != null)
			{
				_parent.ChildFinished();
			}
		}

		public virtual void YieldUntilIdle()
		{
			if (_thread != null)
			{
				_thread.DrainTaskList();
			}
		}

		public virtual void YieldCount(int count)
		{
			if (_thread == null)
			{
				return;
			}
			while (count != 0)
			{
				BaseTask task = _thread.GetTask();
				if (task != null)
				{
					_thread.DoTask(task);
					count--;
				}
				else
				{
					count = 0;
				}
			}
		}

		public virtual void YieldOnce()
		{
			YieldCount(1);
		}

		public virtual void Interrupt()
		{
			_interrupted = true;
		}

		public virtual void Release()
		{
			_thread = null;
			_context = null;
			_workDelegate = null;
			_parent = null;
			_interrupted = false;
		}
	}
}
