using StreamGlass.StreamChat;

namespace StreamGlass.StreamAlert
{
    public class Alert
    {
        private readonly string m_ImagePath;
        private readonly DisplayableMessage m_DisplayableMessage;

        public string ImagePath => m_ImagePath;
        public DisplayableMessage DisplayableMessage => m_DisplayableMessage;

        public Alert(string imagePath, DisplayableMessage displayableMessage)
        {
            m_ImagePath = imagePath;
            m_DisplayableMessage = displayableMessage;
        }
    }
}
