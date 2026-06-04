using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskSchedulerDemo
{

    /// <summary>
    /// 基于时间间隔的任务调度器
    /// </summary>
    public class IntervalTaskScheduler : TaskScheduler
    {
        private readonly TimeSpan _interval;
        private readonly Queue<Task> _taskQueue = new Queue<Task>();
        private readonly object _lock = new object();
        private bool _isRunning;

        public IntervalTaskScheduler(TimeSpan interval)
        {
            _interval = interval;
            _isRunning = false;
        }

        // 获取已调度的任务
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            lock (_lock)
            {
                foreach (var task in _taskQueue)
                {
                    yield return task;
                }
            }
        }

        // 将任务放入队列并调度
        protected override void QueueTask(Task task)
        {
            lock (_lock)
            {
                _taskQueue.Enqueue(task);
            }

            if (!_isRunning)
            {
                _isRunning = true;
                StartExecution();
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        // 启动任务执行，按指定间隔执行任务
        private void StartExecution()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    Task taskToExecute = null;

                    lock (_lock)
                    {
                        if (_taskQueue.Count > 0)
                        {
                            taskToExecute = _taskQueue.Dequeue();
                        }
                    }

                    if (taskToExecute != null)
                    {
                        TryExecuteTask(taskToExecute);
                    }

                    await Task.Delay(_interval); // 等待指定的时间间隔
                }
            });
        }
    }
}