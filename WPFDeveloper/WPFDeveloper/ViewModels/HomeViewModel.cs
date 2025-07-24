namespace WPFDeveloper.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        private string _welcomeMessage = "欢迎使用WPF开发者工具";

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        public HomeViewModel()
        {
            // 初始化逻辑
        }
    }
}