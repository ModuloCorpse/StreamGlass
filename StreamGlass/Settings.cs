using CorpseLib;
using CorpseLib.Json;

namespace StreamGlass
{
    public class Settings
    {
        public class JsonSerializer : AJsonSerializer<Settings>
        {
            protected override OperationResult<Settings> Deserialize(JsonObject reader)
            {
                Settings settings = new();
                if (reader.TryGet("language", out string? language) && language != null)
                    settings.CurrentLanguage = language;
                return new(settings);
            }

            protected override void Serialize(Settings obj, JsonObject writer)
            {
                writer["language"] = obj.CurrentLanguage;
            }
        }

        public string CurrentLanguage = string.Empty;
    }
}
