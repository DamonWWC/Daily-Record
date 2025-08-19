using System.Configuration;
using System.Data;
using System.Windows;

namespace Flyleaf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
           // AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;     
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            
        }
        //private void CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
            
        //}
    }

}
