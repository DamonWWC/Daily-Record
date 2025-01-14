using System.Threading.Tasks.Dataflow;

namespace DataFlowSample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var actionBlock = new ActionBlock<int>(n => Console.WriteLine(n));

            for (int i = 0; i < 3; i++)
            {
                actionBlock.Post(i);
            }

            actionBlock.Complete();
            actionBlock.Completion.Wait();

            var transformmanyblock = new TransformManyBlock<string, char>(s => s.ToCharArray());

            var batchBlock = new BatchBlock<int>(10);

            for (int i = 0; i < 13; i++)
            {
                batchBlock.Post(i);
            }

            batchBlock.Complete();
            Console.WriteLine("The sum of the elements in batch 1 is {0}.",
   batchBlock.Receive().Sum());

            Console.WriteLine("The sum of the elements in batch 2 is {0}.",
               batchBlock.Receive().Sum());

            Console.ReadKey();

            Console.WriteLine("Hello, World!");
        }



    }
}