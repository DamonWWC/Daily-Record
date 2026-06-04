using Polly;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace PollyWpfDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //var policy = Policy.TimeoutAsync(2, (context, timespan, task) =>
            //{
            //    return Task.CompletedTask;
            //});

            var policy = Policy.TimeoutAsync(TimeSpan.FromMilliseconds(1000), TimeoutStrategy.Pessimistic);

     await policy.ExecuteAsync(async () =>
                    {
                         await Fucntion();
                    });
            //var result = await FuncHelper.CallFunc(Fucntion, 1000);
        }

        private async Task<bool> Fucntion()
        {
            await Task.Delay(5000);
            return true;
        }
    }
}