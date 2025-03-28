using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskSchedulerDemo
{
    public class LimitedConcurrencyTaskScheduler : TaskScheduler
    {
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>();
        private readonly int _maxDegreeOfParallelism;
        private int _currentRunningTasks = 0;

        public LimitedConcurrencyTaskScheduler(int maxDegreeOfParallelism)
        {
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        protected override void QueueTask(Task task)
        {
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_currentRunningTasks < _maxDegreeOfParallelism)
                {
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                while (true)
                {
                    Task task;
                    lock (_tasks)
                    {
                        if (_tasks.Count == 0)
                        {
                            _currentRunningTasks--;
                            break;
                        }
                        task = _tasks.First.Value;
                        _tasks.RemoveFirst();
                        _currentRunningTasks++;
                    }
                    TryExecuteTask(task);
                }
            }, null);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false; // 禁止内联执行
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            lock (_tasks)
            {
                return _tasks.ToArray();
            }
        }

        public override int MaximumConcurrencyLevel => _maxDegreeOfParallelism;
    }
}
