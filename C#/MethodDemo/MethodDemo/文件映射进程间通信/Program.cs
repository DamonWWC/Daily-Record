using System.Collections.Frozen;

namespace 文件映射进程间通信
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int index = 1;
            while (index<=10)
            {
                index++;
                UserCache.AddNewUser(new User { Id = index, Name = "Alice_" + index });
                var user = UserCache.GetUserById(index);
                Console.WriteLine($"{user.Id},{user.Name}");
                Thread.Sleep(1000);
            }

            //while (true)
            //{
                var count = UserCache.GetUserCount();
                Console.WriteLine("已添加用户数量:" + count);
                Thread.Sleep(1000);
            //}


            var mutableDict = new Dictionary<int, string>
        {
            { 1, "one" },
            { 2, "two" },
            { 3, "three" }
        };
            var fro = mutableDict.ToFrozenDictionary();

            AssemblyCSharpBuilder builder = new();
            var func = builder
                .UseRandomLoadContext()
                .UseSimpleMode()
                .ConfigLoadContext(ctx => ctx
                    .AddReferenceAndUsingCode(typeof(Math))
                    .AddReferenceAndUsingCode(typeof(double)))
                .Add("public static class A{ public static double Invoke(double value){ return Math.Floor(value/0.3);  }}")
                .GetAssembly()
                .GetDelegateFromShortName<Func<double, double>>("A", "Invoke");

        }
    }
}
