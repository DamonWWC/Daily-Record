using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using System.IO;
using System.Windows;
using WPFDeveloper.Extensions;
using WPFDeveloper.Services;
using WPFDeveloper.Views;

namespace WPFDeveloper
{
    /// <summary>
    /// WPF开发者应用程序主类
    /// 在保留现有Services和ServiceProvider结构的基础上进行优化
    /// </summary>
    public partial class App : Application
    {
        private static readonly Logger _nlogLogger = LogManager.GetCurrentClassLogger();
        private static IServiceProvider? _serviceProvider;
        private static IConfiguration? _configuration;

        /// <summary>
        /// 全局服务集合（保留原有结构）
        /// </summary>
        public static IServiceCollection Services { get; } = new ServiceCollection();

        /// <summary>
        /// 全局服务提供者（保留原有结构，但优化实现）
        /// </summary>
        public static IServiceProvider ServiceProvider 
        { 
            get 
            {              
                _serviceProvider = Services.BuildServiceProvider();               
                return _serviceProvider;
            }
        }

        /// <summary>
        /// 应用程序配置
        /// </summary>
        public static IConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = BuildConfiguration();
                }
                return _configuration;
            }
        }

        /// <summary>
        /// 应用程序启动
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                // 设置全局异常处理
                SetupGlobalExceptionHandling();

                // 配置NLog
                ConfigureNLog();

                _nlogLogger.Info("=== WPF Developer 应用程序启动 ===");
                _nlogLogger.Info($"启动参数: {string.Join(" ", e.Args)}");

                // 配置服务
                ConfigureServices(Services);
                // 重新构建ServiceProvider以包含所有服务
              
                _nlogLogger.Info("服务配置完成，开始初始化主窗口");

                // 验证关键服务注册
                //ValidateServices();

                // 显示主窗口
                ShowMainWindow();

                _nlogLogger.Info("应用程序启动完成");

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                _nlogLogger.Fatal(ex, "应用程序启动失败");
                HandleStartupError(ex);
            }
        }

        /// <summary>
        /// 应用程序退出
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _nlogLogger.Info($"应用程序正在退出，退出代码: {e.ApplicationExitCode}");

                // 清理资源
                CleanupResources();

                _nlogLogger.Info("应用程序已安全退出");
            }
            catch (Exception ex)
            {
                _nlogLogger.Error(ex, "应用程序退出时发生错误");
            }
            finally
            {
                // 刷新并关闭NLog
                LogManager.Shutdown();
                base.OnExit(e);
            }
        }

        /// <summary>
        /// 配置NLog
        /// </summary>
        private static void ConfigureNLog()
        {
            try
            {
                // 确保日志目录存在
                var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // 加载NLog配置
                var nlogConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NLog.config");
                if (File.Exists(nlogConfigPath))
                {
                    LogManager.Setup().LoadConfigurationFromFile(nlogConfigPath);
                    _nlogLogger.Debug("NLog配置文件加载成功");
                }
                else
                {
                    // 如果配置文件不存在，使用代码配置
                    ConfigureNLogProgrammatically();
                    _nlogLogger.Debug("使用程序化配置NLog");
                }
            }
            catch (Exception ex)
            {
                // NLog配置失败时使用默认配置
                System.Diagnostics.Debug.WriteLine($"NLog配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 程序化配置NLog（备用方案）
        /// </summary>
        private static void ConfigureNLogProgrammatically()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // 控制台目标
            var consoleTarget = new NLog.Targets.ColoredConsoleTarget("console")
            {
                Layout = "${longdate} [${level:uppercase=true}] ${logger:shortName=true} - ${message} ${exception:format=tostring}"
            };

            // 文件目标
            var fileTarget = new NLog.Targets.FileTarget("file")
            {
                FileName = "logs/WPFDeveloper-${shortdate}.log",
                Layout = "${longdate} [${level:uppercase=true}] [${threadid}] ${logger} - ${message} ${exception:format=tostring}",
                ArchiveEvery = NLog.Targets.FileArchivePeriod.Day,
                MaxArchiveFiles = 30
            };

            // 添加规则
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, consoleTarget);
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, fileTarget);

            LogManager.Configuration = config;
        }

        /// <summary>
        /// 构建配置
        /// </summary>
        private static IConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            // 添加环境特定配置
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
            builder.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

            // 添加环境变量和命令行参数
            builder.AddEnvironmentVariables();
            
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                builder.AddCommandLine(args);
            }

            return builder.Build();
        }

        /// <summary>
        /// 配置服务
        /// </summary>
        private static void ConfigureServices(IServiceCollection services)
        {
            _nlogLogger.Debug("开始配置服务");

            // 注册配置
            services.AddSingleton(Configuration);

            // 配置日志
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                builder.AddNLog();
            });

            // 添加 WPF 开发者服务（包含自动注册）
            services.AddWpfDeveloperServices();

            // 使用自动导航注册表替代原来的 JSON 配置
            // 注意：不再手动注册 INavigationRegistry，因为会与 SourceGenerator 生成的注册冲突
            // services.AddSingleton<INavigationRegistry, AutoNavigationRegistry>();

            // 添加应用程序特定服务
            AddApplicationServices(services);

            _nlogLogger.Debug($"服务配置完成，共注册 {services.Count} 个服务");
        }

        /// <summary>
        /// 添加应用程序特定服务
        /// </summary>
        private static void AddApplicationServices(IServiceCollection services)
        {
            // 添加性能监控服务
            services.AddSingleton<IPerformanceMonitor, PerformanceMonitor>();

            // 添加应用程序信息服务
            services.AddSingleton<IApplicationInfo, ApplicationInfo>();

            // 可以在这里添加其他应用程序特定的服务
        }

        /// <summary>
        /// 验证关键服务注册
        /// </summary>
        private static void ValidateServices()
        {
            try
            {
                _nlogLogger.Debug("开始验证服务注册");

                // 验证关键服务
                var mainWindow = ServiceProvider.GetService<MainWindow>();
                var homeView = ServiceProvider.GetService<HomeView>();
                var aboutView = ServiceProvider.GetService<AboutView>();
                var logger = ServiceProvider.GetService<ILogger<App>>();

                _nlogLogger.Info($"服务验证结果: MainWindow={mainWindow != null}, HomeView={homeView != null}, AboutView={aboutView != null}, Logger={logger != null}");

                if (mainWindow == null)
                {
                    throw new InvalidOperationException("MainWindow 服务未正确注册");
                }
            }
            catch (Exception ex)
            {
                _nlogLogger.Error(ex, "服务验证失败");
                throw;
            }
        }

        /// <summary>
        /// 显示主窗口
        /// </summary>
        private static void ShowMainWindow()
        {
            try
            {
                var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                
                // 设置主窗口属性
                Current.MainWindow = mainWindow;
                
                // 显示窗口
                mainWindow.Show();

                _nlogLogger.Info("主窗口显示成功");
            }
            catch (Exception ex)
            {
                _nlogLogger.Error(ex, "显示主窗口失败");
                throw;
            }
        }

        /// <summary>
        /// 设置全局异常处理
        /// </summary>
        private void SetupGlobalExceptionHandling()
        {
            // UI线程异常
            DispatcherUnhandledException += (sender, e) =>
            {
                _nlogLogger.Error(e.Exception, "UI线程发生未处理异常");
                HandleGlobalException(e.Exception);
                e.Handled = true; // 防止应用程序崩溃
            };

            // 非UI线程异常
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var exception = e.ExceptionObject as Exception;
                _nlogLogger.Fatal(exception, "应用程序域发生未处理异常，终止状态: {IsTerminating}", e.IsTerminating);
                
                if (exception != null && !e.IsTerminating)
                {
                    HandleGlobalException(exception);
                }
            };

            // Task异常
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                _nlogLogger.Error(e.Exception, "Task发生未观察异常");
                HandleGlobalException(e.Exception);
                e.SetObserved(); // 标记异常已处理
            };

            _nlogLogger.Debug("全局异常处理设置完成");
        }

        /// <summary>
        /// 处理启动错误
        /// </summary>
        private static void HandleStartupError(Exception ex)
        {
            var message = $"应用程序启动失败：\n\n{ex.Message}\n\n详细信息请查看日志文件。";
            
            MessageBox.Show(
                message,
                "启动错误",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Environment.Exit(1);
        }

        /// <summary>
        /// 处理全局异常
        /// </summary>
        private static void HandleGlobalException(Exception ex)
        {
            try
            {
                var message = $"发生未处理异常：\n\n{ex.Message}\n\n是否继续运行应用程序？\n\n详细信息请查看日志文件。";
                
                var result = MessageBox.Show(
                    message,
                    "未处理异常",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    _nlogLogger.Info("用户选择退出应用程序");
                    Current.Shutdown();
                }
            }
            catch (Exception innerEx)
            {
                _nlogLogger.Fatal(innerEx, "异常处理器本身发生异常");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private static void CleanupResources()
        {
            try
            {
                // 清理ServiceProvider
                if (_serviceProvider is IDisposable disposableProvider)
                {
                    disposableProvider.Dispose();
                }

                _nlogLogger.Debug("资源清理完成");
            }
            catch (Exception ex)
            {
                _nlogLogger.Error(ex, "资源清理时发生错误");
            }
        }

        /// <summary>
        /// 获取服务实例（保留原有方法）
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>服务实例</returns>
        public static T GetService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// 尝试获取服务实例（保留原有方法）
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>服务实例或null</returns>
        public static T? GetOptionalService<T>() where T : class
        {
            return ServiceProvider.GetService<T>();
        }

        /// <summary>
        /// 获取配置值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        public static string GetConfigValue(string key, string defaultValue = "")
        {
            return Configuration[key] ?? defaultValue;
        }

        /// <summary>
        /// 获取应用程序信息
        /// </summary>
        /// <returns>应用程序信息</returns>
        public static string GetApplicationInfo()
        {
            var name = GetConfigValue("AppSettings:ApplicationName", "WPF Developer");
            var version = GetConfigValue("AppSettings:Version", "1.0.0");
            var environment = GetConfigValue("AppSettings:Environment", "Unknown");
            
            return $"{name} v{version} ({environment})";
        }
    }

    /// <summary>
    /// 性能监控服务接口
    /// </summary>
    public interface IPerformanceMonitor
    {
        void StartMonitoring();
        void StopMonitoring();
        void LogPerformanceMetric(string name, double value, string unit = "ms");
    }

    /// <summary>
    /// 性能监控服务实现
    /// </summary>
    public class PerformanceMonitor : IPerformanceMonitor
    {
        private readonly ILogger<PerformanceMonitor> _logger;
        private readonly System.Diagnostics.Stopwatch _appStopwatch;

        public PerformanceMonitor(ILogger<PerformanceMonitor> logger)
        {
            _logger = logger;
            _appStopwatch = System.Diagnostics.Stopwatch.StartNew();
        }

        public void StartMonitoring()
        {
            _logger.LogInformation("性能监控已启动");
            LogPerformanceMetric("ApplicationStartup", _appStopwatch.ElapsedMilliseconds);
        }

        public void StopMonitoring()
        {
            _logger.LogInformation("性能监控已停止，总运行时间: {TotalTime}ms", _appStopwatch.ElapsedMilliseconds);
        }

        public void LogPerformanceMetric(string name, double value, string unit = "ms")
        {
            _logger.LogInformation("Performance: {MetricName} = {Value}{Unit}", name, value, unit);
        }
    }

    /// <summary>
    /// 应用程序信息服务接口
    /// </summary>
    public interface IApplicationInfo
    {
        string Name { get; }
        string Version { get; }
        string Environment { get; }
        DateTime StartTime { get; }
    }

    /// <summary>
    /// 应用程序信息服务实现
    /// </summary>
    public class ApplicationInfo : IApplicationInfo
    {
        public string Name { get; }
        public string Version { get; }
        public string Environment { get; }
        public DateTime StartTime { get; }

        public ApplicationInfo(IConfiguration configuration)
        {
            Name = configuration["AppSettings:ApplicationName"] ?? "WPF Developer";
            Version = configuration["AppSettings:Version"] ?? "1.0.0";
            Environment = configuration["AppSettings:Environment"] ?? "Unknown";
            StartTime = DateTime.Now;
        }
    }
}
