using Polly;

namespace PollyDemo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
           await GetSessionIdByClientId();
            Console.WriteLine("Hello, World!");
        }

        public static async Task<string> GetSessionIdByClientId()
        {
            _ = int.TryParse("ww", out int retryCount);
            var retry = Policy.HandleResult<string>(response => string.IsNullOrWhiteSpace(response))
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(2));

            var result = await retry.ExecuteAsync(async () => await GetString());
            return result;
        }

        public static async Task<string> GetString()
        {
            await Task.Delay(1000);
            return default;
        }
    }
}
