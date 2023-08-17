using CorpseLib.StructuredText;
using System;

namespace StreamGlass.StreamChat
{
    public class UserMessage
    {
        private readonly Text m_Message;
        private readonly User m_User;
        private readonly string m_ID;
        private readonly string m_Color;
        private readonly string m_Channel;
        private readonly bool m_IsHighlighted;

        public UserMessage(User user, bool ishighlighted, string channel, string message)
        {
            m_IsHighlighted = ishighlighted;
            m_ID = Guid.NewGuid().ToString();
            m_User = user;
            m_Color = "#6441A5";
            m_Channel = channel;
            m_Message = new(message);
        }

        public UserMessage(User user, bool ishighlighted, string id, string color, string channel, Text displayableMessage)
        {
            m_User = user;
            m_IsHighlighted = ishighlighted;
            m_ID = id;
            m_Color = color;
            m_Channel = channel;
            m_Message = displayableMessage;
        }

        public bool IsHighlighted() => m_IsHighlighted;

        public string ID => m_ID;
        public string UserID => m_User.ID;
        public string UserDisplayName => m_User.DisplayName;
        public string Color => m_Color;
        public Text Message => m_Message;
        public User.Type SenderType => m_User.UserType;

        public User Sender => m_User;
    }
}
