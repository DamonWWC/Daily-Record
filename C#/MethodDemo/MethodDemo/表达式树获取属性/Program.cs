using System;
using System.Linq.Expressions;

namespace 表达式树获取属性
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Person person = new Person { Name = "Alice", Age = 30 };
            Func<Person, object> getProperty = ExpressionCache<Person>.GetPropertyAccessor("Name");
            string name = (string)getProperty(person);




            ParameterExpression a = Expression.Parameter(typeof(int), "a");
            ConstantExpression constant = Expression.Constant(1,typeof(int));
            ParameterExpression b = Expression.Parameter(typeof(int), "b");
            BinaryExpression body = Expression.Add(a, b);
            BinaryExpression body1 = Expression.Add(body, constant);


            var add = Expression.Lambda<Func<int, int, int>>(body, a, b).Compile();
            var aa = add.Invoke(1, 2);
            Console.WriteLine("Hello, World!");
        }
    }


    public class ExpressionCache<T>
    {
        private static readonly Dictionary<string, Func<T, object>> cache = new Dictionary<string, Func<T, object>>();
        public static Func<T,object> GetPropertyAccessor(string propertyName)
        {
            if(!cache.TryGetValue(propertyName, out Func<T, object>? value))
            {
                
                var parameter = Expression.Parameter(typeof(T), "obj");
                var property = Expression.Property(parameter, propertyName);
                var convert = Expression.Convert(property, typeof(object));
                var lambda = Expression.Lambda<Func<T, object>>(convert, parameter);

                var compiled = lambda.Compile();
                value = compiled;
                cache[propertyName] = value;

            }
            return value;
        }
    }
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
