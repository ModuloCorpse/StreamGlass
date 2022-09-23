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
        private bool m_Reversed = false;
        private bool m_IsOnBottom = true;
        private double m_MessagesHeight = 0;
        private string m_CurrentChannel = "";
        private readonly Dictionary<string, List<StreamChatMessage>> m_ControlMessages = new();
        private readonly Dictionary<string, HashSet<string>> m_ChatHighlightedUsers = new();

        public StreamChat()
        {
            Loaded += StreamChat_Loaded;
            InitializeComponent();
            m_ChatPalette.Load();
            m_ChatPalette.FillComboBox(ref ChatColorModeComboBox);
            ChatModeComboBox.Items.Add("To bottom");
            ChatModeComboBox.Items.Add("Reversed to bottom");
            ChatModeComboBox.Items.Add("To top");
            ChatModeComboBox.Items.Add("Reversed to top");
            ChatModeComboBox.SelectedIndex = 0;
            UpdateColorPalette();
        }

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

        private void ChatColorModeComboBox_SelectionChanged(object sender, EventArgs e)
        {
            BrushPalette.Info selector = (BrushPalette.Info)ChatColorModeComboBox.SelectedItem;
            m_ChatPalette.SetCurrentPalette(selector.ID);
            UpdateColorPalette();
        }

        private void ChatModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string displayType = (string)ChatModeComboBox.SelectedItem;
            switch (displayType)
            {
                case "To bottom": SetDisplayType(DisplayType.TOP_TO_BOTTOM); break;
                case "Reversed to bottom": SetDisplayType(DisplayType.REVERSED_TOP_TO_BOTTOM); break;
                case "To top": SetDisplayType(DisplayType.BOTTOM_TO_TOP); break;
                case "Reversed to top": SetDisplayType(DisplayType.REVERSED_BOTTOM_TO_TOP); break;
            }
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

        private void UpdateMessagePosition()
        {
            if (m_ControlMessages.TryGetValue(m_CurrentChannel, out var chatMessageList))
            {
                double posY = 0;
                if (m_IsOnBottom && m_MessagesHeight < (ChatScrollViewer.ActualHeight - 5))
                    posY = ChatScrollViewer.ActualHeight - m_MessagesHeight - 5;
                ChatPanel.Children.Clear();
                if (m_Reversed)
                {
                    for (int i = (chatMessageList.Count - 1); i >= 0; --i)
                    {
                        StreamChatMessage message = chatMessageList[i];
                        if (message.MessageContent.IsLoaded)
                        {
                            message.Margin = new(0, posY, 0, 0);
                            posY = message.MessageContent.ActualHeight;
                        }
                        ChatPanel.Children.Add(message);
                    }
                }
                else
                {
                    for (int i = 0; i != chatMessageList.Count; ++i)
                    {
                        StreamChatMessage message = chatMessageList[i];
                        if (message.MessageContent.IsLoaded)
                        {
                            message.Margin = new(0, posY, 0, 0);
                            posY = message.MessageContent.ActualHeight;
                        }
                        ChatPanel.Children.Add(message);
                    }
                }
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
                    StreamChatMessage chatMessage = new(this, m_ChatPalette, message, m_ChatHighlightedUsers[message.Channel].Contains(message.UserName));
                    chatMessage.MessageContent.Loaded += ChatMessage_Loaded;
                    ChatPanel.Children.Add(chatMessage);
                    chatMessageList.Add(chatMessage);
                }
            }));
        }

        private void UpdateScrollBar(bool extentHeightChange)
        {
            if (m_IsOnBottom != m_Reversed)
            {
                if (!extentHeightChange)
                    m_AutoScroll = (ChatScrollViewer.VerticalOffset == ChatScrollViewer.ScrollableHeight);
                if (m_AutoScroll && extentHeightChange)
                    ChatScrollViewer.ScrollToVerticalOffset(ChatScrollViewer.ExtentHeight);
            }
            else
            {
                if (!extentHeightChange)
                    m_AutoScroll = (ChatScrollViewer.VerticalOffset == 0);
                if (m_AutoScroll && extentHeightChange)
                    ChatScrollViewer.ScrollToVerticalOffset(0);
            }
        }

        private void ChatScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            UpdateScrollBar(e.ExtentHeightChange != 0);
        }

        private void ChatMessage_Loaded(object sender, RoutedEventArgs e)
        {
            StreamChatMessage chatMessage = (StreamChatMessage)((StackPanel)((Canvas)((StackPanel)((TextBox)sender).Parent).Parent).Parent).Parent;
            chatMessage.UpdateEmotes();
            m_MessagesHeight += chatMessage.MessageContent.ActualHeight;
            UpdateMessagePosition();
        }
    }
}
