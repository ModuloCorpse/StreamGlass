using CorpseLib;
using CorpseLib.Json;
using CorpseLib.StructuredText;
using TwitchCorpse;

namespace StreamGlass.Core
{
    public class UserMessage
    {
        public class JSerializer : AJSerializer<UserMessage>
        {
            protected override OperationResult<UserMessage> Deserialize(JObject reader)
            {
                if (reader.TryGet("message", out Text? message) &&
                    reader.TryGet("user", out TwitchUser? user) &&
                    reader.TryGet("id", out string? id) &&
                    reader.TryGet("color", out string? color) &&
                    reader.TryGet("channel", out string? channel) &&
                    reader.TryGet("is_highlighted", out bool? isHighlighted))
                    return new(new(user!, (bool)isHighlighted!, id!, color!, channel!, message!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(UserMessage obj, JObject writer)
            {
                writer["message"] = obj.m_Message;
                writer["user"] = obj.m_User;
                writer["id"] = obj.m_ID;
                writer["color"] = obj.m_Color;
                writer["channel"] = obj.m_Channel;
                writer["is_highlighted"] = obj.m_IsHighlighted;
            }
        }

        private readonly Text m_Message;
        private readonly TwitchUser m_User;
        private readonly string m_ID;
        private readonly string m_Color;
        private readonly string m_Channel;
        private readonly bool m_IsHighlighted;

        public UserMessage(TwitchUser user, bool ishighlighted, string channel, string message)
        {
            m_IsHighlighted = ishighlighted;
            m_ID = Guid.NewGuid().ToString();
            m_User = user;
            m_Color = "#6441A5";
            m_Channel = channel;
            m_Message = new(message);
        }

        public UserMessage(TwitchUser user, bool ishighlighted, string id, string color, string channel, Text displayableMessage)
        {
            m_User = user;
            m_IsHighlighted = ishighlighted;
            m_ID = id;
            m_Color = color;
            m_Channel = channel;
            m_Message = displayableMessage;
        }

        public bool IsHighlighted() => m_IsHighlighted;

        public string ID => m_ID;
        public string UserID => m_User.ID;
        public string UserDisplayName => m_User.DisplayName;
        public string Color => m_Color;
        public Text Message => m_Message;
        public TwitchUser.Type SenderType => m_User.UserType;

        public TwitchUser Sender => m_User;
    }
}
