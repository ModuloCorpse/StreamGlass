using CorpseLib.Web.API;
using StreamGlass.Core.Settings;

namespace StreamGlass.Core.Plugin
{
    public class Plugin
    {
        private readonly PluginInfo m_PluginInfo;
        private readonly Metadata m_Metadata;
        private readonly APlugin m_Instance;

        public PluginInfo Info => m_PluginInfo;
        public Metadata Metadata => m_Metadata;
        public string Name => m_Instance.Name;

        public Plugin(Metadata metadata, APlugin instance)
        {
            m_Metadata = metadata;
            m_Instance = instance;
            m_PluginInfo = m_Instance.GetPluginInfo();
            m_Instance.InitPluginDirectory(m_Metadata.Directory);
            m_Instance.RegisterPlugin();
        }

        public void Init() => m_Instance.Init();
        internal void RegisterPlugin() => m_Instance.RegisterPlugin();
        public AEndpoint[] GetPluginEndpoints() => m_Instance.GetPluginEndpoints();
        public void UnregisterPlugin() => m_Instance.UnregisterPlugin();
        public void Test() => m_Instance.Test();
        public void Tick(long deltaTime) => m_Instance.Tick(deltaTime);
        public void FillSettings(Dialog settingsDialog) => m_Instance.FillSettings(settingsDialog);
    }
}
