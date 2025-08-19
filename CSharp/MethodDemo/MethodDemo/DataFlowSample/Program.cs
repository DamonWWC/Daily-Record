using System.Threading.Tasks.Dataflow;

namespace DataFlowSample
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            //         var actionBlock = new ActionBlock<int>(n => Console.WriteLine(n));

            //         for (int i = 0; i < 3; i++)
            //         {
            //             actionBlock.Post(i);
            //         }

            //         actionBlock.Complete();
            //         actionBlock.Completion.Wait();

            //         var transformmanyblock = new TransformManyBlock<string, char>(s => s.ToCharArray());

            //         var batchBlock = new BatchBlock<int>(10);

            //         for (int i = 0; i < 13; i++)
            //         {
            //             batchBlock.Post(i);
            //         }

            //         batchBlock.Complete();
            //         Console.WriteLine("The sum of the elements in batch 1 is {0}.",
            //batchBlock.Receive().Sum());

            //         Console.WriteLine("The sum of the elements in batch 2 is {0}.",
            //            batchBlock.Receive().Sum());


            

            Task task= new Task(() =>
            {
            });
            //task.ContinueWith
            //var cts = new CancellationTokenSource();
            // 假设有一组数据
            var dataItems = Enumerable.Range(0, 1000).Select(x => $"data_{x}").ToList();

           // var processor = new DataProcessor(10, cts.Token);
           // await processor.ProcessAsync(dataItems);



            Console.ReadKey();

            Console.WriteLine("Hello, World!");
        }

        /// <summary>
        /// 数据处理器
        /// </summary>
        public class DataProcessor(int maxDegreeOfParallelism, CancellationToken cancellationToken)
        {
            public async Task ProcessAsync(List<string> dataItems)
            {
                // 创建一个 TransformBlock 用于步骤1的处理，并将结果发送到步骤2的 ActionBlock
                var step1Block = new TransformBlock<string, string>(async dataItem => await Step1(dataItem), new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                    CancellationToken = cancellationToken
                });

                // 创建一个 ActionBlock 用于步骤2的处理
                var step2Block = new ActionBlock<string>(async dataItem =>
                {
                    await Step2(dataItem);
                }, new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                    CancellationToken = cancellationToken
                });

                // 将 TransformBlock 链接到 ActionBlock
                step1Block.LinkTo(step2Block, new DataflowLinkOptions { PropagateCompletion = true });

                // 启动多个步骤1的任务（生产者）
                foreach (var dataItem in dataItems)
                {
                    await step1Block.SendAsync(dataItem, cancellationToken);
                }

                // 完成步骤1的 TransformBlock 的写入
                step1Block.Complete();
                // 等待步骤1的 TransformBlock 处理完成
                await step1Block.Completion;

                // 完成步骤2的 ActionBlock 的写入
                step2Block.Complete();
                // 等待步骤2的 ActionBlock 处理完成
                await step2Block.Completion;
            }

            private async Task<string> Step1(string dataItem)
            {
                // 模拟步骤1的处理（如初步处理数据）
                await Task.Delay(10, cancellationToken);
                Console.WriteLine($"Step1 processed data item: {dataItem}");
                return dataItem;
            }

            private async Task Step2(string dataItem)
            {
                // 模拟步骤2的处理（如进一步处理数据）
                await Task.Delay(10, cancellationToken);
                Console.WriteLine($"Step2 processed data item: {dataItem}");
            }

        }
    }
}