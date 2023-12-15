using CorpseLib.Ini;
using CorpseLib.Web.API;
using StreamGlass.Core.Profile;

namespace StreamGlass.Core.Connections
{
    public abstract class APlugin
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private ProfileManager m_ProfileManager = null;
        private ConnectionManager m_ConnectionManager = null;
        private IniSection m_Settings = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        private readonly string m_Name;

        protected ProfileManager ProfileManager => m_ProfileManager;
        protected ConnectionManager ConnectionManager => m_ConnectionManager;
        protected IniSection Settings => m_Settings;
        public string Name => m_Name;

        protected APlugin(string name) => m_Name = name;

        protected void CreateSetting(string name, string value) => m_Settings.Add(name, value);
        protected string GetSetting(string name) => m_Settings.Get(name);
        protected void SetSetting(string name, string value) => m_Settings.Set(name, value);

        public void OnInit(ProfileManager profileManager, ConnectionManager connectionManager, IniSection settings)
        {
            m_ProfileManager = profileManager;
            m_ConnectionManager = connectionManager;
            m_Settings = settings;
            InitTranslation();
            InitSettings();
        }

        public void RegisterPluginToAPI(API api) => RegisterAPI(api);
        public void RegisterPlugin() => Register();

        protected abstract void InitTranslation();
        protected abstract void InitSettings();

        protected abstract void Register();
        protected abstract void RegisterAPI(API api);
    }
}
