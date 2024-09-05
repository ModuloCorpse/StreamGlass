using CorpseLib.StructuredText;
using StreamGlass.Core;
using StreamGlass.Twitch.Events;
using System.Windows;
using System.Windows.Media;
using TwitchCorpse;
using TwitchCorpse.API;

namespace StreamGlass.Twitch.StreamChat
{
    public partial class Message : StreamGlass.Core.Controls.UserControl
    {
        private readonly UserMessageScrollPanel m_StreamChat;
        private readonly Twitch.Message m_Message;
        private readonly Twitch.Message? m_ReplyMessage;
        private readonly bool m_IsHighlighted;
        private readonly bool m_ShowBadges;

        private static Text ConvertUserReply(Twitch.Message message)
        {
            Text displayedMessage = [];
            TwitchUser user = message.Sender;
            displayedMessage.AddText("⤷");
            displayedMessage.AddText(user.DisplayName);
            displayedMessage.AddText(": ");

            foreach (Section section in message.ChatMessage)
            {
                switch (section.SectionType)
                {
                    case Section.Type.TEXT:
                    {
                        displayedMessage.AddText(section.Content, section.Properties);
                        break;
                    }
                    case Section.Type.IMAGE:
                    {
                        Dictionary<string, object> imageProperties = section.Properties;
                        imageProperties.TryAdd("Ratio", 1.5);
                        displayedMessage.AddImage(section.Content, section.Alt, imageProperties);
                        break;
                    }
                    case Section.Type.ANIMATED_IMAGE:
                    {
                        Dictionary<string, object> animatedImageProperties = section.Properties;
                        animatedImageProperties.TryAdd("Ratio", 1.5);
                        displayedMessage.AddAnimatedImage(section.Content, section.Alt, animatedImageProperties);
                        break;
                    }
                }
            }
            return displayedMessage;
        }

        private Text ConvertUserMessage(Twitch.Message message)
        {
            Text displayedMessage = [];
            TwitchUser user = message.Sender;
            if (m_ShowBadges)
            {
                Dictionary<string, object> badgeProperties = new() {
                    { "Ratio", 0.85 },
                    { "Margin-Right", 3.0 }
                };
                foreach (TwitchBadgeInfo badgeInfo in user.Badges)
                    displayedMessage.AddImage(badgeInfo.URL4x, badgeProperties);
            }
            Dictionary<string, object> properties = [];
            if (!string.IsNullOrWhiteSpace(message.Color))
                properties["Color"] = message.Color;
            properties["Bold"] = true;
            displayedMessage.AddText(user.DisplayName, properties);
            displayedMessage.AddText(": ");

            foreach (Section section in message.ChatMessage)
            {
                switch (section.SectionType)
                {
                    case Section.Type.TEXT:
                    {
                        displayedMessage.AddText(section.Content, section.Properties);
                        break;
                    }
                    case Section.Type.IMAGE:
                    {
                        Dictionary<string, object> imageProperties = section.Properties;
                        imageProperties.TryAdd("Ratio", 1.5);
                        displayedMessage.AddImage(section.Content, section.Alt, imageProperties);
                        break;
                    }
                    case Section.Type.ANIMATED_IMAGE:
                    {
                        Dictionary<string, object> animatedImageProperties = section.Properties;
                        animatedImageProperties.TryAdd("Ratio", 1.5);
                        displayedMessage.AddAnimatedImage(section.Content, section.Alt, animatedImageProperties);
                        break;
                    }
                }
            }
            return displayedMessage;
        }

        public Message(UserMessageScrollPanel streamChatPanel, Twitch.Message message, Twitch.Message? reply, double contentFontSize, bool isHighligted, bool showBadges)
        {
            InitializeComponent();
            BanMenuItem.SetTranslationKey(TwitchPlugin.TranslationKeys.MENU_BAN);
            ShoutoutMenuItem.SetTranslationKey(TwitchPlugin.TranslationKeys.MENU_SHOUTOUT);
            m_StreamChat = streamChatPanel;
            m_Message = message;
            m_ReplyMessage = reply;
            m_ShowBadges = showBadges;
            MessageContent.SetText(ConvertUserMessage(message));
            MessageContent.SetFontSize(contentFontSize);

            if (m_ReplyMessage != null)
            {
                ReplyContent.Visibility = Visibility.Visible;
                Text replyText = ConvertUserReply(m_ReplyMessage);
                ReplyContent.SetText(replyText);
                ReplyContent.SetFontSize(contentFontSize * 0.8);
            }
            else
                ReplyContent.Visibility = Visibility.Collapsed;

            BrushConverter converter = new();
            m_IsHighlighted = (isHighligted || message.IsHighlighted() || (message.SenderType > TwitchUser.Type.MOD && message.SenderType < TwitchUser.Type.BROADCASTER));
            if (m_IsHighlighted)
            {
                MessagePanel.BrushPaletteKey = "chat_highlight_background";
                MessageContent.BrushPaletteKey = "chat_highlight_background";
                MessageContent.TextBrushPaletteKey = "chat_highlight_message";
            }
            else
                MessagePanel.BrushPaletteKey = "chat_background";

            if (!string.IsNullOrEmpty(message.AnnouncementColor))
            {
                SolidColorBrush? announcementColor = (SolidColorBrush?)converter.ConvertFrom(message.AnnouncementColor);
                if (announcementColor != null)
                    AnnouncementBorder.BorderBrush = announcementColor;
            }
            else
                AnnouncementBorder.BorderBrush = Brushes.Transparent;
        }

        public Twitch.Message TwitchMessage => m_Message;
        public string UserID => m_Message.UserID;
        public string ID => m_Message.ID;

        public void SetMessageFontSize(double fontSize) => MessageContent.SetFontSize(fontSize);

        private void ToggleHighlight_Click(object sender, RoutedEventArgs e)
        {
            m_StreamChat.ToggleHighlightedUser(m_Message.UserID);
        }

        private void BanUser_Click(object _, RoutedEventArgs e)
        {
            TwitchUser sender = m_Message.Sender;
            if (sender.UserType == TwitchUser.Type.SELF)
                return;
            BanDialog dialog = new(GetWindow(), sender);
            dialog.ShowDialog();
            BanEventArgs? args = dialog.Event;
            if (args != null)
                StreamGlassCanals.Emit(TwitchPlugin.Canals.BAN, args);
        }

        private void ShoutoutUser_Click(object _, RoutedEventArgs e)
        {
            StreamGlassCanals.Emit(TwitchPlugin.Canals.SEND_SHOUTOUT, m_Message.Sender);
        }
    }
}
