using CorpseLib.Json;
using CorpseLib;
using TwitchCorpse;

namespace StreamGlass.Events
{
    public class RewardEventArgs
    {
        public class JSerializer : AJSerializer<RewardEventArgs>
        {
            protected override OperationResult<RewardEventArgs> Deserialize(JObject reader)
            {
                if (reader.TryGet("from", out TwitchUser? from) &&
                    reader.TryGet("reward", out string? reward) &&
                    reader.TryGet("input", out string? input))
                    return new(new(from!, reward!, input!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(RewardEventArgs obj, JObject writer)
            {
                writer["from"] = obj.m_From;
                writer["reward"] = obj.m_Reward;
                writer["input"] = obj.m_Input;
            }
        }

        private readonly TwitchUser m_From;
        private readonly string m_Reward;
        private readonly string m_Input;

        public TwitchUser From => m_From;
        public string Reward => m_Reward;
        public string Input => m_Input;

        public RewardEventArgs(TwitchUser from, string reward, string input)
        {
            m_From = from;
            m_Reward = reward;
            m_Input = input;
        }
    }
}
