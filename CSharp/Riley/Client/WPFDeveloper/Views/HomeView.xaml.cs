using System.Windows.Controls;
using WPFDeveloper.Attributes;

namespace WPFDeveloper.Views
{
    [View("home", DisplayName = "首页", Group = "General", Order = 0)]
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }
    }
}


