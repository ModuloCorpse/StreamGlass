﻿using System;
using System.Collections.Generic;
using static StreamGlass.Twitch.IRC.Message;

namespace StreamGlass.Twitch.IRC
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

        private static readonly List<string> ms_Colors = new()
        {
            "#ff0000",
            "#00ff00",
            "#0000ff",
            "#b22222",
            "#ff7f50",
            "#9acd32",
            "#ff4500",
            "#2e8b57",
            "#daa520",
            "#d2691e",
            "#5f9ea0",
            "#1e90ff",
            "#ff69b4",
            "#8a2be2",
            "#00ff7f"
        };

        private readonly string m_ID;
        private readonly string m_UserName;
        private readonly string m_Color;
        private readonly string m_Message;
        private readonly string m_EmotelessMessage;
        private readonly string m_Channel;
        private readonly UserType m_UserType;
        private readonly List<Tuple<int, string>> m_Emotes = new();

        internal UserMessage(Message message)
        {
            m_ID = message.GetTag("id");
            m_UserName = message.GetTag("display-name");
            if (string.IsNullOrEmpty(m_UserName))
                m_UserName = message.Nick;
            m_Color = message.GetTag("color");
            if (string.IsNullOrEmpty(m_Color))
            {
                int colorIdx = 0;
                foreach (char c in m_UserName)
                    colorIdx += c;
                colorIdx %= ms_Colors.Count;
                m_Color = ms_Colors[colorIdx];
            }
            m_Message = message.Parameters;
            m_EmotelessMessage = ComputeEmotelessString(message);
            m_Channel = message.GetCommand().Channel;
            m_UserType = UserType.NONE;
            if (message.GetTag("mod") == "1")
                m_UserType = UserType.MOD;
            switch (message.GetTag("user-type"))
            {
                case "admin": m_UserType = UserType.ADMIN; break;
                case "global_mod": m_UserType = UserType.GLOBAL_MOD; break;
                case "staff": m_UserType = UserType.STAFF; break;
            }
            if (message.HaveBadge("broadcaster"))
                m_UserType = UserType.BROADCASTER;
        }

        internal UserMessage(Message message, UserInfo? selfInfo, string color)
        {
            m_ID = message.GetTag("id");
            if (selfInfo != null)
                m_UserName = selfInfo.DisplayName;
            else
                m_UserName = "StreamGlass";
            m_Color = color;
            if (string.IsNullOrEmpty(m_Color))
            {
                int colorIdx = 0;
                foreach (char c in m_UserName)
                    colorIdx += c;
                colorIdx %= ms_Colors.Count;
                m_Color = ms_Colors[colorIdx];
            }
            m_Message = message.Parameters;
            m_EmotelessMessage = ComputeEmotelessString(message);
            m_Channel = message.GetCommand().Channel;
            m_UserType = UserType.SELF;
        }

        private string ComputeEmotelessString(Message message)
        {
            string replacement = "     ";
            int offset = 0;
            string ret = message.Parameters;
            List<SimpleEmote> emotes = message.Emotes;
            foreach (SimpleEmote emote in emotes)
            {
                EmoteInfo? emoteInfo = API.GetEmoteFromID(emote.ID);
                if (emoteInfo != null)
                {
                    int emoteLength = (emote.End + 1) - emote.Start;
                    int idx = emote.Start + offset;
                    offset += replacement.Length - emoteLength;
                    ret = ret[..idx] + replacement + ret[(idx + emoteLength)..];
                    m_Emotes.Add(new(idx, emote.ID));
                }
            }
            return ret;
        }

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