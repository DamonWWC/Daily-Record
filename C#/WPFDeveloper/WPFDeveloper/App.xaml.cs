using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Logging (optional)
            services.AddLogging(builder =>
            {
                builder.AddDebug();
            });

            // Services
            services.AddSingleton<Services.INavigationService, Services.NavigationService>();
            services.AddSingleton<Services.INavigationRegistry, Services.NavigationRegistry>();
            services.AddSingleton<Services.IViewFactory, Services.ViewFactory>();

            // ViewModels
            services.AddTransient<ViewModels.MainWindowViewModel>();

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<Views.HomeView>();
            services.AddTransient<Views.AboutView>();
        }
    }
}
