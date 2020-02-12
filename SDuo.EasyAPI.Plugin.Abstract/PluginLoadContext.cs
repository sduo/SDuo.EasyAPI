using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace SDuo.EasyAPI.Plugin.Abstract
{
    public class PluginLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver resolver { get; set; }

        public PluginLoadContext(string plugin) :base(true)
        {
            resolver = new AssemblyDependencyResolver(plugin);
        }

        protected override Assembly Load(AssemblyName name)
        {
            string path = resolver.ResolveAssemblyToPath(name);
            if (path != null)
            {                
                return LoadFromAssemblyPath(path);
            }
            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string name)
        {
            string path = resolver.ResolveUnmanagedDllToPath(name);
            if (path != null)
            {
                return LoadUnmanagedDllFromPath(path);
            }
            return IntPtr.Zero;
        }
    }
}
