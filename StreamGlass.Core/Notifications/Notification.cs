using CorpseLib.StructuredText;
using StreamGlass.Core.Audio;

namespace StreamGlass.Core.Notifications
{
    public class Notification(string imagePath, Text displayableMessage, Sound? notificationSound = null)
    {
        private readonly Sound? m_Sound = notificationSound;
        private readonly Text m_Message = displayableMessage;
        private readonly string m_ImagePath = imagePath;

        public Sound? Sound => m_Sound;
        public Text Message => m_Message;
        public string ImagePath => m_ImagePath;

        public void ValidateNotification(bool validate)
        {
            //TODO
        }
    }
}
