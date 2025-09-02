using System.Windows;
using WPFDeveloper.Attributes;
using WPFDeveloper.ViewModels;

namespace WPFDeveloper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [ServiceRegistration(WPFDeveloper.Attributes.ServiceLifetime.Transient)]
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // 绑定关闭按钮事件
            CloseButton.MouseLeftButtonDown += (sender, e) =>
            {
              Close();
            };
        }
    }
}