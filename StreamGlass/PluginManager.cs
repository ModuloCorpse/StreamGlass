using CorpseLib.Ini;
using StreamGlass.Core.Connections;
using StreamGlass.Core.Profile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace StreamGlass
{
    public class PluginManager(IniFile settings, ProfileManager profileManager, ConnectionManager connectionManager)
    {
        private readonly IniFile m_Settings = settings;
        private readonly ProfileManager m_ProfileManager = profileManager;
        private readonly ConnectionManager m_ConnectionManager = connectionManager;
        private readonly List<APlugin> m_Plugins = [];

        public void RegisterToAPI(CorpseLib.Web.API.API api)
        {
            foreach (APlugin plugin in m_Plugins)
                plugin.RegisterPluginToAPI(api);
        }

        public void LoadPlugin(APlugin plugin)
        {
            plugin.OnInit(m_ProfileManager, m_ConnectionManager, m_Settings.GetOrAdd(plugin.Name));
            plugin.RegisterPlugin();
            m_Plugins.Add(plugin);
        }

        public void LoadPlugins()
        {
            if (Directory.Exists("plugins/"))
            {
                string[] files = Directory.GetFiles("plugins/");
                foreach (string file in files)
                {
                    try
                    {
                        Assembly dllAssembly = Assembly.LoadFile(file);
                        foreach (Type type in dllAssembly.GetExportedTypes())
                        {
                            if (type.IsAssignableTo(typeof(APlugin)))
                            {
                                object? pluginInstance = Activator.CreateInstance(type);
                                if (pluginInstance != null && pluginInstance is APlugin plugin)
                                    LoadPlugin(plugin);
                            }
                        }
                    }
                    catch { }
                }
            }
        }
    }
}
