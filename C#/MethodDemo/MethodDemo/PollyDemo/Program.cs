using Polly;
using System.Threading.RateLimiting;

namespace PollyDemo
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await GetSessionIdByClientId();
            Console.WriteLine("Hello, World!");
        }

        public static async Task<string> GetSessionIdByClientId()
        {
            var aa = new ResiliencePipelineBuilder()
           .AddRateLimiter(new SlidingWindowRateLimiter(
        new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 1,
            SegmentsPerWindow = 4,
            Window = TimeSpan.FromMinutes(1)
        })).Build();
            string result = null;
            for (int i = 0; i < 10; i++)
            {
                result = await aa.ExecuteAsync(async (token) => await GetString());
                //result = await Policy.RateLimitAsync(1, TimeSpan.FromSeconds(10)).ExecuteAsync(async () => await GetString());
                Console.WriteLine(result);
                await Task.Delay(500);
            }

            //_ = int.TryParse("ww", out int retryCount);
            //var retry = Policy.HandleResult<string>(response => string.IsNullOrWhiteSpace(response))
            //    .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(2));

            //var result = await retry.ExecuteAsync(async () => await GetString());
            return result;
        }

        public static async Task<string> GetString()
        {
            await Task.Delay(500);
            return "111";
        }
    }
}