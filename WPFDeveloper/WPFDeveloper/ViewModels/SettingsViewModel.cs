using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace WPFDeveloper.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private bool _isDarkTheme;
        private bool _enableNotifications = true;
        private string _language = "中文";

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set => SetProperty(ref _isDarkTheme, value);
        }

        public bool EnableNotifications
        {
            get => _enableNotifications;
            set => SetProperty(ref _enableNotifications, value);
        }

        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        public ICommand SaveSettingsCommand { get; }

        public SettingsViewModel()
        {
            SaveSettingsCommand = new RelayCommand(SaveSettings);
        }

        private void SaveSettings()
        {
            // 这里实现保存设置的逻辑
            // 实际应用中，可能会将设置保存到配置文件或数据库中
        }
    }
}