using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WPFDeveloper.Models;

namespace WPFDeveloper.Services
{
    public class NavigationRegistry : INavigationRegistry
    {
        private const string NavigationFile = "Resources/navigation.json";

        public async Task<IReadOnlyList<NavigationItem>> GetNavigationItemsAsync()
        {
            if (!File.Exists(NavigationFile))
            {
                return Array.Empty<NavigationItem>();
            }

            await using var stream = File.OpenRead(NavigationFile);
            var items = await JsonSerializer.DeserializeAsync<List<NavigationItem>>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                .ConfigureAwait(false);

            return (items ?? new List<NavigationItem>())
                .OrderBy(i => i.Order)
                .ToList();
        }
    }

    public class NavigationService : INavigationService
    {
        private readonly Dictionary<string, Type> keyToViewType = new(StringComparer.OrdinalIgnoreCase)
        {
            ["home"] = typeof(Views.HomeView),
            ["about"] = typeof(Views.AboutView)
        };

        public Type ResolveViewType(string key)
        {
            if (!keyToViewType.TryGetValue(key, out var type))
            {
                throw new KeyNotFoundException($"未注册的导航键: {key}");
            }
            return type;
        }
    }

    public class ViewFactory : IViewFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ViewFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public object CreateView(Type viewType)
        {
            return serviceProvider.GetRequiredService(viewType);
        }
    }
}


