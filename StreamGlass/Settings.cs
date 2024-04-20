using CorpseLib;
using CorpseLib.DataNotation;

namespace StreamGlass
{
    public class Settings
    {
        public class DataSerializer : ADataSerializer<Settings>
        {
            protected override OperationResult<Settings> Deserialize(DataObject reader)
            {
                Settings settings = new();
                if (reader.TryGet("language", out string? language) && language != null)
                    settings.CurrentLanguage = language;
                return new(settings);
            }

            protected override void Serialize(Settings obj, DataObject writer)
            {
                writer["language"] = obj.CurrentLanguage;
            }
        }

        public string CurrentLanguage = string.Empty;
    }
}
