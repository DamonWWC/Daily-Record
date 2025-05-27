using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace BlankApp2.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        bool isCan = true;
        public DelegateCommand<object> UncheckedCommand => new(Unchecked,(p)=>isCan);

        private void Unchecked(object parameter)
        {
            CheckAllToggleChecked();
        }

        /// <summary>
        /// 指标选择
        /// </summary>
        public DelegateCommand<object> CheckedCommand => new DelegateCommand<object>(Checked,(p)=>isCan);

        private void Checked(object parameter)
        {
            CheckAllToggleChecked();
        }
        bool isCan1;
        private void CheckAllToggleChecked()
        {       
            ToggleChecked();
        }
        private void ToggleChecked()
        {
            isCan1 = true;
            IsToggleChecked =!(TrainPIDSInfos.Count==0)&& TrainPIDSInfos.All(p => p.IsSelected);
        }

        private List<string> _DirectionInfo;

        public List<string> DirectionInfo
        {
            get { return _DirectionInfo; }
            set { SetProperty(ref _DirectionInfo, value); }
        }
        private string selecedDir = "全部";
        private DelegateCommand<object> _DirSelectionChangedCommand;

        public DelegateCommand<object> DirSelectionChangedCommand =>
            _DirSelectionChangedCommand ?? (_DirSelectionChangedCommand = new DelegateCommand<object>(ExecuteDirSelectionChangedCommand));

        private List<TrainPIDSInfo> _TrainPIDSInfos = new List<TrainPIDSInfo>();

        public List<TrainPIDSInfo> TrainPIDSInfos
        {
            get { return _TrainPIDSInfos; }
            set { SetProperty(ref _TrainPIDSInfos, value); }
        }
        
        private bool _isToggleChecked;

        /// <summary>
        /// 是否全选
        /// </summary>
        public bool IsToggleChecked
        {
            get => _isToggleChecked;
            set
            {
                if (SetProperty(ref _isToggleChecked, value))
                {     
                    if(!isCan1)
                    {
                        ToggleAllTrainSelected(_isToggleChecked);
                    }                                                                               
                }
                isCan1 = false;
            }
        }
        private void ToggleAllTrainSelected(bool isToggleChecked)
        {
            isCan = false;
            foreach (var train in TrainPIDSInfos)
            {
                train.IsSelected = isToggleChecked;
            }
            isCan = true;
        }
        private TrainPIDSInfo _selectedTrain;

        /// <summary>
        /// 当前车辆
        /// </summary>
        public TrainPIDSInfo SelectedTrain
        {
            get => _selectedTrain;
            set
            {              
                SetProperty(ref _selectedTrain, value);                    
            }
        }
        List<TrainPIDSInfo> trainPIDSInfosAll = new List<TrainPIDSInfo>();
        
        /// <summary>
        /// 车辆方向选择
        /// </summary>
        /// <param name="param"></param>
        private void ExecuteDirSelectionChangedCommand(object param)
        {                    
            selecedDir = param.ToString();
            FilterTrainPIDSInfos();
            ToggleChecked();           
        }

        private void FilterTrainPIDSInfos()
        {
            if (selecedDir != "全部")
            {
                TrainPIDSInfos = trainPIDSInfosAll.Where(p => p.Redundancy == selecedDir).ToList();
            }
            else
            {
                TrainPIDSInfos = trainPIDSInfosAll;
            }
        }
        public MainWindowViewModel()
        {
            DirectionInfo = new List<string> { "全部", "上行", "下行", "未知" };
            trainPIDSInfosAll = new List<TrainPIDSInfo>
            {
                new TrainPIDSInfo{ TrainNo="111",TrainNumber="222",Redundancy="上行",StatusDesc="ss"},
                new TrainPIDSInfo{ TrainNo="222",TrainNumber="222",Redundancy="下行",StatusDesc="ss"},
                //new TrainPIDSInfo{ TrainNo="333",TrainNumber="222",Redundancy="未知",StatusDesc="ss"},
            };
            FilterTrainPIDSInfos();
        }
    }
    public class TrainPIDSInfo : BindableBase
    {
        public int Id { get; set; }

        /// <summary>
        /// 车组号
        /// </summary>
        public string TrainNo { get; set; }

        private string _TrainNumber = "0";
        /// <summary>
        /// 服务号
        /// </summary>
        public string TrainNumber
        {
            get { return _TrainNumber; }
            set { SetProperty(ref _TrainNumber, value); }
        }

        private string _Redundancy = "未知";
        /// <summary>
        /// 方向
        /// </summary>
        public string Redundancy
        {
            get { return _Redundancy; }
            set { SetProperty(ref _Redundancy, value); }
        }
        /// <summary>
        /// 列车设备信息
        /// </summary>
        public string TrainEquipment { get; set; }

        public string PointName { get; set; }

        /// <summary>
        /// 实体PKEY
        /// </summary>
        public string EntityKey { get; set; }

        private string _StatusDesc = "未定义";

        public string StatusDesc
        {
            get { return _StatusDesc; }
            set { SetProperty(ref _StatusDesc, value); }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { SetProperty(ref _IsSelected, value); }
        }
    }

    public class TrainPIDSInfoComparer : IEqualityComparer<TrainPIDSInfo>
    {
        public bool Equals(TrainPIDSInfo x, TrainPIDSInfo y)
        {
            if (object.ReferenceEquals(x, y)) return true;
            return x.TrainNo == y.TrainNo;
        }

        public int GetHashCode(TrainPIDSInfo obj)
        {
            return obj.TrainNo.GetHashCode();
        }
    }
}
