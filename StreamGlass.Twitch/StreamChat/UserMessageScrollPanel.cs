using StreamGlass.Core;
using StreamGlass.Core.Controls;

namespace StreamGlass.Twitch.StreamChat
{
    public class UserMessageScrollPanel : ScrollPanel<Message>
    {
        private readonly HashSet<string> m_ChatHighlightedUsers = [];
        private readonly object m_MessageLock = new();
        private double m_MessageContentFontSize = 14;
        private bool m_ShowBadges = true;

        public UserMessageScrollPanel() : base() { }

        public void Init()
        {
            StreamGlassCanals.Register<Twitch.Message>(TwitchPlugin.Canals.CHAT_MESSAGE, OnMessage);
            StreamGlassCanals.Register(TwitchPlugin.Canals.CHAT_CLEAR, ClearMessages);
            StreamGlassCanals.Register<string>(TwitchPlugin.Canals.CHAT_CLEAR_USER, RemoveAllMessagesFrom);
            StreamGlassCanals.Register<string>(TwitchPlugin.Canals.CHAT_CLEAR_MESSAGE, RemoveMessage);
        }

        internal double MessageContentFontSize => m_MessageContentFontSize;

        public void SetContentFontSize(double fontSize)
        {
            m_MessageContentFontSize = fontSize;
            foreach (Message message in Controls)
                message.SetMessageFontSize(m_MessageContentFontSize);
            UpdateControlsPosition();
        }

        public void ToggleHighlightedUser(string userID)
        {
            if (!m_ChatHighlightedUsers.Remove(userID))
                m_ChatHighlightedUsers.Add(userID);
        }

        private void OnMessage(Twitch.Message? message)
        {
            if (message == null)
                return;

            Dispatcher.BeginInvoke(() =>
            {
                lock (m_MessageLock)
                {
                    Twitch.Message? reply = null;
                    if (!string.IsNullOrEmpty(message.ReplyID))
                    {
                        foreach (Message replyMessage in Controls)
                        {
                            if (replyMessage.ID == message.ReplyID)
                                reply = replyMessage.TwitchMessage;
                        }
                    }

                    Message chatMessage = new(this, message, reply, m_MessageContentFontSize, m_ChatHighlightedUsers.Contains(message.UserID), m_ShowBadges);
                    chatMessage.MessageContent.Loaded += (sender, e) => { UpdateControlsPosition(); };
                    AddControl(chatMessage);
                }
            });
        }

        private void RemoveAllMessagesFrom(string? userID)
        {
            if (userID == null)
                return;
            Dispatcher.BeginInvoke(() =>
            {
                lock (m_MessageLock)
                {
                    List<Message> messageToRemove = [];
                    foreach (Message message in Controls)
                    {
                        if (message.UserID == userID)
                            messageToRemove.Add(message);
                    }
                    if (messageToRemove.Count > 0)
                        Remove(messageToRemove);
                }
            });
        }

        private void RemoveMessage(string? messageID)
        {
            if (messageID == null)
                return;
            Dispatcher.BeginInvoke(() =>
            {
                lock (m_MessageLock)
                {
                    foreach (Message message in Controls)
                    {
                        if (message.ID == messageID)
                        {
                            Remove(message);
                            return;
                        }
                    }
                }
            });
        }

        private void ClearMessages()
        {
            Dispatcher.BeginInvoke(() =>
            {
                lock (m_MessageLock)
                {
                    Clear();
                }
            });
        }
    }
}
