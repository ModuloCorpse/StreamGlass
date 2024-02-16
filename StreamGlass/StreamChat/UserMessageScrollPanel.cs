using StreamGlass.Core.Controls;
using System;
using System.Collections.Generic;
using StreamGlass.Core;

namespace StreamGlass.StreamChat
{
    public class UserMessageScrollPanel : ScrollPanel<Message>
    {
        private BrushPaletteManager m_ChatPalette = new();
        private readonly HashSet<string> m_ChatHighlightedUsers = [];
        private double m_MessageContentFontSize = 14;
        private bool m_ShowBadges = true;

        public UserMessageScrollPanel() : base()
        {
            StreamGlassCanals.Register<UserMessage>("chat_message", OnMessage);
            StreamGlassCanals.Register("chat_clear", ClearMessages);
            StreamGlassCanals.Register<string>("chat_clear_user", RemoveAllMessagesFrom);
            StreamGlassCanals.Register<string>("chat_clear_message", RemoveMessage);
        }

        internal void SetBrushPalette(BrushPaletteManager colorPalette) => m_ChatPalette = colorPalette;

        internal double MessageContentFontSize => m_MessageContentFontSize;

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

            Dispatcher.Invoke(() =>
            {
                Message chatMessage = new(this,
                m_ChatPalette,
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
