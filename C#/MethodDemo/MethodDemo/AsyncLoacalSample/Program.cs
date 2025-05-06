namespace AsyncLoacalSample
{
    internal class Program
    {
        private static AsyncLocal<int> asyncLocalValue = new AsyncLocal<int>();
        static async Task Main(string[] args)
        {

            //await Start();
            asyncLocalValue.Value = 10;
            Console.WriteLine($"Main before async:{asyncLocalValue.Value}");

            await DoAsyncWork().ConfigureAwait(false);
            Console.WriteLine($"Main after async: {asyncLocalValue.Value}");
            Console.WriteLine("Hello, World!");
        }
        static async Task DoAsyncWork()
        {
            
            Console.WriteLine($"DoAsyncWork before change: {asyncLocalValue.Value}");
            asyncLocalValue.Value = 20;
            Console.WriteLine($"DoAsyncWork after change: {asyncLocalValue.Value}");
            await Task.Delay(100).ConfigureAwait(false);
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
}
