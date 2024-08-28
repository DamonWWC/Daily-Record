using InitializeDatabase.Views;
using LiteDB;
using PCI.Framework.ORM;
using Prism.Ioc;
using Prism.Regions;
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
            containerRegistry.RegisterInstance<ILiteDatabase>(new LiteDatabase(@"Filename=InitData.db;Password=micsServer"));
            containerRegistry.RegisterForNavigation<LocationInfoConfigurationView>();
            
        }

        


    }
}
