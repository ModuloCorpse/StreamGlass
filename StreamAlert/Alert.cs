using CorpseLib.StructuredText;

namespace StreamGlass.StreamAlert
{
    public class Alert
    {
        private readonly Text m_Message;
        private readonly string m_ImagePath;

        public string ImagePath => m_ImagePath;
        public Text Message => m_Message;

        public Alert(string imagePath, Text displayableMessage)
        {
            m_ImagePath = imagePath;
            m_Message = displayableMessage;
        }
    }
}
