using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace WPFDeveloper.SourceGenerators
{
    /// <summary>
    /// 依赖注入自动注册代码生成器
    /// </summary>
    [Generator]
    public class DependencyInjectionGenerator : IIncrementalGenerator
    {
        private const string ServiceRegistrationAttributeName = "WPFDeveloper.Attributes.ServiceRegistrationAttribute";
        private const string ServiceLifetimeEnumName = "WPFDeveloper.Attributes.ServiceLifetime";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 添加调试支持
#if DEBUG
            if (!Debugger.IsAttached)
            {
                //Debugger.Launch();
            }
#endif
            // 查找所有带有 ServiceRegistration 特性的类
            var serviceProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    ServiceRegistrationAttributeName,
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (context, _) => GetServiceInfo(context))
                .Where(static m => m is not null);

            // 收集所有服务信息并生成代码
            context.RegisterSourceOutput(serviceProvider.Collect(), static (context, services) =>
            {
                if (services.IsDefaultOrEmpty)
                    return;

                var source = GenerateExtensionMethod(services);
                context.AddSource("ServiceRegistrationExtensions.g.cs", SourceText.From(source, Encoding.UTF8));
            });
        }

        private static ServiceInfo? GetServiceInfo(GeneratorAttributeSyntaxContext context)
        {
            try
            {
                if (context.TargetNode is not ClassDeclarationSyntax classDeclaration)
                    return null;

                var classSymbol = context.TargetSymbol as INamedTypeSymbol;
                if (classSymbol == null)
                    return null;

                var attribute = context.Attributes.FirstOrDefault();
                if (attribute == null)
                    return null;

                // 解析特性参数
                var lifetime = GetLifetime(attribute);
                var serviceType = GetServiceType(attribute, classSymbol);
                var replaceExisting = GetReplaceExisting(attribute);

                return new ServiceInfo(
                    classSymbol.ToDisplayString(),
                    serviceType,
                    lifetime,
                    replaceExisting,
                    classSymbol.ContainingNamespace.ToDisplayString()
                );
            }
            catch
            {
                // 忽略解析错误，返回 null
                return null;
            }
        }

        private static string GetLifetime(AttributeData attribute)
        {
            // 查找 Lifetime 属性
            var lifetimeArg = attribute.NamedArguments.FirstOrDefault(arg => arg.Key == "Lifetime");
            if (lifetimeArg.Key != null && lifetimeArg.Value.Value != null)
            {
                return MapLifetime((int)lifetimeArg.Value.Value);
            }

            // 检查构造函数参数 - 可能是第一个或第二个参数
            foreach (var arg in attribute.ConstructorArguments)
            {
                if (arg.Type?.Name == "ServiceLifetime" && arg.Value != null)
                {
                    return MapLifetime((int)arg.Value);
                }
            }

            return "Transient"; // 默认值
        }

        private static string GetServiceType(AttributeData attribute, INamedTypeSymbol implementationType)
        {
            // 查找 ServiceType 属性或第一个Type参数
            var serviceTypeArg = attribute.NamedArguments.FirstOrDefault(arg => arg.Key == "ServiceType");
            if (serviceTypeArg.Key != null && serviceTypeArg.Value.Value is INamedTypeSymbol serviceTypeSymbol)
            {
                return serviceTypeSymbol.ToDisplayString();
            }

            // 检查构造函数中的Type参数
            foreach (var arg in attribute.ConstructorArguments)
            {
                if (arg.Type?.Name == "Type" && arg.Value is INamedTypeSymbol typeSymbol)
                {
                    return typeSymbol.ToDisplayString();
                }
            }

            // 如果没有显式指定服务类型，使用实现类型本身
            // 不要自动选择接口，这可能导致注册到错误的接口
            return implementationType.ToDisplayString();
        }

        private static bool GetReplaceExisting(AttributeData attribute)
        {
            var replaceArg = attribute.NamedArguments.FirstOrDefault(arg => arg.Key == "ReplaceExisting");
            if (replaceArg.Key != null && replaceArg.Value.Value is bool replace)
            {
                return replace;
            }
            return false; // 默认值
        }

        private static string MapLifetime(int lifetimeValue)
        {
            return lifetimeValue switch
            {
                0 => "Transient",
                1 => "Scoped",
                2 => "Singleton",
                _ => "Transient"
            };
        }

        private static string GenerateExtensionMethod(ImmutableArray<ServiceInfo?> services)
        {
            var validServices = services.Where(s => s != null).Cast<ServiceInfo>().ToList();
            
            if (!validServices.Any())
                return string.Empty;

            var sb = new StringBuilder();

            // 生成文件头
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine("// 此文件由 DependencyInjectionGenerator 自动生成");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            
            // 收集所有需要的命名空间
            var namespaces = validServices
                .SelectMany(s => new[] { s.Namespace, GetNamespace(s.ServiceType), GetNamespace(s.ImplementationType) })
                .Where(ns => !string.IsNullOrEmpty(ns) && ns != "System" && ns != "Microsoft.Extensions.DependencyInjection")
                .Distinct()
                .OrderBy(ns => ns);

            foreach (var ns in namespaces)
            {
                sb.AppendLine($"using {ns};");
            }

            sb.AppendLine();
            sb.AppendLine("namespace WPFDeveloper.Extensions");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 自动生成的服务注册扩展方法");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static partial class ServiceCollectionExtensions");
            sb.AppendLine("    {");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 添加由 SourceGenerator 自动发现的服务");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"services\">服务集合</param>");
            sb.AppendLine("        /// <returns>服务集合</returns>");
            sb.AppendLine("        public static IServiceCollection AddGeneratedServices(this IServiceCollection services)");
            sb.AppendLine("        {");

            // 按生命周期分组服务
            var groupedServices = validServices.GroupBy(s => s.Lifetime);

            foreach (var group in groupedServices.OrderBy(g => g.Key))
            {
                sb.AppendLine($"            // {group.Key} 服务");
                
                foreach (var service in group)
                {
                    var methodName = $"Add{service.Lifetime}";
                    var serviceTypeName = GetTypeName(service.ServiceType);
                    var implementationTypeName = GetTypeName(service.ImplementationType);
                    
                    if (service.ServiceType == service.ImplementationType)
                    {
                        // 注册为自身
                        sb.AppendLine($"            services.{methodName}<{serviceTypeName}>();");
                    }
                    else
                    {
                        // 注册接口和实现
                        sb.AppendLine($"            services.{methodName}<{serviceTypeName}, {implementationTypeName}>();");
                    }
                }
                sb.AppendLine();
            }

            sb.AppendLine("            return services;");
            sb.AppendLine("        }");
            
            // 生成服务信息方法
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 获取所有自动注册的服务信息");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <returns>服务信息列表</returns>");
            sb.AppendLine("        public static IReadOnlyList<GeneratedServiceInfo> GetGeneratedServiceInfos()");
            sb.AppendLine("        {");
            sb.AppendLine("            return new[]");
            sb.AppendLine("            {");

            foreach (var service in validServices)
            {
                var serviceTypeName = GetTypeName(service.ServiceType);
                var implementationTypeName = GetTypeName(service.ImplementationType);
                
                sb.AppendLine($"                new GeneratedServiceInfo(");
                sb.AppendLine($"                    serviceType: typeof({serviceTypeName}),");
                sb.AppendLine($"                    implementationType: typeof({implementationTypeName}),");
                sb.AppendLine($"                    lifetime: ServiceLifetime.{service.Lifetime}),");
            }

            sb.AppendLine("            };");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();

            // 生成服务信息类
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 生成的服务信息");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public class GeneratedServiceInfo");
            sb.AppendLine("    {");
            sb.AppendLine("        public Type ServiceType { get; }");
            sb.AppendLine("        public Type ImplementationType { get; }");
            sb.AppendLine("        public ServiceLifetime Lifetime { get; }");
            sb.AppendLine();
            sb.AppendLine("        public GeneratedServiceInfo(Type serviceType, Type implementationType, ServiceLifetime lifetime)");
            sb.AppendLine("        {");
            sb.AppendLine("            ServiceType = serviceType;");
            sb.AppendLine("            ImplementationType = implementationType;");
            sb.AppendLine("            Lifetime = lifetime;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");

            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string GetNamespace(string fullTypeName)
        {
            var lastDotIndex = fullTypeName.LastIndexOf('.');
            return lastDotIndex > 0 ? fullTypeName.Substring(0, lastDotIndex) : string.Empty;
        }

        private static string GetTypeName(string fullTypeName)
        {
            var lastDotIndex = fullTypeName.LastIndexOf('.');
            return lastDotIndex > 0 ? fullTypeName.Substring(lastDotIndex + 1) : fullTypeName;
        }

        private class ServiceInfo
        {
            public string ImplementationType { get; }
            public string ServiceType { get; }
            public string Lifetime { get; }
            public bool ReplaceExisting { get; }
            public string Namespace { get; }

            public ServiceInfo(string implementationType, string serviceType, string lifetime, bool replaceExisting, string @namespace)
            {
                ImplementationType = implementationType;
                ServiceType = serviceType;
                Lifetime = lifetime;
                ReplaceExisting = replaceExisting;
                Namespace = @namespace;
            }
        }
    }
}
