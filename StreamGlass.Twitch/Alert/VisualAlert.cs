using CorpseLib;
using CorpseLib.DataNotation;
using CorpseLib.StructuredText;

namespace StreamGlass.Twitch.Alerts
{
    public class VisualAlert(string imagePath, Text displayableMessage)
    {
        public class DataSerializer : ADataSerializer<VisualAlert>
        {
            protected override OperationResult<VisualAlert> Deserialize(DataObject reader)
            {
                if (reader.TryGet("text", out Text? text) &&
                    reader.TryGet("image", out string? image))
                    return new(new(image!, text!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(VisualAlert obj, DataObject writer)
            {
                writer["text"] = obj.m_Message;
                writer["image"] = obj.m_ImagePath;
            }
        }

        private readonly Text m_Message = displayableMessage;
        private readonly string m_ImagePath = imagePath;
        public string ImagePath => m_ImagePath;
        public Text Message => m_Message;
    }
}
