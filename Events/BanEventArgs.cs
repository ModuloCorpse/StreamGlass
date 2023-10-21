using TwitchCorpse;

namespace StreamGlass.Events
{
    public class BanEventArgs
    {
        private readonly TwitchUser m_User;
        private readonly string m_Reason;
        private readonly uint m_Delay;

        public TwitchUser User => m_User;
        public string Reason => m_Reason;
        public uint Delay => m_Delay;

        public BanEventArgs(TwitchUser user, string reason, uint delay)
        {
            m_User = user;
            m_Reason = reason;
            m_Delay = delay;
        }

        public BanEventArgs(TwitchUser user, string reason)
        {
            m_User = user;
            m_Reason = reason;
            m_Delay = 0;
        }
    }
}
