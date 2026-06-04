using System.Windows.Controls;
using WPFDeveloper.Attributes;

namespace WPFDeveloper.Views
{
    [View("wpf-demo", DisplayName = "WPF 演示", Group = "WPF", Order = 2)]
    [ServiceRegistration(WPFDeveloper.Attributes.ServiceLifetime.Transient)]
    public partial class WpfDemoView : UserControl
    {
        public WpfDemoView()
        {
            InitializeComponent();
        }
    }
}
