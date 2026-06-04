using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using WPFDeveloper.Services;

namespace WPFDeveloper.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private string _pageTitle = "首页";

        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        public ICommand NavigateCommand { get; }
        public ICommand NavigateHomeCommand { get; }
        public ICommand NavigateSettingsCommand { get; }
        public ICommand NavigateAboutCommand { get; }

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            NavigateCommand = new RelayCommand<string>(pageKey =>
            {
                switch (pageKey)
                {
                    case "Home":
                        _navigationService.NavigateTo("Home");
                        PageTitle = "首页";
                        break;
                    case "Settings":
                        _navigationService.NavigateTo("Settings");
                        PageTitle = "设置";
                        break;
                    case "About":
                        _navigationService.NavigateTo("About");
                        PageTitle = "关于";
                        break;
                }
            });

            NavigateHomeCommand = new RelayCommand(() => 
            {
                _navigationService.NavigateTo("Home");
                PageTitle = "首页";
            });

            NavigateSettingsCommand = new RelayCommand(() => 
            {
                _navigationService.NavigateTo("Settings");
                PageTitle = "设置";
            });

            NavigateAboutCommand = new RelayCommand(() => 
            {
                _navigationService.NavigateTo("About");
                PageTitle = "关于";
            });
        }
    }
}