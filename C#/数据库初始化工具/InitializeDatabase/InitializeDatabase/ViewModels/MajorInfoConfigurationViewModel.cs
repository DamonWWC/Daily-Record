using LiteDB;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using Prism.Ioc;
using System.Dynamic;
using System.Xml;
using System.Runtime.Remoting.Messaging;
using HandyControl.Tools.Converter;
using System.Globalization;

namespace InitializeDatabase.ViewModels
{
    public class MajorInfoConfigurationViewModel : BindableBase
    {
        private readonly ILiteDatabase initDataDb;
        public ObservableCollection<DataGridBoundColumn> Columns { get; set; }

        public MajorInfoConfigurationViewModel()
        {
          
            initDataDb = ContainerLocator.Container.Resolve<ILiteDatabase>("InitData");

            InitData();
        }

        private void InitData()
        {
            Columns =
               [new DataGridTextColumn { Binding = new Binding("Name"), Header = "名称" },                   
                 ];

            var locationinfos = initDataDb.GetCollection<LocationInfo>("locationInfos").Query().ToArray();
            var majorinfos = initDataDb.GetCollection<MajorInfo>("majorInfos").Query().ToArray();

            int i = 0;
          
            foreach (var item in majorinfos)
            {      
                

                Columns.Add(new DataGridCheckBoxColumn { Binding = new Binding($"SelectedMajor[{i++}].IsChecked"), Header = item.Name });
                //var multi = new MultiBinding()
                //{
                //    Converter = new BooleanArr2BooleanConverter(),

                //};
                //multi.Bindings.Add(new Binding($"SelectedMajor[{i++}].IsChecked"));
                //multi.Bindings.Add(new Binding("IsSelected")
                //{
                //    RelativeSource=new RelativeSource()
                //    {
                //        AncestorType=typeof(DataGridCell)
                //    }

                //});

                //Columns.Add(new DataGridCheckBoxColumn { Binding = multi, Header = item.Name });
            }
            List<MajorConfigInfo> majorConfigInfos = new List<MajorConfigInfo>();
            foreach (var item in locationinfos)
            {
                majorConfigInfos.Add(new MajorConfigInfo { Name = item.Name, SelectedMajor = majorinfos.Select(p => new MajorInfo { Name = p.Name, SubName = p.SubName, IsChecked = p.IsChecked }).ToArray() });
            }
            MajorConfigInfos = majorConfigInfos;
        }


        private List<MajorConfigInfo> _MajorConfigInfos;

        public List<MajorConfigInfo> MajorConfigInfos
        {
            get { return _MajorConfigInfos; }
            set { SetProperty(ref _MajorConfigInfos, value); }
        }



        private DelegateCommand _SaveCommand;
        public DelegateCommand SaveCommand => _SaveCommand ??= new DelegateCommand(ExecuteSaveCommand);

        private void ExecuteSaveCommand()
        {
            InitData();
            //CreateSqlText();
        }

        private DelegateCommand _ShowCommand;

        public DelegateCommand ShowCommand =>
            _ShowCommand ?? (_ShowCommand = new DelegateCommand(ExecuteShowCommand));

        private void ExecuteShowCommand()
        {
            //CreateSqlText();
           // _ea.GetEvent<ShowSqlEvent>().Publish(SqlText);
        }
        private DelegateCommand<object> _SelectionChanged;
        public DelegateCommand<object> SelectionChanged =>
_SelectionChanged ??= new DelegateCommand<object>(ExecuteSelectionChanged);

        void ExecuteSelectionChanged(object param)
        {
            if(param is SelectedCellsChangedEventArgs e)
            {
                var item = e.AddedCells;
                var header = item.ElementAtOrDefault(0).Column.Header;
                var ii = item.ElementAtOrDefault(0).Item;
                if(ii is MajorConfigInfo ob)
                {
                    var aa = ob.SelectedMajor.FirstOrDefault(p => p.Name == header.ToString());
                    aa.IsChecked = !aa.IsChecked;
                }
            }
        }
    }

    public class MajorConfigInfo
    {
        public string Name { get; set; }

        public MajorInfo[] SelectedMajor { get; set; }
    }
    public class BooleanArr2BooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
            {
                return false;
            }

            var arr = new List<bool>();
            foreach (var item in values)
            {
                if (item is bool boolValue)
                {
                    arr.Add(boolValue);
                }
                else
                {
                    return false;
                }
            }

            return arr.Any(item => item);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}