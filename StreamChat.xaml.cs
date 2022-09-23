using StreamGlass.Twitch.IRC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private BrushPaletteManager m_ChatPalette = new();
        private bool m_AutoScroll = true;
        private bool m_Reversed = false;
        private bool m_IsOnBottom = true;
        private double m_MessagesHeight = 0;
        private readonly List<ChatMessage> m_ControlMessages = new();

        public StreamChat()
        {
            InitializeComponent();
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
        }

        private void UpdateMessagePosition()
        {
            double offset = 0;
            if (m_IsOnBottom && m_MessagesHeight < (Height - 5))
                offset = Height - m_MessagesHeight - 5;
            double posY = offset;
            if (m_Reversed)
            {
                for (int i = (m_ControlMessages.Count - 1); i >= 0; --i)
                {
                    ChatMessage message = m_ControlMessages[i];
                    if (message.MessageContent.IsLoaded)
                    {
                        message.Margin = new(0, posY, 0, 0);
                        posY = message.MessageContent.ActualHeight;
                    }
                }
            }
            else
            {
                for (int i = 0; i != m_ControlMessages.Count; ++i)
                {
                    ChatMessage message = m_ControlMessages[i];
                    if (message.MessageContent.IsLoaded)
                    {
                        message.Margin = new(0, posY, 0, 0);
                        posY = message.MessageContent.ActualHeight;
                    }
                }
            }
        }

        public void InitBrushPalette(BrushPaletteManager chatPalette)
        {
            m_ChatPalette = chatPalette;
        }

        public void UpdateColorPalette()
        {
            ChatPanel.Background = m_ChatPalette.GetColor("background");
            foreach (var child in ChatPanel.Children)
            {
                if (child is ChatMessage chatMessage)
                    chatMessage.UpdatePalette();
            }
        }

        public void AddMessage(UserMessage message)
        {
            Dispatcher.Invoke(() => {
                ChatMessage chatMessage = new(m_ChatPalette, message, false);
                chatMessage.MessageContent.Loaded += ChatMessage_Loaded;
                ChatPanel.Children.Add(chatMessage);
                m_ControlMessages.Add(chatMessage);
            });
        }

        private void ChatScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (m_IsOnBottom != m_Reversed)
            {
                if (e.ExtentHeightChange == 0)
                    m_AutoScroll = (ChatScrollViewer.VerticalOffset == ChatScrollViewer.ScrollableHeight);
                if (m_AutoScroll && e.ExtentHeightChange != 0)
                    ChatScrollViewer.ScrollToVerticalOffset(ChatScrollViewer.ExtentHeight);
            }
            else
            {
                if (e.ExtentHeightChange == 0)
                    m_AutoScroll = (ChatScrollViewer.VerticalOffset == 0);
                if (m_AutoScroll && e.ExtentHeightChange != 0)
                    ChatScrollViewer.ScrollToVerticalOffset(0);
            }
        }

        private void ChatMessage_Loaded(object sender, RoutedEventArgs e)
        {
            ChatMessage chatMessage = (ChatMessage)((StackPanel)((Canvas)((StackPanel)((TextBox)sender).Parent).Parent).Parent).Parent;
            chatMessage.UpdateEmotes();
            m_MessagesHeight += chatMessage.MessageContent.ActualHeight;
            UpdateMessagePosition();
        }
    }
}
