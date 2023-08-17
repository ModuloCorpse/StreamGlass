using CorpseLib.StructuredText;

namespace StreamGlass.Events
{
    public class FollowEventArgs : FollowEventArgsBase
    {
        private readonly int m_MonthTotal;
        private readonly int m_MonthStreak;

        public int MonthTotal => m_MonthTotal;
        public int MonthStreak => m_MonthStreak;

        public FollowEventArgs(string name, Text message, int tier, int monthTotal, int monthStreak) : base(name, message, tier)
        {
            m_MonthTotal = monthTotal;
            m_MonthStreak = monthStreak;
        }
    }
}
