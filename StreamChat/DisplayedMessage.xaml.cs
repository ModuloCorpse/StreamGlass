using StreamFeedstock.Controls;
using StreamGlass.Connections;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace StreamGlass.StreamChat
{
    public partial class DisplayedMessage : StreamFeedstock.Controls.UserControl
    {
        private static readonly int EMOTE_SIZE = 20;
        private ConnectionManager? m_ConnectionManager = null;
        private BrushPaletteManager? m_BrushPalette = null;
        private DisplayableMessage? m_Message = null;

        public DisplayedMessage()
        {
            InitializeComponent();
        }

        internal void SetDisplayableMessage(ConnectionManager connectionManager, BrushPaletteManager? brushPalette, DisplayableMessage message)
        {
            m_ConnectionManager = connectionManager;
            m_BrushPalette = brushPalette;
            m_Message = message;
            MessageContent.Text = message.EmotelessMessage;
        }

        internal void SetPalette(string background, string foreground)
        {
            MessageContent.BrushPaletteKey = background;
            MessageContent.TextBrushPaletteKey = foreground;
        }

        public double MessageFontSize { get => MessageContent.FontSize; }

        public void SetMessageFontSize(double fontSize, bool updateEmotes = true)
        {
            MessageContent.FontSize = fontSize;
            if (updateEmotes)
                UpdateEmotes();
        }

        internal void UpdateEmotes()
        {
            if (m_ConnectionManager == null || m_BrushPalette == null || m_Message == null)
                return;
            EmoteLayer.Height = MessageContent.ActualHeight;
            EmoteLayer.Children.Clear();
            foreach (var emoteData in m_Message.Emotes)
            {
                Rect charRect = MessageContent.GetRectFromCharacterIndex(emoteData.Item1);
                string emoteURL = m_ConnectionManager.GetEmoteURL(emoteData.Item2, m_BrushPalette);
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
    }
}
