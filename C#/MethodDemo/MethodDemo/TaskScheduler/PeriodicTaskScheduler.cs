using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskSchedulerDemo
{
    public class PeriodicTaskScheduler : TaskScheduler
    {
        private readonly TimeSpan _interval;
        private readonly DateTime _startTime;
        private readonly Queue<Task> _taskQueue = new Queue<Task>();
        private readonly object _lock = new object();
        private bool _isRunning;

        public PeriodicTaskScheduler(DateTime startTime,TimeSpan interval)
        {
            _startTime = startTime;
            _interval = interval;
            _isRunning = false;
        }
        protected override IEnumerable<Task>? GetScheduledTasks()
        {
            lock(_lock)
            {
                foreach(var task in _taskQueue)
                {
                    yield return task;
                }
            }
        }

        protected override void QueueTask(Task task)
        {
            lock(_lock)
            {
                _taskQueue.Enqueue(task);
            }

            if(!_isRunning)
            {
                _isRunning = true;
                StartExecution();
            }
        }

        private void StartExecution()
        {
            Task.Run(async () =>
            {
                var delayUntilStart = _startTime - DateTime.Now;
                if(delayUntilStart>TimeSpan.Zero)
                {
                    await Task.Delay(delayUntilStart);
                }
                while(true)
                {
                    Task taskToExecute = null;

                    lock(_lock)
                    {
                        if(_taskQueue.Count>0)
                        {
                            taskToExecute = _taskQueue.Dequeue();
                        }
                    }
                    if(taskToExecute!=null)
                    {
                        TryExecuteTask(taskToExecute);
                    }
                    await Task.Delay(_interval);
                }
            });
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            throw new NotImplementedException();
        }
    }
}
