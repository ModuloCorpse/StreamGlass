using TwitchCorpse;

namespace StreamGlass.Events
{
    public class BanEventArgs
    {
        private readonly User m_User;
        private readonly string m_Reason;
        private readonly uint m_Delay;

        public User User => m_User;
        public string Reason => m_Reason;
        public uint Delay => m_Delay;

        public BanEventArgs(User user, string reason, uint delay)
        {
            m_User = user;
            m_Reason = reason;
            m_Delay = delay;
        }

        public BanEventArgs(User user, string reason)
        {
            m_User = user;
            m_Reason = reason;
            m_Delay = 0;
        }
    }
}
