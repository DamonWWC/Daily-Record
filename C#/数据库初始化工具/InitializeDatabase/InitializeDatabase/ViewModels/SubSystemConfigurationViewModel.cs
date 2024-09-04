using LiteDB;
using PCI.Framework.ORM;
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

namespace InitializeDatabase.ViewModels
{
    public class SubSystemConfigurationViewModel : BindableBase
    {
        private readonly IDAFacade _dAFacade;
        private readonly ILiteDatabase initDataDb;
        private readonly ILiteDatabase rawDataDb;
        private IEventAggregator _ea;

        public SubSystemConfigurationViewModel(IDAFacade dAFacade, IEventAggregator ea)
        {
            _dAFacade = dAFacade;
            _ea = ea;
            initDataDb = ContainerLocator.Container.Resolve<ILiteDatabase>("InitData");
            rawDataDb = ContainerLocator.Container.Resolve<ILiteDatabase>("RawData");
        }

        private ObservableCollection<MajorInfo> _MajorInfos;

        public ObservableCollection<MajorInfo> MajorInfos
        {
            get { return _MajorInfos; }
            set { SetProperty(ref _MajorInfos, value); }
        }

        private string _SqlText;

        public string SqlText
        {
            get { return _SqlText; }
            set { SetProperty(ref _SqlText, value); }
        }

        #region cmd

        private DelegateCommand _PasteCommand;
        public DelegateCommand PasteCommand => _PasteCommand ??= new DelegateCommand(ExecutePasteCommand);

        private void ExecutePasteCommand()
        {
            var pasteinfo = Clipboard.GetText();
            List<MajorInfo> majorInfos = [];
            var col = rawDataDb.GetCollection<SubSystem>("subsystem");

            var cells = pasteinfo.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (string item in cells)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                var items = item.Split('\t');
                var subname = items.ElementAtOrDefault(1);

                majorInfos.Add(new MajorInfo
                {
                    Name = items.ElementAtOrDefault(0),
                    SubName = items.ElementAtOrDefault(1),
                    Agent = items.ElementAtOrDefault(2),
                    SubSystemKey = col.FindOne(p => p.NAME == subname)?.PKEY
                });
            }

            MajorInfos = new ObservableCollection<MajorInfo>(majorInfos);
        }

        private DelegateCommand _SaveCommand;
        public DelegateCommand SaveCommand => _SaveCommand ??= new DelegateCommand(ExecuteSaveCommand);

        private void ExecuteSaveCommand()
        {
            CreateSqlText();
        }

        private DelegateCommand _ShowCommand;

        public DelegateCommand ShowCommand =>
            _ShowCommand ?? (_ShowCommand = new DelegateCommand(ExecuteShowCommand));

        private void ExecuteShowCommand()
        {
            CreateSqlText();
            _ea.GetEvent<ShowSqlEvent>().Publish(SqlText);
        }

        #endregion cmd

        #region Function

        private void CreateSqlText()
        {
            StringBuilder sql = new();

            List<string> insertMICS_DIC_SUBSYSNAME = new List<string>();
            List<string> insertMICS_DATAPOINT_SYSTEM = new List<string>();
            ushort i = 1;
            foreach (var item in MajorInfos)
            {
                insertMICS_DIC_SUBSYSNAME.Add($"INSERT INTO MICS_DIC_SUBSYSNAME (SUBSYSTEM_NAME,DESCRIPTION) VALUES({item.Name},{item.SubName});");
                insertMICS_DATAPOINT_SYSTEM.Add($"INSERT INTO MICS_DATAPOINT_SYSTEM (PKEY,NAME,DESCRIPTION,AGENT,STATUS,UPDATE_TIME,RELATESUBSYSTEMKEY) VALUES ({i++},'{item.Name}','{item.SubName}','{item.Agent}','0',sysdate,{item.SubSystemKey});");
            }

            sql.AppendLine("TRUNCATE TABLE MICS_DIC_SUBSYSNAME;")
                .AppendLine(string.Join("\t\n", insertMICS_DIC_SUBSYSNAME));
            sql.AppendLine("DELETE FROM MICS_DATAPOINT_SYSTEM ;")
                .AppendLine(string.Join("\t\n", insertMICS_DATAPOINT_SYSTEM));
            SqlText = sql.ToString();
            SaveDb();
        }

        private void SaveDb()
        {
            if (MajorInfos != null)
            {
                var col = initDataDb.GetCollection<MajorInfo>("majorInfos");
                col.DeleteAll();
                col.InsertBulk(MajorInfos);
            }
        }

        #endregion Function
    }

    public class MajorInfo : BindableBase
    {
        public int? SubSystemKey { get; set; }
        public string Name { get; set; }
        public string SubName { get; set; }
        public string Agent { get; set; }

        private bool _IsChecked;

        public bool IsChecked
        {
            get { return _IsChecked; }
            set { SetProperty(ref _IsChecked, value); }
        }
    }
}