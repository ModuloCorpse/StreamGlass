using StreamGlass.Controls;
using StreamGlass.Connections;
using System;
using System.Collections.Generic;

namespace StreamGlass.StreamChat
{
    public class UserMessageScrollPanel : ScrollPanel<Message>
    {
        private BrushPaletteManager m_ChatPalette = new();
        private readonly HashSet<string> m_ChatHighlightedUsers = [];
        private ConnectionManager? m_ConnectionManager = null;
        private double m_MessageSenderFontSize = 14;
        private double m_MessageSenderWidth = 100;
        private double m_MessageContentFontSize = 14;

        public UserMessageScrollPanel() : base()
        {
            StreamGlassCanals.CHAT_MESSAGE.Register(OnMessage);
            StreamGlassCanals.CHAT_CLEAR.Register(ClearMessages);
            StreamGlassCanals.CHAT_CLEAR_USER.Register(RemoveAllMessagesFrom);
            StreamGlassCanals.CHAT_CLEAR_MESSAGE.Register(RemoveMessage);
        }

        internal void SetBrushPalette(BrushPaletteManager colorPalette) => m_ChatPalette = colorPalette;

        public void SetConnectionManager(ConnectionManager connectionManager) => m_ConnectionManager = connectionManager;

        internal double MessageSenderFontSize => m_MessageSenderFontSize;
        internal double MessageSenderWidth => m_MessageSenderWidth;
        internal double MessageContentFontSize => m_MessageContentFontSize;

        internal void SetSenderWidth(double width)
        {
            m_MessageSenderWidth = width;
            foreach (Message message in Controls)
                message.SetSenderNameWidth(m_MessageSenderWidth);
            UpdateControlsPosition();
        }

        internal void SetSenderFontSize(double fontSize)
        {
            m_MessageSenderFontSize = fontSize;
            foreach (Message message in Controls)
                message.SetSenderNameFontSize(m_MessageSenderFontSize);
            UpdateControlsPosition();
        }

        internal void SetContentFontSize(double fontSize)
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

        private void OnMessage(UserMessage? message)
        {
            if (message == null)
                return;

            Dispatcher.Invoke((Delegate)(() =>
            {
                Message chatMessage = new(this,
                m_ConnectionManager!,
                m_ChatPalette,
                message,
                m_ChatHighlightedUsers.Contains(message.UserID),
                m_MessageSenderWidth,
                m_MessageSenderFontSize,
                m_MessageContentFontSize);
                chatMessage.MessageContent.Loaded += (sender, e) => { UpdateControlsPosition(); };
                AddControl(chatMessage);
            }));
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
