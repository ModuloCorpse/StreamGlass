using CorpseLib.DataNotation;
using CorpseLib;
using CorpseLib.StructuredText;
using System.Windows.Media;

namespace StreamGlass.Core.StreamChat
{
    public class Message
    {
        public class DataSerializer : ADataSerializer<Message>
        {
            protected override OperationResult<Message> Deserialize(DataObject reader)
            {
                if (reader.TryGet("message", out Text? message) &&
                    reader.TryGet("user", out ChatUser? user) &&
                    reader.TryGet("id", out string? id) &&
                    reader.TryGet("source", out string? source) &&
                    reader.TryGet("reply", out string? reply) &&
                    reader.TryGet("timestamp", out int? timestamp) &&
                    reader.TryGet("is_highlighted", out bool? isHighlighted))
                    return new(new(user!, id!, [..reader.GetList<string>("badges")], message!, (int)timestamp!, source!, reply!, reader.GetOrDefault<Color?>("border_color", null), (bool)isHighlighted!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(Message obj, DataObject writer)
            {
                writer["message"] = obj.m_Content;
                writer["user"] = obj.m_User;
                writer["id"] = obj.m_ID;
                writer["badges"] = obj.m_Badges;
                writer["timestamp"] = obj.m_Timestamp;
                writer["source"] = obj.m_SourceID;
                writer["reply"] = obj.m_ReplyID;
                if (obj.m_BorderColor != null)
                    writer["border_color"] = obj.m_BorderColor;
                writer["is_highlighted"] = obj.m_IsHighlighted;
            }
        }

        private readonly ChatUser m_User;
        private readonly Text m_Content;
        private readonly Color? m_BorderColor;
        private readonly string[] m_Badges;
        private readonly string m_ID;
        private readonly string m_SourceID;
        private readonly string m_ReplyID;
        private readonly int m_Timestamp;
        private readonly bool m_IsHighlighted;

        internal Message(ChatUser user, string id, string[] badges, Text content, int timestamp, string sourceID, string replyID, Color? borderColor, bool isHighlighted)
        {
            m_User = user;
            m_ID = id;
            m_Badges = badges;
            m_Content = content;
            m_SourceID = sourceID;
            m_ReplyID = replyID;
            m_BorderColor = borderColor;
            m_Timestamp = timestamp;
            m_IsHighlighted = isHighlighted;
        }

        public ChatUser User => m_User;
        public Text Content => m_Content;
        public Color? BorderColor => m_BorderColor;
        public string[] Badges => m_Badges;
        public string ID => m_ID;
        public string SourceID => m_SourceID;
        public string ReplyID => m_ReplyID;
        public bool IsHighlighted => m_IsHighlighted;
    }
}
