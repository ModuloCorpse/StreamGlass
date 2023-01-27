using System;
using System.Collections.Generic;

namespace StreamGlass.StreamChat
{
    public class UserMessage
    {
        private readonly DisplayableMessage m_DisplayableMessage;
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
            m_DisplayableMessage = new(message);
        }

        public UserMessage(User user, bool ishighlighted, string id, string color, string channel, DisplayableMessage displayableMessage)
        {
            m_User = user;
            m_IsHighlighted = ishighlighted;
            m_ID = id;
            m_Color = color;
            m_Channel = channel;
            m_DisplayableMessage = displayableMessage;
        }

        public bool IsHighlighted() => m_IsHighlighted;
        public bool IsOfType(User.Type type) => type <= m_User.UserType;
        public bool IsNotOfType(User.Type type) => type > m_User.UserType;

        public string ID => m_ID;
        public string UserID => m_User.ID;
        public string UserName => m_User.Name;
        public string UserDisplayName => m_User.DisplayName;
        public string Color => m_Color;
        public DisplayableMessage DisplayableMessage => m_DisplayableMessage;
        public string Message => m_DisplayableMessage.Message;
        public string EmotelessMessage => m_DisplayableMessage.EmotelessMessage;
        public string Channel => m_Channel;
        public User.Type SenderType => m_User.UserType;
        public List<Tuple<int, string>> Emotes => m_DisplayableMessage.Emotes;
    }
}
