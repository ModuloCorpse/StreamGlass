using CorpseLib.Ini;
using CorpseLib.Translation;

namespace StreamGlass.Twitch.Alerts
{
    internal class Alert(TranslationKey translationKey, string id, AlertSettings settings)
    {
        private readonly TranslationKey m_TranslationKey = translationKey;
        private AlertSettings m_Settings = settings;
        private readonly string m_ID = id;

        public AlertSettings Settings => m_Settings;
        public string ID => m_ID;
        public string VisualName => m_TranslationKey.ToString();

        public void SetSettings(AlertSettings settings) => m_Settings = settings;

        public void LoadSettings(IniSection section)
        {
            string audioFilePath = section.GetOrAdd(string.Format("{0}_audio_file", m_ID), m_Settings.Audio?.File ?? string.Empty);
            string audioOutputPath = section.GetOrAdd(string.Format("{0}_audio_output", m_ID), m_Settings.Audio?.Output ?? string.Empty);
            string loadedImgPath = section.GetOrAdd(string.Format("{0}_path", m_ID), m_Settings.ImgPath);
            string loadedPrefix = section.GetOrAdd(string.Format("{0}_prefix", m_ID), m_Settings.Prefix);
            string loadedChatMessage = section.GetOrAdd(string.Format("{0}_chat_message", m_ID), m_Settings.ChatMessage);
            bool loadedIsEnabled = section.GetOrAdd(string.Format("{0}_enabled", m_ID), (m_Settings.IsEnabled) ? "true" : "false") == "true";
            bool loadedChatMessageIsEnabled = section.GetOrAdd(string.Format("{0}_chat_message_enabled", m_ID), (m_Settings.HaveChatMessage) ? "true" : "false") == "true";
            m_Settings = new(new(audioFilePath, audioOutputPath), loadedImgPath, loadedPrefix, loadedChatMessage, loadedIsEnabled, loadedChatMessageIsEnabled);
        }

        public void SaveSettings(IniSection section)
        {
            section.Set(string.Format("{0}_audio_file", m_ID), m_Settings.Audio?.File ?? string.Empty);
            section.Set(string.Format("{0}_audio_output", m_ID), m_Settings.Audio?.Output ?? string.Empty);
            section.Set(string.Format("{0}_path", m_ID), m_Settings.ImgPath);
            section.Set(string.Format("{0}_prefix", m_ID), m_Settings.Prefix);
            section.Set(string.Format("{0}_enabled", m_ID), (m_Settings.IsEnabled) ? "true" : "false");
            section.Set(string.Format("{0}_chat_message_enabled", m_ID), (m_Settings.HaveChatMessage) ? "true" : "false");
            section.Set(string.Format("{0}_chat_message", m_ID), (m_Settings.HaveChatMessage) ? m_Settings.ChatMessage : string.Empty);
        }
    }
}
