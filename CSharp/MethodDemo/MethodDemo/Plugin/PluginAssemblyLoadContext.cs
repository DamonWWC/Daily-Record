using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace Plugin
{
    public class PluginAssemblyLoadContext:AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        public PluginAssemblyLoadContext(string pluginPath) : base(isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath) ;
        }
        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }
            return null;
        }
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string dllPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (dllPath != null)
            {
                return LoadUnmanagedDllFromPath(dllPath);
            }
            return IntPtr.Zero;
        }
    }
}
