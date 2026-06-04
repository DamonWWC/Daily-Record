using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Plugin
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MethodName();
            Console.WriteLine("Hello, World!");
        }
        static void MethodName( [CallerMemberName] string memberName = "", [CallerFilePath] string path="")
        {
            var aa = Mame();
            var bb = GetFullCallStack();
            Console.WriteLine(memberName);
        }
        static string Mame()
        {
            var frame = new StackTrace().GetFrame(1);
            var aa = $"{frame.GetMethod().DeclaringType.Name}.{frame.GetMethod().Name}";
            return aa;
        }
        public static string GetFullCallStack()
        {
            var stack = new StackTrace();
            return string.Join(" -> ",
                stack.GetFrames()
                    .Skip(1) // 跳过当前方法
                    .Take(3) // 获取最近3层调用
                    .Select(f => $"{f.GetMethod().DeclaringType.Name}.{f.GetMethod().Name}"));
        }
    }
}
