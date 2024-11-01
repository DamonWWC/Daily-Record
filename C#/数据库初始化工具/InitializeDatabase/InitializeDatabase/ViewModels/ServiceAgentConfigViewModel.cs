using HandyControl.Tools.Extension;
using InitializeDatabase.ViewModels.Dialog;
using LiteDB;
using PCI.Framework.ORM;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace InitializeDatabase.ViewModels
{
    public class ServiceAgentConfigViewModel : BindableBase
    {
        private readonly IDAFacade _dAFacade;
        private readonly ILiteDatabase db;
        private readonly ILiteDatabase rawDataDb;
        private readonly ILiteDatabase initDataDb;
        private readonly IDialogService _dialogService;
        private readonly IEventAggregator _ea;

        public ServiceAgentConfigViewModel(IDAFacade dAFacade, IDialogService dialogService, IEventAggregator ea)
        {
            _dAFacade = dAFacade;
            _dialogService = dialogService;
            _ea = ea;
            rawDataDb = ContainerLocator.Container.Resolve<ILiteDatabase>("RawData");
            initDataDb = ContainerLocator.Container.Resolve<ILiteDatabase>("InitData");
            db = ContainerLocator.Container.Resolve<ILiteDatabase>("InitData");
            _agentInfos = rawDataDb.GetCollection<AgentInfo>("agentInfo").Query().ToList();
            _monitorProcessInfos = rawDataDb.GetCollection<MonitorProcessInfo>("monitorProcessInfo").Query().ToList();
            InitData();
        }

        private readonly List<AgentInfo> _agentInfos;
        private readonly List<MonitorProcessInfo> _monitorProcessInfos;

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
                _dialogService.ShowDialog("AppServiceMapView", new DialogParameters { { "info", agentConfigInfo.AppServiceMap }, { "title", agentConfigInfo.AgentName } }, r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue("info", out IEnumerable<IceServiceInfo> _icsServiceInfos))
                        {
                            if (_icsServiceInfos != null && _icsServiceInfos.Count() > 0)
                            {
                                StringBuilder str = new();
                                str.AppendLine($"中央：{string.Join(",", _icsServiceInfos.Where(p => p.OCC).Select(p => p.ServiceId))}");
                                str.AppendLine($"车站：{string.Join(",", _icsServiceInfos.Where(p => p.Station).Select(p => p.ServiceId))}");
                                str.AppendLine($"段场：{string.Join(",", _icsServiceInfos.Where(p => p.Depot).Select(p => p.ServiceId))}");
                                str.AppendLine($"主变：{string.Join(",", _icsServiceInfos.Where(p => p.Substation).Select(p => p.ServiceId))}");
                                agentConfigInfo.AppServiceMap = str.ToString();
                                agentConfigInfo.IceServiceInfos = _icsServiceInfos.ToList();
                            }
                        }
                    }
                    else
                    {
                    }
                });
            }
        }

        private DelegateCommand _SaveCommand;
        public DelegateCommand SaveCommand => _SaveCommand ??= new DelegateCommand(ExecuteSaveCommand);

        private void ExecuteSaveCommand()
        {
            CreateSqlText();
        }

        private DelegateCommand _ShowCommand;

        /// <summary>
        /// 查看sql
        /// </summary>
        public DelegateCommand ShowCommand => _ShowCommand ??= new DelegateCommand(ExecuteShowCommand);

        private void ExecuteShowCommand()
        {
            CreateSqlText();
            _ea.GetEvent<ShowSqlEvent>().Publish(SqlText);
        }

        private void CreateSqlText()
        {
            StringBuilder sql = new();

            sql.AppendLine(CreateMonitoredProcessSql());
            sql.AppendLine(CreateAgentSql());
            sql.AppendLine(CreateMicsApplication());
            sql.AppendLine(CreateIceServiceSql());
            SqlText = sql.ToString();
        }

        private string CreateMonitoredProcessSql()
        {
            Dictionary<LocationType, List<string>> dic = new()
            {
                {LocationType.中心,new List<string>() },
                {LocationType.车站,new List<string>() },
                {LocationType.车辆段,new List<string>() },
                {LocationType.主变,new List<string>() }
            };

            AgentConfigInfos.ForEach(item =>
            {
                if (item.OCC)
                {
                    dic[LocationType.中心].Add(SqlText(item));
                }
                if (item.Station)
                {
                    dic[LocationType.车站].Add(SqlText(item));
                }
                if (item.Depot)
                {
                    dic[LocationType.车辆段].Add(SqlText(item));
                }
                if (item.Substation)
                {
                    dic[LocationType.主变].Add(SqlText(item));
                }
            });

            string SqlText(AgentConfigInfo item)
            {
                var process = _monitorProcessInfos.FirstOrDefault(p => p.Name == (item.AgentName.Replace("Agent", "") + "MonitoredProcess"));
                return ($"select '{process.Name}' name,'{process.Address}' address,' {process.Description}' description ,'{process.SubName}' subname from dual");
            }
            var locationInfos = initDataDb.GetCollection<LocationInfo>("locationInfos").Query();
            StringBuilder sql = new();

            dic.ForEach(item =>
            {
                if (item.Value.Count > 0)
                {
                    string str = string.Join(",", initDataDb.GetCollection<LocationInfo>("locationInfos").Find(p => p.Type == item.Key).Select(p => p.Id));
                    sql.AppendLine($"---{item.Key}\nINSERT INTO ENTITY (NAME,ADDRESS,DESCRIPTION,SUBSYSTEMKEY,LOCATIONKEY,SEREGI_ID,TYPEKEY,PHYSICAL_SUBSYSTEM_KEY,SCSDAT_ID,PARENTKEY,AGENTKEY,DELETED,CREATED_BY,DATE_CREATED,MODIFIED_BY,DATE_MODIFIED)\r\nSELECT initcap(lower(l.name))||t.name,t.address,l.name||t.description,(SELECT PKEY FROM SUBSYSTEM s WHERE name=t.subname) SUBSYSTEMKEY,l.pkey,l.pkey,2 typekey,\r\n NULL,NULL,(SELECT pkey FROM entity WHERE name='Parent Entity') PARENTKEY, (SELECT pkey FROM entity WHERE name='Parent Entity') AGENTKEY,0 DELETED ,'MICS' ,SYSDATE,NULL,NULL\r\n FROM LOCATION L JOIN \n(\n{string.Join(" union all \n", item.Value)}\n) T ON 1=1 WHERE l.PKEY IN ({str});\n\n");
                }
            });
            return sql.ToString();
        }

        private string CreateAgentSql()
        {
            Dictionary<LocationType, List<string>> dic = new()
            {
                {LocationType.中心,new List<string>() },
                {LocationType.车站,new List<string>() },
                {LocationType.车辆段,new List<string>() },
                {LocationType.主变,new List<string>() }
            };

            AgentConfigInfos.ForEach(item =>
            {
                if (item.OCC)
                {
                    dic[LocationType.中心].Add(SqlText(item));
                }
                if (item.Station)
                {
                    dic[LocationType.车站].Add(SqlText(item));
                }
                if (item.Depot)
                {
                    dic[LocationType.车辆段].Add(SqlText(item));
                }
                if (item.Substation)
                {
                    dic[LocationType.主变].Add(SqlText(item));
                }
            });

            string SqlText(AgentConfigInfo item)
            {
                var agent = _agentInfos.FirstOrDefault(p => p.Name == item.AgentName);
                return ($"select '{agent.Name}' name,'{agent.Address}' address,' {agent.Description}' description ,'{agent.SubName}' subname, '{agent.TypeName}' typename from dual");
            }
            var locationInfos = initDataDb.GetCollection<LocationInfo>("locationInfos").Query();
            StringBuilder sql = new();

            dic.ForEach(item =>
            {
                if (item.Value.Count > 0)
                {
                    string str = string.Join(",", initDataDb.GetCollection<LocationInfo>("locationInfos").Find(p => p.Type == item.Key).Select(p => p.Id));
                    sql.AppendLine($"---{item.Key}\nINSERT INTO ENTITY (NAME,ADDRESS,DESCRIPTION,SUBSYSTEMKEY,LOCATIONKEY,SEREGI_ID,TYPEKEY,PHYSICAL_SUBSYSTEM_KEY,SCSDAT_ID,PARENTKEY,AGENTKEY,DELETED,CREATED_BY,DATE_CREATED,MODIFIED_BY,DATE_MODIFIED)\r\nSELECT initcap(lower(l.name))||t.name,t.address,l.name||t.description,(SELECT PKEY FROM SUBSYSTEM s WHERE name=t.subname) SUBSYSTEMKEY\r\n ,l.pkey,l.pkey,\r\n (SELECT pkey FROM entitytype WHERE name=t.typename) typekey,\r\n (SELECT PKEY FROM SUBSYSTEM s WHERE name=t.subname) PHYSICAL_SUBSYSTEM_KEY,NULL,(SELECT pkey FROM entity WHERE name=initcap(lower(l.name))||REPLACE(t.name,'Agent','')||'MonitoredProcess') PARENTKEY, null AGENTKEY,0 DELETED ,'MICS' ,SYSDATE,NULL,NULL\r\nFROM LOCATION L JOIN  \n(\n{string.Join(" union all \n", item.Value)}\n) T ON 1=1 WHERE l.PKEY IN ({str});\n\n");
                }
            });
            return sql.ToString();
        }

        /// <summary>
        /// MICS_APPLICATION表数据初始化
        /// </summary>
        private string CreateMicsApplication()
        {
            Dictionary<string, List<string>> dic = new()
            {
                {"check_db",new List<string>() },
                {"check_comm",new List<string>() },
                {"exchange_type",new List<string>() },
                {"auto_failback",new List<string>() }
            };
            List<string> exclude = new();
            AgentConfigInfos.ForEach(item =>
            {
                if (item.IsConfig)
                {
                    exclude.Add($"name NOT LIKE '%{item.AgentName}'");
                }
                else
                {
                    if (item.CheckDB)
                    {
                        dic["check_db"].Add($"e.name LIKE '%{item.AgentName}'");
                    }
                    if (item.CheckComm)
                    {
                        dic["check_comm"].Add($"e.name LIKE '%{item.AgentName}'");
                    }
                    if (item.ExchangeType)
                    {
                        dic["exchange_type"].Add($"e.name LIKE '%{item.AgentName}'");
                    }
                    if (item.AutoFailBack)
                    {
                        dic["auto_failback"].Add($"e.name LIKE '%{item.AgentName}'");
                    }
                }
            });
            var items = AgentConfigInfos.Where(p => (p.OCC || p.Station || p.Substation || p.Depot) && !p.IsConfig).Select(p => $"name LIKE '%{p.AgentName}'");
            if (!items.Any()) return default;
            StringBuilder sql = new();
            sql.AppendLine("TRUNCATE TABLE MICS_APPLICATION;\nINSERT INTO MICS_APPLICATION\n(AGENT_KEY, ENABLED, DISPLAY_NAME, APP_TYPE, APP_PATH, APP_NAME, PID_FILE, CHECK_DB, CHECK_COMM, EXCHANGE_TYPE, AUTO_FAILBACK, LOG_LEVEL, LOG_SIZE, LOG_COUNT, AQ_LOCATION, AQ_QUALIFIER, DESCRIPTION)");
            sql.AppendLine("SELECT pkey AGENT_KEY, '1' enabled, DESCRIPTION display_name");
            sql.AppendLine(",(SELECT type_id FROM MICS_APP_TYPE mat WHERE upper(REPLACE(mat.TYPE_NAME,'_',''))=upper(substr(name,4))) app_type\r\n      ,'/u01/mics/bin' app_path\r\n      ,(SELECT 'mics_'||LOWER(type_name) FROM MICS_APP_TYPE mat WHERE upper(REPLACE(mat.TYPE_NAME,'_',''))=upper(substr(name,4))) app_name\r\n      ,(SELECT lower(substr(e.name,1,3))||'_'||LOWER(type_name)||'.pid' FROM MICS_APP_TYPE mat WHERE upper(REPLACE(mat.TYPE_NAME,'_',''))=upper(substr(name,4))) app_name");
            sql.AppendLine(dic["check_db"].Count == 0 ? ",'0' check_db" : $",CASE WHEN {string.Join("OR", dic["check_db"])} \nTHEN '1' ELSE '0' END check_db");
            sql.AppendLine(dic["check_comm"].Count == 0 ? ",'0' check_comm" : $",CASE WHEN {string.Join("OR", dic["check_comm"])} \nTHEN '1' ELSE '0' END check_comm");
            sql.AppendLine(dic["exchange_type"].Count == 0 ? ",'0' exchange_type" : $",CASE WHEN {string.Join("OR", dic["exchange_type"])} \nTHEN '1' ELSE '0' END exchange_type");
            sql.AppendLine(dic["auto_failback"].Count == 0 ? ",'0' auto_failback" : $",CASE WHEN {string.Join("OR", dic["auto_failback"])} \nTHEN '1' ELSE '0' END auto_failback");
            sql.AppendLine(",'3' log_level\r\n,'30' log_size\r\n,'1000' log_count\r\n,'MICS'||upper(substr(name,1,3)) aq_location\r\n,'GROUP' aq_qualifier\r\n,e.description \r\nFROM entity e");

            sql.Append($"WHERE ({string.Join(" OR ", items)})");
            if (exclude.Any())
            {
                sql.Append($"AND {string.Join("AND", exclude)}");
            }

            return sql.Append(";\n\n").ToString();
        }

        private string CreateIceServiceSql()
        {
            var locationinfos = db.GetCollection<LocationInfo>("locationInfos").Query().ToArray();
            StringBuilder sql = new("TRUNCATE TABLE MICS_APPLICATIONSERVICE_MAP;\nBEGIN\n");
            foreach (var item in AgentConfigInfos)
            {
                if (item.IceServiceInfos == null) continue;
                Dictionary<string, List<int>> dic = [];
                foreach(var iceitem in item.IceServiceInfos)
                {
                    List<string> id = [];
                    if (iceitem.OCC)
                    {
                        id.AddRange(locationinfos.Where(p => p.Type == LocationType.中心).Select(p => p.Id));
                    }
                    if (iceitem.Station)
                    {
                        id.AddRange( locationinfos.Where(p => p.Type == LocationType.车站).Select(p => p.Id));
                    }
                    if (iceitem.Depot)
                    {
                        id.AddRange(locationinfos.Where(p => p.Type == LocationType.车辆段).Select(p => p.Id));
                    }
                    if (iceitem.Substation)
                    {
                        id.AddRange(locationinfos.Where(p => p.Type == LocationType.主变).Select(p => p.Id));
                    }
                    var value = iceitem.ServiceId;
                    var key = string.Join(",", id);
                    if (dic.ContainsKey(key))
                    {
                        dic[key].Add(value);
                    }
                    else
                    {
                        dic[key]= [value];
                    }
                }  
                foreach (var item1 in dic)
                {
                    sql.AppendLine($"  FOR x IN (SELECT pkey FROM entity WHERE name LIKE '%{item.AgentName}' AND LOCATIONKEY IN ({item1.Key}))\n  LOOP");
                    foreach(var item2 in item1.Value)
                    {
                        sql.AppendLine($"    INSERT INTO MICS_APPLICATIONSERVICE_MAP (AGENT_KEY, SERVICE_ID) VALUES ( x.pkey, {item2} );");
                    }
                    sql.AppendLine("  END LOOP;\n");
                }
            }
            sql.Append("END;");

            return sql.ToString();
        }

        private string _SqlText;

        public string SqlText
        {
            get { return _SqlText; }
            set { SetProperty(ref _SqlText, value); }
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

        /// <summary>
        /// 车站
        /// </summary>
        public bool Station { get; set; }

        /// <summary>
        /// 段场
        /// </summary>
        public bool Depot { get; set; }

        /// <summary>
        /// 主变
        /// </summary>
        public bool Substation { get; set; }

        private bool _CheckDB = false;

        public bool CheckDB
        {
            get { return _CheckDB; }
            set { SetProperty(ref _CheckDB, value); }
        }

        private bool _CheckComm = false;

        public bool CheckComm
        {
            get { return _CheckComm; }
            set { SetProperty(ref _CheckComm, value); }
        }

        private bool _ExchangeType = false;

        public bool ExchangeType
        {
            get { return _ExchangeType; }
            set { SetProperty(ref _ExchangeType, value); }
        }

        private bool _AutoFailBack = false;

        public bool AutoFailBack
        {
            get { return _AutoFailBack; }
            set { SetProperty(ref _AutoFailBack, value); }
        }

        private bool _IsConfig;

        public bool IsConfig
        {
            get { return _IsConfig; }
            set
            {
                if (SetProperty(ref _IsConfig, value) && value)
                {
                    CheckDB = false;
                    CheckComm = false;
                    ExchangeType = false;
                    AutoFailBack = false;
                }
            }
        }

        private string _AppServiceMap;

        public string AppServiceMap
        {
            get { return _AppServiceMap; }
            set { SetProperty(ref _AppServiceMap, value); }
        }

        private List<IceServiceInfo> _IceServiceInfos;

        public List<IceServiceInfo> IceServiceInfos
        {
            get { return _IceServiceInfos; }
            set { SetProperty(ref _IceServiceInfos, value); }
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