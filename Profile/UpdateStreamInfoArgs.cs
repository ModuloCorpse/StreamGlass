using CorpseLib.Json;
using CorpseLib;

namespace StreamGlass.Profile
{
    public class UpdateStreamInfoArgs
    {
        public class JSerializer : AJSerializer<UpdateStreamInfoArgs>
        {
            protected override OperationResult<UpdateStreamInfoArgs> Deserialize(JObject reader)
            {
                if (reader.TryGet("category", out CategoryInfo? category) &&
                    reader.TryGet("title", out string? title) &&
                    reader.TryGet("description", out string? description) &&
                    reader.TryGet("language", out string? language))
                    return new(new(title!, description!, category!, language!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(UpdateStreamInfoArgs obj, JObject writer)
            {
                writer["category"] = obj.m_Category;
                writer["title"] = obj.m_Title;
                writer["description"] = obj.m_Description;
                writer["language"] = obj.m_Language;
            }
        }

        private readonly CategoryInfo m_Category;
        private readonly string m_Title;
        private readonly string m_Description;
        private readonly string m_Language;

        public CategoryInfo Category => m_Category;
        public string Title => m_Title;
        public string Description => m_Description;
        public string Language => m_Language;

        public UpdateStreamInfoArgs(string title, string description, CategoryInfo category, string language)
        {
            m_Title = title;
            m_Description = description;
            m_Category = category;
            m_Language = language;
        }
    }
}
