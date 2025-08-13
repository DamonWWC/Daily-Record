using System.Windows.Controls;
using WPFDeveloper.Attributes;

namespace WPFDeveloper.Views
{
    [View("data-binding", DisplayName = "数据绑定", Group = "WPF", Order = 4)]
    public partial class DataBindingView : UserControl
    {
        public DataBindingView()
        {
            InitializeComponent();
        }
    }
}
