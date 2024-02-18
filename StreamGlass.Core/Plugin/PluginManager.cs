using System.IO;
using System.Reflection;

namespace StreamGlass.Core.Plugin
{
    public class PluginManager
    {
        private readonly List<APlugin> m_Plugins = [];

        public void RegisterToAPI(CorpseLib.Web.API.API api)
        {
            foreach (APlugin plugin in m_Plugins)
                plugin.RegisterPluginToAPI(api);
        }

        public void FillSettings(Settings.Dialog settingsDialog)
        {
            foreach (APlugin plugin in m_Plugins)
                plugin.FillSettings(settingsDialog);
        }

        public void Update(long deltaTime)
        {
            foreach (APlugin plugin in m_Plugins)
                plugin.Tick(deltaTime);
        }

        public void Test()
        {
            foreach (APlugin plugin in m_Plugins)
                plugin.Test();
        }

        public void LoadPlugin(APlugin plugin)
        {
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

        public void Clear()
        {
            foreach (APlugin plugin in m_Plugins)
                plugin.UnregisterPlugin();
            m_Plugins.Clear();
        }

        public void InitPlugins()
        {
            foreach (APlugin plugin in m_Plugins)
                plugin.Init();
        }
    }
}
