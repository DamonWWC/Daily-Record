using System.Reflection;

namespace 消除If方法
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ExampleService service = new ExampleService();
            MethodInfo method = typeof(ExampleService).GetMethod("ExampleMethod");
            object[] parameters = new object[] { null, 123 }; // 这里故意传入一个null值以触发校验失败
            Validator.ValidateParameters(method, parameters); // 这行会抛出ArgumentNullException异常，因为param1为null且被标记为[NotNull]
                                                              // 如果校验通过，则继续执行方法体
                                                              // method.Invoke(service, parameters); // 实际使用时，在校验通过后再调用方法
            Console.WriteLine("Hello, World!");
        }



        // 定义一个自定义的校验特性
        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
        public class NotNullAttribute : Attribute
        {
            // 可以在这里添加一些自定义的逻辑
        }


        public class ExampleService
        {
            public void ExampleMethod(string param1, [NotNull] int param2)
            {
                // 方法体
            }
        }
        public class Validator
        {
            public static void ValidateParameters(MethodInfo method, object[] parameters)
            {
                ParameterInfo[] paramInfos = method.GetParameters();
                for (int i = 0; i < paramInfos.Length; i++)
                {
                    object param = parameters[i];
                    NotNullAttribute attr = paramInfos[i].GetCustomAttribute<NotNullAttribute>();
                    if (attr != null && param == null)
                    {
                        throw new ArgumentNullException(paramInfos[i].Name);
                    }
                    // 可以根据需要添加更多的校验逻辑
                }
            }
        }
    }
}
