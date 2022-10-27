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

        private readonly string m_ID;
        private readonly string m_UserName;
        private readonly string m_Color;
        private readonly string m_Message;
        private readonly string m_EmotelessMessage;
        private readonly string m_Channel;
        private readonly UserType m_UserType;
        private readonly List<Tuple<int, string>> m_Emotes = new();

        public UserMessage(string id, string userName, string color, string message, string emotelessMessage, string channel, UserType userType)
        {
            m_ID = id;
            m_UserName = userName;
            m_Color = color;
            m_Message = message;
            m_EmotelessMessage = emotelessMessage;
            m_Channel = channel;
            m_UserType = userType;
        }

        public void AddEmote(int emotePos, string emoteID) => m_Emotes.Add(new(emotePos, emoteID));

        public bool IsOfType(UserType type) => type <= m_UserType;
        public bool IsNotOfType(UserType type) => type > m_UserType;

        public string ID => m_ID;
        public string UserName => m_UserName;
        public string Color => m_Color;
        public string Message => m_Message;
        public string EmotelessMessage => m_EmotelessMessage;
        public string Channel => m_Channel;
        public UserType SenderType => m_UserType;
        public List<Tuple<int, string>> Emotes => m_Emotes;
    }
}
