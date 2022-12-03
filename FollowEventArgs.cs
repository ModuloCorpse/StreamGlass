using StreamGlass.StreamChat;

namespace StreamGlass
{
    public class FollowEventArgs
    {
        private readonly string m_Name;
        private readonly DisplayableMessage m_Message;
        private readonly int m_Tier;
        private readonly bool m_IsGift;
        private readonly int m_MonthTotal;
        private readonly int m_MonthStreak;
        private readonly int m_NbGift;

        public string Name => m_Name;
        public DisplayableMessage Message => m_Message;
        public int Tier => m_Tier;
        public bool IsGift => m_IsGift;
        public int MonthTotal => m_MonthTotal;
        public int MonthStreak => m_MonthStreak;
        public int NbGift => m_NbGift;

        public FollowEventArgs(string name, DisplayableMessage message, int tier, bool isGift, int monthTotal, int monthStreak, int nbGift)
        {
            m_Name = name;
            m_Message = message;
            m_Tier = tier;
            m_IsGift = isGift;
            m_MonthTotal = monthTotal;
            m_MonthStreak = monthStreak;
            m_NbGift = nbGift;
        }
    }
}
