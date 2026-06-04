
namespace 初始化共享资源
{
    internal class Program
    {
        static int _simpleValue;
        static readonly Lazy<int> MySharedInteger = new Lazy<int>(() => _simpleValue++);
        static void Main(string[] args)
        {
            UseSharedInteger();
            UseSharedInteger();
            UseSharedInteger();
            Console.WriteLine("Hello, World!");
        }
        static void UseSharedInteger()
        {
            var operationId = Guid.NewGuid();
      

            int sharedValue = MySharedInteger.Value;
        }
    }
}
