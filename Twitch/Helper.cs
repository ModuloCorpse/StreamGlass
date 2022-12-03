using StreamGlass.StreamChat;
using System;
using System.Collections.Generic;
using static StreamGlass.Twitch.IRC.Message;

namespace StreamGlass.Twitch
{
    public static class Helper
    {
        public static DisplayableMessage Convert(string message, List<SimpleEmote> emoteList)
        {
            List<Tuple<int, string>> emotes = new();
            string replacement = "     ";
            int offset = 0;
            string emotelessMessage = message;
            foreach (SimpleEmote emote in emoteList)
            {
                EmoteInfo? emoteInfo = API.GetEmoteFromID(emote.ID);
                if (emoteInfo != null)
                {
                    int emoteLength = (emote.End + 1) - emote.Start;
                    int idx = emote.Start + offset;
                    offset += replacement.Length - emoteLength;
                    emotelessMessage = emotelessMessage[..idx] + replacement + emotelessMessage[(idx + emoteLength)..];
                    emotes.Add(new(idx, emote.ID));
                }
            }
            DisplayableMessage displayableMessage = new(message, emotelessMessage);
            foreach (Tuple<int, string> emote in emotes)
                displayableMessage.AddEmote(emote.Item1, emote.Item2);
            return displayableMessage;
        }
    }
}
