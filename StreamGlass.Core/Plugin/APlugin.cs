using CorpseLib.Ini;
using CorpseLib.Web.API;
using StreamGlass.Core.Settings;
using System.IO;

namespace StreamGlass.Core.Plugin
{
    public abstract class APlugin(string name, string settingsFile)
    {
        protected readonly IniFile m_Settings = [];
        private readonly string m_SettingsFile = settingsFile;
        private readonly string m_Name = name;
        private string m_PluginDirectory = string.Empty;

        internal string Name => m_Name;

        protected APlugin(string name) : this(name, string.Empty) { }

        internal void InitPluginDirectory(string directory) => m_PluginDirectory = directory;
        internal PluginInfo GetPluginInfo() => GeneratePluginInfo();

        protected string GetFilePath(string fileName) => string.Format("{0}{1}", m_PluginDirectory, fileName);

        internal void Init() => InitPlugin();
        internal void RegisterPlugin()
        {
            if (!string.IsNullOrEmpty(m_SettingsFile))
            {
                string settingsFilePath = GetFilePath(m_SettingsFile);
                if (File.Exists(settingsFilePath))
                {
                    IniFile iniFile = IniParser.LoadFromFile(settingsFilePath);
                    if (!iniFile.HaveEmptySection)
                        m_Settings.Merge(iniFile);
                }
            }

            InitTranslation();
            InitSettings();
            InitCommands();
            InitCanals();
        }

        internal AEndpoint[] GetPluginEndpoints() => GetEndpoints();
        internal void UnregisterPlugin()
        {
            Unregister();
            if (!string.IsNullOrEmpty(m_SettingsFile))
                m_Settings.WriteToFile(string.Format("{0}{1}", m_PluginDirectory, m_SettingsFile));
        }
        internal void Test() => TestPlugin();
        internal void Tick(long deltaTime) => Update(deltaTime);
        internal void FillSettings(Dialog settingsDialog)
        {
            TabItemContent[] settings = GetSettings();
            foreach (TabItemContent setting in settings)
                settingsDialog.AddTabItem(setting);
        }

        protected abstract PluginInfo GeneratePluginInfo();

        protected abstract void InitTranslation();
        protected abstract void InitSettings();
        protected abstract void InitCommands();
        protected abstract void InitCanals();
        protected abstract void InitPlugin();

        protected abstract AEndpoint[] GetEndpoints();
        protected abstract void Unregister();
        protected abstract void Update(long deltaTime);

        protected abstract TabItemContent[] GetSettings();

        protected abstract void TestPlugin();
    }
}
