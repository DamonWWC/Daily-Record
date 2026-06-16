namespace SubProcessDemo.Common.Services;

/// <summary>
/// 演示用数据服务接口。
/// 主进程有真实实现，子进程通过 ServiceProxy 代理调用。
/// </summary>
public interface IDataService
{
    /// <summary>获取站点列表</summary>
    List<string> GetStations();

    /// <summary>获取用户信息</summary>
    string GetUserInfo(string userId);

    /// <summary>获取告警数量</summary>
    int GetAlarmCount();

    /// <summary>获取系统状态</summary>
    Dictionary<string, string> GetSystemStatus();
}
