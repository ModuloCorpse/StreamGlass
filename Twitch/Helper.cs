using CorpseLib.StructuredText;
using StreamGlass.Controls;
using System.Collections.Generic;
using TwitchCorpse;
using static StreamGlass.Twitch.IRC.Message;

namespace StreamGlass.Twitch
{
    public static class Helper
    {
        public static Text Convert(API api, string message, List<SimpleEmote> emoteList)
        {
            Text ret = new();
            int lastIndex = 0;
            foreach (SimpleEmote emote in emoteList)
            {
                ret.AddText(message[lastIndex..emote.Start]);
                EmoteInfo? emoteInfo = api.GetEmoteFromID(emote.ID);
                if (emoteInfo != null)
                    ret.AddImage(api.GetEmoteURL(emote.ID, false, 1, false));
                lastIndex = emote.End + 1;
            }
            if (lastIndex < message.Length)
                ret.AddText(message[lastIndex..message.Length]);
            return ret;
        }
    }
}
