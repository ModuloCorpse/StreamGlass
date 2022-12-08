using StreamFeedstock;

namespace StreamGlass.Profile
{
    public class StreamInfo
    {
        private string m_StreamTitle = "";
        private string m_StreamDescription = "";
        private string m_StreamCategory = "";
        private string m_StreamLanguage = "";

        public bool HaveStreamTitle() => !string.IsNullOrWhiteSpace(m_StreamTitle);
        public string GetStreamTitle() => m_StreamTitle;
        public bool HaveStreamDescription() => !string.IsNullOrWhiteSpace(m_StreamDescription);
        public string GetStreamDescription() => m_StreamDescription;
        public bool HaveStreamCategory() => !string.IsNullOrWhiteSpace(m_StreamCategory);
        public string GetStreamCategory() => m_StreamCategory;
        public bool HaveStreamLanguage() => !string.IsNullOrWhiteSpace(m_StreamLanguage);
        public string GetStreamLanguage() => m_StreamLanguage;
        public void SaveStreamInfo(string title, string description, string category, string language)
        {
            m_StreamTitle = title;
            m_StreamDescription = description;
            m_StreamCategory = category;
            m_StreamLanguage = language;
        }

        internal void Save(ref Json json)
        {
            if (HaveStreamTitle())
                json.Set("stream_title", m_StreamTitle);
            if (HaveStreamDescription())
                json.Set("stream_description", m_StreamDescription);
            if (HaveStreamCategory())
                json.Set("stream_category", m_StreamCategory);
            if (HaveStreamLanguage())
                json.Set("stream_language", m_StreamLanguage);
        }

        internal void Load(Json json)
        {
            m_StreamTitle = json.GetOrDefault("stream_title", "");
            m_StreamDescription = json.GetOrDefault("stream_description", "");
            m_StreamCategory = json.GetOrDefault("stream_category", "");
            m_StreamLanguage = json.GetOrDefault("stream_language", "");
        }
    }
}
