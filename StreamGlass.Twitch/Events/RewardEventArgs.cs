using CorpseLib;
using CorpseLib.DataNotation;
using CorpseLib.StructuredText;
using TwitchCorpse.API;

namespace StreamGlass.Twitch.Events
{
    public class RewardEventArgs(TwitchUser from, string reward, Text input)
    {
        public class DataSerializer : ADataSerializer<RewardEventArgs>
        {
            protected override OperationResult<RewardEventArgs> Deserialize(DataObject reader)
            {
                if (reader.TryGet("from", out TwitchUser? from) &&
                    reader.TryGet("reward", out string? reward) &&
                    reader.TryGet("input", out Text? input))
                    return new(new(from!, reward!, input!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(RewardEventArgs obj, DataObject writer)
            {
                writer["from"] = obj.m_From;
                writer["reward"] = obj.m_Reward;
                writer["input"] = obj.m_Input;
            }
        }

        private readonly TwitchUser m_From = from;
        private readonly string m_Reward = reward;
        private readonly Text m_Input = input;

        public TwitchUser From => m_From;
        public string Reward => m_Reward;
        public Text Input => m_Input;
    }
}
