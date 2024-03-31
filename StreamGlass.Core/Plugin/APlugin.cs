using CorpseLib.Ini;
using StreamGlass.Core.Settings;
using System.IO;

namespace StreamGlass.Core.Plugin
{
    public abstract class APlugin(string name, string settingsFile) : IPlugin
    {
        private Metadata? m_Metadata = null;
        protected readonly IniFile m_Settings = [];
        private readonly string m_SettingsFile = settingsFile;
        private readonly string m_Name = name;

        public string Name => m_Name;

        protected APlugin(string name) : this(name, string.Empty) { }

        public void InitMetadata(Metadata metadata) => m_Metadata = metadata;

        internal PluginInfo GetPluginInfo() => GeneratePluginInfo();
        protected abstract PluginInfo GeneratePluginInfo();

        protected string GetFilePath(string fileName) => string.Format("{0}{1}", m_Metadata?.Directory ?? string.Empty, fileName);

        public void RegisterPlugin()
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

            OnLoad();
        }
        protected abstract void OnLoad();

        public void Init() => OnInit();
        protected abstract void OnInit();

        public void UnregisterPlugin()
        {
            OnUnload();
            if (!string.IsNullOrEmpty(m_SettingsFile))
                m_Settings.WriteToFile(GetFilePath(m_SettingsFile));
        }
        protected abstract void OnUnload();
    }
}
