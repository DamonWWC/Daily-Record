using System.Windows.Controls;
using WPFDeveloper.ViewModels;

namespace WPFDeveloper.Views
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
        }
    }
}