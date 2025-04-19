using CorpseLib.DataNotation;
using CorpseLib.StructuredText;

namespace StreamGlass.Core.StreamChat
{
    public class MessageSource
    {
        public delegate void SendMessageDelegate(DataObject messageData);

        private readonly MessageManager m_MessageManager;
        private readonly List<IMessageFilter> m_Filters = [];
        private readonly SendMessageDelegate m_SendMessageDelegate;
        private readonly string m_ID;
        private readonly string m_Description;
        private readonly string m_LogoPath;

        public string ID => m_ID;
        public string Description => m_Description;
        public string LogoPath => m_LogoPath;

        internal MessageSource(MessageManager messageManager, string id, string description, string logoPath, SendMessageDelegate sendMessageDelegate)
        {
            m_MessageManager = messageManager;
            m_SendMessageDelegate = sendMessageDelegate;
            m_ID = id;
            m_Description = description;
            m_LogoPath = logoPath;
        }

        internal void SendMessage(DataObject messageData) => m_SendMessageDelegate.Invoke(messageData);
        public string PostMessage(MessageInfo messageInfo)
        {
            Text content = (Text)messageInfo.Content.Clone();
            foreach (IMessageFilter globalFilter in m_Filters)
                content = globalFilter.Filter(content);
            MessageInfo newInfo = new(messageInfo.UserID, content, messageInfo.Timestamp);
            newInfo.CopyFrom(messageInfo);
            return m_MessageManager.PostMessage(newInfo, m_ID);
        }

        public string NewChatUser(ChatUserInfo chatUserInfo) => m_MessageManager.NewChatUser(m_ID, chatUserInfo);
        public void UpdateChatUser(string id, ChatUserInfo chatUserInfo) => m_MessageManager.UpdateChatUser(id, chatUserInfo);
        public void RemoveMessages(string[] messageIDs) => m_MessageManager.RemoveMessages(messageIDs);
        public void RemoveAllMessagesFrom(string userID) => m_MessageManager.RemoveAllMessagesFrom(userID);
        public void ClearMessages() => m_MessageManager.ClearMessages(m_ID);
    }
}
