using TwitchCorpse;

namespace StreamGlass.Events
{
    public class MessageAllowedEventArgs
    {
        private readonly TwitchUser m_Sender;
        private readonly string m_MessageID;
        private readonly bool m_IsAllowed;

        public TwitchUser Sender => m_Sender;
        public string MessageID => m_MessageID;
        public bool IsAllowed => m_IsAllowed;

        public MessageAllowedEventArgs(TwitchUser sender, string messageID, bool isAllowed)
        {
            m_Sender = sender;
            m_MessageID = messageID;
            m_IsAllowed = isAllowed;
        }
    }
}
