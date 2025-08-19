using Polly.Timeout;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PollyWpfDemo
{
    public static class FuncHelper
    {
        public static async Task<T> CallFunc<T>(Func<T> func, int milliseconds = 1000) where T:Task
        {
            CancellationToken cancellationToken = new CancellationToken();
            ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
                .AddTimeout(new TimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromMilliseconds(milliseconds),        
                    OnTimeout = args =>        
                    {           
                        Console.WriteLine("TimeOut limit has been exceeded");          
                        return default;       
                    }   
                })                  
                .Build();

            try
            {
              
                var result = await pipeline.ExecuteAsync( async token =>
                {
                   await func();
                    return true;
                   

                }, cancellationToken);

                return default;
            }
            catch(Exception ex)
            {
                return default;
            }
         
        }
    }
}