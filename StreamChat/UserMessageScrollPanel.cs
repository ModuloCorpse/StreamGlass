﻿using StreamFeedstock;
using StreamFeedstock.Controls;
using StreamGlass.Connections;
using StreamGlass.Twitch.IRC;
using System;
using System.Collections.Generic;
using static StreamGlass.Twitch.EventSub.EventData;

namespace StreamGlass.StreamChat
{
    public class UserMessageScrollPanel : ScrollPanel<Message>
    {
        private BrushPaletteManager m_ChatPalette = new();
        private TranslationManager m_Translations = new();
        private readonly HashSet<string> m_ChatHighlightedUsers = new();
        private ConnectionManager? m_ConnectionManager = null;
        private double m_MessageSenderFontSize = 14;
        private double m_MessageSenderWidth = 100;
        private double m_MessageContentFontSize = 14;

        public UserMessageScrollPanel() : base()
        {
            CanalManager.Register<UserMessage>(StreamGlassCanals.CHAT_MESSAGE, (int _, object? message) => OnMessage((UserMessage?)message));
            CanalManager.Register(StreamGlassCanals.CHAT_CLEAR, (int _) => ClearMessages());
            CanalManager.Register<string>(StreamGlassCanals.CHAT_CLEAR_USER, (int _, object? message) => RemoveAllMessagesFrom((string?)message));
            CanalManager.Register<string>(StreamGlassCanals.CHAT_CLEAR_MESSAGE, (int _, object? message) => RemoveMessage((string?)message));
        }

        internal void SetBrushPalette(BrushPaletteManager colorPalette) => m_ChatPalette = colorPalette;

        internal void SetTranslations(TranslationManager translations) => m_Translations = translations;

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
            if (m_ChatHighlightedUsers.Contains(userID))
                m_ChatHighlightedUsers.Remove(userID);
            else
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
                m_Translations,
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
            List<Message> messageToRemove = new();
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