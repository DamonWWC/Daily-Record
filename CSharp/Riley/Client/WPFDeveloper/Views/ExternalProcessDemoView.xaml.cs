using System.Windows.Controls;
using WPFDeveloper.Attributes;

namespace WPFDeveloper.Views
{
    /// <summary>
    /// 外部程序嵌入演示视图
    /// 展示如何在WPF应用程序中嵌入外部EXE程序的功能
    /// </summary>
    [View("ExternalProcessDemo", DisplayName = "外部程序嵌入", Group = "WPF", Order = 5)]
  
    public partial class ExternalProcessDemoView : UserControl
    {
        public ExternalProcessDemoView()
        {
            InitializeComponent();
        }
    }
}
