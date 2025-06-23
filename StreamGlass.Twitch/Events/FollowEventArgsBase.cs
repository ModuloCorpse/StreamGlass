using CorpseLib.StructuredText;
using TwitchCorpse.API;

namespace StreamGlass.Twitch.Events
{
    public class FollowEventArgsBase(TwitchUser? user, Text message, int tier)
    {
        private readonly Text m_Message = message;
        private readonly TwitchUser? m_User = user;
        private readonly int m_Tier = tier;

        public Text Message => m_Message;
        public TwitchUser? User => m_User;
        public int Tier => m_Tier;
    }
}
