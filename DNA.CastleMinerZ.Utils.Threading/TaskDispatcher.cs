using System;
using System.Threading;

namespace DNA.CastleMinerZ.Utils.Threading
{
	public class TaskDispatcher
	{
		private string[] _threadNames;

		private static TaskDispatcher _theInstance;

		private TaskThread[] _taskThreads;

		private Thread[] _systemThreads;

		private int _numThreads;

		private volatile bool _stopped;

		private SynchronizedQueue<BaseTask> _tasks = new SynchronizedQueue<BaseTask>();

		private SynchronizedQueue<BaseTask> _mainThreadTasks = new SynchronizedQueue<BaseTask>();

		public bool Stopped
		{
			get
			{
				return _stopped;
			}
		}

		public static TaskDispatcher Instance
		{
			get
			{
				return _theInstance;
			}
		}

		private string TheadName(int i)
		{
			return _threadNames[i];
		}

		public static TaskDispatcher Create()
		{
			if (_theInstance == null)
			{
				_theInstance = new TaskDispatcher();
			}
			return _theInstance;
		}

		private void CreateThreads(CountdownLatch cdown)
		{
			cdown.Value = _numThreads;
			_taskThreads = new TaskThread[_numThreads];
			_systemThreads = new Thread[_numThreads];
			for (int i = 0; i < _numThreads; i++)
			{
				_taskThreads[i] = new TaskThread(cdown, (TaskThreadEnum)i);
			}
			_threadNames = new string[_numThreads];
			for (int j = 0; j < _numThreads; j++)
			{
				_threadNames[j] = "TASK_THREAD_" + (j + 1);
			}
		}

		private void InitThreads(CountdownLatch numThreadsToInitialize)
		{
			_numThreads = Math.Max(Environment.ProcessorCount - 1, 2);
			CreateThreads(numThreadsToInitialize);
			for (int i = 0; i != _numThreads; i++)
			{
				TaskThread @object = _taskThreads[i];
				_systemThreads[i] = new Thread(@object.ThreadStart, 262144);
			}
		}

		private TaskDispatcher()
		{
			_theInstance = this;
			_stopped = false;
			CountdownLatch numThreadsToInitialize = default(CountdownLatch);
			InitThreads(numThreadsToInitialize);
			for (int i = 0; i != _numThreads; i++)
			{
				_systemThreads[i].Name = _threadNames[i];
				_systemThreads[i].Start();
			}
			while (numThreadsToInitialize.Value != 0)
			{
			}
		}

		public bool IsIdle(TaskThreadEnum skipIndex)
		{
			for (int i = 0; i < _numThreads; i++)
			{
				if (skipIndex != (TaskThreadEnum)i && !_taskThreads[i].Idle)
				{
					return false;
				}
			}
			return true;
		}

		public bool IsIdle(TaskThread thread)
		{
			return IsIdle(thread._threadIndex);
		}

		public void Stop()
		{
			if (_stopped)
			{
				return;
			}
			_stopped = true;
			WaitableTask.WakeAll();
			for (int i = 0; i < _numThreads; i++)
			{
				_taskThreads[i].Abort();
			}
			for (int j = 0; j < _numThreads; j++)
			{
				if (!_systemThreads[j].Join(1000))
				{
					_systemThreads[j].Abort();
				}
			}
		}

		private void WakeThreads()
		{
			for (int i = 0; i < _numThreads; i++)
			{
				if (!_taskThreads[i].Dormant)
				{
					_taskThreads[i].Wake();
				}
			}
		}

		private void InsertTask(BaseTask task)
		{
			_tasks.Queue(task);
			WakeThreads();
		}

		private void AppendTask(BaseTask task)
		{
			_tasks.Undequeue(task);
			WakeThreads();
		}

		private void InsertTask(TaskThreadEnum thread, BaseTask task)
		{
			if (thread == TaskThreadEnum.THREAD_ANY)
			{
				InsertTask(task);
			}
			else
			{
				_taskThreads[(int)thread].AddTask(task);
			}
		}

		public void AddTask(BaseTask task)
		{
			if (_stopped)
			{
				task.Interrupt();
			}
			else
			{
				InsertTask(task);
			}
		}

		public void AddRushTask(BaseTask task)
		{
			if (_stopped)
			{
				task.Interrupt();
			}
			else
			{
				AppendTask(task);
			}
		}

		public void AddTask(TaskThreadEnum thread, BaseTask task)
		{
			if (thread == TaskThreadEnum.THREAD_ANY)
			{
				AddTask(task);
			}
			else if (_stopped)
			{
				task.Interrupt();
			}
			else
			{
				InsertTask(thread, task);
			}
		}

		public BaseTask GetTask()
		{
			return _tasks.Dequeue();
		}

		public void AddTask(TaskDelegate work, object context)
		{
			if (!_stopped)
			{
				Task task = Task.Alloc();
				task.Init(work, context);
				InsertTask(task);
			}
		}

		public void AddRushTask(TaskDelegate work, object context)
		{
			if (!_stopped)
			{
				Task task = Task.Alloc();
				task.Init(work, context);
				AppendTask(task);
			}
		}

		public void AddTask(TaskThreadEnum thread, TaskDelegate work, object context)
		{
			if (!_stopped)
			{
				if (thread == TaskThreadEnum.THREAD_ANY)
				{
					AddTask(work, context);
					return;
				}
				Task task = Task.Alloc();
				task.Init(work, context);
				InsertTask(thread, task);
			}
		}

		public void AddTaskForMainThread(TaskDelegate work, object context)
		{
			Task task = Task.Alloc();
			task.Init(work, context);
			_mainThreadTasks.Queue(task);
		}

		public void AddTaskForMainThread(WaitCallback callback, object context)
		{
			LambdaProxy @object = LambdaProxy.Alloc(callback);
			AddTaskForMainThread(@object.Execute, context);
		}

		public void AddTaskForMainThread(ThreadStart callback)
		{
			LambdaProxy @object = LambdaProxy.Alloc(callback);
			AddTaskForMainThread(@object.Execute, null);
		}

		public void RunMainThreadTasks()
		{
			while (!_mainThreadTasks.Empty)
			{
				BaseTask baseTask = _mainThreadTasks.Dequeue();
				baseTask.DoWork(null);
			}
		}

		public WaitableTask AddWaitableTask(TaskThreadEnum thread, TaskDelegate work, object context)
		{
			WaitableTask waitableTask;
			if (thread == TaskThreadEnum.THREAD_ANY)
			{
				waitableTask = AddWaitableTask(work, context);
			}
			else
			{
				waitableTask = WaitableTask.Alloc();
				waitableTask.Init(work, context);
				if (_stopped)
				{
					waitableTask.Interrupt();
				}
				else
				{
					InsertTask(thread, waitableTask);
				}
			}
			return waitableTask;
		}

		public WaitableTask AddWaitableTask(TaskDelegate work, object context)
		{
			WaitableTask waitableTask = WaitableTask.Alloc();
			waitableTask.Init(work, context);
			if (_stopped)
			{
				waitableTask.Interrupt();
			}
			else
			{
				InsertTask(waitableTask);
			}
			return waitableTask;
		}

		public WaitableTask AddWaitableRushTask(TaskDelegate work, object context)
		{
			WaitableTask waitableTask = WaitableTask.Alloc();
			waitableTask.Init(work, context);
			if (_stopped)
			{
				waitableTask.Interrupt();
			}
			else
			{
				AppendTask(waitableTask);
			}
			return waitableTask;
		}

		public GatherTask AddGatherTask(TaskThreadEnum thread, TaskDelegate work, object context)
		{
			GatherTask gatherTask = AddGatherTask(work, context);
			gatherTask.DesiredThreadIndex = thread;
			return gatherTask;
		}

		public GatherTask AddGatherTask(TaskDelegate work, object context)
		{
			GatherTask gatherTask = GatherTask.Alloc();
			gatherTask.Init(work, context);
			return gatherTask;
		}
	}
}
