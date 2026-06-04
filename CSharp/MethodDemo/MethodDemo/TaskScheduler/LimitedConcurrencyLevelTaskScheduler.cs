using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskSchedulerDemo
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        private readonly int _maxDegreeOfParallelism;
        private readonly ConcurrentQueue<Task> _tasks = new ConcurrentQueue<Task>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly object _lockObject = new object();
        private int _currentActiveTasks;

        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism <= 0) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        protected override void QueueTask(Task task)
        {
            _tasks.Enqueue(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            List<int> result = new List<int>();
  
            return _tasks;
        }

        public void Start()
        {
            for (int i = 0; i < _maxDegreeOfParallelism; i++)
            {
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        while (!_tasks.IsEmpty || !_cts.Token.IsCancellationRequested)
                        {
                            Task task;
                            if (_tasks.TryDequeue(out task))
                            {
                                base.TryExecuteTask(task);
                            }
                            else
                            {
                                Thread.Yield();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Thread encountered an exception: " + ex.Message);
                    }
                });

                thread.IsBackground = true;
                thread.Start();
            }
        }

        public void Stop()
        {
            _cts.Cancel();
        }
    }
}
