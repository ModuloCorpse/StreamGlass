using CorpseLib;
using CorpseLib.Json;
using CorpseLib.StructuredText;
using TwitchCorpse;

namespace StreamGlass.Twitch
{
    public class TwitchMessage
    {
        public class JSerializer : AJsonSerializer<TwitchMessage>
        {
            protected override OperationResult<TwitchMessage> Deserialize(JsonObject reader)
            {
                if (reader.TryGet("message", out Text? message) &&
                    reader.TryGet("user", out TwitchUser? user) &&
                    reader.TryGet("id", out string? id) &&
                    reader.TryGet("announcement_color", out string? announcementColor) &&
                    reader.TryGet("color", out string? color) &&
                    reader.TryGet("channel", out string? channel) &&
                    reader.TryGet("is_highlighted", out bool? isHighlighted))
                    return new(new(user!, (bool)isHighlighted!, id!, announcementColor!, color!, channel!, message!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(TwitchMessage obj, JsonObject writer)
            {
                writer["message"] = obj.m_Message;
                writer["user"] = obj.m_User;
                writer["id"] = obj.m_ID;
                writer["announcement_color"] = obj.m_AnnouncementColor;
                writer["color"] = obj.m_Color;
                writer["channel"] = obj.m_Channel;
                writer["is_highlighted"] = obj.m_IsHighlighted;
            }
        }

        private readonly Text m_Message;
        private readonly TwitchUser m_User;
        private readonly string m_ID;
        private readonly string m_AnnouncementColor;
        private readonly string m_Color;
        private readonly string m_Channel;
        private readonly bool m_IsHighlighted;

        public TwitchMessage(TwitchUser user, bool ishighlighted, string channel, string message)
        {
            m_IsHighlighted = ishighlighted;
            m_ID = Guid.NewGuid().ToString();
            m_User = user;
            m_AnnouncementColor = string.Empty;
            m_Color = "#6441A5";
            m_Channel = channel;
            m_Message = new(message);
        }

        public TwitchMessage(TwitchUser user, bool ishighlighted, string id, string announcementColor, string color, string channel, Text displayableMessage)
        {
            m_User = user;
            m_IsHighlighted = ishighlighted;
            m_ID = id;
            m_AnnouncementColor = announcementColor;
            m_Color = color;
            m_Channel = channel;
            m_Message = displayableMessage;
        }

        public bool IsHighlighted() => m_IsHighlighted;

        public string ID => m_ID;
        public string UserID => m_User.ID;
        public string UserDisplayName => m_User.DisplayName;
        public string AnnouncementColor => m_AnnouncementColor;
        public string Color => m_Color;
        public Text Message => m_Message;
        public TwitchUser.Type SenderType => m_User.UserType;

        public TwitchUser Sender => m_User;
    }
}
