using System.Reflection;

namespace WPFDeveloper.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        private string _version;
        private string _description;

        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public AboutViewModel()
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
            Description = "WPF开发者工具是一个用于辅助WPF开发的工具集合，提供了多种实用功能来提高开发效率。";
        }
    }
}