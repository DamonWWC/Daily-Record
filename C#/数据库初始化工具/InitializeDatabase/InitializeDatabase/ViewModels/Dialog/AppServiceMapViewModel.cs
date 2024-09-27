using LiteDB;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using Prism.Ioc;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using HandyControl.Data;
using F23.StringSimilarity;
using InitializeDatabase.Helper;

namespace InitializeDatabase.ViewModels.Dialog
{
    public class AppServiceMapViewModel : BindableBase, IDialogAware
    {
        private readonly ILiteDatabase rawDataDb;

        public AppServiceMapViewModel()
        {
            rawDataDb = ContainerLocator.Container.Resolve<ILiteDatabase>("RawData");
        }

        public string Title => "应用服务选择";

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
            if (parameters.TryGetValue("info", out string param))
            {
                _allIceServiceInfos = rawDataDb.GetCollection<IceServiceInfo>("iceServiceInfo").Query().ToList();
                if (!string.IsNullOrWhiteSpace(param))
                {
                    string[] maps = param.Split(',');
                    _allIceServiceInfos.FindAll(p => param.Contains(p.ServiceId.ToString())).ForEach(p => p.IsChecked = true);
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
           
            if(result.Count()!=0)
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

        public DelegateCommand<object> SearchStartedCommand =>_SearchStartedCommand ??= new DelegateCommand<object>(ExecuteSearchStartedCommand);


        DebounceDispatcher dispatcher = new DebounceDispatcher();
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
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK, new DialogParameters { { "info", string.Join(",", IceServiceInfos.Where(p => p.IsChecked).Select(p => p.ServiceId)) } }));
        }

        private DelegateCommand _CancelCommand;
        public DelegateCommand CancelCommand => _CancelCommand ??= new DelegateCommand(ExecuteCancelCommand);

        private void ExecuteCancelCommand()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
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
            get { return _IsChecked; }
            set { SetProperty(ref _IsChecked, value); }
        }
    }
}