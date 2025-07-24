using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace WPFDeveloper.ViewModels
{
    public class ToolsViewModel : ViewModelBase
    {
        public ICommand OpenJsonToolCommand { get; }
        public ICommand OpenBase64ToolCommand { get; }
        public ICommand OpenUrlToolCommand { get; }
        public ICommand OpenTimestampToolCommand { get; }

        public ToolsViewModel()
        {
            // 初始化命令
            OpenJsonToolCommand = new RelayCommand(OpenJsonTool);
            OpenBase64ToolCommand = new RelayCommand(OpenBase64Tool);
            OpenUrlToolCommand = new RelayCommand(OpenUrlTool);
            OpenTimestampToolCommand = new RelayCommand(OpenTimestampTool);
        }

        private void OpenJsonTool()
        {
            // 打开JSON格式化工具的实现
            // TODO: 实现工具窗口打开逻辑
        }

        private void OpenBase64Tool()
        {
            // 打开Base64转换工具的实现
            // TODO: 实现工具窗口打开逻辑
        }

        private void OpenUrlTool()
        {
            // 打开URL编解码工具的实现
            // TODO: 实现工具窗口打开逻辑
        }

        private void OpenTimestampTool()
        {
            // 打开时间戳转换工具的实现
            // TODO: 实现工具窗口打开逻辑
        }
    }
}