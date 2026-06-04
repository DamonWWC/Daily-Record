using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VLCDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            
            InitializeComponent();
            DataContext = this;
            this.Loaded+= (s, e) =>
            {
                VideoPanelView videoPanelView = new VideoPanelView();
                videoPanelView.Row = 4;
                 CameraCodes = new List<string>
                {
                    //"E:\\资料\\学习代码\\Daily-Record\\CSharp\\MethodDemo\\MethodDemo\\VLCDemo\\bin\\Debug\\Video\\出入口.mp4",
                    //"E:\\资料\\学习代码\\Daily-Record\\CSharp\\MethodDemo\\MethodDemo\\VLCDemo\\bin\\Debug\\Video\\出入口.mp4",
                    //"E:\\资料\\学习代码\\Daily-Record\\CSharp\\MethodDemo\\MethodDemo\\VLCDemo\\bin\\Debug\\Video\\出入口.mp4",
                    //"E:\\资料\\学习代码\\Daily-Record\\CSharp\\MethodDemo\\MethodDemo\\VLCDemo\\bin\\Debug\\Video\\出入口.mp4",
                    //"E:\\资料\\学习代码\\Daily-Record\\CSharp\\MethodDemo\\MethodDemo\\VLCDemo\\bin\\Debug\\Video\\出入口.mp4",
                    //"E:\\资料\\学习代码\\Daily-Record\\CSharp\\MethodDemo\\MethodDemo\\VLCDemo\\bin\\Debug\\Video\\出入口.mp4"
                    "rtmp://ns8.indexforce.com/home/mystream",
                    "rtmp://ns8.indexforce.com/home/mystream",
                    "E:\\资料\\学习代码\\Daily-Record\\CSharp\\MethodDemo\\MethodDemo\\VLCDemo\\bin\\Debug\\Video\\出入口.mp4",
                    "rtmp://ns8.indexforce.com/home/mystream",
                     "E:\\资料\\学习代码\\Daily-Record\\CSharp\\MethodDemo\\MethodDemo\\VLCDemo\\bin\\Debug\\Video\\出入口.mp4",
                     "E:\\资料\\学习代码\\Daily-Record\\CSharp\\MethodDemo\\MethodDemo\\VLCDemo\\bin\\Debug\\Video\\出入口.mp4"
                };
            };
        }

        public List<string> _CameraCodes;

        public List<string> CameraCodes
        {
            get=>_CameraCodes;
            set 
            {
                if (_CameraCodes == value) return;
                _CameraCodes = value;
                NotifyPropertyChanged(() => CameraCodes);
            }
        }

        /// <summary>
        /// 属性变化事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 实现变化通知
        /// </summary>
        /// <param name="info"></param>
        protected void NotifyPropertyChanged([CallerMemberName] string info = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        /// <summary>
        /// 实现变化通知
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression"></param>
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            try
            {
                MemberExpression memberExpression = propertyExpression.Body as MemberExpression;
                this.NotifyPropertyChanged(memberExpression.Member.Name);
            }
            catch
            {
                throw new System.ArgumentNullException("propertyExpression");
            }
        }
    }
  
}
