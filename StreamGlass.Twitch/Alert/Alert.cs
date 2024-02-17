using CorpseLib.StructuredText;

namespace StreamGlass.Twitch.Alert
{
    public class Alert(string imagePath, Text displayableMessage)
    {
        private readonly Text m_Message = displayableMessage;
        private readonly string m_ImagePath = imagePath;
        public string ImagePath => m_ImagePath;
        public Text Message => m_Message;
    }
}
