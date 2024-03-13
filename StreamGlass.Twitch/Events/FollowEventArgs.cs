using CorpseLib.Json;
using CorpseLib;
using CorpseLib.StructuredText;
using TwitchCorpse;

namespace StreamGlass.Twitch.Events
{
    public class FollowEventArgs(TwitchUser? user, Text message, int tier, int monthTotal, int monthStreak) : FollowEventArgsBase(user, message, tier)
    {
        public class JSerializer : AJsonSerializer<FollowEventArgs>
        {
            protected override OperationResult<FollowEventArgs> Deserialize(JsonObject reader)
            {
                if (reader.TryGet("message", out Text? message) &&
                    reader.TryGet("user", out TwitchUser? user) &&
                    reader.TryGet("tier", out int? tier) &&
                    reader.TryGet("month_total", out int? monthTotal) &&
                    reader.TryGet("month_streak", out int? monthStreak))
                    return new(new(user!, message!, (int)tier!, (int)monthTotal!, (int)monthStreak!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(FollowEventArgs obj, JsonObject writer)
            {
                writer["message"] = obj.Message;
                writer["user"] = obj.User;
                writer["tier"] = obj.Tier;
                writer["month_total"] = obj.m_MonthTotal;
                writer["month_streak"] = obj.m_MonthStreak;
            }
        }

        private readonly int m_MonthTotal = monthTotal;
        private readonly int m_MonthStreak = monthStreak;

        public int MonthTotal => m_MonthTotal;
        public int MonthStreak => m_MonthStreak;
    }
}
