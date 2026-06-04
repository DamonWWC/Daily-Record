using Riley.Core.Examples;

namespace Riley.Core.Examples;

/// <summary>
/// HTTP客户端示例程序入口
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Riley.Core HTTP客户端使用示例");
        Console.WriteLine("================================");

        try
        {
            // 基本使用示例
            await HttpClientUsageExamples.BasicUsageExample();
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // 多客户端示例
            await HttpClientUsageExamples.MultipleClientsExample();
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // 多BaseUrl基本示例
            await MultiBaseUrlUsageExamples.BasicMultiBaseUrlExample();
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // 命名客户端多BaseUrl示例
            await MultiBaseUrlUsageExamples.NamedClientMultiBaseUrlExample();
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // 动态服务配置示例
            await MultiBaseUrlUsageExamples.DynamicServiceConfigurationExample();
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // 微服务架构示例
            await MultiBaseUrlUsageExamples.MicroserviceArchitectureExample();
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // 环境切换示例
            await MultiBaseUrlUsageExamples.EnvironmentSwitchingExample();
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // 文件操作示例
            await HttpClientUsageExamples.FileOperationsExample();
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // 错误处理示例
            await HttpClientUsageExamples.ErrorHandlingExample();
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // WebSocket基本示例
            await WebSocketUsageExamples.BasicWebSocketExample();
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // WebSocket消息处理器示例
            await WebSocketUsageExamples.MessageHandlerExample();
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // WebSocket多客户端示例
            await WebSocketUsageExamples.MultipleClientsExample();
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // WebSocket重连机制示例
            await WebSocketUsageExamples.ReconnectionExample();
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // WebSocket实时聊天示例
            await WebSocketUsageExamples.RealtimeChatExample();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"程序执行出错: {ex.Message}");
            Console.WriteLine($"详细信息: {ex}");
        }

        Console.WriteLine("\n示例执行完成，按任意键退出...");
        Console.ReadKey();
    }
}
