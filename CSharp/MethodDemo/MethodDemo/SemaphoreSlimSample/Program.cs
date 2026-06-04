namespace SemaphoreSlimSample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ///await RunUseSemaphoreAsync();


            var thread1 = new Thread(ThreadMethod);
            var thread2 = new Thread(ThreadMethod);

            thread1.Start();
            thread2.Start();

            // 模拟经过一段时间后资源变为可用
            Thread.Sleep(9000);
            resourceAvailable = true;

            thread1.Join();
            thread2.Join();



            Console.WriteLine("Hello, World!");
            Console.ReadLine();

        }

        static bool resourceAvailable;
        static void ThreadMethod()
        {
            var spinWait = new SpinWait();
            //while (!resourceAvailable)
            //{
            //    spinWait.SpinOnce();
            //}

            SpinWait.SpinUntil(() => resourceAvailable);
            Console.WriteLine($"线程 {Thread.CurrentThread.ManagedThreadId} 获取到资源并执行操作");
        }





        private static SemaphoreSlim s_asyncLock = new SemaphoreSlim(1);

        private static AsyncSemaphore s_asyncSemaphore=new AsyncSemaphore();
        static async Task LockWithSemaphore(string title)
        {
            Console.WriteLine($"{title} waiting for lock");
            //await s_asyncLock.WaitAsync();
            //try
            //{
            //    Console.WriteLine($"{title} {nameof(LockWithSemaphore)} started");
            //    await Task.Delay(500);
            //    Console.WriteLine($"{title} {nameof(LockWithSemaphore)} end"); 
            //}
            //finally
            //{
            //    s_asyncLock.Release();
            //}
            using(await s_asyncSemaphore.WaitAsync())
            {
                Console.WriteLine($"{title} {nameof(LockWithSemaphore)} started");
                await Task.Delay(500);
                Console.WriteLine($"{title} {nameof(LockWithSemaphore)} end");
            }

        }

        static async Task RunUseSemaphoreAsync()
        {
            Console.WriteLine(nameof(RunUseSemaphoreAsync));
            string[] messages = { "one", "two", "three", "four", "five", "six" };
            Task[] tasks = new Task[messages.Length];
            for(int i =0;i<messages.Length;i++)
            {
                string message = messages[i];
                tasks[i] = Task.Run(async () => await LockWithSemaphore(message));
            }
            await Task.WhenAll(tasks);
        }
    }
}
