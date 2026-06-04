using Microsoft.Extensions.ObjectPool;

namespace 多线程
{
    internal class Program
    {
        static void Main(string[] args)
        {
            


            Console.WriteLine("Hello, World!");
        }
    }


    public class DataProcessor
    {
        private readonly ObjectPool<ProcessedItem> _itemPool;

        public async Task ProcessLargeDataSet(IAsyncEnumerable<DataItem> items)
        {
            await foreach(var item in items.ConfigureAwait(false))
            {
                var aa = _itemPool.Get();
                
            }
        }
    }
    public class DataItem:IAsyncDisposable
    {
        public string Name { get; set; }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
    public class ProcessedItem
    {
        public string Name { get; set; }
    }
}
