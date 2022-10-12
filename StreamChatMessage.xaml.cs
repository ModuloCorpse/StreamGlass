using StreamGlass.Twitch;
using StreamGlass.Twitch.IRC;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StreamGlass
{
    /// <summary>
    /// Interaction logic for ChatMessage.xaml
    /// </summary>
    public partial class StreamChatMessage : UserControl
    {
        private static readonly int EMOTE_SIZE = 20;
        private readonly StreamChat m_StreamChat;
        private readonly BrushPaletteManager m_Palette;
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

        public StreamChatMessage(StreamChat streamChat, BrushPaletteManager palette, UserMessage message, bool isHighligted, double senderWidth, double senderFontSize, double contentFontSize)
        {
            InitializeComponent();
            m_StreamChat = streamChat;
            m_Palette = palette;
            m_Message = message;
            MessageSender.Text = message.UserName;
            MessageSender.Width = senderWidth;
            MessageContentCanvas.Margin = new Thickness(senderWidth, 0, 0, 0);
            SetSenderNameFontSize(senderFontSize);
            SetMessageFontSize(contentFontSize);
            BrushConverter converter = new();
            if (!string.IsNullOrWhiteSpace(message.Color))
            {
                SolidColorBrush? color = (SolidColorBrush?)converter.ConvertFrom(message.Color);
                if (color != null)
                    MessageSender.Foreground = color;
            }
            MessageContent.Text = message.EmotelessMessage;
            m_IsHighlighted = (isHighligted || (message.SenderType > UserMessage.UserType.MOD && message.SenderType < UserMessage.UserType.MOD));
            UpdatePaletteColor();
        }

        public double NameWidth { get => MessageSender.Width; }
        public double NameFontSize { get => MessageSender.FontSize; }
        public double MessageFontSize { get => MessageContent.FontSize; }

        public void SetSenderNameWidth(double width)
        {
            MessageSender.Width = width;
            MessageContentCanvas.Margin = new Thickness(width, 0, 0, 0);
            MessageContent.Width = (MessagePanel.ActualWidth - MessageSender.ActualWidth) - 20;
        }

        public void SetSenderNameFontSize(double fontSize)
        {
            MessageSender.FontSize = GetFontSize(MessageSender, fontSize);
        }

        public void SetMessageFontSize(double fontSize)
        {
            MessageContent.FontSize = fontSize;
        }

        internal void UpdateEmotes()
        {
            EmoteLayer.Height = MessageContent.ActualHeight;
            EmoteLayer.Children.Clear();
            foreach (var emoteData in m_Message.Emotes)
            {
                Rect charRect = MessageContent.GetRectFromCharacterIndex(emoteData.Item1);
                string emoteURL = API.GetEmoteURL(emoteData.Item2, m_Palette.GetPaletteType());
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

        private void UpdatePaletteColor()
        {
            if (m_IsHighlighted)
            {
                MessageContent.Foreground = m_Palette.GetColor("highlight_message");
                MessagePanel.Background = m_Palette.GetColor("highlight_background");
            }
            else
            {
                MessageContent.Foreground = m_Palette.GetColor("message");
                MessagePanel.Background = m_Palette.GetColor("background");
            }
        }

        internal void UpdatePalette()
        {
            UpdatePaletteColor();
            UpdateEmotes();
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
