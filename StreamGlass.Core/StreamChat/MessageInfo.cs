using CorpseLib.StructuredText;
using System.Windows.Media;

namespace StreamGlass.Core.StreamChat
{
    public class MessageInfo(string userID, Text content, int timestamp)
    {
        private readonly List<string> m_Badges = [];
        private readonly Text m_Content = content;
        private Color? m_BorderColor = null;
        private readonly string m_UserID = userID;
        private string m_ReplyID = string.Empty;
        private readonly int m_Timestamp = timestamp;
        private bool m_IsHighlighted = false;

        public string[] Badges => [..m_Badges];
        public string UserID => m_UserID;
        public Text Content => m_Content;
        public int Timestamp => m_Timestamp;
        public string ReplyID => m_ReplyID;
        public Color? BorderColor => m_BorderColor;
        public bool IsHighlighted => m_IsHighlighted;

        public void CopyFrom(MessageInfo messageInfo)
        {
            m_Badges.AddRange(messageInfo.Badges);
            m_BorderColor = messageInfo.BorderColor;
            m_ReplyID = messageInfo.ReplyID;
            m_IsHighlighted = messageInfo.IsHighlighted;
        }

        public void AddBadge(string badge) => m_Badges.Add(badge);
        public void SetReplyID(string replyID) => m_ReplyID = replyID;
        public void SetBorderColor(Color? borderColor) => m_BorderColor = borderColor;
        public void SetIsHighlighted(bool isHighlighted) => m_IsHighlighted = isHighlighted;
    }
}