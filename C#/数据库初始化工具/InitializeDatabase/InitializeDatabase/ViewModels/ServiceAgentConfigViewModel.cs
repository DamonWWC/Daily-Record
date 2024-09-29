using LiteDB;
using PCI.Framework.ORM;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace InitializeDatabase.ViewModels
{
    public class ServiceAgentConfigViewModel : BindableBase
    {
        private readonly IDAFacade _dAFacade;
        private readonly ILiteDatabase rawDataDb;
        private readonly IDialogService _dialogService;

        public ServiceAgentConfigViewModel(IDAFacade dAFacade, IDialogService dialogService)
        {
            _dAFacade = dAFacade;
            _dialogService = dialogService;
            rawDataDb = ContainerLocator.Container.Resolve<ILiteDatabase>("RawData");
            _agentInfos = rawDataDb.GetCollection<AgentInfo>("agentInfo").Query().ToList();
            InitData();
        }

        private readonly List<AgentInfo> _agentInfos;

        private void InitData()
        {
            //var monitorProcessInfos = rawDataDb.GetCollection<MonitorProcessInfo>("monitorProcessInfo").Query().ToList();
        }

        private ObservableCollection<AgentConfigInfo> _AgentConfigInfos = [];

        /// <summary>
        /// Agent配置信息
        /// </summary>
        public ObservableCollection<AgentConfigInfo> AgentConfigInfos
        {
            get { return _AgentConfigInfos; }
            set { SetProperty(ref _AgentConfigInfos, value); }
        }

        private DelegateCommand _SelectCommand;

        public DelegateCommand SelectCommand => _SelectCommand ??= new DelegateCommand(ExecuteSelectCommand);

        private void ExecuteSelectCommand()
        {
            _dialogService.ShowDialog("AgentSelectView", new DialogParameters { { "info", _agentInfos } }, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue("info", out List<AgentInfo> newinfo))
                    {
                        foreach (var item in newinfo)
                        {
                            var finditem = AgentConfigInfos.FirstOrDefault(p => p.AppType == item.Id);
                            if (finditem != null)
                            {
                                if (!item.IsChecked)
                                {
                                    AgentConfigInfos.Remove(finditem);
                                }
                            }
                            else
                            {
                                AgentConfigInfos.Add(new AgentConfigInfo
                                {
                                    AgentName = item.Name,
                                    AppType = item.Id,
                                    OCC = true
                                });
                            }
                        }
                    }
                }
                else
                {
                }
            });
        }

        private DelegateCommand<object> _SetCommand;

        /// <summary>
        /// 设置服务
        /// </summary>
        public DelegateCommand<object> SetCommand => _SetCommand ??= new DelegateCommand<object>(ExecuteSetCommand);

        private void ExecuteSetCommand(object param)
        {
            if (param is AgentConfigInfo agentConfigInfo)
            {
                _dialogService.ShowDialog("AppServiceMapView", new DialogParameters { { "info", agentConfigInfo.AppServiceMap },{ "title",agentConfigInfo.AgentName} }, r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue("info", out string result))
                        {
                            if (!string.IsNullOrWhiteSpace(result))
                            {
                                agentConfigInfo.AppServiceMap = result;
                            }
                        }
                    }
                    else
                    {
                    }
                });
            }
        }

        //private void InitData()
        //{
        //    var pasteinfo = Clipboard.GetText();
        //    List<AgentBaseInfo> agentBaseInfos = [];
        //    var cells = pasteinfo.Split(new string[] { "\r\n" }, StringSplitOptions.None);
        //    foreach (string item in cells)
        //    {
        //        if (string.IsNullOrWhiteSpace(item)) continue;
        //        var items = item.Split('\t');
        //        agentBaseInfos.Add(new AgentBaseInfo
        //        {
        //            //Id = int.Parse(items[0]),
        //            Name = items[0],
        //            Address = items[1],
        //            Description = items[2],
        //            SubName = items[3],
        //            //TypeName = items[5],
        //        });

        //    }
        //    var col = rawDataDb.GetCollection<AgentBaseInfo>("monitorProcessInfo");
        //    col.DeleteAll();
        //    //col.InsertBulk(agentBaseInfos.OrderBy(p => p.Id));
        //    col.InsertBulk(agentBaseInfos);
        //}
    }

    public class AgentConfigInfo : BindableBase
    {
        public string AgentName { get; set; }
        public int AppType { get; set; }
        private bool _OCC;

        public bool OCC
        {
            get { return _OCC; }
            set { SetProperty(ref _OCC, value); }
        }

        public bool Station { get; set; }

        /// <summary>
        /// 段场
        /// </summary>
        public bool Depot { get; set; }

        /// <summary>
        /// 主变
        /// </summary>
        public bool Substation { get; set; }

        public bool CheckDB { get; set; }
        public bool CheckComm { get; set; }
        public bool ExchangeType { get; set; }
        public bool AutoFailBack { get; set; }
        private string _AppServiceMap;

        public string AppServiceMap
        {
            get { return _AppServiceMap; }
            set { SetProperty(ref _AppServiceMap, value); }
        }
    }

    public class MonitorProcessInfo
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string SubName { get; set; }
    }

    public class AgentBaseInfo : BindableBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string SubName { get; set; }
        public string TypeName { get; set; }
    }

    public class AgentInfo : AgentBaseInfo
    {
        private bool _IsChecked;

        public bool IsChecked
        {
            get { return _IsChecked; }
            set { SetProperty(ref _IsChecked, value); }
        }
    }

    public class AppType
    {
        public int TypeId { get; set; }
        public int TypeKey { get; set; }
        public string TypeName { get; set; }
    }
}