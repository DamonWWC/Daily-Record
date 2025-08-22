using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using WPFDeveloper.Models;
using WPFDeveloper.Services;
using WPFDeveloper.Views;

namespace WPFDeveloper.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
       
        private readonly IServiceProvider serviceProvider;
        private readonly IViewAutoRegistration viewAutoRegistration;
        private readonly INavigationRegistry navigationRegistry;
      
        private readonly ILogger<MainWindowViewModel> logger;

        public ObservableCollection<NavigationItem> NavigationItems { get; } = new();

        private NavigationItem? selectedNavigationItem;
        public NavigationItem? SelectedNavigationItem
        {
            get => selectedNavigationItem;
            set
            {
                if (SetProperty(ref selectedNavigationItem, value))
                {
                    if (value != null)
                    {
                        NavigateTo(value.Key);
                        UpdatePageInfo(value);
                    }
                }
            }
        }

        private object? currentView;
        public object? CurrentView
        {
            get => currentView;
            set => SetProperty(ref currentView, value);
        }

        // 新增页面信息属性
        private string pageTitle = "功能展示区";
        public string PageTitle
        {
            get => pageTitle;
            set => SetProperty(ref pageTitle, value);
        }

        private string pageDescription = "选择左侧导航项查看相应功能";
        public string PageDescription
        {
            get => pageDescription;
            set => SetProperty(ref pageDescription, value);
        }

        private string pageIcon = "📋";
        public string PageIcon
        {
            get => pageIcon;
            set => SetProperty(ref pageIcon, value);
        }

        public IAsyncRelayCommand InitializeCommand { get; }

        public MainWindowViewModel(
                                   INavigationRegistry navigationRegistry,
                                  
                                   IServiceProvider serviceProvider,
                                   IViewAutoRegistration viewAutoRegistration,
                                   ILogger<MainWindowViewModel> logger)
        {
            this.viewAutoRegistration = viewAutoRegistration;
            
            this.serviceProvider= serviceProvider;
            this.navigationRegistry = navigationRegistry;
           
            this.logger = logger;

            InitializeCommand = new AsyncRelayCommand(InitializeAsync);

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                NavigationItems.Clear();
                var items = await navigationRegistry.GetNavigationItemsAsync();
                foreach (var item in items)
                {
                    // 只添加启用的导航项
                    if (item.IsEnabled)
                    {
                        NavigationItems.Add(item);
                    }
                }

                if (NavigationItems.Count > 0)
                {
                    SelectedNavigationItem = NavigationItems[0];
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "初始化导航项失败");
                MessageBox.Show($"初始化失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateTo(string key)
        {
            try
            {
                var viewType = viewAutoRegistration.GetViewType(key);
                //var view2 = serviceProvider.GetRequiredService<AboutView>();
                //var aaa = serviceProvider.GetRequiredKeyedService(viewType,"about");
                var view = serviceProvider.GetRequiredService(viewType);

                //var viewType = navigationService.ResolveViewType(key);
               // var view = viewFactory.CreateView(viewType);

               
                CurrentView = view;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "导航到视图失败: {Key}", key);
                
                // 创建错误视图
                CurrentView = CreateErrorView(ex.Message);
                
                // 更新页面信息为错误状态
                PageTitle = "加载失败";
                PageDescription = $"无法加载页面: {ex.Message}";
                PageIcon = "❌";
            }
        }

        private object CreateErrorView(string errorMessage)
        {
            // 创建一个简单的错误显示视图
            var errorView = new System.Windows.Controls.UserControl();
            var stackPanel = new System.Windows.Controls.StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };

            var errorIcon = new System.Windows.Controls.TextBlock
            {
                Text = "❌",
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16)
            };

            var errorTitle = new System.Windows.Controls.TextBlock
            {
                Text = "页面加载失败",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var errorMessageBlock = new System.Windows.Controls.TextBlock
            {
                Text = errorMessage,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 400
            };

            stackPanel.Children.Add(errorIcon);
            stackPanel.Children.Add(errorTitle);
            stackPanel.Children.Add(errorMessageBlock);
            errorView.Content = stackPanel;

            return errorView;
        }

        private void UpdatePageInfo(NavigationItem navigationItem)
        {
            // 从配置文件中更新页面信息
            PageTitle = navigationItem.PageTitle;
            PageDescription = navigationItem.PageDescription ?? "暂无描述";
            PageIcon = navigationItem.PageIcon ?? "📄";
        }
    }
}


