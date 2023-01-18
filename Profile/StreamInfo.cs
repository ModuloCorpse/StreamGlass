using StreamFeedstock;

namespace StreamGlass.Profile
{
    public class StreamInfo
    {
        private string m_StreamTitle = "";
        private string m_StreamDescription = "";
        private CategoryInfo m_StreamCategory = new("");
        private string m_StreamLanguage = "";

        public bool HaveStreamTitle() => !string.IsNullOrWhiteSpace(m_StreamTitle);
        public string GetStreamTitle() => m_StreamTitle;
        public bool HaveStreamDescription() => !string.IsNullOrWhiteSpace(m_StreamDescription);
        public string GetStreamDescription() => m_StreamDescription;
        public bool HaveStreamCategory() => !string.IsNullOrWhiteSpace(m_StreamCategory.ID) && !string.IsNullOrWhiteSpace(m_StreamCategory.Name);
        public CategoryInfo GetStreamCategory() => m_StreamCategory;
        public bool HaveStreamLanguage() => !string.IsNullOrWhiteSpace(m_StreamLanguage);
        public string GetStreamLanguage() => m_StreamLanguage;
        public void SaveStreamInfo(string title, string description, CategoryInfo category, string language)
        {
            m_StreamTitle = title;
            m_StreamDescription = description;
            m_StreamCategory.Copy(category);
            m_StreamLanguage = language;
        }

        internal void Save(ref Json json)
        {
            if (HaveStreamTitle())
                json.Set("stream_title", m_StreamTitle);
            if (HaveStreamDescription())
                json.Set("stream_description", m_StreamDescription);
            if (HaveStreamCategory())
            {
                json.Set("stream_category_id", m_StreamCategory.ID);
                json.Set("stream_category_name", m_StreamCategory.Name);
            }
            if (HaveStreamLanguage())
                json.Set("stream_language", m_StreamLanguage);
        }

        internal void Load(Json json)
        {
            m_StreamTitle = json.GetOrDefault("stream_title", "");
            m_StreamDescription = json.GetOrDefault("stream_description", "");
            m_StreamCategory.SetID(json.GetOrDefault("stream_category_id", ""));
            m_StreamCategory.SetName(json.GetOrDefault("stream_category_name", ""));
            m_StreamLanguage = json.GetOrDefault("stream_language", "");
        }
    }
}
