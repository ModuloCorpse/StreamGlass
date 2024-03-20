using CorpseLib;
using CorpseLib.Json;
using CorpseLib.StructuredText;
using TwitchCorpse;

namespace StreamGlass.Twitch.Events
{
    public class GiftFollowEventArgs(TwitchUser? user, TwitchUser? gifter, Text message, int tier, int monthGifted, int monthStreak, int nbGift) : FollowEventArgsBase(user, message, tier)
    {
        public class JSerializer : AJsonSerializer<GiftFollowEventArgs>
        {
            protected override OperationResult<GiftFollowEventArgs> Deserialize(JsonObject reader)
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

            protected override void Serialize(GiftFollowEventArgs obj, JsonObject writer)
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

        private readonly TwitchUser? m_Gifter = gifter;
        private readonly int m_NbGift = nbGift;
        private readonly int m_MonthGifted = monthGifted;
        private readonly int m_MonthStreak = monthStreak;

        public TwitchUser? Gifter => m_Gifter;
        public int NbGift => m_NbGift;
        public int MonthGifted => m_MonthGifted;
        public int MonthStreak => m_MonthStreak;
    }
}
