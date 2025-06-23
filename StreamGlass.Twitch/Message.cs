using CorpseLib;
using CorpseLib.DataNotation;
using CorpseLib.StructuredText;
using TwitchCorpse.API;

namespace StreamGlass.Twitch
{
    public class Message
    {
        public class DataSerializer : ADataSerializer<Message>
        {
            protected override OperationResult<Message> Deserialize(DataObject reader)
            {
                if (reader.TryGet("message", out Text? message) &&
                    reader.TryGet("user", out TwitchUser? user) &&
                    reader.TryGet("id", out string? id) &&
                    reader.TryGet("reply", out string? reply) &&
                    reader.TryGet("announcement_color", out string? announcementColor) &&
                    reader.TryGet("color", out string? color) &&
                    reader.TryGet("channel", out string? channel) &&
                    reader.TryGet("is_highlighted", out bool? isHighlighted))
                    return new(new(user!, (bool)isHighlighted!, id!, reply!, announcementColor!, color!, channel!, message!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(Message obj, DataObject writer)
            {
                writer["message"] = obj.m_Message;
                writer["user"] = obj.m_User;
                writer["id"] = obj.m_ID;
                writer["reply"] = obj.m_ReplyID;
                writer["announcement_color"] = obj.m_AnnouncementColor;
                writer["color"] = obj.m_Color;
                writer["channel"] = obj.m_Channel;
                writer["is_highlighted"] = obj.m_IsHighlighted;
            }
        }

        private readonly Text m_Message;
        private readonly TwitchUser m_User;
        private readonly string m_ID;
        private readonly string m_ReplyID;
        private readonly string m_AnnouncementColor;
        private readonly string m_Color;
        private readonly string m_Channel;
        private readonly bool m_IsHighlighted;

        public Message(TwitchUser user, bool ishighlighted, string channel, string message)
        {
            m_IsHighlighted = ishighlighted;
            m_ID = Guid.NewGuid().ToString();
            m_ReplyID = string.Empty;
            m_User = user;
            m_AnnouncementColor = string.Empty;
            m_Color = "#6441A5";
            m_Channel = channel;
            m_Message = new(message);
        }

        public Message(TwitchUser user, bool ishighlighted, string id, string replyID, string announcementColor, string color, string channel, Text displayableMessage)
        {
            m_User = user;
            m_IsHighlighted = ishighlighted;
            m_ID = id;
            m_ReplyID = replyID;
            m_AnnouncementColor = announcementColor;
            m_Color = color;
            m_Channel = channel;
            m_Message = displayableMessage;
        }

        public bool IsHighlighted() => m_IsHighlighted;

        public string ID => m_ID;
        public string ReplyID => m_ReplyID;
        public string UserID => m_User.ID;
        public string UserDisplayName => m_User.DisplayName;
        public string AnnouncementColor => m_AnnouncementColor;
        public string Color => m_Color;
        public Text ChatMessage => m_Message;
        public TwitchUser.Type SenderType => m_User.UserType;

        public TwitchUser Sender => m_User;
    }
}
