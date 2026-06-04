using System.Windows.Controls;
using WPFDeveloper.ViewModels;

namespace WPFDeveloper.Views
{
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            DataContext = new HomeViewModel();
        }
    }
}