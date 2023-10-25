using CorpseLib.Json;
using CorpseLib;
using TwitchCorpse;

namespace StreamGlass.Events
{
    public class MessageAllowedEventArgs
    {
        public class JSerializer : AJSerializer<MessageAllowedEventArgs>
        {
            protected override OperationResult<MessageAllowedEventArgs> Deserialize(JObject reader)
            {
                if (reader.TryGet("sender", out TwitchUser? sender) &&
                    reader.TryGet("message_id", out string? messageID) &&
                    reader.TryGet("is_allowed", out bool? isAllowed))
                    return new(new(sender!, messageID!, (bool)isAllowed!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(MessageAllowedEventArgs obj, JObject writer)
            {
                writer["sender"] = obj.m_Sender;
                writer["message_id"] = obj.m_MessageID;
                writer["is_allowed"] = obj.m_IsAllowed;
            }
        }

        private readonly TwitchUser m_Sender;
        private readonly string m_MessageID;
        private readonly bool m_IsAllowed;

        public TwitchUser Sender => m_Sender;
        public string MessageID => m_MessageID;
        public bool IsAllowed => m_IsAllowed;

        public MessageAllowedEventArgs(TwitchUser sender, string messageID, bool isAllowed)
        {
            m_Sender = sender;
            m_MessageID = messageID;
            m_IsAllowed = isAllowed;
        }
    }
}
