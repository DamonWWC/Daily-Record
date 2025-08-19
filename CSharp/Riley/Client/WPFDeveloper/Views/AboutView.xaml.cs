using System.Windows.Controls;
using WPFDeveloper.Attributes;

namespace WPFDeveloper.Views
{
    [View("about", DisplayName = "关于", Group = "General", Order = 1)]
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
        }
    }
}


