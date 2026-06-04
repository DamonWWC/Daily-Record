using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace 动态编译代码
{
    internal class Program2
    {
        private static void Main2(string[] args)
        {
            const string programText =
 @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TopLevel
{
    using Microsoft;
    using System.ComponentModel;

    namespace Child1
    {
        using Microsoft.Win32;
        using System.Runtime.InteropServices;

        class Foo { }
    }

    namespace Child2
    {
        using System.CodeDom;
        using Microsoft.CSharp;

        class Bar { }
    }
}";


            SyntaxTree syntaxTree = CSharpSyntaxTree .ParseText(programText);
            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();


            var collector = new UsingCollector();
            collector.Visit(root);
            foreach (var directive in collector.Usings)
            {
                WriteLine(directive.Name);
            }
        }
    }
    class UsingCollector: CSharpSyntaxWalker
    {
        public ICollection<UsingDirectiveSyntax> Usings { get; } = new List<UsingDirectiveSyntax>();
        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            WriteLine($"\tVisitUsingDirective called with {node.Name}.");
            if (node.Name.ToString() != "System" &&
                !node.Name.ToString().StartsWith("System."))
            {
                WriteLine($"\t\tSuccess. Adding {node.Name}.");
                this.Usings.Add(node);
            }
        }
    }
}