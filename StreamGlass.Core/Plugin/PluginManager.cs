using CorpseLib.DataNotation;
using CorpseLib.Json;
using CorpseLib.Logging;
using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using StreamGlass.Core.API.Overlay;
using StreamGlass.Core.Plugin;
using StreamGlass.Core.Settings;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace StreamGlass.Core
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
            private bool m_Enable = true;

            public void SetEnable(bool enable) => m_Enable = enable;

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
                        PLUGIN_LOGGER.Log("Cannot load dependency ${0}", dependencyFile);
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
                            {
                                Metadata metadata = new(type, m_PluginRoot, Path.GetFileName(file), File.GetLastWriteTime(file));
                                metadata.SetEnable(m_Enable);
                                pluginMetadatas.Add(metadata);
                            }
                        }
                    }
                    catch (BadImageFormatException) { } //The file is not an assembly
                    catch (FileLoadException) { } //The assembly has already been loaded
                    catch (Exception ex)
                    {
                        PLUGIN_LOGGER.Log("Cannot load ${0}", file);
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
        private readonly List<APlugin> m_ActivePlugins = [];

        public PluginManager(TickManager tickManager)
        {
            tickManager.RegisterTickFunction(Update);
        }

        public void RegisterOverlays(ResourceSystem.Directory overlayDirectory)
        {
            foreach (APlugin plugin in m_ActivePlugins)
            {
                if (plugin is IOverlayPlugin overlayPlugin)
                {
                    Overlay[] overlays = overlayPlugin.GetOverlays();
                    foreach (Overlay overlay in overlays)
                        overlayDirectory.Add(CorpseLib.Web.Http.Path.Append(new($"/plugin/{plugin.Namespace.ToLower()}"), new(overlay.Name)), overlay);
                }
            }
        }

        public void RegisterToAPI(CorpseLib.Web.API.API api)
        {
            foreach (APlugin plugin in m_ActivePlugins)
            {
                if (plugin is IAPIPlugin apiPlugin)
                {
                    CorpseLib.Web.Http.Path path = new($"/{plugin.Namespace.ToLower()}");
                    ResourceSystem.Directory endpointTree = new();
                    foreach (var pair in apiPlugin.GetEndpoints())
                        endpointTree.Add(pair.Key, pair.Value);
                    api.AddDirectory(path, endpointTree);
                }
            }
        }

        public void FillSettings(Dialog settingsDialog)
        {
            foreach (APlugin plugin in m_ActivePlugins)
            {
                if (plugin is ISettingsPlugin settingsPlugin)
                {
                    TabItemContent[] settings = settingsPlugin.GetSettings();
                    foreach (TabItemContent setting in settings)
                        settingsDialog.AddTabItem(setting);
                }
            }
        }

        public void Update(long deltaTime)
        {
            foreach (APlugin plugin in m_ActivePlugins)
            {
                if (plugin is IUpdatablePlugin updatablePlugin)
                    updatablePlugin.Update(deltaTime);
            }
        }

        public void Test()
        {
            foreach (APlugin plugin in m_ActivePlugins)
            {
                if (plugin is ITestablePlugin testablePlugin)
                    testablePlugin.Test();
            }
        }

        public void LoadPlugin(Metadata metadata, APlugin plugin)
        {
            if (PluginNameRegex().Match(plugin.Name).Success)
            {
                plugin.InitMetadata(metadata);
                plugin.RegisterPlugin();
                m_Plugins.Add(plugin);
                if (plugin.Enable)
                    m_ActivePlugins.Add(plugin);
                PLUGIN_LOGGER.Log("Plugin loaded ${0}", plugin.Name);
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
                    PLUGIN_LOGGER.Log("Cannot instantiate plugin ${0}", metadata.Filename);
            }
            catch (Exception ex)
            {
                PLUGIN_LOGGER.Log("Cannot load plugin ${0}", metadata.Filename);
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
                    string metadataFile = Path.GetFullPath(Path.Combine(pluginDirectory, "plugin.json"));
                    if (File.Exists(metadataFile))
                    {
                        PluginLoader loader = new(pluginDirectory);
                        DataObject settings = JsonParser.LoadFromFile(metadataFile);
                        loader.SetEnable(settings.GetOrDefault("enable", true));
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

        private void SetEnablePlugin(APlugin plugin, bool enable)
        {
            if (!plugin.Metadata!.Native && enable != plugin.Enable)
            {
                plugin.SetEnable(enable);
                if (enable)
                {
                    plugin.RegisterPlugin();
                    plugin.Init();
                    m_ActivePlugins.Add(plugin);
                }
                else
                {
                    plugin.UnregisterPlugin();
                    m_ActivePlugins.Remove(plugin);
                }
            }
        }

        public void Clear()
        {
            foreach (APlugin plugin in m_ActivePlugins)
                plugin.UnregisterPlugin();
            m_ActivePlugins.Clear();
            foreach (APlugin plugin in m_Plugins)
            {
                Metadata metadata = plugin.Metadata!;
                if (!metadata.Native)
                {
                    string metadataFilePath = Path.GetFullPath(Path.Combine(metadata.Directory, "plugin.json"));
                    DataObject settings = JsonParser.LoadFromFile(metadataFilePath);
                    settings["enable"] = metadata.Enable;
                    JsonParser.WriteToFile(metadataFilePath, settings);
                }
            }
            m_Plugins.Clear();
        }

        public void InitPlugins()
        {
            foreach (APlugin plugin in m_ActivePlugins)
                plugin.Init();
        }

        public Control? GetPanel(string panelID)
        {
            foreach (APlugin plugin in m_ActivePlugins)
            {
                if (plugin is IPanelPlugin panelPlugin)
                {
                    Control? control = panelPlugin.GetPanel(panelID);
                    if (control != null)
                        return control;
                }
            }
            return null;
        }
    }
}
