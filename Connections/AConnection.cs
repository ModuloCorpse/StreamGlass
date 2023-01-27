using StreamGlass.Settings;

namespace StreamGlass.Connections
{
    public abstract class AConnection
    {
        private readonly Data m_Settings;
        private readonly StreamGlassWindow m_Form;
        private readonly string m_SettingsSection;
        private bool m_IsConnected = false;

        protected Data Settings => m_Settings;
        protected StreamGlassWindow Form => m_Form;

        protected AConnection(Data settings, StreamGlassWindow form, string settingsSection)
        {
            m_Settings = settings;
            m_Form = form;
            m_SettingsSection = settingsSection;
        }

        internal void OnCreate()
        {
            CreateSetting("auto_connect", "false");
            InitSettings();
            LoadSettings();
            if (GetSetting("auto_connect") == "true")
                Connect();
        }

        protected void CreateSetting(string name, string value) => m_Settings.Create(m_SettingsSection, name, value);
        protected string GetSetting(string name) => m_Settings.Get(m_SettingsSection, name);
        protected void SetSetting(string name, string value) => m_Settings.Set(m_SettingsSection, name, value);

        protected abstract void InitSettings();
        protected abstract void LoadSettings();
        public abstract TabItemContent[] GetSettings();

        public bool Connect()
        {
            if (!m_IsConnected)
            {
                BeforeConnect();
                if (!Authenticate())
                    return false;
                if (Init())
                {
                    m_IsConnected = true;
                    AfterConnect();
                    return true;
                }
                return false;
            }
            else
                return OnReconnect();
        }

        protected abstract void BeforeConnect();
        protected abstract bool Authenticate();
        protected abstract bool Init();
        protected abstract void AfterConnect();

        protected abstract bool OnReconnect();

        public void Update(long deltaTime)
        {
            OnUpdate(deltaTime);
        }

        protected abstract void OnUpdate(long deltaTime);

        public void Disconnect()
        {
            if (m_IsConnected)
            {
                BeforeConnect();
                Clean();
                Unauthenticate();
                m_IsConnected = false;
                AfterDisconnect();
            }
        }

        protected abstract void BeforeDisconnect();
        protected abstract void Unauthenticate();
        protected abstract void Clean();
        protected abstract void AfterDisconnect();

        public void Test()
        {
            OnTest();
        }

        protected abstract void OnTest();
    }
}
