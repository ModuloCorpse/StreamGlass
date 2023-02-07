using StreamFeedstock.StructuredText;

namespace StreamGlass.Events
{
    public class FollowEventArgs
    {
        private readonly Text m_Message;
        private readonly string m_Name;
        private readonly int m_Tier;
        private readonly int m_MonthTotal;
        private readonly int m_MonthStreak;
        private readonly int m_NbGift;
        private readonly bool m_IsGift;

        public Text Message => m_Message;
        public string Name => m_Name;
        public int Tier => m_Tier;
        public int MonthTotal => m_MonthTotal;
        public int MonthStreak => m_MonthStreak;
        public int NbGift => m_NbGift;
        public bool IsGift => m_IsGift;

        public FollowEventArgs(string name, Text message, int tier, bool isGift, int monthTotal, int monthStreak, int nbGift)
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
