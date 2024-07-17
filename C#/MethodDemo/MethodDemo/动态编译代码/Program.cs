using BenchmarkDotNet.Running;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System.Globalization;
using System.Reflection;

namespace 动态编译代码
{
    internal class Program
    {
        private static void Main1(string[] args)
        {
            Console.WriteLine(TimeSpan.FromMilliseconds(1).Humanize());
            Console.WriteLine(
                DateTime.Now
                    .AddHours(-1)
                    .Humanize(utcDate: false, culture: new CultureInfo("zh-CN"))
            );

            const string programText =
                @"using System;
            using System.Collections;
            using System.Linq;
            using System.Text;

            namespace HelloWorld
            {
                class Program
                {
                    static void Main(string[] args)
                    {
                        Console.WriteLine(""Hello, World!"");
                    }
                }
                class Demo
                {
                {
            }";

            var syntaxTree = CSharpSyntaxTree.ParseText(programText);
            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

            Console.WriteLine($"The tree is a {root.Kind()} node.");
            Console.WriteLine($"The tree has {root.Members.Count} elements in it.");
            Console.WriteLine($"The tree has {root.Usings.Count} using statements. They are:");
            foreach (UsingDirectiveSyntax element in root.Usings)
                Console.WriteLine($"\t{element.Name}");

            MemberDeclarationSyntax firstMember = root.Members[0];
            Console.WriteLine($"The first member is a {firstMember.Kind()}.");
            var helloWorldDeclaration = (NamespaceDeclarationSyntax)firstMember;

            Console.WriteLine(
                $"There are {helloWorldDeclaration.Members.Count} members declared in this namespace."
            );
            Console.WriteLine($"The first member is a {helloWorldDeclaration.Members[0].Kind()}.");

            var programDeclaration = (ClassDeclarationSyntax)helloWorldDeclaration.Members[0];
            Console.WriteLine(
                $"There are {programDeclaration.Members.Count} members declared in the {programDeclaration.Identifier} class."
            );
            Console.WriteLine($"The first member is a {programDeclaration.Members[0].Kind()}.");
            var mainDeclaration = (MethodDeclarationSyntax)programDeclaration.Members[0];

            Console.WriteLine(
                $"The return type of the {mainDeclaration.Identifier} method is {mainDeclaration.ReturnType}."
            );
            Console.WriteLine(
                $"The method has {mainDeclaration.ParameterList.Parameters.Count} parameters."
            );
            foreach (ParameterSyntax item in mainDeclaration.ParameterList.Parameters)
                Console.WriteLine($"The type of the {item.Identifier} parameter is {item.Type}.");
            Console.WriteLine($"The body text of the {mainDeclaration.Identifier} method follows:");
            Console.WriteLine(mainDeclaration.Body?.ToFullString());

            var argsParameter = mainDeclaration.ParameterList.Parameters[0];

            var firstParameters =
                from methodDeclaration in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                where methodDeclaration.Identifier.ValueText == "Main"
                select methodDeclaration.ParameterList.Parameters.First();

            var argsParameter2 = firstParameters.Single();

            Console.WriteLine(argsParameter == argsParameter2);

            //new BB();

            //string str1 = "123" + "abc";
            //int a = 123;
            //string str2 = a + "abc";
            //string str3 = "123abc";

            //Console.WriteLine(str1 == str2);//true
            //Console.WriteLine(str1.Equals(str2));//true
            //Console.WriteLine(ReferenceEquals(str1, str2));//false
            //Console.WriteLine(ReferenceEquals(str1, str3));//true
        }

        //private static void Main(string[] args)
        //{
        //    string code = @"
        //using System;
        //public class DynamicClass
        //{
        //    public string Execute()
        //    {
        //        return ""Hello from dynamically compiled code!"";
        //    }
        //}";

        //    // 创建一个语法树
        //    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

        //    // 引用程序集
        //    MetadataReference[] references = new MetadataReference[]
        //    {
        //    MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        //    };

        //    // 创建编译器选项
        //    CSharpCompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        //    // 创建编译对象
        //    CSharpCompilation compilation = CSharpCompilation.Create("DynamicAssembly", new[] { syntaxTree }, references, options);

        //    // 编译代码
        //    using (var ms = new MemoryStream())
        //    {
        //        EmitResult result = compilation.Emit(ms);

        //        if (result.Success)
        //        {
        //            // 加载编译的程序集
        //            ms.Seek(0, SeekOrigin.Begin);
        //            Assembly assembly = Assembly.Load(ms.ToArray());

        //            // 创建动态类型的实例并执行方法
        //            Type type = assembly.GetType("DynamicClass");
        //            object obj = Activator.CreateInstance(type);
        //            MethodInfo method = type.GetMethod("Execute");
        //            string output = (string)method.Invoke(obj, null);

        //            Console.WriteLine(output);
        //        }
        //        else
        //        {
        //            // 显示编译错误
        //            foreach (Diagnostic diagnostic in result.Diagnostics)
        //            {
        //                Console.WriteLine(diagnostic.ToString());
        //            }
        //        }
        //    }
        //}
    }

    public class AA
    {
        public AA()
        {
            printFields();
        }

        public virtual void printFields() { }
    }

    public class BB : AA
    {
        private int x = 1;
        private int y;

        public BB()
        {
            y = -1;
        }

        public override void printFields()
        {
            Console.WriteLine("x={0},y={1}", x, y);
        }
    }
}
