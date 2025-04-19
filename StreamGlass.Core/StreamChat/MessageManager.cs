using CorpseLib.DataNotation;
using CorpseLib.Translation;
using StreamGlass.Core.Controls;
using System.Collections.Concurrent;
using static StreamGlass.Core.StreamChat.MessageSource;

namespace StreamGlass.Core.StreamChat
{
    internal class MessageManager
    {
        private class Event { }
        private class MessageEvent(Message message) : Event { public readonly Message m_Message = message; }
        private class DeleteMessagesEvent(string[] messageIDs) : Event { public readonly string[] m_MessageIDs = messageIDs; }
        private class DeleteUserEvent(string userID) : Event { public readonly string m_UserID = userID; }
        private class ClearMessagesEvent(string sourceID) : Event { public readonly string m_SourceID = sourceID; }

        private readonly ConcurrentQueue<Event> m_Events = new();
        private readonly UserMessageScrollPanel m_StreamChatPanel = new();
        private readonly Dictionary<string, ChatUser> m_ChatUsers = [];
        private readonly Dictionary<string, Message> m_Messages = [];
        private readonly Dictionary<string, MessageSource> m_MessageSources = [];
        private readonly List<IMessageReceiver> m_MessageReceivers = [];

        internal UserMessageScrollPanel StreamChatPanel => m_StreamChatPanel;

        public MessageManager(TickManager tickManager)
        {
            m_StreamChatPanel.RegisterChatContextMenu(StreamGlassTranslationKeys.CHAT_TOGGLE_HIGHLIGHT_USER, ToggleHighlightedUser);
            m_StreamChatPanel.RegisterChatContextMenu(StreamGlassTranslationKeys.CHAT_DELETE_MESSAGE, DeleteMessage);

            tickManager.RegisterTickFunction(Tick);
            m_MessageReceivers.Add(m_StreamChatPanel);
        }

        internal void AddMessageReceiver(IMessageReceiver messageReceiver)
        {
            if (!m_MessageReceivers.Contains(messageReceiver))
                m_MessageReceivers.Add(messageReceiver);
        }

        internal void RemoveMessageReceiver(IMessageReceiver messageReceiver) => m_MessageReceivers.Remove(messageReceiver);

        internal Message? GetMessage(string id)
        {
            m_Messages.TryGetValue(id, out Message? message);
            return message;
        }

        internal void SendMessage(string id, string message)
        {
            if (m_MessageSources.TryGetValue(id, out MessageSource? source))
                source.SendMessage(new(){ {"message", message} });
        }

        internal void SendMessage(string id, DataObject messageData)
        {
            if (m_MessageSources.TryGetValue(id, out MessageSource? source))
                source.SendMessage(messageData);
        }

        internal void SendMessage(string[] sourceIDs, string message)
        {
            foreach (string id in sourceIDs)
            {
                if (m_MessageSources.TryGetValue(id, out MessageSource? source))
                    source.SendMessage(new() { { "message", message } });
            }
        }

        internal void SendMessage(string[] sourceIDs, DataObject messageData)
        {
            foreach (string id in sourceIDs)
            {
                if (m_MessageSources.TryGetValue(id, out MessageSource? source))
                    source.SendMessage(messageData);
            }
        }

        internal string NewChatUser(string sourceID, ChatUserInfo chatUserInfo)
        {
            string id = Guid.NewGuid().ToString();
            ChatUser user = new(sourceID, id, chatUserInfo.Name, chatUserInfo.UserType, chatUserInfo.Color);
            m_ChatUsers[id] = user;
            return id;
        }

        internal void UpdateChatUser(string id, ChatUserInfo chatUserInfo)
        {
            if (m_ChatUsers.TryGetValue(id, out ChatUser? user))
            {
                user.SetName(chatUserInfo.Name);
                user.SetUserType(chatUserInfo.UserType);
                user.SetColor(chatUserInfo.Color);
            }
        }

        internal string PostMessage(MessageInfo messageInfo, string sourceID)
        {
            if (m_ChatUsers.TryGetValue(messageInfo.UserID, out ChatUser? user))
            {
                if (user.ChatID != sourceID)
                {
                    //Log : user is not from this source ?
                    return string.Empty;
                }
                string id = Guid.NewGuid().ToString();
                Message message = new(user, id, messageInfo.Badges, messageInfo.Content, messageInfo.Timestamp, sourceID, messageInfo.ReplyID, messageInfo.BorderColor, messageInfo.IsHighlighted);
                m_Messages[id] = message;
                m_Events.Enqueue(new MessageEvent(message));
                return id;
            }
            //Log : user does not exist ?
            return string.Empty;
        }

        public MessageSource GetOrCreateMessageSource(string id, string description, string logoPath, SendMessageDelegate sendMessageDelegate)
        {
            if (m_MessageSources.TryGetValue(id, out MessageSource? source))
                return source;
            else
            {
                MessageSource newSource = new(this, id, description, logoPath, sendMessageDelegate);
                m_MessageSources[id] = newSource;
                return newSource;
            }
        }

        public void RemoveMessages(string[] messageIDs) => m_Events.Enqueue(new DeleteMessagesEvent(messageIDs));
        public void RemoveAllMessagesFrom(string userID) => m_Events.Enqueue(new DeleteUserEvent(userID));
        public void ClearMessages(string source) => m_Events.Enqueue(new ClearMessagesEvent(source));

        private delegate bool MessageQuery(Message message);
        private string[] QueryMessages(MessageQuery query)
        {
            List<string> messageIDs = [];
            foreach (Message message in m_Messages.Values)
            {
                if (query(message))
                    messageIDs.Add(message.ID);
            }
            return [.. messageIDs];
        }

        private void DeleteMessages(string[] messageIDs)
        {
            foreach (string messageID in messageIDs)
                m_Messages.Remove(messageID);
            foreach (IMessageReceiver messageReceiver in m_MessageReceivers)
                messageReceiver.RemoveMessages(messageIDs);
        }

        //TODO Handle in message source the destruction of the message
        private void DeleteMessage(Window _, Message message) => RemoveMessages([message.ID]);
        public void ToggleHighlightedUser(Window _, Message message) => m_StreamChatPanel.ToggleHighlightedUser(message.User.ID);

        internal void Tick(long _)
        {
            while (m_Events.TryDequeue(out Event? result))
            {
                if (result is MessageEvent messageEvent)
                {
                    Message message = messageEvent.m_Message;
                    m_Messages[message.ID] = message;
                    foreach (IMessageReceiver messageReceiver in m_MessageReceivers)
                        messageReceiver.AddMessage(message);
                }
                else if (result is DeleteMessagesEvent deleteMessagesEvent)
                    DeleteMessages(deleteMessagesEvent.m_MessageIDs);
                else if (result is DeleteUserEvent deleteUserEvent)
                    DeleteMessages(QueryMessages((Message message) => message.User.ID == deleteUserEvent.m_UserID));
                else if (result is ClearMessagesEvent clearMessagesEvent)
                    DeleteMessages(QueryMessages((Message message) => message.SourceID == clearMessagesEvent.m_SourceID));
            }
        }
    }
}
