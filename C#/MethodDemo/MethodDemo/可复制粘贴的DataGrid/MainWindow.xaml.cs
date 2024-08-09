using GalaSoft.MvvmLight.Command;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 可复制粘贴的DataGrid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            KeyDown+= (sender, e) =>
            {
                var isCtrl = e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl);
                if (isCtrl && e.Key == Key.V)
                {
                    var content = Clipboard.GetText();
                    sourceDatas = new List<SourceData>();
                    var aa = content.Split("\r\n");
                    foreach (var item in content.Split("\r\n"))
                    {
                        if (string.IsNullOrWhiteSpace(item)) continue;
                        var items = item.Split('\t');
                        sourceDatas.Add(new SourceData{
                            Name = items[0],
                            SubSystem = items[1]
                        });
                        dg.Columns.Add(new DataGridCheckBoxColumn { Header = items[0], Binding = new Binding(items[0]) });
                    }
                    dg.ItemsSource = sourceDatas;
                }
            };
            DataContext = this;
            this.ConnectCommand = new RelayCommand<SourceData>((p) => 
            {
                selectedData = p;


            });
        }
        List<SourceData> sourceDatas;
        SourceData selectedData;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var content = Clipboard.GetText();
            var content1 = Clipboard.GetDataObject();
            var aaa = content1.GetFormats();
            foreach (var item in aaa)
            {
                var result = content1.GetData(item, true); 
            }
        }

       public ICommand ConnectCommand { get; set; }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            sourceDatas[0].Name = "aaa";
        }
    }
    public class SourceData
    {
        public string Name { get; set; }
        public string SubSystem { get; set; }
    }
    
}