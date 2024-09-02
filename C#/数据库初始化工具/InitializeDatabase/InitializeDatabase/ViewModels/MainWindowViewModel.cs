using HandyControl.Controls;
using HandyControl.Data;
using PCI.Framework.ORM;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace InitializeDatabase.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        IEventAggregator _ea;
        public MainWindowViewModel(IDAFacade dAFacade, IRegionManager regionManager, IEventAggregator ea)
        {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<ShowSqlEvent>().Subscribe(ShowSql);//订阅事件
            _regionManager.RegisterViewWithRegion("ContentRegion", "LocationInfoConfigurationView");
        }

        private void ShowSql(string sql)
        {
            IsOpen = true;
            SqlText = sql;
        }
        private string _SqlText;
        public string SqlText
        {
            get { return _SqlText; }
            set { SetProperty(ref _SqlText, value); }
        }
        private bool _IsOpen;
        public bool IsOpen
        {
            get { return _IsOpen; }
            set { SetProperty(ref _IsOpen, value); }
        }

        private DelegateCommand<FunctionEventArgs<object>> _SwitchItemCmd;
        public DelegateCommand<FunctionEventArgs<object>> SwitchItemCmd => _SwitchItemCmd ??= new DelegateCommand<FunctionEventArgs<object>>(ExecuteSwitchItemCmd);

        private void ExecuteSwitchItemCmd(FunctionEventArgs<object> args)
        {
            if (args.Info is SideMenuItem item)
            {
                switch (item.Header)
                {
                    case "车站信息":
                        _regionManager.RequestNavigate("ContentRegion", "LocationInfoConfigurationView");
                        break;

                    case "专业信息":
                        _regionManager.RequestNavigate("ContentRegion", "SubSystemConfigurationView");
                        break;

                    case "子系统信息":
                        _regionManager.RequestNavigate("ContentRegion", "SubSystemInfoView");
                        break;
                }
            }
        }
    }
}