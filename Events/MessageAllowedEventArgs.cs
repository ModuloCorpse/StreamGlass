namespace StreamGlass.Events
{
    public class MessageAllowedEventArgs
    {
        private readonly User m_Sender;
        private readonly string m_MessageID;
        private readonly bool m_IsAllowed;

        public User Sender => m_Sender;
        public string MessageID => m_MessageID;
        public bool IsAllowed => m_IsAllowed;

        public MessageAllowedEventArgs(User sender, string messageID, bool isAllowed)
        {
            m_Sender = sender;
            m_MessageID = messageID;
            m_IsAllowed = isAllowed;
        }
    }
}
