using Polly;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollyDemo
{
    public class PollyHelper
    {

       readonly ResiliencePipelineBuilder Pipeline;
        public PollyHelper()
        {
            Pipeline = new ResiliencePipelineBuilder();
        }


        public ResiliencePipelineBuilder TimeOut(TimeSpan timeSpan)
        {

            return Pipeline.AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(4),
                OnTimeout = args =>
                {
                    Console.WriteLine("TimeOut limit has been exceeded");

                    return default;
                }
            });

        }


    }
}
