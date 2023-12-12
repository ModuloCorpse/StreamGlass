using CorpseLib.Json;
using CorpseLib;
using CorpseLib.StructuredText;
using TwitchCorpse;

namespace StreamGlass.Events
{
    public class GiftFollowEventArgs : FollowEventArgsBase
    {
        public class JSerializer : AJSerializer<GiftFollowEventArgs>
        {
            protected override OperationResult<GiftFollowEventArgs> Deserialize(JObject reader)
            {
                if (reader.TryGet("message", out Text? message) &&
                    reader.TryGet("user", out TwitchUser? user) &&
                    reader.TryGet("gifter", out TwitchUser? gifter) &&
                    reader.TryGet("tier", out int? tier) &&
                    reader.TryGet("nb_gift", out int? nbGift) &&
                    reader.TryGet("month_gifted", out int? monthGifted) &&
                    reader.TryGet("month_streak", out int? monthStreak))
                    return new(new(user!, gifter, message!, (int)tier!, (int)monthGifted!, (int)monthStreak!, (int)nbGift!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(GiftFollowEventArgs obj, JObject writer)
            {
                writer["message"] = obj.Message;
                writer["user"] = obj.User;
                writer["tier"] = obj.Tier;
                writer["gifter"] = obj.m_Gifter;
                writer["nb_gift"] = obj.m_NbGift;
                writer["month_gifted"] = obj.m_MonthGifted;
                writer["month_streak"] = obj.m_MonthStreak;
            }
        }

        private readonly TwitchUser? m_Gifter;
        private readonly int m_NbGift;
        private readonly int m_MonthGifted;
        private readonly int m_MonthStreak;

        public TwitchUser? Gifter => m_Gifter;
        public int NbGift => m_NbGift;
        public int MonthGifted => m_MonthGifted;
        public int MonthStreak => m_MonthStreak;

        public GiftFollowEventArgs(TwitchUser? user, TwitchUser? gifter, Text message, int tier, int monthGifted, int monthStreak, int nbGift) : base(user, message, tier)
        {
            m_Gifter = gifter;
            m_NbGift = nbGift;
            m_MonthGifted = monthGifted;
            m_MonthStreak = monthStreak;
        }
    }
}
