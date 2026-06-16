using System.Diagnostics;
using SubProcessDemo.Common.Models;

namespace SubProcessDemo.Worker;

/// <summary>
/// 模拟模块加载耗时。
/// 冷启动模拟完整流程，热启动仅模拟激活阶段。
/// </summary>
public static class ModuleSimulator
{
    /// <summary>模拟冷启动（~2000ms）</summary>
    public static async Task SimulateColdStart(string moduleName, Action<string>? log = null)
    {
        var sw = Stopwatch.StartNew();
        var module = DemoModules.All.FirstOrDefault(m => m.ModuleName == moduleName);
        var extraMs = module?.SimulatedLoadMs ?? 200;

        Step(log, sw, "阶段 1/6：进程创建 + OS 初始化...", 200);
        Step(log, sw, "阶段 2/6：CLR Runtime 初始化...", 400);
        Step(log, sw, "阶段 3/6：公共程序集加载 (Prism, Newtonsoft, LiveCharts...)", 350);
        Step(log, sw, "阶段 4/6：读取配置文件 + 网络初始化...", 500);
        Step(log, sw, "阶段 5/6：服务注册 (45+ Singletons)...", 300);
        Step(log, sw, $"阶段 6/6：模块初始化 + UI 首帧渲染...", extraMs);

        log?.Invoke($"冷启动总计：{sw.ElapsedMilliseconds}ms");
    }

    /// <summary>模拟热启动激活（~200-350ms）</summary>
    public static async Task SimulateWarmActivation(string moduleName, int loadMs, Action<string>? log = null)
    {
        log?.Invoke("模块 RegisterTypes + OnInitialized...");
        await Task.Delay(loadMs / 2);

        log?.Invoke("创建 View + ViewModel + 首帧渲染...");
        await Task.Delay(loadMs / 2);

        log?.Invoke("激活完成");
    }

    private static void Step(Action<string>? log, Stopwatch sw, string desc, int delayMs)
    {
        log?.Invoke($"{desc}");
        Thread.Sleep(delayMs);
        log?.Invoke($"  ({sw.ElapsedMilliseconds}ms)");
    }
}
