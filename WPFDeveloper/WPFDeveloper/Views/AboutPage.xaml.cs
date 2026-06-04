using System.Windows.Controls;
using WPFDeveloper.ViewModels;

namespace WPFDeveloper.Views
{
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
            DataContext = new AboutViewModel();
        }
    }
}