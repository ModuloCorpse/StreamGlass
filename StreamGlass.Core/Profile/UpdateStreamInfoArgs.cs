using CorpseLib.Json;
using CorpseLib;

namespace StreamGlass.Core.Profile
{
    public class UpdateStreamInfoArgs(string title, string description, CategoryInfo category, string language)
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

        private readonly CategoryInfo m_Category = category;
        private readonly string m_Title = title;
        private readonly string m_Description = description;
        private readonly string m_Language = language;

        public CategoryInfo Category => m_Category;
        public string Title => m_Title;
        public string Description => m_Description;
        public string Language => m_Language;
    }
}
