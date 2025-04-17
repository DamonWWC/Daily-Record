using System;
using System.Linq.Expressions;

namespace 表达式树获取属性
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Person person = new Person { Name = "Alice", Age = 30 };

            var getter = CreateGetter<Person, string>("Name");
            Console.WriteLine(getter(person));

            var setter = CreateSetter<Person, int>("Age");
            setter(person, 40);
            Console.WriteLine(person.Age);

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

        static Func<T,TProperty> CreateGetter<T,TProperty>(string propertyName)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(param, propertyName);
            var lambda=Expression.Lambda<Func<T, TProperty>>(property, param);
            return lambda.Compile();
        }

        static Action<T,TProperty> CreateSetter<T,TProperty>(string propertyName)
        {
            var objParam = Expression.Parameter(typeof(T), "obj");
            var valueParam = Expression.Parameter(typeof(TProperty), "value");
            var propertry = Expression.Property(objParam, propertyName);
            var assign= Expression.Assign(propertry, valueParam);
            var lambda= Expression.Lambda<Action<T, TProperty>>(assign, objParam, valueParam);
            return lambda.Compile();
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
