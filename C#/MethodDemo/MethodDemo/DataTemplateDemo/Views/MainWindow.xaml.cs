using DataTemplateDemo.ViewModels;
using System.Windows;

namespace DataTemplateDemo.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Content = new MainWindowViewModel2();
        }
    }
}
