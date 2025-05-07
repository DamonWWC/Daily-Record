namespace AsyncLoacalSample
{
    internal class Program
    {
        private static AsyncLocal<int> asyncLocalValue = new AsyncLocal<int>();
        static async Task Main(string[] args)
        {

            //await Start();
            SpinLock.Enter();
            asyncLocalValue.Value = 10;
            Console.WriteLine($"Main before async:{asyncLocalValue.Value}");
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            await DoAsyncWork();
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine($"Main after async: {asyncLocalValue.Value}");
            Console.WriteLine("Hello, World!");
        }
        static async Task DoAsyncWork()
        {
            int value = 10;
            int comparand = 10;
            int newValue = 20;
            int original = Interlocked.CompareExchange(ref value, newValue, comparand);
            Console.WriteLine($"子{Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"DoAsyncWork before change: {asyncLocalValue.Value}");
            asyncLocalValue.Value = 20;
            Console.WriteLine($"DoAsyncWork after change: {asyncLocalValue.Value}");
            await Task.Delay(100).ConfigureAwait(false);
            Console.WriteLine($"子{Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"DoAsyncWork after delay: {asyncLocalValue.Value}");
        }
        //private static AsyncLocal<string> _context = new AsyncLocal<string>();
        //public static async Task Start()
        //{
        //    _context.Value = "Initial Value";
        //    Console.WriteLine($"Start: {_context.Value}"); // 输出 Initial Value

        //    await StepOne();
        //    Console.WriteLine($"After StepOne: {_context.Value}"); // 输出 Updated in StepTwo
        //}

        //private static async Task StepOne()
        //{
        //    _context.Value = "Updated in StepOne";
        //    await StepTwo();
        //}

        //private static async Task StepTwo()
        //{
        //    await Task.Delay(100); // 模拟异步操作
        //    Console.WriteLine($"StepTwo: {_context.Value}"); // 输出 Updated in StepOne
        //    _context.Value = "Updated in StepTwo";
        //}

       
    }

    public class Singleton
    {
        private static volatile Singleton _instance;
        private static readonly object _lock = new object();

        private Singleton() { }
        public static Singleton Instance
        {
            get
            {
                if(_instance==null)
                {
                    Singleton newInstance = new Singleton();
                    Interlocked.CompareExchange(ref _instance, newInstance, null);
                }
                return _instance;
            }
        }
    }
    public class SpinLock
    {
        private static int _locked = 1;
        public static void Enter()
        {
            while(Interlocked.CompareExchange(ref _locked,1,0)!=0)
            {

            }
        }
        public static void Exit()
        {
            Interlocked.Exchange(ref _locked, 0);
        }
    }
}
