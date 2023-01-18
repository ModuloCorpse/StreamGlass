using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StreamFeedstock;
using StreamFeedstock.Controls;
using StreamGlass.Connections;

namespace StreamGlass.StreamChat
{
    public partial class Message : StreamFeedstock.Controls.UserControl
    {
        private readonly UserMessageScrollPanel m_StreamChat;
        private readonly UserMessage m_Message;
        private double m_MaxFontSize;
        private readonly bool m_IsHighlighted;

        private static double GetFontSize(TextBlock textBlock, double textBlockFontSize)
        {
            Typeface typeFace = new(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch);
            Brush foreground = textBlock.Foreground;
            double dpi = VisualTreeHelper.GetDpi(textBlock).PixelsPerDip;
            double fontSize = textBlockFontSize;
            FormattedText ft = new(textBlock.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeFace, fontSize, foreground, dpi);
            while (textBlock.Width < ft.Width)
            {
                fontSize -= 1;
                if (fontSize < 0)
                    return textBlockFontSize;
                ft = new(textBlock.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeFace, fontSize, foreground, dpi);
            }
            return fontSize;
        }

        public Message(UserMessageScrollPanel streamChatPanel, ConnectionManager connectionManager, BrushPaletteManager palette, TranslationManager translation, UserMessage message, bool isHighligted, double senderWidth, double senderFontSize, double contentFontSize)
        {
            InitializeComponent();
            m_StreamChat = streamChatPanel;
            m_Message = message;
            MessageSender.Text = message.UserName;
            MessageSender.Width = senderWidth;
            SetSenderNameFontSize(senderFontSize, false);
            MessageContent.SetDisplayableMessage(connectionManager, palette, message.DisplayableMessage);
            MessageContent.SetMessageFontSize(contentFontSize, false);
            BrushConverter converter = new();
            if (!string.IsNullOrWhiteSpace(message.Color))
            {
                SolidColorBrush? color = (SolidColorBrush?)converter.ConvertFrom(message.Color);
                if (color != null)
                    MessageSender.Foreground = color;
            }
            m_IsHighlighted = (isHighligted || message.IsHighlighted() || (message.SenderType > UserMessage.UserType.MOD && message.SenderType < UserMessage.UserType.MOD));
            if (m_IsHighlighted)
            {
                MessagePanel.BrushPaletteKey = "chat_highlight_background";
                MessageContent.SetPalette("chat_highlight_background", "chat_highlight_message");
            }
            else
            {
                MessagePanel.BrushPaletteKey = "chat_background";
                MessageContent.SetPalette("chat_background", "chat_message");
            }
            Update(palette, translation);
        }

        public double NameWidth { get => MessageSender.Width; }
        public double NameFontSize { get => MessageSender.FontSize; }
        public double MessageFontSize { get => MessageContent.FontSize; }

        public void SetSenderNameWidth(double width, bool updateEmotes = true)
        {
            MessageSender.Width = width;
            MessageContent.Width = (MessagePanel.ActualWidth - MessageSender.ActualWidth) - 20;
            MessageSender.FontSize = GetFontSize(MessageSender, m_MaxFontSize);
            if (updateEmotes)
                UpdateEmotes();
        }

        public void SetSenderNameFontSize(double fontSize, bool updateEmotes = true)
        {
            m_MaxFontSize = fontSize;
            MessageSender.FontSize = GetFontSize(MessageSender, fontSize);
            if (updateEmotes)
                UpdateEmotes();
        }

        public void SetMessageFontSize(double fontSize, bool updateEmotes = true)
        {
            MessageContent.SetMessageFontSize(fontSize, updateEmotes);
        }

        internal void UpdateEmotes()
        {
            MessageContent.UpdateEmotes();
        }

        private void ToggleHighlight_Click(object sender, RoutedEventArgs e)
        {
            m_StreamChat.ToggleHighlightedUser(m_Message.UserName);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MessageContent.MessageContent.Width = (MessagePanel.ActualWidth - MessageSender.ActualWidth) - 20;
            Height = MessageContent.MessageContent.ActualHeight + Margin.Top + Margin.Bottom + MessageContent.Margin.Top + MessageContent.Margin.Bottom;
            UpdateEmotes();
        }
    }
}
