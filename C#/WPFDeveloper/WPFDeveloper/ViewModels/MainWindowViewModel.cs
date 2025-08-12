using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPFDeveloper.Models;
using WPFDeveloper.Services;

namespace WPFDeveloper.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private readonly INavigationService navigationService;
        private readonly INavigationRegistry navigationRegistry;
        private readonly IViewFactory viewFactory;

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

        public IAsyncRelayCommand InitializeCommand { get; }

        public MainWindowViewModel(INavigationService navigationService,
                                   INavigationRegistry navigationRegistry,
                                   IViewFactory viewFactory)
        {
            this.navigationService = navigationService;
            this.navigationRegistry = navigationRegistry;
            this.viewFactory = viewFactory;

            InitializeCommand = new AsyncRelayCommand(InitializeAsync);

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            NavigationItems.Clear();
            var items = await navigationRegistry.GetNavigationItemsAsync();
            foreach (var item in items)
            {
                NavigationItems.Add(item);
            }

            if (NavigationItems.Count > 0)
            {
                SelectedNavigationItem = NavigationItems[0];
            }
        }

        private void NavigateTo(string key)
        {
            var viewType = navigationService.ResolveViewType(key);
            var view = viewFactory.CreateView(viewType);
            CurrentView = view;
        }
    }
}


