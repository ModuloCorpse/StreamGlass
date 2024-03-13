using CorpseLib.Logging;
using CorpseLib.Web.API;
using StreamGlass.Core.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace StreamGlass
{
    public class PluginManager
    {
        public static readonly Logger PLUGIN_LOGGER = new("[${d}-${M}-${y} ${h}:${m}:${s}.${ms}] ${log}") { new LogInFile("./log/${y}${M}${d}${h}_Plugins.log") };

        private readonly List<Plugin> m_Plugins = [];
        private readonly Dictionary<string, Assembly> m_LoadedDependencies = [];

        public void RegisterToAPI(CorpseLib.Web.API.API api)
        {
            foreach (Plugin plugin in m_Plugins)
            {
                AEndpoint[] endpoints = plugin.GetPluginEndpoints();
                foreach (AEndpoint endpoint in endpoints)
                    api.AddEndpoint(endpoint);
            }
        }

        public void FillSettings(Core.Settings.Dialog settingsDialog)
        {
            foreach (Plugin plugin in m_Plugins)
                plugin.FillSettings(settingsDialog);
        }

        public void Update(long deltaTime)
        {
            foreach (Plugin plugin in m_Plugins)
                plugin.Tick(deltaTime);
        }

        public void Test()
        {
            foreach (Plugin plugin in m_Plugins)
                plugin.Test();
        }

        public void LoadPlugin(Metadata metadata, APlugin plugin)
        {
            m_Plugins.Add(new(metadata, plugin));
        }

        private static APlugin? Instantiate(Metadata metadata)
        {
            Type? pluginType = metadata.Type;
            if (pluginType == null)
                return null;
            try
            {
                object? pluginInstance = Activator.CreateInstance(pluginType);
                if (pluginInstance != null && pluginInstance is APlugin plugin)
                    return plugin;
                else
                    PLUGIN_LOGGER.Log(string.Format("Cannot instantiate plugin {0}", metadata.Filename));
            }
            catch (Exception ex)
            {
                PLUGIN_LOGGER.Log(string.Format("Cannot load plugin {0}", metadata.Filename));
                PLUGIN_LOGGER.Log(ex.ToString());
            }
            return null;
        }

        private List<Metadata> ExtractPluginsMetadata()
        {
            List<Metadata> pluginMetadatas = [];
            if (Directory.Exists("plugins/"))
            {
                string[] directories = Directory.GetDirectories("plugins/");
                foreach (string directory in directories)
                {
                    string pluginDirectory = (directory[^1] == '/') ? directory : string.Format("{0}/", directory);

                    //We load all dependencies first
                    string dependencyDirectory = string.Format("{0}dep/", pluginDirectory);
                    if (Directory.Exists(dependencyDirectory))
                    {
                        string[] dependencyFiles = Directory.GetFiles(dependencyDirectory);
                        foreach (string dependencyFile in dependencyFiles)
                        {
                            try
                            {
                                Assembly dependencyAssembly = Assembly.LoadFile(Path.GetFullPath(dependencyFile));
                                m_LoadedDependencies[dependencyAssembly.FullName!] = dependencyAssembly;
                            }
                            catch (BadImageFormatException) { } //The file is not an assembly
                            catch (FileLoadException) { } //The assembly has already been loaded
                            catch (Exception ex)
                            {
                                PLUGIN_LOGGER.Log(string.Format("Cannot load dependency {0}", dependencyFile));
                                PLUGIN_LOGGER.Log(ex.ToString());
                            }
                        }
                    }

                    string[] files = Directory.GetFiles(pluginDirectory);
                    foreach (string file in files)
                    {
                        try
                        {
                            string fullPath = Path.GetFullPath(file);
                            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += MyResolve;
                            AppDomain.CurrentDomain.AssemblyResolve += MyResolve;
                            Assembly dllAssembly = Assembly.LoadFile(fullPath);
                            foreach (Type type in dllAssembly.GetExportedTypes())
                            {
                                if (type.IsAssignableTo(typeof(APlugin)))
                                    pluginMetadatas.Add(new(type, pluginDirectory, Path.GetFileName(file), File.GetLastWriteTime(fullPath)));
                            }
                        }
                        catch (BadImageFormatException) { } //The file is not an assembly
                        catch (FileLoadException) { } //The assembly has already been loaded
                        catch (Exception ex)
                        {
                            PLUGIN_LOGGER.Log(string.Format("Cannot load {0}", file));
                            PLUGIN_LOGGER.Log(ex.ToString());
                        }
                    }

                    m_LoadedDependencies.Clear();
                }
            }
            return pluginMetadatas;
        }

        private Assembly MyResolve(object? sender, ResolveEventArgs e) => m_LoadedDependencies[e.Name];

        private void SortReloadedPlugins(List<Metadata> loaded, ref List<Metadata> toLoad, ref List<Plugin> toRemove)
        {
            //TODO
        }

        public void LoadPlugins()
        {
            List<Metadata> pluginTypes = ExtractPluginsMetadata();

            foreach (Metadata pluginMetadata in pluginTypes)
            {
                APlugin? plugin = Instantiate(pluginMetadata);
                if (plugin != null)
                    LoadPlugin(pluginMetadata, plugin);
            }
        }

        public void Clear()
        {
            foreach (Plugin plugin in m_Plugins)
                plugin.UnregisterPlugin();
            m_Plugins.Clear();
        }

        public void InitPlugins()
        {
            foreach (Plugin plugin in m_Plugins)
                plugin.Init();
        }
    }
}
