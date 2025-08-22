using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WPFDeveloper.Extensions;
using WPFDeveloper.Services;
using WPFDeveloper.Views;

namespace WPFDeveloper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            
            // 测试视图注册是否成功
            var homeView = ServiceProvider.GetService<HomeView>();
            var aboutView = ServiceProvider.GetService<AboutView>();
            
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // 添加 WPF 开发者服务（包含自动注册）
            services.AddWpfDeveloperServices();

            // 使用自动导航注册表替代原来的 JSON 配置
            services.AddSingleton<INavigationRegistry, AutoNavigationRegistry>();
        }
    }
}
