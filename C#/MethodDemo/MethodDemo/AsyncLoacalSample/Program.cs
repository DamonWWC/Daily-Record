namespace AsyncLoacalSample
{
    internal class Program
    {
        private static AsyncLocal<int> asyncLocalValue = new AsyncLocal<int>();
        static async Task Main(string[] args)
        {

            asyncLocalValue.Value = 10;
            Console.WriteLine($"Main before async:{asyncLocalValue.Value}");

            await DoAsyncWork();
            Console.WriteLine($"Main after async: {asyncLocalValue.Value}");
            Console.WriteLine("Hello, World!");
        }
        static async Task DoAsyncWork()
        {
            Console.WriteLine($"DoAsyncWork before change: {asyncLocalValue.Value}");
            asyncLocalValue.Value = 20;
            Console.WriteLine($"DoAsyncWork after change: {asyncLocalValue.Value}");
            await Task.Delay(100);
            Console.WriteLine($"DoAsyncWork after delay: {asyncLocalValue.Value}");
        }
    }
}
