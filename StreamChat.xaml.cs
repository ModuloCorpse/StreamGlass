using StreamGlass.Twitch.IRC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace StreamGlass
{
    /// <summary>
    /// Interaction logic for StreamChat.xaml
    /// </summary>
    public partial class StreamChat : UserControl
    {
        public enum DisplayType
        {
            TOP_TO_BOTTOM,
            REVERSED_TOP_TO_BOTTOM,
            BOTTOM_TO_TOP,
            REVERSED_BOTTOM_TO_TOP
        }

        private readonly BrushPaletteManager m_ChatPalette = new();
        private bool m_AutoScroll = true;
        private bool m_ForceAutoScroll = false;
        private bool m_Reversed = false;
        private bool m_IsOnBottom = false;
        private double m_MessageSenderFontSize = 14;
        private double m_MessageSenderWidth = 100;
        private double m_MessageContentFontSize = 14;
        private string m_CurrentChannel = "";
        private readonly Dictionary<string, List<StreamChatMessage>> m_ControlMessages = new();
        private readonly Dictionary<string, HashSet<string>> m_ChatHighlightedUsers = new();
        private readonly Dictionary<TextBox, StreamChatMessage> m_Converter = new();

        public StreamChat()
        {
            Loaded += StreamChat_Loaded;
            InitializeComponent();
            m_ChatPalette.Load();
            UpdateColorPalette();
        }

        internal double MessageSenderFontSize => m_MessageSenderFontSize;
        internal double MessageSenderWidth => m_MessageSenderWidth;
        internal double MessageContentFontSize => m_MessageContentFontSize;

        internal void SetChannel(string channel)
        {
            m_CurrentChannel = channel;
            Dispatcher.Invoke(() => UpdateMessagePosition());
        }

        internal void AddChannel(string channel)
        {
            m_ControlMessages[channel] = new();
            m_ChatHighlightedUsers[channel] = new();
        }

        private void StreamChat_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.Closing += Window_Closing;
        }

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            m_ChatPalette.Save();
        }

        internal BrushPaletteManager GetBrushPalette() => m_ChatPalette;

        internal void SetChatPalette(string paletteID)
        {
            m_ChatPalette.SetCurrentPalette(paletteID);
            UpdateColorPalette();
        }

        internal void SetDisplayType(DisplayType displayType)
        {
            switch (displayType)
            {
                case DisplayType.TOP_TO_BOTTOM:
                    {
                        m_IsOnBottom = false;
                        m_Reversed = false;
                        break;
                    }
                case DisplayType.REVERSED_TOP_TO_BOTTOM:
                    {
                        m_IsOnBottom = false;
                        m_Reversed = true;
                        break;
                    }
                case DisplayType.BOTTOM_TO_TOP:
                    {
                        m_IsOnBottom = true;
                        m_Reversed = false;
                        break;
                    }
                case DisplayType.REVERSED_BOTTOM_TO_TOP:
                    {
                        m_IsOnBottom = true;
                        m_Reversed = true;
                        break;
                    }
            }
            UpdateMessagePosition();
            UpdateScrollBar(false);
        }

        internal void SetSenderWidth(double width)
        {
            m_MessageSenderWidth = width;
            if (m_ControlMessages.TryGetValue(m_CurrentChannel, out var chatMessageList))
            {
                foreach (StreamChatMessage message in chatMessageList)
                    message.SetSenderNameWidth(m_MessageSenderWidth);
            }
        }

        internal void SetSenderFontSize(double fontSize)
        {
            m_MessageSenderFontSize = fontSize;
            if (m_ControlMessages.TryGetValue(m_CurrentChannel, out var chatMessageList))
            {
                foreach (StreamChatMessage message in chatMessageList)
                    message.SetSenderNameFontSize(m_MessageSenderFontSize);
            }
        }

        internal void SetContentFontSize(double fontSize)
        {
            m_MessageContentFontSize = fontSize;
            if (m_ControlMessages.TryGetValue(m_CurrentChannel, out var chatMessageList))
            {
                foreach (StreamChatMessage message in chatMessageList)
                    message.SetMessageFontSize(m_MessageContentFontSize);
            }
        }

        private void UpdateMessagePosition()
        {
            if (m_ControlMessages.TryGetValue(m_CurrentChannel, out var chatMessageList))
            {
                if (m_IsOnBottom)
                    DockPanel.SetDock(ChatPanel, Dock.Bottom);
                else
                    DockPanel.SetDock(ChatPanel, Dock.Top);
                double chatPanelHeight = 0;
                ChatPanel.Children.Clear();
                int i = (m_Reversed) ? chatMessageList.Count - 1 : 0;
                int last = (m_Reversed) ? -1 : chatMessageList.Count;
                int increment = (m_Reversed) ? -1 : 1;
                while (i != last)
                {
                    StreamChatMessage message = chatMessageList[i];
                    chatPanelHeight += message.ActualHeight;
                    ChatPanel.Children.Add(message);
                    i += increment;
                }
                ChatPanelDock.Height = chatPanelHeight;
            }
        }

        public void UpdateColorPalette()
        {
            ChatPanelBackground.Background = m_ChatPalette.GetColor("background");
            ChatPanelHeader.Background = m_ChatPalette.GetColor("background");
            ChatPanel.Background = m_ChatPalette.GetColor("background");
            foreach (var child in ChatPanel.Children)
            {
                if (child is StreamChatMessage chatMessage)
                    chatMessage.UpdatePalette();
            }
        }

        internal void ToggleHighlightedUser(string userName)
        {
            if (m_ChatHighlightedUsers.TryGetValue(m_CurrentChannel, out var highlightetUsers))
            {
                if (highlightetUsers.Contains(userName))
                    highlightetUsers.Remove(userName);
                else
                    highlightetUsers.Add(userName);
            }
        }

        public void AddMessage(UserMessage message)
        {
            Dispatcher.Invoke((Delegate)(() => {
                if (m_ControlMessages.TryGetValue(message.Channel, out var chatMessageList))
                {
                    StreamChatMessage chatMessage = new(this,
                        m_ChatPalette,
                        message,
                        m_ChatHighlightedUsers[message.Channel].Contains(message.UserName),
                        m_MessageSenderWidth,
                        m_MessageSenderFontSize,
                        m_MessageContentFontSize);
                    m_Converter[chatMessage.MessageContent] = chatMessage;
                    chatMessage.MessageContent.Loaded += ChatMessage_Loaded;
                    ChatPanel.Children.Add(chatMessage);
                    chatMessageList.Add(chatMessage);
                }
            }));
        }

        private void UpdateScrollBar(bool extentHeightChange)
        {
            if (m_IsOnBottom == m_Reversed)
            {
                if (!extentHeightChange)
                    m_AutoScroll = (ChatScrollViewer.VerticalOffset == ChatScrollViewer.ScrollableHeight);
                if ((m_AutoScroll && extentHeightChange) || m_ForceAutoScroll)
                    ChatScrollViewer.ScrollToVerticalOffset(ChatScrollViewer.ExtentHeight);
            }
            else
            {
                if (!extentHeightChange)
                    m_AutoScroll = (ChatScrollViewer.VerticalOffset == 0);
                if ((m_AutoScroll && extentHeightChange) || m_ForceAutoScroll)
                    ChatScrollViewer.ScrollToVerticalOffset(0);
            }
        }

        private void ChatScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            UpdateScrollBar(e.ExtentHeightChange != 0);
        }

        private void ChatMessage_Loaded(object sender, RoutedEventArgs e)
        {
            if (m_Converter.TryGetValue((TextBox)sender, out var chatMessage))
            {
                chatMessage.UpdateEmotes();
                UpdateMessagePosition();
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChatPanelDock.MinHeight = ChatScrollViewer.ActualHeight;
            ChatPanel.Width = e.NewSize.Width - 20;
            if (m_ControlMessages.TryGetValue(m_CurrentChannel, out var chatMessageList))
            {
                foreach (StreamChatMessage message in chatMessageList)
                    message.Width = ChatPanel.Width;
            }
            UpdateMessagePosition();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StreamChatSettingsDialog dialog = new(this)
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            dialog.Show();

        }
    }
}
