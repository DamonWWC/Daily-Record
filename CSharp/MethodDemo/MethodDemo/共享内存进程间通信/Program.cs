using System.IO.MemoryMappedFiles;
using System.Text;

namespace 共享内存进程间通信
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateNew("MySharedMemory", 1024))
            {
                using (MemoryMappedViewAccessor accessor=mmf.CreateViewAccessor())
                {
                    Thread thread = new Thread(() =>
                    {
                        using (MemoryMappedFile mmfReader=MemoryMappedFile.OpenExisting("MySharedMemory"))
                        {
                            using (MemoryMappedViewAccessor accessorReader=mmfReader.CreateViewAccessor())
                            {
                                byte[] buffer = new byte[1024];
                                accessorReader.ReadArray(0, buffer, 0, buffer.Length);
                                string data = Encoding.UTF8.GetString(buffer);
                                Console.WriteLine($"读取到共享内存中的数据：{data}");
                            }
                        }
                    });

                    thread.Start();
                    Thread.Sleep(2000);
                    string dataWrite = "Hello, Shared Memory!";
                    byte[] bufferToWrite = Encoding.UTF8.GetBytes(dataWrite);
                    accessor.WriteArray(0, bufferToWrite, 0, bufferToWrite.Length);
                    //accessor.WriteArray(0, bufferToWrite, 0, bufferToWrite.Length);
                    thread.Join();
                }
            }

            Console.ReadKey();
                Console.WriteLine("Hello, World!");
        }

        public static void CreateDictionary()
        {
            Type typeToConstruct = typeof(Dictionary<,>);
            Type[] typeArguments = { typeof(int), typeof(string) };
            Type newType = typeToConstruct.MakeGenericType(typeArguments);

            Dictionary<int, string> dict = (Dictionary<int, string>)Activator.CreateInstance(newType);

        }
    }

   
}
