using Microsoft.Extensions.DependencyInjection;

namespace Riley.Core.Helper
{
    public class ServiceLocator
    {
        private static IServiceCollection _services;
        private static IServiceProvider _serviceProvider;

        public static IServiceProvider Provider => _serviceProvider ?? _services?.BuildServiceProvider();

        public static void Register(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public static void RegisterServices(IServiceCollection services)
        {
            _services = services;
        }
    }
}
