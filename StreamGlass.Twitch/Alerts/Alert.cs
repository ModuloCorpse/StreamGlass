using CorpseLib.Translation;

namespace StreamGlass.Twitch.Alerts
{
    internal class Alert(TranslationKey translationKey, string id)
    {
        private readonly TranslationKey m_TranslationKey = translationKey;
        private AlertSettings m_Settings = new(null, string.Empty, string.Empty, string.Empty, true, false);
        private readonly string m_ID = id;

        public AlertSettings Settings => m_Settings;
        public string ID => m_ID;
        public string VisualName => m_TranslationKey.ToString();

        public void SetSettings(AlertSettings settings) => m_Settings = settings;

        public void LoadSettings(Settings.AlertsSettings settings)
        {
            if (settings.AlertSettings.TryGetValue(m_ID, out AlertSettings? alertSettings))
                m_Settings = alertSettings!.Duplicate();
        }

        public void SaveSettings(Settings.AlertsSettings settings) => settings.AlertSettings[m_ID] = m_Settings.Duplicate();
    }
}
