namespace TaskSchedulerDemo
{
    public class Program
    {
        static void Main(string[] args)
        {

            //var scheduler = new IntervalTaskScheduler(TimeSpan.FromSeconds(5));
            //var taskFactory = new TaskFactory(scheduler);

            //// 创建并启动定时任务
            //for (int i = 0; i < 10; i++)
            //{
            //    int taskId = i;
            //    taskFactory.StartNew(() =>
            //    {
            //        Console.WriteLine($"Task {taskId} started at {DateTime.Now:HH:mm:ss.fff}");
            //    });
            //}

            var scheduler = new PeriodicTaskScheduler(DateTime.Now.AddSeconds(5), TimeSpan.FromSeconds(5));
            var taskFactory = new TaskFactory(scheduler);
            for(int i=0;i<5;i++)
            {
                int taskId = i;
                taskFactory.StartNew(() =>
                {
                    Console.WriteLine($"Task {taskId} started at {DateTime.Now:HH:mm:ss.fff}");
                });
            }
            Console.ReadKey();
            Console.WriteLine("Hello, World!");
        }
    }
}
