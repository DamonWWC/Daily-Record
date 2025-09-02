using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WPFDeveloper.Attributes;
using WPFDeveloper.Models;

namespace WPFDeveloper.Services
{
    /// <summary>
    /// 基于自动注册的导航注册表实现
    /// 这个实现将由SourceGenerator自动注册
    /// </summary>
    [ServiceRegistration(typeof(INavigationRegistry), WPFDeveloper.Attributes.ServiceLifetime.Singleton)]
    public class NavigationRegistry : INavigationRegistry
    {
        private readonly IViewAutoRegistration _viewAutoRegistration;
        private readonly ILogger<NavigationRegistry> _logger;

        public NavigationRegistry(IViewAutoRegistration viewAutoRegistration, ILogger<NavigationRegistry> logger)
        {
            _viewAutoRegistration = viewAutoRegistration;
            _logger = logger;
        }

        public Task<IReadOnlyList<NavigationItem>> GetNavigationItemsAsync()
        {
            try
            {
                var viewInfos = _viewAutoRegistration.GetRegisteredViews();
                var navigationItems = viewInfos.Select(ConvertToNavigationItem).ToList();

                _logger.LogInformation("从自动注册中获取到 {Count} 个导航项", navigationItems.Count);

                return Task.FromResult<IReadOnlyList<NavigationItem>>(navigationItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取导航项失败");
                return Task.FromResult<IReadOnlyList<NavigationItem>>(new List<NavigationItem>());
            }
        }

        private static NavigationItem ConvertToNavigationItem(ViewInfo viewInfo)
        {
            return new NavigationItem
            {
                Key = viewInfo.Key,
                Title = viewInfo.DisplayName,
                Icon = GetIconByGroup(viewInfo.Group),
                ViewType = viewInfo.ViewType.FullName ?? viewInfo.ViewType.Name,
                Order = viewInfo.Order,
                Group = viewInfo.Group,
                PageTitle = $"{viewInfo.DisplayName} - WPFDeveloper",
                PageDescription = GetDescriptionByKey(viewInfo.Key),
                PageIcon = GetEmojiIconByGroup(viewInfo.Group),
                PageCategory = viewInfo.Group ?? "未分类",
                IsEnabled = viewInfo.IsEnabled,
                ToolTip = $"打开 {viewInfo.DisplayName} 页面"
            };
        }

        private static string GetIconByGroup(string? group)
        {
            return group switch
            {
                "General" => "Home",
                "WPF" => "Desktop",
                "Architecture" => "Code",
                "Tools" => "Tools",
                _ => "Page"
            };
        }

        private static string GetEmojiIconByGroup(string? group)
        {
            return group switch
            {
                "General" => "🏠",
                "WPF" => "🖥️",
                "Architecture" => "🏗️",
                "Tools" => "🛠️",
                _ => "📄"
            };
        }

        private static string GetDescriptionByKey(string key)
        {
            return key switch
            {
                "home" => "欢迎使用 WPFDeveloper，这是一个用于日常技术收集与实现的客户端应用。",
                "about" => "了解 WPFDeveloper 的技术架构、功能特性、技术栈等信息。",
                "wpf-demo" => "展示各种 WPF 技术的实际应用，包括控件、绑定、动画、样式等。",
                "mvvm-pattern" => "深入理解 MVVM 架构模式，包括数据绑定、命令模式、依赖注入等核心概念。",
                "data-binding" => "学习 WPF 数据绑定的各种用法，包括单向绑定、双向绑定、转换器等。",
                "custom-controls" => "学习如何创建自定义 WPF 控件，包括 UserControl、CustomControl 等。",
                _ => "暂无描述"
            };
        }
    }
}


