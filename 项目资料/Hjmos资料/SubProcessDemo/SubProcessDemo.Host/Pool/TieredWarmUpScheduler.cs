using SubProcessDemo.Common.Models;
using SubProcessDemo.Host.Pool;

namespace SubProcessDemo.Host.Pool;

/// <summary>
/// 策略⑤：分级预热调度器。
/// 按模块优先级分批预热，平衡内存占用与启动速度。
/// </summary>
public class TieredWarmUpScheduler
{
    private readonly ProcessPoolManager _poolManager;
    private readonly List<ModuleConfig> _modules;

    public event Action<string>? OnLog;

    public TieredWarmUpScheduler(ProcessPoolManager poolManager, List<ModuleConfig> modules)
    {
        _poolManager = poolManager;
        _modules = modules;
    }

    /// <summary>按优先级分批调度预热</summary>
    public async Task ScheduleAsync()
    {
        var preWarmModules = _modules.Where(m => m.PreWarm).OrderBy(m => m.Priority).ToList();

        if (!preWarmModules.Any())
        {
            Log("没有需要预热的模块");
            return;
        }

        var grouped = preWarmModules.GroupBy(m => m.Priority).OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            var delay = group.Key switch
            {
                1 => 0,       // 优先级 1：立即
                2 => 3000,    // 优先级 2：延迟 3 秒
                _ => 8000,    // 优先级 3+：延迟 8 秒
            };

            if (delay > 0)
            {
                Log($"优先级 {group.Key}：等待 {delay / 1000} 秒后预热 [{string.Join(", ", group.Select(m => m.DisplayName))}]");
                await Task.Delay(delay);
            }
            else
            {
                Log($"优先级 {group.Key}：立即预热 [{string.Join(", ", group.Select(m => m.DisplayName))}]");
            }

            // 检查池中是否有 Ready 进程
            if (_poolManager.HasReadyProcess)
            {
                foreach (var module in group)
                {
                    var (success, ms) = await _poolManager.ActivateAsync(module.ModuleName);
                    if (success)
                        Log($"  {module.DisplayName} 预热激活完成 ({ms:F0}ms)");
                    else
                        Log($"  {module.DisplayName} 无可用进程，跳过");
                }
            }
            else
            {
                Log("  池中无 Ready 进程，跳过此批次");
            }
        }
    }

    private void Log(string msg) => OnLog?.Invoke($"[调度器] {msg}");
}
