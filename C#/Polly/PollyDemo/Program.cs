using Polly;
using Polly.CircuitBreaker;
using Polly.RateLimiting;
using Polly.Retry;
using System.Threading.RateLimiting;

CancellationToken cancellationToken = new CancellationToken();


//var optionsComplex = new CircuitBreakerStrategyOptions
//{
//    FailureRatio = 0.5,
//    SamplingDuration = TimeSpan.FromSeconds(10),
//    MinimumThroughput = 6,
//    BreakDuration = TimeSpan.FromSeconds(20),
//    //ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>()
//};

////.AddRetry(new RetryStrategyOptions())
////    .AddTimeout(TimeSpan.FromSeconds(1))

//ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
//    .AddRetry(new RetryStrategyOptions())
//    .AddTimeout(TimeSpan.FromSeconds(1))
//    .AddCircuitBreaker(optionsComplex)
//    .Build();
//await pipeline.ExecuteAsync(
//    static async token =>
//    {
//        await Task.Delay(1000, token);
//        Console.WriteLine("Error");
//        throw new InvalidOperationException();

//    }, cancellationToken);



//var context = ResilienceContextPool.Shared.Get();
//var circuitBreaker = new ResiliencePipelineBuilder()
//    .AddRetry(new RetryStrategyOptions())
//    .AddTimeout(TimeSpan.FromSeconds(1))
//    .AddCircuitBreaker(new CircuitBreakerStrategyOptions
//    {
//        FailureRatio = 0.5,
//        SamplingDuration = TimeSpan.FromSeconds(10),
//        ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
//        BreakDuration = TimeSpan.FromSeconds(20),
//        MinimumThroughput = 10
//    })
//    .Build();

//Outcome<HttpResponseMessage> outcome = await circuitBreaker.ExecuteOutcomeAsync(static async (ctx, state) =>
//{
//    var response = await IssueRequest();
//    if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
//        throw new BrokenCircuitException();
//    return Outcome.FromResult(response);
//}, context, "state");

//ResilienceContextPool.Shared.Return(context);

//if (outcome.Exception is BrokenCircuitException)
//{

//}
//else
//{

//}

//async static Task<HttpResponseMessage> IssueRequest()
//{
//    await Task.Delay(1000);
//    Console.WriteLine("error");
//    //throw new BrokenCircuitException();
//    return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);

//}



#region rate limiter

var widthOnRejected = new ResiliencePipelineBuilder()
    .AddRateLimiter(new RateLimiterStrategyOptions
    {
        DefaultRateLimiterOptions = new ConcurrencyLimiterOptions
        {
            //并发个数，也就是同时可运行个数，超过这个数量的任务将被阻塞或者排队等待。
            PermitLimit = 3,
            //任务在被执行之前可以排队等待的最大数量，一旦达到了设置的值，任何新添加的任务请求将被拒绝，通常会返回一个错误或者采取其他的处理方式
            QueueLimit = 0
        },
        OnRejected = args =>
        {
            Console.WriteLine("Rate limit has been exceeded");
            return default;
        }
    }).Build();




Run(1);
Run(2);
Run(3);
//Run(4);
//Run(5);
//Run(6);
//Run(7);
//Run(8);




async void Run(int num)
{
    try
    {
       var aa= await widthOnRejected.ExecuteAsync(async ct =>
        {
            await Task.Delay(2000*num, ct);
            
            Console.WriteLine($"Run{num}{DateTime.Now}");
            return new ValueTask<string>("1");
        }, CancellationToken.None);
        var bb = aa.Result;
    }
    catch (RateLimiterRejectedException)
    {
        Console.WriteLine("Rate limit has been exceeded22");
    }
}



#endregion


Console.ReadLine();

