using CorpseLib.Json;
using CorpseLib.Logging;
using CorpseLib.Web.API;
using StreamGlass.Core.Plugin;
using StreamGlass.Core.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace StreamGlass
{
    public partial class PluginManager
    {
        public static readonly Logger PLUGIN_LOGGER = new("[${d}-${M}-${y} ${h}:${m}:${s}.${ms}] ${log}") { new LogInFile("./log/${y}${M}${d}${h}_Plugins.log") };

        public class PluginLoader(string root)
        {
            private readonly Dictionary<string, Assembly> m_LoadedDependencies = [];
            private readonly List<string> m_DependenciesPath = [];
            private readonly List<string> m_PluginsPath = [];
            private readonly string m_PluginRoot = root;

            public void AddDependency(string dependency)
            {
                string pathToAdd = (Path.IsPathRooted(dependency)) ? dependency : Path.GetFullPath(Path.Combine(m_PluginRoot, dependency));
                m_DependenciesPath.Add(pathToAdd);
            }
            public void AddPlugin(string plugin)
            {
                string pathToAdd = (Path.IsPathRooted(plugin)) ? plugin : Path.GetFullPath(Path.Combine(m_PluginRoot, plugin));
                m_PluginsPath.Add(pathToAdd);
            }

            private Assembly MyResolve(object? sender, ResolveEventArgs e) => m_LoadedDependencies[e.Name];

            private void LoadDependencies()
            {
                foreach (string dependencyFile in m_DependenciesPath)
                {
                    try
                    {
                        Assembly dependencyAssembly = Assembly.LoadFile(dependencyFile);
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

            private List<Metadata> LoadPlugins()
            {
                List<Metadata> pluginMetadatas = [];
                foreach (string file in m_PluginsPath)
                {
                    try
                    {
                        Assembly dllAssembly = Assembly.LoadFile(file);
                        foreach (Type type in dllAssembly.GetExportedTypes())
                        {
                            if (type.IsAssignableTo(typeof(APlugin)))
                                pluginMetadatas.Add(new(type, m_PluginRoot, Path.GetFileName(file), File.GetLastWriteTime(file)));
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
                return pluginMetadatas;
            }

            public List<Metadata> Load()
            {
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += MyResolve;
                AppDomain.CurrentDomain.AssemblyResolve += MyResolve;
                LoadDependencies();
                List<Metadata> ret = LoadPlugins();
                AppDomain.CurrentDomain.AssemblyResolve -= MyResolve;
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= MyResolve;
                m_LoadedDependencies.Clear();
                return ret;
            }
        }

        [GeneratedRegex(@"^[0-9a-zA-Z-_]+$", RegexOptions.IgnoreCase, "fr-FR")]
        private static partial Regex PluginNameRegex();
        private readonly List<APlugin> m_Plugins = [];
        private readonly List<ISettingsPlugin> m_SettingsPlugins = [];
        private readonly List<IAPIPlugin> m_APIPlugins = [];
        private readonly List<IUpdatablePlugin> m_UpdatablePlugins = [];
        private readonly List<ITestablePlugin> m_TestablePlugins = [];
        private readonly List<IPanelPlugin> m_PanelPlugins = [];

        public void RegisterToAPI(CorpseLib.Web.API.API api)
        {
            foreach (IAPIPlugin plugin in m_APIPlugins)
            {
                CorpseLib.Web.Http.Path path = new($"/{plugin.Name.ToLower()}");
                HTTPEndpointNode httpEndpointNode = new();
                WebSocketEndpointNode webSocketEndpointNode = new();
                AEndpoint[] endpoints = plugin.GetEndpoints();
                foreach (AEndpoint endpoint in endpoints)
                {
                    if (endpoint is AWebsocketEndpoint websocketEndpoint)
                        webSocketEndpointNode.Add(websocketEndpoint);
                    else if (endpoint is AHTTPEndpoint httpEndpoint)
                        httpEndpointNode.Add(httpEndpoint);
                }
                api.AddEndpointNode(path, httpEndpointNode);
                api.AddEndpointNode(path, webSocketEndpointNode);
            }
        }

        public void FillSettings(Dialog settingsDialog)
        {
            foreach (ISettingsPlugin plugin in m_SettingsPlugins)
            {
                TabItemContent[] settings = plugin.GetSettings();
                foreach (TabItemContent setting in settings)
                    settingsDialog.AddTabItem(setting);
            }
        }

        public void Update(long deltaTime)
        {
            foreach (IUpdatablePlugin plugin in m_UpdatablePlugins)
                plugin.Update(deltaTime);
        }

        public void Test()
        {
            foreach (ITestablePlugin plugin in m_TestablePlugins)
                plugin.Test();
        }

        public void LoadPlugin(Metadata metadata, APlugin plugin)
        {
            if (PluginNameRegex().Match(plugin.Name).Success)
            {
                plugin.InitMetadata(metadata);
                plugin.RegisterPlugin();
                if (plugin is IAPIPlugin apiPlugin)
                    m_APIPlugins.Add(apiPlugin);
                if (plugin is IUpdatablePlugin updatablePlugin)
                    m_UpdatablePlugins.Add(updatablePlugin);
                if (plugin is ITestablePlugin testablePlugin)
                    m_TestablePlugins.Add(testablePlugin);
                if (plugin is ISettingsPlugin settingsPlugin)
                    m_SettingsPlugins.Add(settingsPlugin);
                if (plugin is IPanelPlugin panelPlugin)
                    m_PanelPlugins.Add(panelPlugin);
                m_Plugins.Add(plugin);
                PLUGIN_LOGGER.Log(string.Format("Plugin loaded {0}", plugin.Name));
            }
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

        private static List<Metadata> ExtractPluginsMetadata()
        {
            List<Metadata> pluginMetadatas = [];
            if (Directory.Exists("plugins/"))
            {
                string[] directories = Directory.GetDirectories("plugins/");
                foreach (string directory in directories)
                {
                    string pluginDirectory = (directory[^1] == '/') ? directory : string.Format("{0}/", directory);
                    string jsonFile = Path.GetFullPath(Path.Combine(pluginDirectory, "plugin.json"));
                    if (File.Exists(jsonFile))
                    {
                        PluginLoader loader = new(pluginDirectory);
                        JsonObject settings = JsonParser.LoadFromFile(jsonFile);
                        List<string> dependencies = settings.GetList<string>("dependencies");
                        foreach (string dependency in dependencies)
                            loader.AddDependency(dependency);
                        List<string> plugins = settings.GetList<string>("plugins");
                        foreach (string plugin in plugins)
                            loader.AddPlugin(plugin);
                        pluginMetadatas.AddRange(loader.Load());
                    }
                }
            }
            return pluginMetadatas;
        }

        /*private void SortReloadedPlugins(List<Metadata> loaded, ref List<Metadata> toLoad, ref List<Plugin> toRemove)
        {
            //TODO
        }*/

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
            foreach (APlugin plugin in m_Plugins)
                plugin.UnregisterPlugin();
            m_Plugins.Clear();
        }

        public void InitPlugins()
        {
            foreach (APlugin plugin in m_Plugins)
                plugin.Init();
        }

        public Control? GetPanel(string panelID)
        {
            foreach (IPanelPlugin panelPlugin in m_PanelPlugins)
            {
                Control? control = panelPlugin.GetPanel(panelID);
                if (control != null)
                    return control;
            }
            return null;
        }
    }
}
