using SubProcessDemo.Common.Services;

namespace SubProcessDemo.Host.Services;

/// <summary>
/// 真实数据服务实现（主进程中使用）。
/// 模拟从数据库/API 获取数据。
/// </summary>
public class DataService : IDataService
{
    private readonly List<string> _stations = new()
    {
        "北京南站", "北京西站", "北京北站", "北京东站", "清河站",
        "丰台站", "朝阳站", "海淀站", "通州站", "大兴站"
    };

    public List<string> GetStations()
    {
        // 模拟网络延迟
        Thread.Sleep(50);
        return _stations;
    }

    public string GetUserInfo(string userId)
    {
        Thread.Sleep(30);
        return $"用户 {userId} — 角色: 调度员, 权限: 全站, 部门: 运营中心";
    }

    public int GetAlarmCount()
    {
        Thread.Sleep(20);
        return Random.Shared.Next(0, 15);
    }

    public Dictionary<string, string> GetSystemStatus()
    {
        Thread.Sleep(40);
        return new Dictionary<string, string>
        {
            ["CPU"] = $"{Random.Shared.Next(10, 45)}%",
            ["Memory"] = $"{Random.Shared.Next(40, 70)}%",
            ["Network"] = "正常",
            ["MQ"] = "已连接",
            ["WebSocket"] = "已连接",
            ["Database"] = "正常"
        };
    }
}
