using CorpseLib.StructuredText;
using TwitchCorpse;

namespace StreamGlass.Events
{
    public class FollowEventArgsBase
    {
        private readonly Text m_Message;
        private readonly TwitchUser? m_User;
        private readonly int m_Tier;

        public Text Message => m_Message;
        public TwitchUser? User => m_User;
        public int Tier => m_Tier;

        public FollowEventArgsBase(TwitchUser? user, Text message, int tier)
        {
            m_User = user;
            m_Message = message;
            m_Tier = tier;
        }
    }
}
