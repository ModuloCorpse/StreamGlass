using StreamFeedstock;
using StreamFeedstock.Controls;
using System;
using System.Collections.Generic;

namespace StreamGlass.StreamChat
{
    public class UserMessageScrollPanel : ScrollPanel<Message>
    {
        private BrushPaletteManager m_ChatPalette = new();
        private TranslationManager m_Translations = new();
        private double m_MessageSenderFontSize = 14;
        private double m_MessageSenderWidth = 100;
        private double m_MessageContentFontSize = 14;
        private readonly HashSet<string> m_ChatHighlightedUsers = new();
        private IStreamChat? m_StreamChat = null;

        public UserMessageScrollPanel() : base()
        {
            CanalManager.Register<UserMessage>(StreamGlassCanals.CHAT_MESSAGE, (int _, object? message) => OnMessage((UserMessage?)message));
        }

        internal void SetBrushPalette(BrushPaletteManager colorPalette) => m_ChatPalette = colorPalette;

        internal void SetTranslations(TranslationManager translations) => m_Translations = translations;

        public void SetStreamChat(IStreamChat streamChat) => m_StreamChat = streamChat;

        public string GetEmoteURL(string id) => m_StreamChat!.GetEmoteURL(id, m_ChatPalette);

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

        public void ToggleHighlightedUser(string userName)
        {
            if (m_ChatHighlightedUsers.Contains(userName))
                m_ChatHighlightedUsers.Remove(userName);
            else
                m_ChatHighlightedUsers.Add(userName);
        }

        private void OnMessage(UserMessage? message)
        {
            if (message == null)
                return;

            Dispatcher.Invoke((Delegate)(() =>
            {
                Message chatMessage = new(this,
                m_StreamChat!,
                m_ChatPalette,
                m_Translations,
                message,
                m_ChatHighlightedUsers.Contains(message.UserName),
                m_MessageSenderWidth,
                m_MessageSenderFontSize,
                m_MessageContentFontSize);
                chatMessage.MessageContent.Loaded += (sender, e) =>
                {
                    chatMessage.UpdateEmotes();
                    UpdateControlsPosition();
                };
                AddControl(chatMessage);
            }));
        }
    }
}
