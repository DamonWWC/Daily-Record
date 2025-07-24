using System.Windows.Controls;
using WPFDeveloper.ViewModels;

namespace WPFDeveloper.Views
{
    public partial class ToolsPage : Page
    {
        public ToolsPage()
        {
            InitializeComponent();
            DataContext = new ToolsViewModel();
        }
    }
}