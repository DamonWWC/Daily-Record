namespace SubProcessDemo.Common.Models;

/// <summary>
/// 模块配置（对应设计文档中的分级加载配置）
/// </summary>
public class ModuleConfig
{
    public string ModuleName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public int Priority { get; set; } = 3;
    public bool PreWarm { get; set; } = false;
    /// <summary>模拟加载耗时（毫秒），用于演示对比</summary>
    public int SimulatedLoadMs { get; set; } = 200;
}

/// <summary>
/// 预定义的演示模块列表
/// </summary>
public static class DemoModules
{
    public static readonly List<ModuleConfig> All = new()
    {
        new() { ModuleName = "StationModule", DisplayName = "全息感知车站", Priority = 1, PreWarm = true, SimulatedLoadMs = 300 },
        new() { ModuleName = "EmergencyModule", DisplayName = "应急指挥", Priority = 1, PreWarm = true, SimulatedLoadMs = 250 },
        new() { ModuleName = "CctvModule", DisplayName = "视频中心", Priority = 2, PreWarm = true, SimulatedLoadMs = 350 },
        new() { ModuleName = "DataBoardModule", DisplayName = "数据看板", Priority = 3, PreWarm = false, SimulatedLoadMs = 200 },
        new() { ModuleName = "DeviceModule", DisplayName = "设备监控", Priority = 3, PreWarm = false, SimulatedLoadMs = 180 },
        new() { ModuleName = "IntelligentAppModule", DisplayName = "智能应用", Priority = 4, PreWarm = false, SimulatedLoadMs = 150 },
    };
}
