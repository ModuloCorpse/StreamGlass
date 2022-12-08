using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StreamFeedstock;
using StreamFeedstock.Controls;

namespace StreamGlass.StreamChat
{
    public partial class Message : StreamFeedstock.Controls.UserControl
    {
        private static readonly int EMOTE_SIZE = 20;
        private readonly Chat m_StreamChat;
        private readonly UserMessage m_Message;
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

        public Message(Chat streamChat, BrushPaletteManager palette, TranslationManager translation, UserMessage message, bool isHighligted, double senderWidth, double senderFontSize, double contentFontSize)
        {
            InitializeComponent();
            m_StreamChat = streamChat;
            m_Message = message;
            MessageSender.Text = message.UserName;
            MessageSender.Width = senderWidth;
            MessageContentCanvas.Margin = new Thickness(senderWidth, 0, 0, 0);
            MessageContent.Text = message.EmotelessMessage;
            SetSenderNameFontSize(senderFontSize, false);
            SetMessageFontSize(contentFontSize, false);
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
                MessageContent.BrushPaletteKey = "chat_highlight_background";
                MessageContent.TextBrushPaletteKey = "chat_highlight_message";
            }
            else
            {
                MessagePanel.BrushPaletteKey = "chat_background";
                MessageContent.BrushPaletteKey = "chat_background";
                MessageContent.TextBrushPaletteKey = "chat_message";
            }
            Update(palette, translation);
        }

        public double NameWidth { get => MessageSender.Width; }
        public double NameFontSize { get => MessageSender.FontSize; }
        public double MessageFontSize { get => MessageContent.FontSize; }

        public void SetSenderNameWidth(double width, bool updateEmotes = true)
        {
            MessageSender.Width = width;
            MessageContentCanvas.Margin = new Thickness(width, 0, 0, 0);
            MessageContent.Width = (MessagePanel.ActualWidth - MessageSender.ActualWidth) - 20;
            if (updateEmotes)
                UpdateEmotes();
        }

        public void SetSenderNameFontSize(double fontSize, bool updateEmotes = true)
        {
            MessageSender.FontSize = GetFontSize(MessageSender, fontSize);
            if (updateEmotes)
                UpdateEmotes();
        }

        public void SetMessageFontSize(double fontSize, bool updateEmotes = true)
        {
            MessageContent.FontSize = fontSize;
            if (updateEmotes)
                UpdateEmotes();
        }

        internal void UpdateEmotes()
        {
            EmoteLayer.Height = MessageContent.ActualHeight;
            EmoteLayer.Children.Clear();
            foreach (var emoteData in m_Message.Emotes)
            {
                Rect charRect = MessageContent.GetRectFromCharacterIndex(emoteData.Item1);
                string emoteURL = m_StreamChat.GetEmoteURL(emoteData.Item2);
                BitmapImage bitmap = new();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(emoteURL, UriKind.Absolute);
                bitmap.EndInit();
                EmoteLayer.Children.Add(new Image()
                {
                    Width = EMOTE_SIZE,
                    Height = EMOTE_SIZE,
                    Source = bitmap,
                    Margin = new(charRect.X, charRect.Y, 0, 0)
                });
            }
        }

        private void ToggleHighlight_Click(object sender, RoutedEventArgs e)
        {
            m_StreamChat.ToggleHighlightedUser(m_Message.UserName);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MessageContent.Width = (MessagePanel.ActualWidth - MessageSender.ActualWidth) - 20;
            Height = MessageContent.ActualHeight + Margin.Top + Margin.Bottom + MessageContent.Margin.Top + MessageContent.Margin.Bottom;
            UpdateEmotes();
        }
    }
}
