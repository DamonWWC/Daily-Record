using Polly;
using Polly.CircuitBreaker;
using Polly.RateLimiting;
using Polly.Retry;
using Polly.Timeout;
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

//    .AddTimeout(new TimeoutStrategyOptions
//    {
//        Timeout = TimeSpan.FromSeconds(4),
//        OnTimeout = args =>
//        {
//            Console.WriteLine("TimeOut limit has been exceeded");

//            return default;
//        }
//    }) 
//    .Build();
// //.AddRetry(new RetryStrategyOptions())
// //.AddCircuitBreaker(optionsComplex)
// var result=await pipeline.ExecuteAsync(
//    static async token =>
//    {
//        await Task.Delay(3000, token);
//        return true;
//        //Console.WriteLine("Error");
//        //throw new InvalidOperationException();

//    });



// 创建超时策略，等待最多 1 秒钟
// 如果在等待时间内未完成操作，则会抛出 TimeoutRejectedException 异常。
//var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(1), TimeoutStrategy.Pessimistic);

//try
//{
//    // 在超时策略中执行操作
//    await timeoutPolicy.ExecuteAsync(async (ct) =>
//    {
//        await Task.Delay(5000);
//        Console.WriteLine("true");
//    }, CancellationToken.None);
//    /*
//     * 最后，我们使用 ExecuteAsync 方法，在组合策略中执行异步操作，并在 CancellationToken 参数中传递 CancellationToken.None 来允许取消操作。
//     * 如果在指定的等待时间内没有得到响应，则会抛出 TimeoutRejectedException 异常。如果操作失败，则会捕获异常并输出相应的信息。
//     */

//}
//catch (TimeoutRejectedException)
//{
//    Console.WriteLine("The operation timed out.");
//}
//catch (Exception ex)
//{
//    Console.WriteLine($"Exception caught: {ex.GetType().Name}");
//}




//var timeoutPolicy = Policy
//            .TimeoutAsync(2, TimeoutStrategy.Optimistic);

//try
//{
//    await timeoutPolicy.ExecuteAsync( async (ct) =>
//    {
//        Console.WriteLine("Starting operation...");
//        await Task.Run(()=>
//        {
//            try
//            {
//                LongRunningOperation(ct);

//            }
//            catch(Exception ex)
//            {

//            }


//            }, ct); // 这可能是一个长时间操作

//        //await LongRunningOperation1(ct); // 模拟长时间操作
//        Console.WriteLine("Operation completed");
//    },CancellationToken.None);
//}
//catch (TimeoutRejectedException)
//{
//    Console.WriteLine("Operation timed out");
//}
//catch(Exception ex)
//{

//}

// static void  LongRunningOperation(CancellationToken ct)
//{
//    for (int i = 0; i < 5; i++)
//    {
//        ct.ThrowIfCancellationRequested();
//        Console.WriteLine($"Operation in progress... {i + 1}");
//        Thread.Sleep(1000);
//        //await Task.Delay(1000); // 每次等待1秒
//    }
//}


//static async Task LongRunningOperation1(CancellationToken ct)
//{
//    for (int i = 0; i < 5; i++)
//    {
//        ct.ThrowIfCancellationRequested();
//        Console.WriteLine($"Operation in progress... {i + 1}");
//        await Task.Delay(1000); // 每次等待1秒
//    }
//}





//Console.WriteLine(result);

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



//#region rate limiter

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
Run(4);
////Run(5);
////Run(6);
////Run(7);
////Run(8);




async void Run(int num)
{
    try
    {
        var aa = await widthOnRejected.ExecuteAsync(async ct =>
         {
             await Task.Delay(2000 * num, ct);

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



//#endregion


#region TimeOut




#endregion

Console.ReadLine();

