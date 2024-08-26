using InitializeDatabase.Helper;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;

namespace InitializeDatabase.ViewModels
{
    public class LocationInfoConfigurationViewModel : BindableBase
    {
        public LocationInfoConfigurationViewModel()
        {
            LocationTypes = EnumHelper.ToList<LocationType>().Where(p=>p!=LocationType.None).ToList();
        }

        private List<LocationInfo> _LocationInfos;
        /// <summary>
        /// 位置信息
        /// </summary>
        public List<LocationInfo> LocationInfos
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

      
        #region cmd
        private DelegateCommand _PasteCommand;
        public DelegateCommand PasteCommand =>
            _PasteCommand ?? (_PasteCommand = new DelegateCommand(ExecutePasteCommand));

        void ExecutePasteCommand()
        {
            var pasteinfo = Clipboard.GetText();
            List<LocationInfo> locationInfos = [];

            var cells = pasteinfo.Split(new string[] { "\r\n" },StringSplitOptions.None);
            foreach(string item in cells)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                var items = item.Split('\t');

                
                locationInfos.Add(new LocationInfo
                {
                    Id = items.ElementAtOrDefault(0),
                    Name = items.ElementAtOrDefault(1),
                    Description = items.ElementAtOrDefault(2),
                    Type = EnumHelper.ToEnum<LocationType>(items.ElementAtOrDefault(3)??"None"),
                    MergInfo = null
                });
            }
            LocationInfos = locationInfos;
        }
        private DelegateCommand _ClearCommand;
        public DelegateCommand ClearCommand =>
            _ClearCommand ?? (_ClearCommand = new DelegateCommand(ExecuteClearCommand));

        void ExecuteClearCommand()
        {
            
            LocationInfos = [];
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
        中心=1,
        车辆段=2,
        车站=3,
        主变=4
    }
}
