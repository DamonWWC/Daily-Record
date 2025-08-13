using System.Windows.Controls;
using WPFDeveloper.Attributes;

namespace WPFDeveloper.Views
{
    [View("mvvm-pattern", DisplayName = "MVVM 模式", Group = "Architecture", Order = 3)]
    public partial class MvvmPatternView : UserControl
    {
        public MvvmPatternView()
        {
            InitializeComponent();
        }
    }
}
