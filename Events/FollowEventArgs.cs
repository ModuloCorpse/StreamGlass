using CorpseLib.Json;
using CorpseLib;
using CorpseLib.StructuredText;
using TwitchCorpse;

namespace StreamGlass.Events
{
    public class FollowEventArgs : FollowEventArgsBase
    {
        public class JSerializer : AJSerializer<FollowEventArgs>
        {
            protected override OperationResult<FollowEventArgs> Deserialize(JObject reader)
            {
                if (reader.TryGet("message", out Text? message) &&
                    reader.TryGet("name", out string? name) &&
                    reader.TryGet("tier", out int? tier) &&
                    reader.TryGet("month_total", out int? monthTotal) &&
                    reader.TryGet("month_streak", out int? monthStreak))
                    return new(new(name!, message!, (int)tier!, (int)monthTotal!, (int)monthStreak!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(FollowEventArgs obj, JObject writer)
            {
                writer["message"] = obj.Message;
                writer["name"] = obj.Name;
                writer["tier"] = obj.Tier;
                writer["month_total"] = obj.m_MonthTotal;
                writer["month_streak"] = obj.m_MonthStreak;
            }
        }

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
