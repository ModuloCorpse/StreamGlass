using CorpseLib.StructuredText;

namespace StreamGlass.Events
{
    public class GiftFollowEventArgs : FollowEventArgsBase
    {
        private readonly string m_GifterName;
        private readonly int m_NbGift;
        private readonly int m_MonthGifted;
        private readonly int m_MonthStreak;

        public string GifterName => m_GifterName;
        public int NbGift => m_NbGift;
        public int MonthGifted => m_MonthGifted;
        public int MonthStreak => m_MonthStreak;

        public GiftFollowEventArgs(string name, string gifterName, Text message, int tier, int monthGifted, int monthStreak, int nbGift) : base(name, message, tier)
        {
            m_GifterName = gifterName;
            m_NbGift = nbGift;
            m_MonthGifted = monthGifted;
            m_MonthStreak = monthStreak;
        }
    }
}
