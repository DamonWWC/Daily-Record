using LiteDB;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace InitializeDatabase.ViewModels
{
    public class MajorInfoConfigurationViewModel : BindableBase
    {
        private readonly ILiteDatabase initDataDb;
        private IEventAggregator _ea;
        public ObservableCollection<DataGridBoundColumn> Columns { get; set; }

        public MajorInfoConfigurationViewModel(IEventAggregator ea)
        {
            initDataDb = ContainerLocator.Container.Resolve<ILiteDatabase>("InitData");
            _ea = ea;
            InitData();
        }

        private List<MajorConfigInfo> _MajorConfigInfos;

        public List<MajorConfigInfo> MajorConfigInfos
        {
            get { return _MajorConfigInfos; }
            set { SetProperty(ref _MajorConfigInfos, value); }
        }

        private string _SqlText;

        public string SqlText
        {
            get { return _SqlText; }
            set { SetProperty(ref _SqlText, value); }
        }

        private void InitData()
        {
            Columns =
               [new DataGridTextColumn { Binding = new Binding("Abbreviation"), Header = "缩写" },
               new DataGridTextColumn { Binding = new Binding("Name"), Header = "名称" }
                 ];
            var locationinfos = initDataDb.GetCollection<LocationInfo>("locationInfos").Query().ToArray();
            var majorinfos = initDataDb.GetCollection<MajorInfo>("majorInfos").Query().ToArray();

            int i = 0;

            foreach (var item in majorinfos)
            {
                Columns.Add(new DataGridCheckBoxColumn { Binding = new Binding($"SelectedMajor[{i++}].IsChecked"), Header = item.Name });
            }
            List<MajorConfigInfo> majorConfigInfos = [];
            foreach (var item in locationinfos)
            {
                majorConfigInfos.Add(new MajorConfigInfo
                {
                    Abbreviation = item.Name,
                    Name = item.Description,
                    Id = item.Id,
                    SelectedMajor = majorinfos.Select(p => new MajorInfo { Name = p.Name, SubName = p.SubName, IsChecked = p.IsChecked }).ToArray()
                });
            }
            MajorConfigInfos = [.. majorConfigInfos.OrderBy(p => int.Parse(p.Id))];
        }

        private void CreateSqlText()
        {
            StringBuilder sql = new();
            sql.AppendLine("INSERT INTO ENTITY (NAME,ADDRESS,DESCRIPTION,SUBSYSTEMKEY,LOCATIONKEY,SEREGI_ID,TYPEKEY,PHYSICAL_SUBSYSTEM_KEY,SCSDAT_ID,PARENTKEY,AGENTKEY,DELETED,CREATED_BY,DATE_CREATED,MODIFIED_BY,DATE_MODIFIED)");
            List<string> majorinfo = [];
            var majorinfos = initDataDb.GetCollection<MajorInfo>("majorInfos").Query().ToArray();
            foreach (var item in majorinfos)
            {
                var infos = MajorConfigInfos.Where(p => p.SelectedMajor.First(x => x.Name == item.Name).IsChecked);
                if (infos == null || infos.Count() == 0) continue;
                majorinfo.Add($"SELECT '{item.Name}' name,'{item.SubName}' subname,'{string.Join(",", infos.Select(p => p.Id))}' lon from dual");
            }
            sql.AppendLine(string.Join("union all \t\n", majorinfo));

            SqlText = sql.ToString();
        }

        #region cmd

        private DelegateCommand _SaveCommand;
        public DelegateCommand SaveCommand => _SaveCommand ??= new DelegateCommand(ExecuteSaveCommand);

        private void ExecuteSaveCommand()
        {
            CreateSqlText();
        }

        private DelegateCommand _ShowCommand;

        public DelegateCommand ShowCommand => _ShowCommand ??= new DelegateCommand(ExecuteShowCommand);

        private void ExecuteShowCommand()
        {
            CreateSqlText();
            _ea.GetEvent<ShowSqlEvent>().Publish(SqlText);
        }

        private DelegateCommand<object> _SelectionChanged;

        public DelegateCommand<object> SelectionChanged =>
            _SelectionChanged ??= new DelegateCommand<object>(ExecuteSelectionChanged);

        private void ExecuteSelectionChanged(object param)
        {
            if (param is SelectedCellsChangedEventArgs e)
            {
                var cells = e.AddedCells;
                var header = cells.ElementAtOrDefault(0).Column.Header;
                var item = cells.ElementAtOrDefault(0).Item;
                if (item is MajorConfigInfo major)
                {
                    var info = major.SelectedMajor.FirstOrDefault(p => p.Name == header.ToString());
                    if (info != null)
                    {
                        info.IsChecked = !info.IsChecked;
                    }
                }
            }
        }

        private DelegateCommand _PasteCommand;

        public DelegateCommand PasteCommand => _PasteCommand ??= new DelegateCommand(ExecutePasteCommand);

        private void ExecutePasteCommand()
        {
            var pasteText = Clipboard.GetText();
            var cells = pasteText.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            for (int i = 0; i < Math.Min(cells.Count(), MajorConfigInfos.Count()); i++)
            {
                var items = cells[i].Split('\t');
                var selectedMajor = MajorConfigInfos[i].SelectedMajor;
                for (int j = 0; j < Math.Min(items.Count(), selectedMajor.Count()); j++)
                {
                    selectedMajor[j].IsChecked = items[j] == "1";
                }
            }
        }

        #endregion cmd
    }

    public class MajorConfigInfo
    {
        public string Abbreviation { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }

        public MajorInfo[] SelectedMajor { get; set; }
    }
}