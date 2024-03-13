using CorpseLib.Json;

namespace StreamGlass.Core.Profile
{
    public class StreamInfo
    {
        private readonly CategoryInfo m_StreamCategory = new(string.Empty);
        private string m_StreamTitle = string.Empty;
        private string m_StreamDescription = string.Empty;
        private string m_StreamLanguage = string.Empty;

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

        internal void Save(ref JsonObject json)
        {
            if (HaveStreamTitle())
                json.Add("stream_title", m_StreamTitle);
            if (HaveStreamDescription())
                json.Add("stream_description", m_StreamDescription);
            if (HaveStreamCategory())
            {
                json.Add("stream_category_id", m_StreamCategory.ID);
                json.Add("stream_category_name", m_StreamCategory.Name);
            }
            if (HaveStreamLanguage())
                json.Add("stream_language", m_StreamLanguage);
        }

        internal void Load(JsonObject json)
        {
            m_StreamTitle = json.GetOrDefault("stream_title", string.Empty);
            m_StreamDescription = json.GetOrDefault("stream_description", string.Empty);
            m_StreamCategory.SetID(json.GetOrDefault("stream_category_id", string.Empty));
            m_StreamCategory.SetName(json.GetOrDefault("stream_category_name", string.Empty));
            m_StreamLanguage = json.GetOrDefault("stream_language", string.Empty);
        }
    }
}
