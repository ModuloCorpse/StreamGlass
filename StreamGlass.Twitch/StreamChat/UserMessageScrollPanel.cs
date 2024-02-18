using StreamGlass.Core.Controls;
using StreamGlass.Core;
using StreamGlass.Core.Profile;

namespace StreamGlass.Twitch.StreamChat
{
    public class UserMessageScrollPanel : ScrollPanel<Message>
    {
        private readonly HashSet<string> m_ChatHighlightedUsers = [];
        private double m_MessageContentFontSize = 14;
        private bool m_ShowBadges = true;

        public UserMessageScrollPanel() : base() { }

        public void Init()
        {
            StreamGlassCanals.Register<TwitchMessage>(TwitchPlugin.CHAT_MESSAGE, OnMessage);
            StreamGlassCanals.Register(TwitchPlugin.CHAT_CLEAR, ClearMessages);
            StreamGlassCanals.Register<string>(TwitchPlugin.CHAT_CLEAR_USER, RemoveAllMessagesFrom);
            StreamGlassCanals.Register<string>(TwitchPlugin.CHAT_CLEAR_MESSAGE, RemoveMessage);
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

        private void OnMessage(TwitchMessage? message)
        {
            if (message == null)
                return;

            Dispatcher.Invoke(() =>
            {
                Message chatMessage = new(this,
                message,
                m_MessageContentFontSize,
                m_ChatHighlightedUsers.Contains(message.UserID),
                m_ShowBadges);
                chatMessage.MessageContent.Loaded += (sender, e) => { UpdateControlsPosition(); };
                AddControl(chatMessage);
            });
        }

        private void RemoveAllMessagesFrom(string? userID)
        {
            if (userID == null)
                return;
            List<Message> messageToRemove = [];
            foreach (Message message in Controls)
            {
                if (message.UserID == userID)
                    messageToRemove.Add(message);
            }
            if (messageToRemove.Count > 0)
                Dispatcher.Invoke((Delegate)(() => { Remove(messageToRemove); }));
        }

        private void RemoveMessage(string? messageID)
        {
            if (messageID == null)
                return;
            foreach (Message message in Controls)
            {
                if (message.ID == messageID)
                {
                    Dispatcher.Invoke((Delegate)(() => { Remove(message); }));
                    return;
                }
            }
        }

        private void ClearMessages()
        {
            Dispatcher.Invoke((Delegate)(() => { Clear(); }));
        }
    }
}
