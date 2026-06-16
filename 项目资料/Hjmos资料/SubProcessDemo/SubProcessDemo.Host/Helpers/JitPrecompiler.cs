using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SubProcessDemo.Host.Helpers;

/// <summary>
/// 策略④：JIT 预编译。
/// 使用 RuntimeHelpers.PrepareMethod 强制 JIT 编译关键路径方法，
/// 减少首次调用时的延迟。
/// </summary>
public static class JitPrecompiler
{
    /// <summary>预编译指定类型的所有方法</summary>
    public static (int methodCount, double elapsedMs) PrecompileType(Type type)
    {
        var sw = Stopwatch.StartNew();
        int count = 0;

        count += PrecompileMethods(type);

        // 递归处理嵌套类型
        foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
        {
            count += PrecompileMethods(nested);
        }

        sw.Stop();
        return (count, sw.Elapsed.TotalMilliseconds);
    }

    private static int PrecompileMethods(Type type)
    {
        int count = 0;
        var flags = BindingFlags.Public | BindingFlags.NonPublic
                  | BindingFlags.Instance | BindingFlags.Static
                  | BindingFlags.DeclaredOnly;

        foreach (var method in type.GetMethods(flags))
        {
            try
            {
                if (!method.IsAbstract && !method.ContainsGenericParameters)
                {
                    RuntimeHelpers.PrepareMethod(method.MethodHandle);
                    count++;
                }
            }
            catch { /* 部分方法可能因依赖缺失而无法预编译 */ }
        }

        return count;
    }

    /// <summary>预编译演示用的关键路径</summary>
    public static (int totalMethods, double elapsedMs) PrecompileCriticalPaths()
    {
        var sw = Stopwatch.StartNew();
        int total = 0;

        var types = new[]
        {
            typeof(System.Text.Json.JsonSerializer),
            typeof(System.Collections.Concurrent.ConcurrentDictionary<,>),
            typeof(System.IO.Pipes.NamedPipeServerStream),
            typeof(System.IO.MemoryMappedFiles.MemoryMappedFile),
        };

        foreach (var type in types)
        {
            var (count, _) = PrecompileType(type);
            total += count;
        }

        sw.Stop();
        return (total, sw.Elapsed.TotalMilliseconds);
    }
}
