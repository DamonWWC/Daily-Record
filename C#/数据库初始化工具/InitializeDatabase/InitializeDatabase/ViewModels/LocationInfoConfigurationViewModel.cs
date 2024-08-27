using InitializeDatabase.Helper;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace InitializeDatabase.ViewModels
{
    public class LocationInfoConfigurationViewModel : BindableBase
    {
        public LocationInfoConfigurationViewModel()
        {
            LocationTypes = EnumHelper.ToList<LocationType>().Where(p => p != LocationType.None).ToList();                
        }

        #region property

        private ObservableCollection<LocationInfo> _LocationInfos=[];

        /// <summary>
        /// 位置信息
        /// </summary>
        public ObservableCollection<LocationInfo> LocationInfos
        {
            get { return _LocationInfos; }
            set { SetProperty(ref _LocationInfos, value); }
        }

     
        private List<LocationType> _LocationTypes;

        public List<LocationType> LocationTypes
        {
            get { return _LocationTypes; }
            set { SetProperty(ref _LocationTypes, value); }
        }

        private string _LineInfo;
        public string LineInfo
        {
            get { return _LineInfo; }
            set { SetProperty(ref _LineInfo, value); }
        }

        private string _SqlText;
        public string SqlText
        {
            get { return _SqlText; }
            set { SetProperty(ref _SqlText, value); }
        }
        #endregion property

        #region cmd

        private DelegateCommand _PasteCommand;
        public DelegateCommand PasteCommand => _PasteCommand ??= new DelegateCommand(ExecutePasteCommand);

        private void ExecutePasteCommand()
        {
            var pasteinfo = Clipboard.GetText();
            List<LocationInfo> locationInfos = [];

            var cells = pasteinfo.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (string item in cells)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                var items = item.Split('\t');

                locationInfos.Add(new LocationInfo
                {
                    Id = items.ElementAtOrDefault(0),
                    Name = items.ElementAtOrDefault(1),
                    Description = items.ElementAtOrDefault(2),
                    Type = EnumHelper.ToEnum<LocationType>(string.IsNullOrWhiteSpace(items.ElementAtOrDefault(3)) || !Enum.IsDefined(typeof(LocationType), items.ElementAtOrDefault(3)) ? "None" : items.ElementAtOrDefault(3)),
                    MergInfo = items.ElementAtOrDefault(4)
                });
            }
            LocationInfos = new ObservableCollection<LocationInfo>(locationInfos);
        }

        private DelegateCommand _ClearCommand;
        public DelegateCommand ClearCommand => _ClearCommand ??= new DelegateCommand(ExecuteClearCommand);

        private void ExecuteClearCommand()
        {
            LocationInfos = [];
        }


        private DelegateCommand _SaveCommand;
        public DelegateCommand SaveCommand =>
            _SaveCommand ?? (_SaveCommand = new DelegateCommand(ExecuteSaveCommand));

        void ExecuteSaveCommand()
        {
            CreateSqlText();            
        }

        private DelegateCommand _AddCommand;
        public DelegateCommand AddCommand => _AddCommand ??= new DelegateCommand(ExecuteAddCommand);

        void ExecuteAddCommand()
        {
            LocationInfos.Add(new LocationInfo());
        }
        #endregion cmd


        #region Func


        private void CreateSqlText()
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("TRUNCATE TABLE LOCATION;").AppendLine();
            List<string> merginfo=[];
            int i = 1;
            foreach (var item in LocationInfos)
            {
                string str = $"INSERT INTO location VALUES ({item.Id}, '{item.Name}', '{item.Description}', {item.Id}, {LineInfo}, 'mics', SYSDATE, null,null, '{item.Description}', 0);";
                sql.Append(str).AppendLine();
                if (!string.IsNullOrWhiteSpace(item.MergInfo))
                {
                    merginfo.Add($"INSERT INTO MICS_LOCATIONMERG (PKEY,LOCATION,LOCATION2,MERGTYPE,DELETED) VALUES ({i++},{item.Id},{LocationInfos.FirstOrDefault(p=>p.Description==item.MergInfo).Id},'1','0');");
                }
            }
            sql.AppendLine();
            sql.Append("TRUNCATE TABLE MICS_LOCATIONMERG;").AppendLine();
            sql.Append(string.Join("\t\n",merginfo));
            

            SqlText = sql.ToString();
        }



        #endregion
    }

    public class LocationInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public LocationType Type { get; set; }
        public string MergInfo { get; set; }
    }

    public enum LocationType
    {
        None,
        中心 = 1,
        车辆段 = 2,
        车站 = 3,
        主变 = 4
    }
}