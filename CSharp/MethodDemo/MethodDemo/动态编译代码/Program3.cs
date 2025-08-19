using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace 动态编译代码
{
    record demo(string Id, string Name);

    internal class TrainPIDSInfoComparer : IEqualityComparer<demo>
    {
        public bool Equals(demo x, demo y)
        {
            if (object.ReferenceEquals(x, y))
                return true;
            return x.Id == y.Id;
        }

        public int GetHashCode(demo obj)
        {
            return obj.Id.GetHashCode();
        }
    }

    internal class Program3
    {
        private static void Main(string[] args)
        {
            List<demo> a = new List<demo>();
            //List<demo> a = new List<demo> { new demo("1", "aa"), new demo("2", "bb"), new demo("3", "cc"), new demo("4", "dd") };
            List<demo> b = new List<demo>
            {
                new demo("3", "c"),
                new demo("4", "dd"),
                new demo("7", "c"),
                new demo("9", "d")
            };

             IEnumerable<int> GetLargeNumbers()
            {
                int a = 1;
                for (int i = 0; i < 1000000; i++)
                {
                    yield return i;
                }
            }

            var res = GetLargeNumbers();
            res.Select(item => item).ToArray();

            foreach(var it in GetLargeNumbers())
            {

            }

            var c = b.Except(a, new TrainPIDSInfoComparer()).ToList();

            const string programText =
                @"using System;
using System.Collections.Generic;
using System.Text;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, World!"");

Console.WriteLine(""Hello, World111!"");
        }
    }
}";

            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);

            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            var compilation = CSharpCompilation
                .Create("HelloWorld")
                .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
                .AddSyntaxTrees(tree);
            SemanticModel model = compilation.GetSemanticModel(tree);

            // Use the syntax tree to find "using System;"
            UsingDirectiveSyntax usingSystem = root.Usings[0];
            NameSyntax systemName = usingSystem.Name;

            // Use the semantic model for symbol information:
            SymbolInfo nameInfo = model.GetSymbolInfo(systemName);

            var systemSymbol = (INamespaceSymbol?)nameInfo.Symbol;
            if (systemSymbol?.GetNamespaceMembers() is not null)
            {
                foreach (INamespaceSymbol ns in systemSymbol?.GetNamespaceMembers()!)
                {
                    Console.WriteLine(ns);
                }
            }

            var aa = root.DescendantNodes();
            // Use the syntax model to find the literal string:
            LiteralExpressionSyntax helloWorldString = root.DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .Last();

            // Use the semantic model for type information:
            TypeInfo literalInfo = model.GetTypeInfo(helloWorldString);

            var stringTypeSymbol = (INamedTypeSymbol?)literalInfo.Type;

            var allMembers = stringTypeSymbol?.GetMembers();
            var methods = allMembers?.OfType<IMethodSymbol>();

            var publicStringReturningMethods = methods?.Where(
                m =>
                    SymbolEqualityComparer.Default.Equals(m.ReturnType, stringTypeSymbol)
                    && m.DeclaredAccessibility == Accessibility.Public
            );

            var distinctMethods = publicStringReturningMethods?.Select(m => m.Name).Distinct();

            foreach (
                string name in (
                    from method in stringTypeSymbol?.GetMembers().OfType<IMethodSymbol>()
                    where
                        SymbolEqualityComparer.Default.Equals(method.ReturnType, stringTypeSymbol)
                        && method.DeclaredAccessibility == Accessibility.Public
                    select method.Name
                ).Distinct()
            )
            {
                Console.WriteLine(name);
            }
        }
    }
}
