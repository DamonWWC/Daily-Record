using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.Json;

namespace SubProcessDemo.Common.Config;

/// <summary>
/// 共享配置数据（模拟主进程中已存在的配置）
/// </summary>
public class SharedConfigData
{
    public string AuthToken { get; set; } = "mock-token-abc123";
    public string MQNameServer { get; set; } = "192.168.1.100:9876";
    public string WebSocketUrl { get; set; } = "ws://192.168.1.100:8080/ws";
    public string RestApiBase { get; set; } = "http://192.168.1.100:5000/api";
    public List<string> StationNames { get; set; } = new()
    {
        "北京南站", "北京西站", "北京北站", "北京东站", "清河站",
        "丰台站", "朝阳站", "海淀站", "通州站", "大兴站"
    };
    public Dictionary<string, string> AppSettings { get; set; } = new()
    {
        ["Theme"] = "Dark",
        ["Language"] = "zh-CN",
        ["LogLevel"] = "Info",
        ["MaxVideoStreams"] = "9",
        ["HeartbeatInterval"] = "60000"
    };
}

/// <summary>
/// 策略②：通过 MemoryMappedFile 在主进程和子进程之间共享配置数据。
/// 避免子进程重复进行网络请求和文件读取。
/// </summary>
public class SharedConfigManager : IDisposable
{
    private MemoryMappedFile? _mmf;
    private readonly string _mapName;
    private readonly Mutex _mutex;
    private const int MaxSize = 4 * 1024 * 1024; // 4MB

    public SharedConfigManager(int parentProcessId)
    {
        _mapName = $"SubProcessDemo_SharedConfig_{parentProcessId}";
        _mutex = new Mutex(false, $"SubProcessDemo_SharedConfig_Mutex_{parentProcessId}");
    }

    /// <summary>主进程调用：将配置写入共享内存</summary>
    public double WriteConfig(SharedConfigData config)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _mutex.WaitOne();
        try
        {
            var json = JsonSerializer.Serialize(config);
            var bytes = Encoding.UTF8.GetBytes(json);

            _mmf?.Dispose();
            _mmf = MemoryMappedFile.CreateOrOpen(_mapName, MaxSize);

            using var accessor = _mmf.CreateViewAccessor();
            // 前 4 字节：数据长度（big-endian）
            accessor.Write(0, (byte)(bytes.Length >> 24));
            accessor.Write(1, (byte)(bytes.Length >> 16));
            accessor.Write(2, (byte)(bytes.Length >> 8));
            accessor.Write(3, (byte)bytes.Length);
            // 后续字节：JSON 数据
            accessor.WriteArray(4, bytes, 0, bytes.Length);

            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    /// <summary>子进程调用：从共享内存读取配置</summary>
    public static (SharedConfigData? config, double elapsedMs) ReadConfig(int parentProcessId)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var mapName = $"SubProcessDemo_SharedConfig_{parentProcessId}";
        var mutexName = $"SubProcessDemo_SharedConfig_Mutex_{parentProcessId}";

        try
        {
            using var mutex = Mutex.OpenExisting(mutexName);
            mutex.WaitOne();
            try
            {
                using var mmf = MemoryMappedFile.OpenExisting(mapName);
                using var accessor = mmf.CreateViewAccessor();

                // 读取长度
                int length = (accessor.ReadByte(0) << 24)
                           | (accessor.ReadByte(1) << 16)
                           | (accessor.ReadByte(2) << 8)
                           | accessor.ReadByte(3);

                if (length <= 0 || length > MaxSize) return (null, sw.Elapsed.TotalMilliseconds);

                var bytes = new byte[length];
                accessor.ReadArray(4, bytes, 0, length);

                var json = Encoding.UTF8.GetString(bytes);
                var config = JsonSerializer.Deserialize<SharedConfigData>(json);

                sw.Stop();
                return (config, sw.Elapsed.TotalMilliseconds);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        catch
        {
            sw.Stop();
            return (null, sw.Elapsed.TotalMilliseconds);
        }
    }

    public void Dispose()
    {
        _mmf?.Dispose();
        _mutex.Dispose();
    }
}
