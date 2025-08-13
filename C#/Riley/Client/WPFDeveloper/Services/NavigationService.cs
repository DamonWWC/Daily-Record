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
        private readonly IViewRegistry _viewRegistry;

        public NavigationService(IViewRegistry viewRegistry)
        {
            _viewRegistry = viewRegistry ?? throw new ArgumentNullException(nameof(viewRegistry));
        }

        public Type ResolveViewType(string key)
        {
            // 首先尝试从 ViewRegistry 解析
            var viewType = _viewRegistry.ResolveViewType(key);
            if (viewType != null)
            {
                return viewType;
            }

            // 如果找不到，抛出异常
            throw new KeyNotFoundException($"未找到视图类型: {key}");
        }
    }

    public class ViewFactory : IViewFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IViewRegistry _viewRegistry;

        public ViewFactory(IServiceProvider serviceProvider, IViewRegistry viewRegistry)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _viewRegistry = viewRegistry ?? throw new ArgumentNullException(nameof(viewRegistry));
        }

        public object CreateView(Type viewType)
        {
            // 尝试从 DI 容器创建视图
            try
            {
                return _serviceProvider.GetRequiredService(viewType);
            }
            catch
            {
                // 如果 DI 容器中没有注册，使用反射创建
                try
                {
                    return Activator.CreateInstance(viewType) ?? 
                           throw new InvalidOperationException($"无法创建视图实例: {viewType.Name}");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"创建视图失败: {viewType.Name}", ex);
                }
            }
        }

        public object CreateView(string viewTypeName)
        {
            var viewType = _viewRegistry.ResolveViewType(viewTypeName);
            if (viewType == null)
            {
                throw new KeyNotFoundException($"未找到视图类型: {viewTypeName}");
            }

            return CreateView(viewType);
        }
    }
}


