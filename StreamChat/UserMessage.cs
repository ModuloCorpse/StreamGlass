using System;
using System.Collections.Generic;

namespace StreamGlass.StreamChat
{
    public class UserMessage
    {
        public enum UserType
        {
            NONE,
            MOD,
            GLOBAL_MOD,
            ADMIN,
            STAFF,
            BROADCASTER,
            SELF
        }

        private readonly bool m_IsHighlighted;
        private readonly string m_ID;
        private readonly string m_UserName;
        private readonly string m_Color;
        private readonly string m_Channel;
        private readonly UserType m_UserType;
        private readonly DisplayableMessage m_DisplayableMessage;

        public UserMessage(bool ishighlighted, string userName, string channel, string message)
        {
            m_IsHighlighted = ishighlighted;
            m_ID = Guid.NewGuid().ToString();
            m_UserName = userName;
            m_Color = "#6441A5";
            m_Channel = channel;
            m_UserType = UserType.NONE;
            m_DisplayableMessage = new(message);
        }

        public UserMessage(bool ishighlighted, string id, string userName, string color, string channel, UserType userType, DisplayableMessage displayableMessage)
        {
            m_IsHighlighted = ishighlighted;
            m_ID = id;
            m_UserName = userName;
            m_Color = color;
            m_Channel = channel;
            m_UserType = userType;
            m_DisplayableMessage = displayableMessage;
        }

        public bool IsHighlighted() => m_IsHighlighted;
        public bool IsOfType(UserType type) => type <= m_UserType;
        public bool IsNotOfType(UserType type) => type > m_UserType;

        public string ID => m_ID;
        public string UserName => m_UserName;
        public string Color => m_Color;
        public DisplayableMessage DisplayableMessage => m_DisplayableMessage;
        public string Message => m_DisplayableMessage.Message;
        public string EmotelessMessage => m_DisplayableMessage.EmotelessMessage;
        public string Channel => m_Channel;
        public UserType SenderType => m_UserType;
        public List<Tuple<int, string>> Emotes => m_DisplayableMessage.Emotes;
    }
}
