using HandyControl.Controls;
using HandyControl.Data;
using PCI.Framework.ORM;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System.Windows.Controls;

namespace InitializeDatabase.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        readonly IRegionManager _regionManager;
        public MainWindowViewModel(IDAFacade dAFacade, IRegionManager regionManager)
        {
            _regionManager = regionManager;
            _regionManager.RegisterViewWithRegion("ContentRegion", "LocationInfoConfigurationView");
        }


        private DelegateCommand<FunctionEventArgs<object>> _SwitchItemCmd;
        public DelegateCommand<FunctionEventArgs<object>> SwitchItemCmd => _SwitchItemCmd ??= new DelegateCommand<FunctionEventArgs<object>>(ExecuteSwitchItemCmd);

        void ExecuteSwitchItemCmd(FunctionEventArgs<object> args)
        {
            if(args.Info is SideMenuItem item)
            {
                switch(item.Header)
                {
                    case "车站信息":
                        _regionManager.RequestNavigate("ContentRegion", "LocationInfoConfigurationView");
                        break;
                    case "子系统专业配置":
                        _regionManager.RequestNavigate("ContentRegion", "SubSystemConfigurationView");
                        break;
                }
                
            }
               
        }

       


    }
   
}
