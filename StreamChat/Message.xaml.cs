﻿using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StreamFeedstock;
using StreamFeedstock.Controls;
using StreamGlass.Connections;
using StreamGlass.Events;

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
            MessageSender.Text = message.UserDisplayName;
            MessageSender.Width = senderWidth;
            SetSenderNameFontSize(senderFontSize);
            MessageContent.SetText(message.Message);
            MessageContent.SetFontSize(contentFontSize);
            BrushConverter converter = new();
            if (!string.IsNullOrWhiteSpace(message.Color))
            {
                SolidColorBrush? color = (SolidColorBrush?)converter.ConvertFrom(message.Color);
                if (color != null)
                    MessageSender.Foreground = color;
            }
            m_IsHighlighted = (isHighligted || message.IsHighlighted() || (message.SenderType > User.Type.MOD && message.SenderType < User.Type.BROADCASTER));
            if (m_IsHighlighted)
            {
                MessagePanel.BrushPaletteKey = "chat_highlight_background";
                MessageContent.BrushPaletteKey = "chat_highlight_background";
                MessageContent.TextBrushPaletteKey = "chat_highlight_message";
            }
            else
                MessagePanel.BrushPaletteKey = "chat_background";
            Update(palette, translation);
        }

        public string UserID => m_Message.UserID;
        public string ID => m_Message.ID;

        public double NameWidth { get => MessageSender.Width; }
        public double NameFontSize { get => MessageSender.FontSize; }
        public double MessageFontSize { get => MessageContent.FontSize; }

        public void SetSenderNameWidth(double width)
        {
            MessageSender.Width = width;
            MessageContent.Width = (MessagePanel.ActualWidth - MessageSender.ActualWidth) - 20;
            MessageSender.FontSize = GetFontSize(MessageSender, m_MaxFontSize);
        }

        public void SetSenderNameFontSize(double fontSize)
        {
            m_MaxFontSize = fontSize;
            MessageSender.FontSize = GetFontSize(MessageSender, fontSize);
        }

        public void SetMessageFontSize(double fontSize) => MessageContent.SetFontSize(fontSize);

        private void ToggleHighlight_Click(object sender, RoutedEventArgs e)
        {
            m_StreamChat.ToggleHighlightedUser(m_Message.UserID);
        }

        private void BanUser_Click(object _, RoutedEventArgs e)
        {
            User sender = m_Message.Sender;
            if (sender.UserType == User.Type.SELF)
                return;
            BanDialog dialog = new(GetWindow(), sender);
            dialog.ShowDialog();
            BanEventArgs? args = dialog.Event;
            if (args != null)
                CanalManager.Emit(StreamGlassCanals.BAN, args);
        }
    }
}