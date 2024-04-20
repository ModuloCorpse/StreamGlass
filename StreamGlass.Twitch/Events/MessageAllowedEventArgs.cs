using CorpseLib;
using CorpseLib.DataNotation;
using TwitchCorpse;

namespace StreamGlass.Twitch.Events
{
    public class MessageAllowedEventArgs(TwitchUser sender, string messageID, bool isAllowed)
    {
        public class DataSerializer : ADataSerializer<MessageAllowedEventArgs>
        {
            protected override OperationResult<MessageAllowedEventArgs> Deserialize(DataObject reader)
            {
                if (reader.TryGet("sender", out TwitchUser? sender) &&
                    reader.TryGet("message_id", out string? messageID) &&
                    reader.TryGet("is_allowed", out bool? isAllowed))
                    return new(new(sender!, messageID!, (bool)isAllowed!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(MessageAllowedEventArgs obj, DataObject writer)
            {
                writer["sender"] = obj.m_Sender;
                writer["message_id"] = obj.m_MessageID;
                writer["is_allowed"] = obj.m_IsAllowed;
            }
        }

        private readonly TwitchUser m_Sender = sender;
        private readonly string m_MessageID = messageID;
        private readonly bool m_IsAllowed = isAllowed;

        public TwitchUser Sender => m_Sender;
        public string MessageID => m_MessageID;
        public bool IsAllowed => m_IsAllowed;
    }
}
