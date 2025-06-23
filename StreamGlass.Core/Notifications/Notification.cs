using CorpseLib;
using CorpseLib.DataNotation;
using CorpseLib.StructuredText;

namespace StreamGlass.Core.Notifications
{
    public class Notification(string imagePath, Text displayableMessage)
    {
        private readonly Text m_Message = displayableMessage;
        private readonly string m_ImagePath = imagePath;

        public Text Message => m_Message;
        public string ImagePath => m_ImagePath;

        public void ValidateNotification(bool validate)
        {
            //TODO
        }
    }
}
