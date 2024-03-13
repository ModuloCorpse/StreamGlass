using StreamGlass.Core.Audio;

namespace StreamGlass.Twitch.Alerts
{
    internal class AlertSettings
    {
        private readonly Sound? m_Audio;
        private readonly string m_ImgPath;
        private readonly string m_Prefix;
        private readonly string m_ChatMessage;
        private readonly bool m_IsEnabled;
        private readonly bool m_HaveChatMessage;

        public Sound? Audio => m_Audio;
        public string ImgPath => m_ImgPath;
        public string Prefix => m_Prefix;
        public string ChatMessage => m_ChatMessage;
        public bool IsEnabled => m_IsEnabled;
        public bool HaveChatMessage => m_HaveChatMessage;

        public AlertSettings(Sound? audio, string imgPath, string prefix, bool isEnabled)
        {
            m_Audio = audio;
            m_ImgPath = imgPath;
            m_Prefix = prefix;
            m_ChatMessage = string.Empty;
            m_IsEnabled = isEnabled;
            m_HaveChatMessage = false;
        }

        public AlertSettings(Sound? audio, string imgPath, string prefix, string chatMessage, bool isEnabled, bool haveChatMessage)
        {
            m_Audio = audio;
            m_ImgPath = imgPath;
            m_Prefix = prefix;
            m_ChatMessage = chatMessage;
            m_IsEnabled = isEnabled;
            m_HaveChatMessage = haveChatMessage;
        }
    }
}
