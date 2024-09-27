using InitializeDatabase.Views;
using InitializeDatabase.Views.Dialog;
using LiteDB;
using PCI.Framework.ORM;
using Prism.Ioc;
using System.Windows;

namespace InitializeDatabase
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance(DAFacadeFactory.CreateDAFacade(ConnectType.Dm, "Server=172.25.11.144;Port=5236;Database=micsDB;User Id=MICS;PWD=DGL1mics;", 120));
            //初始化数据
            containerRegistry.RegisterInstance<ILiteDatabase>(new LiteDatabase(@"Filename=InitData.db;Password=micsServer"), "InitData");
            //原始数据
            containerRegistry.RegisterInstance<ILiteDatabase>(new LiteDatabase(@"Filename=RawData.db;Password=micsServer"), "RawData");
            containerRegistry.RegisterForNavigation<LocationInfoConfigurationView>();
            containerRegistry.RegisterForNavigation<SubSystemConfigurationView>();
            containerRegistry.RegisterForNavigation<SubSystemInfoView>();
            containerRegistry.RegisterForNavigation<MajorInfoConfigurationView>();
            containerRegistry.RegisterForNavigation<ServiceAgentConfigView>();
            containerRegistry.RegisterDialog<AgentSelectView>();
            containerRegistry.RegisterDialog<AppServiceMapView>();
        }
    }
}