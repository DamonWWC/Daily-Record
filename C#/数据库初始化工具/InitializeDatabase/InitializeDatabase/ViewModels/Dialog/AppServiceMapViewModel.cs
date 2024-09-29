using F23.StringSimilarity;
using HandyControl.Data;
using InitializeDatabase.Helper;
using LiteDB;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InitializeDatabase.ViewModels.Dialog
{
    public class AppServiceMapViewModel : BindableBase, IDialogAware
    {
        private readonly ILiteDatabase rawDataDb;

        public AppServiceMapViewModel()
        {
            rawDataDb = ContainerLocator.Container.Resolve<ILiteDatabase>("RawData");
        }

        public string Title { get; set; }

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue("title", out string title))
            {
                Title = $"{title}应用服务选择";
            }
            if (parameters.TryGetValue("info", out string param))
            {
                _allIceServiceInfos = rawDataDb.GetCollection<IceServiceInfo>("iceServiceInfo").Query().ToList();
                if (!string.IsNullOrWhiteSpace(param))
                {
                    string[] maps = param.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                    foreach (var item in _allIceServiceInfos)
                    {
                        item.OCC = maps[0].Contains(item.ServiceId.ToString());
                        item.Station = maps[1].Contains(item.ServiceId.ToString());
                        item.Depot = maps[2].Contains(item.ServiceId.ToString());
                        item.Substation = maps[3].Contains(item.ServiceId.ToString());
                    }
                }
                Filter();
            }
        }

        private List<IceServiceInfo> _IceServiceInfos;

        public List<IceServiceInfo> IceServiceInfos
        {
            get { return _IceServiceInfos; }
            set { SetProperty(ref _IceServiceInfos, value); }
        }

        private bool _IsFilter;

        public bool IsFilter
        {
            get { return _IsFilter; }
            set
            {
                if (SetProperty(ref _IsFilter, value))
                {
                    Filter();
                }
            }
        }

        private void Filter()
        {
            List<IceServiceInfo> allIceServiceInfos;
            if (IsFilter)
            {
                allIceServiceInfos = _allIceServiceInfos.Where(p => p.IsChecked)?.ToList();
            }
            else
            {
                allIceServiceInfos = _allIceServiceInfos;
            }
            var result = Filter1(allIceServiceInfos).ToList();

            if (result.Count() != 0)
            {
                result.Sort((x, y) => y.Item1.CompareTo(x.Item1));
                IceServiceInfos = result.Select(p => p.Item2).ToList();
            }
            else
            {
                IceServiceInfos = allIceServiceInfos;
            }
        }

        private IEnumerable<(double, IceServiceInfo)> Filter1(List<IceServiceInfo> allIceServiceInfos)
        {
            if (string.IsNullOrWhiteSpace(SearchText)) yield break;
            foreach (var item in allIceServiceInfos)
            {
                var similarity = new JaroWinkler().Similarity(SearchText, item.Description);
                if (similarity > 0.5)
                {
                    yield return (similarity, item);
                }
            }
        }

        private string SearchText;
        private DelegateCommand<object> _SearchStartedCommand;

        public DelegateCommand<object> SearchStartedCommand => _SearchStartedCommand ??= new DelegateCommand<object>(ExecuteSearchStartedCommand);

        private DebounceDispatcher dispatcher = new DebounceDispatcher();

        private void ExecuteSearchStartedCommand(object param)
        {
            dispatcher.Debounce(() =>
            {
                if (param is FunctionEventArgs<string> args)
                {
                    SearchText = args.Info;
                    Filter();
                }
            }, 500);
        }

        private DelegateCommand _ConfirmCommand;
        public DelegateCommand ConfirmCommand => _ConfirmCommand ??= new DelegateCommand(ExecuteConfirmCommand);

        private void ExecuteConfirmCommand()
        {
            StringBuilder result = new();
            result.AppendLine($"中央：{string.Join(",", IceServiceInfos.Where(p => p.OCC).Select(p => p.ServiceId))}");
            result.AppendLine($"车站：{string.Join(",", IceServiceInfos.Where(p => p.Station).Select(p => p.ServiceId))}");
            result.AppendLine($"段场：{string.Join(",", IceServiceInfos.Where(p => p.Depot).Select(p => p.ServiceId))}");
            result.AppendLine($"主变：{string.Join(",", IceServiceInfos.Where(p => p.Substation).Select(p => p.ServiceId))}");
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK, new DialogParameters { { "info", result.ToString() } }));
        }

        private DelegateCommand _CancelCommand;
        public DelegateCommand CancelCommand => _CancelCommand ??= new DelegateCommand(ExecuteCancelCommand);

        private void ExecuteCancelCommand()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        private DelegateCommand<object> _CheckCommand;
        public DelegateCommand<object> CheckCommand => _CheckCommand ??= new DelegateCommand<object>(ExecuteCheckCommand);

        private void ExecuteCheckCommand(object param)
        {
            if (param is IceServiceInfo iceServiceInfo && !iceServiceInfo.IsChecked)
            {
                Filter();
            }
        }

        private List<IceServiceInfo> _allIceServiceInfos = [];

        private void InitData()
        {
            //var pasteinfo = Clipboard.GetText();
            //List<IceServiceInfo> agentBaseInfos = [];
            //var cells = pasteinfo.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            //foreach (string item in cells)
            //{
            //    if (string.IsNullOrWhiteSpace(item)) continue;
            //    var items = item.Split('\t');
            //    agentBaseInfos.Add(new IceServiceInfo
            //    {
            //        ServiceId = int.Parse(items[0]),
            //        ServiceType = int.Parse(items[1]),
            //        Enabled = int.Parse(items[2]),
            //        AdapterName = items[3],
            //        IdentifyName = items[4],
            //        ProtocolName = items[5],
            //        ProtocolPort = int.Parse(items[6]),
            //        Description = items[7]
            //    });
            //}
            //var col = rawDataDb.GetCollection<IceServiceInfo>("iceServiceInfo");
            //col.DeleteAll();
            //col.InsertBulk(agentBaseInfos.OrderBy(p => p.ServiceId));
            ////col.InsertBulk(agentBaseInfos);
        }
    }

    public class IceServiceBaseInfo : BindableBase
    {
        public int ServiceId { get; set; }
        public int ServiceType { get; set; }
        public int Enabled { get; set; }
        public string AdapterName { get; set; }
        public string IdentifyName { get; set; }
        public string ProtocolName { get; set; }
        public int ProtocolPort { get; set; }
        public string Description { get; set; }
    }

    public class IceServiceInfo : IceServiceBaseInfo
    {
        private bool _IsChecked;

        [BsonIgnore]
        public bool IsChecked
        {
            get
            {
                return OCC || Station || Depot || Substation;
            }
        }

        private bool _OCC;

        [BsonIgnore]
        public bool OCC
        {
            get { return _OCC; }
            set { SetProperty(ref _OCC, value); }
        }

        [BsonIgnore]
        public bool Station { get; set; }

        /// <summary>
        /// 段场
        /// </summary>
        [BsonIgnore]
        public bool Depot { get; set; }

        /// <summary>
        /// 主变
        /// </summary>
        [BsonIgnore]
        public bool Substation { get; set; }
    }
}