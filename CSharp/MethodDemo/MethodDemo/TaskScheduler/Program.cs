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

            //var scheduler = new PeriodicTaskScheduler(DateTime.Now.AddSeconds(5), TimeSpan.FromSeconds(5));
            //var taskFactory = new TaskFactory(scheduler);
            //for(int i=0;i<5;i++)
            //{
            //    int taskId = i;
            //    taskFactory.StartNew(() =>
            //    {
            //        Console.WriteLine($"Task {taskId} started at {DateTime.Now:HH:mm:ss.fff}");
            //    });
            //}

            CancellationTokenSource tokenSource = new();
            var task = Task.Run(async () =>
            {
                
                    await Task.Delay(5000,tokenSource.Token);
                int ii = 0;

            }, tokenSource.Token);

            
            Task.Delay(2000).Wait();


            tokenSource.Cancel();


            Bar(new Foo(1, 2));

            Console.ReadKey();
            Console.WriteLine("Hello, World!");
        }

        static void Bar(Foo v)
        {
            //var (x, y) = v;
            //Console.WriteLine($"X = {x}, Y = {y}");
            var result = v switch
            {
                Foo(3, var y) => y,
                Foo(var x, var y) => x + y,
                _ => 0
            };

            Console.WriteLine(result);
        }
    }

    class Foo
    {
        private int x;
        private int y;

        public Foo(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
       // public static implicit 
        public void Deconstruct(out int x, out int y)
        {
            x = this.x;
            y = this.y;
        }
    }
}
