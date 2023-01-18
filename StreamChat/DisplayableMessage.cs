using System;
using System.Collections.Generic;

namespace StreamGlass.StreamChat
{
    public class DisplayableMessage
    {
        private readonly List<Tuple<int, string>> m_Emotes = new();
        private readonly string m_RawMessage;
        private readonly string m_EmotelessMessage;

        public string Message => m_RawMessage;
        public string EmotelessMessage => m_EmotelessMessage;
        public List<Tuple<int, string>> Emotes => m_Emotes;

        public DisplayableMessage(string message)
        {
            m_RawMessage = message;
            m_EmotelessMessage = message;
        }

        public DisplayableMessage(string rawMessage, string emotelessMessage)
        {
            m_RawMessage = rawMessage;
            m_EmotelessMessage = emotelessMessage;
        }

        public void AddEmote(int emotePos, string emoteID) => m_Emotes.Add(new(emotePos, emoteID));

        public static DisplayableMessage AppendPrefix(in DisplayableMessage message, string prefix)
        {
            DisplayableMessage displayableMessage = new(prefix + message.Message, prefix + message.EmotelessMessage);
            int prefixLength = prefix.Length;
            foreach (Tuple<int, string> emote in message.Emotes)
                displayableMessage.AddEmote(emote.Item1 + prefixLength, emote.Item2);
            return displayableMessage;
        }
    }
}
